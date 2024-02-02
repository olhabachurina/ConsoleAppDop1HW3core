
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Net.Mail;
namespace ConsoleAppHW2Core;
class Program
{
    static void Main()
    {
        using (var db = new ApplicationContext())
        {
            try
            {
                var newStudent = new Student
                {
                    FirstName = "Maria",
                    LastName = "Nekrasova",
                    Email = "nekrasova@ukr.net",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Group = Guid.NewGuid().ToString()
                };

                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(newStudent);
                if (!Validator.TryValidateObject(newStudent, validationContext, validationResults, true))
                {
                    foreach (var validationResult in validationResults)
                    {
                        Console.WriteLine(validationResult.ErrorMessage);
                    }
                }
                else
                {


                    Console.WriteLine("Студент успешно добавлен.");
                }
                db.Students.Add(newStudent);
                db.SaveChanges();

                var students = db.Students.ToList();
                foreach (var student in students)
                {
                    Console.WriteLine($"ID: {student.Id}, Имя: {student.FirstName}, Фамилия: {student.LastName}, Email: {student.Email}, Дата рождения: {student.DateOfBirth}, Год поступления: {student.YearOfAdmission}, Группа: {student.Group}");
                }
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Ошибка обновления базы данных: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
            
    }
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [MinLength(5)]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^(?!К).*")]
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; } = new DateTime(1900, 1, 1);

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Display(Name = "Year of Admission")]
        public int YearOfAdmission { get; set; }

        [Required]
        [RegularExpression(@"^[A-Fa-f0-9]{32}$")]
        public string Group { get; set; }
    }

    public class ApplicationContext : DbContext
    {

        public DbSet<Student> Students { get; set; }
        private static string MailAddressToString(MailAddress mailAddress)
        {
            return mailAddress?.ToString();
        }

        private static Guid StringToGuid(string value)
        {
            return Guid.Parse(value);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>()
                .HasKey(s => s.Id); // Устанавливаем Id как первичный ключ

            modelBuilder.Entity<Student>()
                .Property(s => s.FirstName)
                .IsRequired()
                .HasMaxLength(5); // Имя обязательное, минимальная длина 5 символов

            modelBuilder.Entity<Student>().HasCheckConstraint("CK_LastName", "LastName NOT LIKE 'К%'"); // Фамилия обязательная, не должна начинаться с буквы 'К'
            modelBuilder.Entity<Student>().Property(s => s.Email).HasMaxLength(100).HasConversion(v => new MailAddress(v).Address, v => v);

            modelBuilder.Entity<Student>()
                .Property(s => s.DateOfBirth)
                .HasDefaultValue(new DateTime(1900, 1, 1)); // Дата рождения - значение по умолчанию 1900-01-01

            modelBuilder.Entity<Student>()
                .Property(s => s.YearOfAdmission)
                .HasDefaultValueSql("YEAR(GETDATE())"); // Год поступления - автоматический генерируемый при добавлении записи

            modelBuilder.Entity<Student>().Property(s => s.Group).IsRequired().HasMaxLength(50).HasConversion(v => Guid.Parse(v), v => v.ToString());
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-4PCU5RA\\SQLEXPRESS;Database=Education;Trusted_Connection=True;TrustServerCertificate=True;");

            }
        }
    }
    public void AddStudent(Student student)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(student);
        if (!Validator.TryValidateObject(student, validationContext, validationResults, true))
        {
            foreach (var validationResult in validationResults)
            {
                Console.WriteLine(validationResult.ErrorMessage);
            }
            return;
        }

        using (var db = new ApplicationContext())
        {
            db.Students.Add(student);
            db.SaveChanges();
        }
    }
}



