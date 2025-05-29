using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models;

public class Program
{
    public int ProgId { get; set; }
    
    [Required(ErrorMessage = "Program title is required.")]
    public required string ProgTitle { get; set; }
    
    [Required(ErrorMessage = "Program code is required.")]
    public required string ProgCode { get; set; }
}