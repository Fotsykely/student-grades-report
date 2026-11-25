namespace Api.Entities;

// One grade per student and per subject: enforced by a unique index (see AppDbContext).
public class Grade
{
    // Shared by the request validation and the database schema (numeric(4,2) holds 0.00 to 20.00).
    public const decimal MinValue = 0m;
    public const decimal MaxValue = 20m;
    public const int MaxDecimals = 2;

    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SubjectId { get; set; }
    public decimal Value { get; set; }

    public Student Student { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}
