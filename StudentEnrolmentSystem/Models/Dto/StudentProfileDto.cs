using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class StudentProfileDto
{
    [StringLength(11)]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "Invalid contact number")]
    public required string StudContactNum { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string StudEmail { get; set; }
    
    public required string StudAddress { get; set; }
    public bool StudIsFirstGen { get; set; }
}