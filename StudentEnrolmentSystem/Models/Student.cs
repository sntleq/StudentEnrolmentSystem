using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models;

public class Student
{
    public int StudId { get; set; }
    
    [Required(ErrorMessage = "Student ID is required")]
    [StringLength(7)]
    [RegularExpression(@"^\d+$", ErrorMessage = "Invalid student ID")]
    public required string StudCode { get; set; }
    
    [Required(ErrorMessage = "First name is required")]
    public required string StudFirstName { get; set; }
    
    [Required(ErrorMessage = "Last name is required")]
    public required string StudLastName { get; set; }
    
    [StringLength(11)]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "Invalid contact number")]
    public string? StudContactNum { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string StudEmail { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    public required string StudPassword { get; set; }
    
    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime StudDob { get; set; }
    
    public string? StudAddress { get; set; }
    public bool? StudIsFirstGen { get; set; }
    public int? ProgId { get; set; }
    public string? StudStatus { get; set; }
}