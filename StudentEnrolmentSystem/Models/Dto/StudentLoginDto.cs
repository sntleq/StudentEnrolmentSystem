using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class StudentLoginDto
{
    [Required(ErrorMessage = "Student ID is required")]
    [StringLength(7)]
    [RegularExpression(@"^\d{7}$", ErrorMessage = "Invalid student ID")]
    public required string StudCode { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public required string StudPassword { get; set; }
}