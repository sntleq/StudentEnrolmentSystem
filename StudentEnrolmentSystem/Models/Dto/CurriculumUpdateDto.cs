using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class CurriculumUpdateDto
{
    [Required(ErrorMessage = "Curriculum ID is required")]
    public int CurId { get; set; }

    [Required(ErrorMessage = "Program ID is required")]
    public int ProgId { get; set; }

    [Required(ErrorMessage = "Academic Year ID is required")]
    public int AyId { get; set; }
}