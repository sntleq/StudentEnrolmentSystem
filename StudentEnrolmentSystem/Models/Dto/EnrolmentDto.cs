using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class EnrolmentDto
{
    public required string StudYrLvl { get; set; }
    
    [Required(ErrorMessage = "Program is required")]
    public int ProgId { get; set; }
    
    [Required(ErrorMessage = "Enrolment status is required")]
    public required string StudStatus { get; set; }
}