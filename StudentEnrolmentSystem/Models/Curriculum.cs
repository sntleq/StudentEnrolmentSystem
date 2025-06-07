namespace StudentEnrolmentSystem.Models;

public class Curriculum
{
    public int CurId { get; set; }
    public int ProgId { get; set; }
    public int AyId { get; set; }
    
    public int? CurGeeUnits { get; set; }
    
    public int? CurPelecUnits { get; set; }
    
    public required string CurStatus { get; set; }
    
    public string? CurRejectReason { get; set; }
}