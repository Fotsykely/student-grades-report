using Api.Data;
using Api.Dtos;
using Api.Reporting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/report")]
public class ReportController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ReportDto> Get(CancellationToken cancellationToken)
    {
        var students = await db.Students.AsNoTracking().ToListAsync(cancellationToken);
        var subjects = await db.Subjects.AsNoTracking().ToListAsync(cancellationToken);
        var grades = await db.Grades.AsNoTracking().ToListAsync(cancellationToken);

        return ReportCalculator.Build(students, subjects, grades);
    }
}
