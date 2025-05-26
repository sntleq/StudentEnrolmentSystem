using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class SignUpDto
{
    [Required(ErrorMessage = "Student ID is required")]
    [StringLength(7)]
    [RegularExpression(@"^\d{7}$", ErrorMessage = "Invalid student ID")]
    public required string StudCode { get; set; }
    
    [Required(ErrorMessage = "First name is required")]
    public required string StudFirstName { get; set; }
    
    [Required(ErrorMessage = "Last name is required")]
    public required string StudLastName { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string StudEmail { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public required string StudPassword { get; set; }
    
    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime StudDob { get; set; }
}