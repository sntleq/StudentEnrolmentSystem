using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class RejectDto
{
    [Required(ErrorMessage = "ID is required")]
    public required int Id { get; set; }
    
    [Required(ErrorMessage = "Reason for rejecting is required")]
    public string? Reason { get; set; }
}