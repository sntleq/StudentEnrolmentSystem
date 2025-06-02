using Microsoft.AspNetCore.Mvc;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class CourseController(
    CourseApiController courseApi, StudentApiController studentApi
    ) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Courses", "Admin");
    }

    public IActionResult Add()
    {
        ViewBag.Categories = courseApi.GetCategories().Result;
        ViewBag.YearLevels = studentApi.GetYearLevels().Result;
        ViewBag.Courses = courseApi.GetCourses().Result;
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
            CrsPreqIds = courseApi.GetPrerequisites(course.CrsId).Result
        };
        
        ViewBag.Categories = courseApi.GetCategories().Result;
        ViewBag.YearLevels = studentApi.GetYearLevels().Result;
        ViewBag.Courses = courseApi.GetCourses().Result;
        return View("~/Views/Course/EditCourse.cshtml", dto);
    }
    
    public IActionResult Delete(int crsId)
    {
        var dto = new DeleteDto { Id = crsId };
        return View("~/Views/Course/DeleteCourse.cshtml", dto);
    }
}