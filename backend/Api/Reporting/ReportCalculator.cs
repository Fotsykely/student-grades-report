using Api.Dtos;
using Api.Entities;

namespace Api.Reporting;

// Pure computation of the report card: no database, no HTTP, easy to unit test.
public static class ReportCalculator
{
    public static ReportDto Build(
        IReadOnlyList<Student> students,
        IReadOnlyList<Subject> subjects,
        IReadOnlyList<Grade> grades)
    {
        var orderedSubjects = subjects.OrderBy(s => s.Id).ToList();
        var gradeByStudentAndSubject = grades.ToDictionary(g => (g.StudentId, g.SubjectId), g => g.Value);

        var rows = students
            .Select(student => BuildRow(student, orderedSubjects, gradeByStudentAndSubject))
            .ToList();

        return new ReportDto(
            orderedSubjects.Select(s => new SubjectDto(s.Id, s.Name, s.Coefficient)).ToList(),
            Rank(rows),
            orderedSubjects.Select(s => BuildClassAverage(s, grades)).ToList());
    }

    private static ReportRowDto BuildRow(
        Student student,
        List<Subject> subjects,
        Dictionary<(int StudentId, int SubjectId), decimal> gradeByStudentAndSubject)
    {
        decimal?[] values = subjects
            .Select(s => gradeByStudentAndSubject.TryGetValue((student.Id, s.Id), out var v) ? (decimal?)v : null)
            .ToArray();

        // A student with a missing grade has no average: showing a partial one would be misleading.
        if (values.Any(v => v is null))
            return new ReportRowDto(student.Id, student.Name, values, null, null, null, false);

        var weightedSum = subjects.Select((s, i) => values[i]!.Value * s.Coefficient).Sum();
        var totalCoefficients = subjects.Sum(s => s.Coefficient);

        // The mention and the rank use the rounded average: what is displayed is what is judged.
        var average = Math.Round(weightedSum / totalCoefficients, 2, MidpointRounding.AwayFromZero);

        return new ReportRowDto(student.Id, student.Name, values, average, MentionRules.For(average), null, true);
    }

    // Competition ranking: equal averages share a rank and the next rank is skipped (1, 2, 2, 4).
    // Complete students come first (best average first), then incomplete ones, both by name.
    private static List<ReportRowDto> Rank(List<ReportRowDto> rows)
    {
        var ranked = rows
            .Where(r => r.IsComplete)
            .OrderByDescending(r => r.Average)
            .ThenBy(r => r.StudentName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var result = ranked
            .Select(row => row with { Rank = ranked.Count(other => other.Average > row.Average) + 1 })
            .ToList();

        result.AddRange(rows
            .Where(r => !r.IsComplete)
            .OrderBy(r => r.StudentName, StringComparer.CurrentCultureIgnoreCase));

        return result;
    }

    private static ClassAverageDto BuildClassAverage(Subject subject, IReadOnlyList<Grade> grades)
    {
        var values = grades.Where(g => g.SubjectId == subject.Id).Select(g => g.Value).ToList();

        return values.Count == 0
            ? new ClassAverageDto(subject.Id, null, 0)
            : new ClassAverageDto(subject.Id, Math.Round(values.Average(), 2, MidpointRounding.AwayFromZero), values.Count);
    }
}
