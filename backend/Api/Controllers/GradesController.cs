using Api.Data;
using Api.Dtos;
using Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace Api.Controllers;

[ApiController]
[Route("api/grades")]
public class GradesController(AppDbContext db) : ControllerBase
{
    // MySQL error 1062: duplicate entry for a unique index.
    private const int MySqlDuplicateEntry = 1062;

    private const string DuplicateMessage =
        "Une note existe déjà pour cet étudiant dans cette matière. Modifiez-la depuis la liste des notes.";

    [HttpGet]
    public async Task<List<GradeDto>> GetAll(int? subjectId, CancellationToken cancellationToken)
    {
        var query = db.Grades.AsNoTracking();
        if (subjectId is not null)
            query = query.Where(g => g.SubjectId == subjectId);

        return await query
            .OrderBy(g => g.SubjectId)
            .ThenBy(g => g.Student.Name)
            .Select(g => new GradeDto(
                g.Id, g.StudentId, g.Student.Name, g.SubjectId, g.Subject.Name, g.Subject.Coefficient, g.Value))
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GradeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var grade = await FindWithReferencesAsync(id, cancellationToken);
        return grade is null ? NotFound() : ToDto(grade);
    }

    [HttpPost]
    public async Task<ActionResult<GradeDto>> Create(GradeRequestDto request, CancellationToken cancellationToken)
    {
        var studentId = request.StudentId!.Value;
        var subjectId = request.SubjectId!.Value;

        if (!await db.Students.AnyAsync(s => s.Id == studentId, cancellationToken))
            return ValidationError(nameof(GradeRequestDto.StudentId), "Cet étudiant n'existe pas.");
        if (!await db.Subjects.AnyAsync(s => s.Id == subjectId, cancellationToken))
            return ValidationError(nameof(GradeRequestDto.SubjectId), "Cette matière n'existe pas.");

        // A second grade for the same student and subject is refused, never silently replaced.
        if (await db.Grades.AnyAsync(g => g.StudentId == studentId && g.SubjectId == subjectId, cancellationToken))
            return ValidationError(nameof(GradeRequestDto.SubjectId), DuplicateMessage);

        var grade = new Grade { StudentId = studentId, SubjectId = subjectId, Value = request.Value!.Value };
        db.Grades.Add(grade);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: MySqlDuplicateEntry })
        {
            // Two requests raced past the check above: the unique index has the last word.
            return ValidationError(nameof(GradeRequestDto.SubjectId), DuplicateMessage);
        }

        var created = await FindWithReferencesAsync(grade.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = grade.Id }, ToDto(created!));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GradeDto>> Update(int id, GradeValueDto request, CancellationToken cancellationToken)
    {
        var grade = await FindWithReferencesAsync(id, cancellationToken);
        if (grade is null)
            return NotFound();

        grade.Value = request.Value!.Value;
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(grade);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deletedRows = await db.Grades.Where(g => g.Id == id).ExecuteDeleteAsync(cancellationToken);
        return deletedRows == 0 ? NotFound() : NoContent();
    }

    private Task<Grade?> FindWithReferencesAsync(int id, CancellationToken cancellationToken) =>
        db.Grades
            .Include(g => g.Student)
            .Include(g => g.Subject)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    private ActionResult ValidationError(string field, string message)
    {
        ModelState.AddModelError(field, message);
        return ValidationProblem(ModelState);
    }

    private static GradeDto ToDto(Grade grade) =>
        new(grade.Id, grade.StudentId, grade.Student.Name, grade.SubjectId, grade.Subject.Name,
            grade.Subject.Coefficient, grade.Value);
}
