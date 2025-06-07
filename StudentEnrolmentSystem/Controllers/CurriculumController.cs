using Microsoft.AspNetCore.Mvc;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class CurriculumController(
    CurriculumApiController curriculumApi, CourseApiController courseApi,
    ProgramApiController programApi, TimeMachineController timeMachineApi
) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Curricula", "Admin");
    }
    
    public IActionResult Update(int curId)
    {
        var curriculum = curriculumApi.GetCurricula().Result.FirstOrDefault(x => x.CurId == curId);
        var courses = courseApi.GetCourses().Result;
        var dto = new CurriculumUpdateDto
        {
            CurId = curriculum!.CurId,
            CurGeeUnits = curriculum!.CurGeeUnits,
            CurPelecUnits = curriculum.CurPelecUnits,
            CrsIds = courseApi.GetCourses().Result.Where(c => c.ProgId == null || c.ProgId == curriculum.ProgId)
                .Select(c => c.CrsId).ToList()
        };
        
        ViewBag.Programs = programApi.GetPrograms().Result;
        ViewBag.AcademicYears = timeMachineApi.GetAcademicYears().Result;
        ViewBag.Courses = courses;
        ViewBag.Categories = courseApi.GetCategories().Result;
        ViewBag.Curriculum = curriculum;
        return View("~/Views/Curriculum/EditCurriculum.cshtml", dto);
    }

    public IActionResult Approve(int curId)
    {
        var dto = new IdDto
        {
            Id = curId
        };
        return View("~/Views/Curriculum/ApproveCurriculum.cshtml", dto);
    }
    
    public IActionResult Reject(int curId)
    {
        var dto = new RejectDto
        {
            Id = curId
        };
        return View("~/Views/Curriculum/RejectCurriculum.cshtml", dto);
    }
}