using Api.Entities;
using Api.Reporting;

namespace Api.Tests;

// Pure unit tests: no database, no Docker.
public class ReportCalculatorTests
{
    private static readonly Subject[] Subjects =
    [
        new() { Id = 1, Name = "Mathématiques", Coefficient = 3 },
        new() { Id = 2, Name = "Informatique", Coefficient = 3 },
        new() { Id = 3, Name = "Anglais", Coefficient = 2 },
        new() { Id = 4, Name = "Gestion", Coefficient = 1 },
    ];

    [Theory]
    [InlineData(0, Mention.Ajourne)]
    [InlineData(9.99, Mention.Ajourne)]
    [InlineData(10, Mention.Passable)]
    [InlineData(11.99, Mention.Passable)]
    [InlineData(12, Mention.AssezBien)]
    [InlineData(13.99, Mention.AssezBien)]
    [InlineData(14, Mention.Bien)]
    [InlineData(15.99, Mention.Bien)]
    [InlineData(16, Mention.TresBien)]
    [InlineData(17.99, Mention.TresBien)]
    [InlineData(18, Mention.Excellent)]
    [InlineData(20, Mention.Excellent)]
    public void Mention_follows_the_inclusive_lower_bounds(double average, Mention expected)
    {
        Assert.Equal(expected, MentionRules.For((decimal)average));
    }

    [Fact]
    public void Average_is_weighted_by_the_coefficients()
    {
        // (12*3 + 15*3 + 10*2 + 8*1) / 9 = 109 / 9 = 12.11
        var report = Build([Student(1, "Hery")], Grades(1, 12, 15, 10, 8));

        var row = Assert.Single(report.Rows);
        Assert.Equal(12.11m, row.Average);
        Assert.Equal(Mention.AssezBien, row.Mention);
        Assert.Equal(1, row.Rank);
    }

    [Fact]
    public void Student_with_a_missing_grade_has_no_average_mention_or_rank()
    {
        var grades = Grades(1, 12, 15, 10, 8).Concat(Grades(2, 14, 14, 14).Take(3)).ToList();

        var report = Build([Student(1, "Hery"), Student(2, "Voahangy")], grades);

        var incomplete = report.Rows.Single(r => r.StudentId == 2);
        Assert.False(incomplete.IsComplete);
        Assert.Null(incomplete.Average);
        Assert.Null(incomplete.Mention);
        Assert.Null(incomplete.Rank);
        Assert.Null(incomplete.Grades[3]);
    }

    [Fact]
    public void Incomplete_students_are_listed_after_the_ranked_ones_and_do_not_shift_ranks()
    {
        var grades = Grades(2, 10, 10, 10, 10).Concat(Grades(3, 18, 18, 18)).ToList(); // 1 has nothing, 3 lacks one

        var report = Build([Student(1, "Hery"), Student(2, "Voahangy"), Student(3, "Tiana")], grades);

        Assert.Equal(["Voahangy", "Hery", "Tiana"], report.Rows.Select(r => r.StudentName));
        Assert.Equal(1, report.Rows[0].Rank);
        Assert.All(report.Rows.Skip(1), r => Assert.Null(r.Rank));
    }

    [Fact]
    public void Equal_averages_share_a_rank_and_the_next_rank_is_skipped()
    {
        var grades = Grades(1, 16, 16, 16, 16)
            .Concat(Grades(2, 12, 12, 12, 12))
            .Concat(Grades(3, 12, 12, 12, 12))
            .Concat(Grades(4, 8, 8, 8, 8))
            .ToList();

        var report = Build([Student(1, "Hery"), Student(2, "Voahangy"), Student(3, "Tiana"), Student(4, "Mamy")], grades);

        Assert.Equal([1, 2, 2, 4], report.Rows.Select(r => r.Rank!.Value));
    }

    [Fact]
    public void Mention_is_decided_on_the_rounded_average()
    {
        // (11.99*3 + 12*3 + 12*2 + 12*1) / 9 = 11.9966... -> 12.00 -> "Assez bien", not "Passable"
        var report = Build([Student(1, "Hery")], Grades(1, 11.99m, 12, 12, 12));

        var row = Assert.Single(report.Rows);
        Assert.Equal(12m, row.Average);
        Assert.Equal(Mention.AssezBien, row.Mention);
    }

    [Fact]
    public void Class_average_ignores_missing_grades_and_counts_the_existing_ones()
    {
        var grades = new List<Grade>
        {
            Grade(1, 1, 10), Grade(2, 1, 15), Grade(3, 1, 12.5m), // Mathématiques: 37.5 / 3 = 12.5
            Grade(1, 2, 8),                                       // Informatique: a single grade
        };

        var report = Build([Student(1, "Hery"), Student(2, "Voahangy"), Student(3, "Tiana")], grades);

        Assert.Equal(new(1, 12.5m, 3), report.ClassAverages[0]);
        Assert.Equal(new(2, 8m, 1), report.ClassAverages[1]);
    }

    [Fact]
    public void Class_average_is_null_for_a_subject_without_any_grade()
    {
        var report = Build([Student(1, "Hery")], []);

        Assert.All(report.ClassAverages, a =>
        {
            Assert.Null(a.Average);
            Assert.Equal(0, a.GradeCount);
        });
    }

    private static Student Student(int id, string name) => new() { Id = id, Name = name };

    private static Grade Grade(int studentId, int subjectId, decimal value) =>
        new() { StudentId = studentId, SubjectId = subjectId, Value = value };

    // Grades of one student, in the order of the subjects (fewer values = the last subjects are missing).
    private static List<Grade> Grades(int studentId, params decimal[] values) =>
        values.Select((value, index) => Grade(studentId, index + 1, value)).ToList();

    private static Api.Dtos.ReportDto Build(Student[] students, IReadOnlyList<Grade> grades) =>
        ReportCalculator.Build(students, Subjects, grades);
}
