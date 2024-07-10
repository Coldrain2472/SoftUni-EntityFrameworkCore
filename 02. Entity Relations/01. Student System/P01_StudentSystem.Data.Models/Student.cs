namespace P01_StudentSystem.Data.Models;

using System.ComponentModel.DataAnnotations;

public class Student
{
    public Student()
    {
        this.StudentsCourses = new HashSet<StudentCourse>();
        this.Homeworks = new HashSet<Homework>();
    }

    [Key]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public DateTime RegisteredOn { get; set; }

    public DateTime? Birthday { get; set; }

    public ICollection<StudentCourse> StudentsCourses { get; set; }

    public ICollection<Homework> Homeworks { get; set; }
}
