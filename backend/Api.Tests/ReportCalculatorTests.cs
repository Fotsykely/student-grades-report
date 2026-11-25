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

        Assert.Equal(new(1, 12.5m, 12.5m, 3), report.ClassAverages[0]);
        Assert.Equal(new(2, 8m, 8m, 1), report.ClassAverages[1]);
    }

    [Fact]
    public void Class_average_is_null_for_a_subject_without_any_grade()
    {
        var report = Build([Student(1, "Hery")], []);

        Assert.All(report.ClassAverages, a =>
        {
            Assert.Null(a.Average);
            Assert.Null(a.Median);
            Assert.Equal(0, a.GradeCount);
        });
    }

    [Fact]
    public void Median_is_the_middle_grade_when_the_count_is_odd_whatever_the_input_order()
    {
        var grades = new List<Grade> { Grade(1, 1, 19), Grade(2, 1, 4), Grade(3, 1, 5) }; // sorted 4, 5, 19

        var maths = Build([Student(1, "Hery"), Student(2, "Voahangy"), Student(3, "Tiana")], grades).ClassAverages[0];

        Assert.Equal(5m, maths.Median);
        Assert.Equal(9.33m, maths.Average); // the median ignores the outlier, the average does not
    }

    [Fact]
    public void Median_is_the_mean_of_the_two_middle_grades_when_the_count_is_even()
    {
        var grades = new List<Grade> { Grade(1, 1, 10), Grade(2, 1, 15), Grade(3, 1, 4), Grade(4, 1, 20) }; // sorted 4, 10, 15, 20

        var maths = Build([Student(1, "Hery"), Student(2, "Voahangy"), Student(3, "Tiana"), Student(4, "Mamy")], grades)
            .ClassAverages[0];

        Assert.Equal(12.5m, maths.Median); // (10 + 15) / 2
    }

    [Fact]
    public void Median_of_two_grades_is_rounded_to_2_decimals_away_from_zero()
    {
        var grades = new List<Grade> { Grade(1, 1, 10), Grade(2, 1, 11.25m) }; // (10 + 11.25) / 2 = 10.625

        var maths = Build([Student(1, "Hery"), Student(2, "Voahangy")], grades).ClassAverages[0];

        Assert.Equal(10.63m, maths.Median);
    }

    [Fact]
    public void Median_of_a_single_grade_is_that_grade()
    {
        var maths = Build([Student(1, "Hery")], [Grade(1, 1, 7.5m)]).ClassAverages[0];

        Assert.Equal(7.5m, maths.Median);
        Assert.Equal(1, maths.GradeCount);
    }

    [Fact]
    public void Median_is_computed_per_subject_and_ignores_missing_grades()
    {
        var grades = new List<Grade> { Grade(1, 1, 10), Grade(2, 1, 20), Grade(1, 2, 6) };

        var report = Build([Student(1, "Hery"), Student(2, "Voahangy")], grades);

        Assert.Equal(15m, report.ClassAverages[0].Median);
        Assert.Equal(6m, report.ClassAverages[1].Median);
        Assert.Null(report.ClassAverages[2].Median);
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
