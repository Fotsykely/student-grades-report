using Api.Reporting;

namespace Api.Dtos;

// The report is computed on the fly from the grades: nothing derived is ever stored.
public record ReportDto(
    IReadOnlyList<SubjectDto> Subjects,
    IReadOnlyList<ReportRowDto> Rows,
    IReadOnlyList<ClassAverageDto> ClassAverages);

// Grades are listed in the same order as ReportDto.Subjects; null means "no grade yet".
// Average, Mention and Rank are null together when the student is incomplete (a grade is missing).
public record ReportRowDto(
    int StudentId,
    string StudentName,
    IReadOnlyList<decimal?> Grades,
    decimal? Average,
    Mention? Mention,
    int? Rank,
    bool IsComplete);

// Average of the grades that exist for a subject; null when nobody has a grade yet.
public record ClassAverageDto(int SubjectId, decimal? Average, int GradeCount);
