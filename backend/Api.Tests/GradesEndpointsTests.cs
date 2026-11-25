using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Data;
using Api.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

[Collection(ApiCollection.Name)]
public class GradesEndpointsTests : IAsyncLifetime
{
    // Ids come from the seed data of the migration.
    private const int Hery = 1, Voahangy = 2;
    private const int Maths = 1, Computing = 2;

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public GradesEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // The container is shared by every test class: start each test from an empty Grades table.
    public async Task InitializeAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Grades.ExecuteDeleteAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Reference_data_is_seeded()
    {
        var students = await _client.GetFromJsonAsync<List<StudentDto>>("/api/students");
        var subjects = await _client.GetFromJsonAsync<List<SubjectDto>>("/api/subjects");

        Assert.Equal(["Hery", "Voahangy", "Tiana", "Mamy", "Lalaina", "Fanja"], students!.Select(s => s.Name));
        Assert.Equal([3, 3, 2, 1], subjects!.Select(s => s.Coefficient));
    }

    [Fact]
    public async Task Created_grade_can_be_read_back_with_a_decimal_value()
    {
        var created = await CreateGradeAsync(Hery, Maths, 12.5m);

        var grade = await _client.GetFromJsonAsync<GradeDto>($"/api/grades/{created.Id}");

        Assert.Equal(12.5m, grade!.Value);
        Assert.Equal("Hery", grade.StudentName);
        Assert.Equal("Mathématiques", grade.SubjectName);
    }

    [Theory]
    [InlineData(20.5)]
    [InlineData(-1)]
    [InlineData(12.345)]
    public async Task Invalid_value_answers_400_with_a_message_for_the_value_field(double value)
    {
        var response = await _client.PostAsJsonAsync("/api/grades",
            new { studentId = Hery, subjectId = Maths, value });

        await AssertValidationErrorAsync(response, "Value");
    }

    [Fact]
    public async Task Missing_value_answers_400()
    {
        var response = await _client.PostAsJsonAsync("/api/grades", new { studentId = Hery, subjectId = Maths });

        await AssertValidationErrorAsync(response, "Value");
    }

    [Fact]
    public async Task Boundary_values_0_and_20_are_accepted()
    {
        await CreateGradeAsync(Hery, Maths, 0m);
        await CreateGradeAsync(Hery, Computing, 20m);
    }

    [Fact]
    public async Task Second_grade_for_the_same_student_and_subject_is_refused_and_keeps_the_first()
    {
        var first = await CreateGradeAsync(Hery, Maths, 10m);

        var response = await _client.PostAsJsonAsync("/api/grades",
            new { studentId = Hery, subjectId = Maths, value = 18 });

        await AssertValidationErrorAsync(response, "SubjectId");
        var kept = await _client.GetFromJsonAsync<GradeDto>($"/api/grades/{first.Id}");
        Assert.Equal(10m, kept!.Value);
    }

    [Fact]
    public async Task Unknown_student_or_subject_answers_400()
    {
        var unknownStudent = await _client.PostAsJsonAsync("/api/grades", new { studentId = 999, subjectId = Maths, value = 10 });
        var unknownSubject = await _client.PostAsJsonAsync("/api/grades", new { studentId = Hery, subjectId = 999, value = 10 });

        await AssertValidationErrorAsync(unknownStudent, "StudentId");
        await AssertValidationErrorAsync(unknownSubject, "SubjectId");
    }

    [Fact]
    public async Task List_can_be_filtered_by_subject()
    {
        await CreateGradeAsync(Hery, Maths, 10m);
        await CreateGradeAsync(Voahangy, Maths, 11m);
        await CreateGradeAsync(Hery, Computing, 12m);

        var all = await _client.GetFromJsonAsync<List<GradeDto>>("/api/grades");
        var maths = await _client.GetFromJsonAsync<List<GradeDto>>($"/api/grades?subjectId={Maths}");

        Assert.Equal(3, all!.Count);
        Assert.Equal(["Hery", "Voahangy"], maths!.Select(g => g.StudentName));
        Assert.All(maths!, g => Assert.Equal(Maths, g.SubjectId));
    }

    [Fact]
    public async Task Updating_changes_only_the_value()
    {
        var created = await CreateGradeAsync(Hery, Maths, 10m);

        var response = await _client.PutAsJsonAsync($"/api/grades/{created.Id}", new { value = 14.25 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var grade = await response.Content.ReadFromJsonAsync<GradeDto>();
        Assert.Equal(14.25m, grade!.Value);
        Assert.Equal(Hery, grade.StudentId);
        Assert.Equal(Maths, grade.SubjectId);
    }

    [Fact]
    public async Task Updating_with_an_invalid_value_answers_400_and_an_unknown_grade_404()
    {
        var created = await CreateGradeAsync(Hery, Maths, 10m);

        var invalid = await _client.PutAsJsonAsync($"/api/grades/{created.Id}", new { value = 25 });
        var unknown = await _client.PutAsJsonAsync("/api/grades/999999", new { value = 10 });

        await AssertValidationErrorAsync(invalid, "Value");
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
    }

    [Fact]
    public async Task Deleting_answers_204_then_404_and_frees_the_student_and_subject_pair()
    {
        var created = await CreateGradeAsync(Hery, Maths, 10m);

        var first = await _client.DeleteAsync($"/api/grades/{created.Id}");
        var second = await _client.DeleteAsync($"/api/grades/{created.Id}");
        await CreateGradeAsync(Hery, Maths, 15m);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
    }

    [Fact]
    public async Task Reading_an_unknown_grade_answers_404()
    {
        var response = await _client.GetAsync("/api/grades/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<GradeDto> CreateGradeAsync(int studentId, int subjectId, decimal value)
    {
        var response = await _client.PostAsJsonAsync("/api/grades", new { studentId, subjectId, value });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<GradeDto>())!;
    }

    private static async Task AssertValidationErrorAsync(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty(field, out var messages));
        Assert.NotEmpty(messages.EnumerateArray());
    }
}
