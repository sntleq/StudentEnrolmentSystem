using Microsoft.AspNetCore.Mvc;
namespace StudentEnrolmentSystem.Controllers;

public class CurriculumController(
    CurriculumApiController curriculumApi 
) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Curricula", "Admin");
    }
    
    public IActionResult Update(int curId)
    {
        var curriculum = curriculumApi.GetCurricula().Result.FirstOrDefault(x => x.CurId == curId);
        return View("~/Views/Curriculum/EditCurriculum.cshtml", curriculum);
    }
}