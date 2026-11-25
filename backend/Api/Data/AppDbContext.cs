using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(student =>
        {
            student.Property(s => s.Name).HasMaxLength(100);
            student.HasData(
                new Student { Id = 1, Name = "Hery" },
                new Student { Id = 2, Name = "Voahangy" },
                new Student { Id = 3, Name = "Tiana" },
                new Student { Id = 4, Name = "Mamy" },
                new Student { Id = 5, Name = "Lalaina" },
                new Student { Id = 6, Name = "Fanja" });
        });

        modelBuilder.Entity<Subject>(subject =>
        {
            subject.Property(s => s.Name).HasMaxLength(100);
            subject.HasData(
                new Subject { Id = 1, Name = "Mathématiques", Coefficient = 3 },
                new Subject { Id = 2, Name = "Informatique", Coefficient = 3 },
                new Subject { Id = 3, Name = "Anglais", Coefficient = 2 },
                new Subject { Id = 4, Name = "Gestion", Coefficient = 1 });
        });

        modelBuilder.Entity<Grade>(grade =>
        {
            grade.Property(g => g.Value).HasPrecision(4, 2);

            // At most one grade per student and per subject, guaranteed by the database itself.
            grade.HasIndex(g => new { g.StudentId, g.SubjectId }).IsUnique();

            // Reference data is never deleted while grades point to it.
            grade.HasOne(g => g.Student).WithMany().HasForeignKey(g => g.StudentId).OnDelete(DeleteBehavior.Restrict);
            grade.HasOne(g => g.Subject).WithMany().HasForeignKey(g => g.SubjectId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
