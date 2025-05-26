using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class AdminLoginDto
{
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}