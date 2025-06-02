using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models;

public class Course
{
    public int CrsId { get; set; }
    
    [Required(ErrorMessage = "Course code is required.")]
    public required string CrsCode { get; set; }
    
    [Required(ErrorMessage = "Course title is required.")]
    public required string CrsTitle { get; set; }
    
    [Required(ErrorMessage = "Number of units is required.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Invalid number of units.")]
    public int CrsUnits { get; set; }
    
    [Required(ErrorMessage = "This is required.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Invalid number of hours.")]
    public int CrsHrsLec { get; set; }
    
    [RegularExpression(@"^\d+$", ErrorMessage = "Invalid number of hours.")]
    [Required(ErrorMessage = "This is required.")]
    public int CrsHrsLab { get; set; }
    
    [Required(ErrorMessage = "Course category is required.")]
    public int CatgId { get; set; }
    
    public int? LvlId { get; set; }
}