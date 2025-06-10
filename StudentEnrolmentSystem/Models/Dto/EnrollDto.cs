using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class EnrollDto
{
    [Required(ErrorMessage = "Program is required")]
    public int ProgId { get; set; }
    public int AyId { get; set; }
    public int SemId { get; set; }
    public int StudId { get; set; }
    
    [Required(ErrorMessage = "Enrolment status is required")]
    public required string StudStatus { get; set; }
}