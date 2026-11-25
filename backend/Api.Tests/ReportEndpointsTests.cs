using System.Net.Http.Json;
using Api.Data;
using Api.Dtos;
using Api.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

[Collection(ApiCollection.Name)]
public class ReportEndpointsTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ReportEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Grades.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Empty_report_lists_every_student_as_incomplete()
    {
        var report = await _client.GetFromJsonAsync<ReportDto>("/api/report");

        Assert.Equal(6, report!.Rows.Count);
        Assert.All(report.Rows, r =>
        {
            Assert.False(r.IsComplete);
            Assert.Null(r.Average);
            Assert.Null(r.Rank);
        });
        Assert.All(report.ClassAverages, a => Assert.Null(a.Average));
    }

    [Fact]
    public async Task Report_reflects_the_entered_grades()
    {
        // Hery (1): complete. Tiana (3): the last subject is missing.
        foreach (var (student, subject, value) in new[]
                 {
                     (1, 1, 12m), (1, 2, 15m), (1, 3, 10m), (1, 4, 8m),
                     (3, 1, 10m), (3, 2, 10m), (3, 3, 10m),
                 })
        {
            var response = await _client.PostAsJsonAsync("/api/grades", new { studentId = student, subjectId = subject, value });
            response.EnsureSuccessStatusCode();
        }

        var report = await _client.GetFromJsonAsync<ReportDto>("/api/report");

        var hery = report!.Rows.Single(r => r.StudentName == "Hery");
        Assert.Equal(12.11m, hery.Average);
        Assert.Equal(Mention.AssezBien, hery.Mention);
        Assert.Equal(1, hery.Rank);

        var tiana = report.Rows.Single(r => r.StudentName == "Tiana");
        Assert.False(tiana.IsComplete);
        Assert.Null(tiana.Average);
        Assert.Null(tiana.Rank);

        Assert.Equal(11m, report.ClassAverages[0].Average); // Mathématiques: (12 + 10) / 2
        Assert.Equal(2, report.ClassAverages[0].GradeCount);
        Assert.Equal(8m, report.ClassAverages[3].Average);   // Gestion: only Hery
    }
}
