using Api.Data;
using Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

// Read-only reference data, used to fill the select lists of the grade form.
[ApiController]
public class ReferenceController(AppDbContext db) : ControllerBase
{
    [HttpGet("api/students")]
    public async Task<List<StudentDto>> GetStudents(CancellationToken cancellationToken)
    {
        return await db.Students
            .OrderBy(s => s.Id)
            .Select(s => new StudentDto(s.Id, s.Name))
            .ToListAsync(cancellationToken);
    }

    [HttpGet("api/subjects")]
    public async Task<List<SubjectDto>> GetSubjects(CancellationToken cancellationToken)
    {
        return await db.Subjects
            .OrderBy(s => s.Id)
            .Select(s => new SubjectDto(s.Id, s.Name, s.Coefficient))
            .ToListAsync(cancellationToken);
    }
}
