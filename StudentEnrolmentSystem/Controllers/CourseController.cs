using Microsoft.AspNetCore.Mvc;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class CourseController(
    CourseApiController courseApi, StudentApiController studentApi, ProgramApiController programApi
    ) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Courses", "Admin");
    }

    public IActionResult Add(int? progId)
    {
        ViewBag.Categories = courseApi.GetCategories().Result;
        ViewBag.YearLevels = studentApi.GetYearLevels().Result;
        ViewBag.Courses = courseApi.GetCourses().Result;
        ViewBag.Programs = programApi.GetPrograms().Result;
        ViewBag.ProgId = progId;
        return View("~/Views/Course/AddCourse.cshtml");
    }
    
    public IActionResult Update(int crsId)
    {
        var course = courseApi.GetCourses().Result.FirstOrDefault(x => x.CrsId == crsId);
        var dto = new CourseCreateDto
        {
            CrsId     = course!.CrsId,
            CrsCode   = course.CrsCode,
            CrsTitle  = course.CrsTitle,
            CrsUnits  = course.CrsUnits,
            CrsHrsLec = course.CrsHrsLec,
            CrsHrsLab = course.CrsHrsLab,
            CatgId    = course.CatgId,
            LvlId     = course.LvlId,
            CrsPreqIds = courseApi.GetPrerequisites(course.CrsId).Result,
            ProgId = course.ProgId,
        };
        
        ViewBag.Categories = courseApi.GetCategories().Result;
        ViewBag.YearLevels = studentApi.GetYearLevels().Result;
        ViewBag.Courses = courseApi.GetCourses().Result;
        return View("~/Views/Course/EditCourse.cshtml", dto);
    }
    
    public IActionResult Delete(int crsId)
    {
        var dto = new IdDto { Id = crsId };
        return View("~/Views/Course/DeleteCourse.cshtml", dto);
    }
}