using Microsoft.AspNetCore.Mvc;
namespace StudentEnrolmentSystem.Controllers;

public class ProgramHeadController(
    CourseApiController courseApi, ProgramApiController programApi, CurriculumApiController curriculumApi,
    FacultyApiController facultyApi, TimeMachineController timeMachineApi, StudentApiController studentApi
) : Controller
{

    public IActionResult Index()
    {
        return RedirectToAction("Courses");
    }

    public IActionResult Courses()
    {
        if (HttpContext.Session.GetInt32("HeadId") == null)
            return RedirectToAction("ProgramHead", "Auth");
        ViewBag.ProgId = facultyApi.GetProgramHeads().Result.FirstOrDefault(x => x.HeadId == HttpContext.Session.GetInt32("HeadId"))?.ProgId;
        
        ViewBag.Courses = courseApi.GetCourses().Result;
        ViewBag.Categories = courseApi.GetCategories().Result;
        ViewBag.Dependencies = courseApi.GetDependencies().Result;
        ViewBag.YearLevels = studentApi.GetYearLevels().Result;
        return View("~/Views/ProgramHead/Courses.cshtml");
    }

    public IActionResult Curricula()
    {
        if (HttpContext.Session.GetInt32("HeadId") == null)
            return RedirectToAction("ProgramHead", "Auth");
        ViewBag.ProgId = facultyApi.GetProgramHeads().Result.FirstOrDefault(x => x.HeadId == HttpContext.Session.GetInt32("HeadId"))?.ProgId;
        
        ViewBag.Curricula = curriculumApi.GetCurricula().Result;
        ViewBag.CurriculumCourses = curriculumApi.GetCurriculumCourses().Result;
        ViewBag.Courses = courseApi.GetCourses().Result;
        ViewBag.Categories = courseApi.GetCategories().Result;
        ViewBag.Programs = programApi.GetPrograms().Result;
        ViewBag.AcademicYears = timeMachineApi.GetAcademicYears().Result;
        ViewBag.Dependencies = courseApi.GetDependencies().Result;
        ViewBag.YearLevels = studentApi.GetYearLevels().Result;
        ViewBag.AyId = HttpContext.Session.GetInt32("AyId");
        return View("~/Views/ProgramHead/Curricula.cshtml");
    }

    public IActionResult Students()
    {
        if (HttpContext.Session.GetInt32("HeadId") == null)
            return RedirectToAction("ProgramHead", "Auth");
        
        return View("~/Views/ProgramHead/Students.cshtml");
    }
}