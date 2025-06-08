using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class CurriculumUpdateDto
{
    public int CurId { get; set; }

    public List<int> CrsIds { get; set; } = [];

    [Required(ErrorMessage = "GEE requirement is required")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Invalid number of units.")]
    public int? CurGeeUnits { get; set; }
    
    [Required(ErrorMessage = "PELEC requirement is required")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Invalid number of units.")]
    public int? CurPelecUnits { get; set; }
}