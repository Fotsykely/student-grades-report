namespace Api.Entities;

// Reference data: the class is fixed and seeded by the migration (no screen to manage students).
public class Student
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
