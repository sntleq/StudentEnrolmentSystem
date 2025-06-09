using Microsoft.AspNetCore.Mvc;
namespace StudentEnrolmentSystem.Controllers;

public class AdminController(
    CourseApiController courseApi, ProgramApiController programApi, CurriculumApiController curriculumApi,
    FacultyApiController facultyApi, TimeMachineController timeMachineApi, StudentApiController studentApi,
    RoomApiController roomApi, ScheduleApiController scheduleApi
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
        ViewBag.Dependencies = courseApi.GetDependencies().Result;
        ViewBag.YearLevels = studentApi.GetYearLevels().Result;
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
        if (HttpContext.Session.GetInt32("AyId") == null || HttpContext.Session.GetInt32("SemId") == null)
            return RedirectToAction("Index", "TimeMachine");
        
        ViewBag.Curricula = curriculumApi.GetCurricula().Result;
        ViewBag.CurriculumCourses = curriculumApi.GetCurriculumCourses().Result;
        ViewBag.Courses = courseApi.GetCourses().Result;
        ViewBag.Categories = courseApi.GetCategories().Result;
        ViewBag.Programs = programApi.GetPrograms().Result;
        ViewBag.AcademicYears = timeMachineApi.GetAcademicYears().Result;
        ViewBag.Dependencies = courseApi.GetDependencies().Result;
        ViewBag.YearLevels = studentApi.GetYearLevels().Result;
        ViewBag.AyId = HttpContext.Session.GetInt32("AyId");
        return View("~/Views/Admin/Curricula.cshtml");
    }

    public IActionResult Faculty()
    {
        ViewBag.ProgramHeads = facultyApi.GetProgramHeads().Result;
        ViewBag.Teachers = facultyApi.GetTeachers().Result;
        ViewBag.Programs = programApi.GetPrograms().Result;
        return View("~/Views/Admin/Faculty.cshtml");
    }
    
    public IActionResult Rooms()
    {
        ViewBag.Rooms = roomApi.GetRooms().Result;
        ViewBag.Programs = programApi.GetPrograms().Result;
        return View("~/Views/Admin/Rooms.cshtml");
    }

    public IActionResult Schedules()
    {
        ViewBag.Courses = courseApi.GetCourses().Result;
        ViewBag.Categories = courseApi.GetCategories().Result;
        ViewBag.Schedules = scheduleApi.GetSchedules().Result;
        ViewBag.Sessions = scheduleApi.GetSessions().Result;
        ViewBag.Teachers = facultyApi.GetTeachers().Result;
        ViewBag.Rooms = roomApi.GetRooms().Result;
        return View("~/Views/Admin/Schedules.cshtml");
    }
}