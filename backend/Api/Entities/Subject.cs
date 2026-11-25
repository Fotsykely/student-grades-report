namespace Api.Entities;

// Reference data: the subjects and their coefficients are seeded by the migration.
public class Subject
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Coefficient { get; set; }
}
