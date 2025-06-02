using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class FacultyCreateDto
{
    [Required(ErrorMessage = "Account type is required")]
    public int Type { get; set; }
    
    [Required(ErrorMessage = "First name is required")]
    public required string FirstName { get; set; }
    
    [Required(ErrorMessage = "Last name is required")]
    public required string LastName { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}