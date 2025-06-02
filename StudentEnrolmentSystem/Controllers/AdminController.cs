using Microsoft.AspNetCore.Mvc;
namespace StudentEnrolmentSystem.Controllers;

public class AdminController(
    CourseApiController courseApi, ProgramApiController programApi, CurriculumApiController curriculumApi,
    FacultyApiController facultyApi, TimeMachineController timeMachineApi
    ) : Controller
{

    public IActionResult Index()
    {
        return RedirectToAction("Courses");
    }

    public IActionResult Courses()
    {
        ViewBag.Courses = courseApi.GetCourses().Result;
        ViewBag.Categories = courseApi.GetCategories().Result;
        return View("~/Views/Admin/Courses.cshtml");
    }

    public IActionResult Programs()
    {
        ViewBag.Programs = programApi.GetPrograms().Result;
        ViewBag.ProgramHeads = programApi.GetProgramHeads().Result;
        return View("~/Views/Admin/Programs.cshtml");
    }

    public IActionResult Curricula()
    {
        ViewBag.Curricula = curriculumApi.GetCurricula().Result;
        ViewBag.CurriculumCourses = curriculumApi.GetCurriculumCourses().Result;
        ViewBag.Courses = courseApi.GetCourses().Result;
        ViewBag.Categories = courseApi.GetCategories().Result;
        ViewBag.Programs = programApi.GetPrograms().Result;
        ViewBag.AcademicYears = timeMachineApi.GetAcademicYears().Result;
        return View("~/Views/Admin/Curricula.cshtml");
    }

    public IActionResult Faculty()
    {
        ViewBag.ProgramHeads = facultyApi.GetProgramHeads().Result;
        ViewBag.Teachers = facultyApi.GetTeachers().Result;
        ViewBag.Programs = programApi.GetPrograms().Result;
        return View("~/Views/Admin/Faculty.cshtml");
    }
}