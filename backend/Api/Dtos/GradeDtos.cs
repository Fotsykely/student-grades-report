using System.ComponentModel.DataAnnotations;
using Api.Entities;

namespace Api.Dtos;

// Request body for POST. Validation attributes are picked up by [ApiController]: invalid input
// returns a 400 with one message list per field.
public class GradeRequestDto : GradeValueDto
{
    [Required(ErrorMessage = "L'étudiant est obligatoire.")]
    public int? StudentId { get; set; }

    [Required(ErrorMessage = "La matière est obligatoire.")]
    public int? SubjectId { get; set; }
}

// Request body for PUT: only the value can change (student and subject are the identity of the grade).
public class GradeValueDto : IValidatableObject
{
    [Required(ErrorMessage = "La note est obligatoire.")]
    [Range(typeof(decimal), "0", "20", ErrorMessage = "La note doit être comprise entre 0 et 20.")]
    public decimal? Value { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // A grade is stored as numeric(4,2): reject 12.345 rather than silently rounding it.
        if (Value is { } value && decimal.Round(value, Grade.MaxDecimals) != value)
            yield return new ValidationResult(
                $"La note ne peut pas avoir plus de {Grade.MaxDecimals} décimales.", [nameof(Value)]);
    }
}

public record GradeDto(
    int Id,
    int StudentId,
    string StudentName,
    int SubjectId,
    string SubjectName,
    int Coefficient,
    decimal Value);
