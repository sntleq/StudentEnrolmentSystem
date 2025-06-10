using Microsoft.AspNetCore.Mvc;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class EnrolmentController(
    CourseApiController courseApi, ProgramApiController programApi, CurriculumApiController curriculumApi,
    FacultyApiController facultyApi, TimeMachineController timeMachineApi, StudentApiController studentApi,
    RoomApiController roomApi, ScheduleApiController scheduleApi, EnrolmentApiController enrolmentApi
) : Controller
{

    public IActionResult Index()
    {
        ViewBag.StudId = HttpContext.Session.GetInt32("StudId");
        ViewBag.Programs = programApi.GetPrograms().Result;
        ViewBag.Year = timeMachineApi.GetAcademicYears().Result.FirstOrDefault(ay => ay.AyId == HttpContext.Session.GetInt32("AyId"));
        ViewBag.Semester = timeMachineApi.GetSemesters().Result.FirstOrDefault(s => s.SemId == HttpContext.Session.GetInt32("SemId"));
        
        return View("~/Views/Enrolment/Enroll.cshtml");
    }

    public IActionResult ChooseCourses(int curId, int studId)
    {
        ViewBag.CurId = curId;
        ViewBag.StudId = studId;
        
        var crsIdChoices = curriculumApi.GetCurriculumCourses().Result.
            Where(cc => cc.CurId == curId).Select(cc => cc.CrsId).ToList();
        var completedEnrls = enrolmentApi.GetEnrolments().Result.Where(e => e.StudId == studId && e.EnrlIsCompleted).Select(e => e.SchedId).ToList();
        var completedCrsIds = scheduleApi.GetSchedules().Result.Where(s => completedEnrls.Contains(s.SchedId)).Select(s => s.CrsId).ToList();
        
        var courses = courseApi.GetCourses().Result;
        var dependencies = courseApi.GetDependencies().Result;
        
        var completedUnits = courses
            .Where(c => completedCrsIds.Contains(c.CrsId))
            .Sum(c => c.CrsUnits);

        var curriculumUnits = courses
            .Where(c => crsIdChoices.Contains(c.CrsId))
            .Sum(c => c.CrsUnits);
        
        var unitRatio = curriculumUnits > 0
            ? (double)completedUnits / curriculumUnits
            : 0.0;
        
        ViewBag.Courses = courses
            .Where(course =>
                crsIdChoices.Contains(course.CrsId) && // in the curriculum
                !completedCrsIds.Contains(course.CrsId) && // not yet completed
                (
                    dependencies.All(dep => dep.CrsId != course.CrsId) || // no prereqs
                    dependencies
                     .Where(dep => dep.CrsId == course.CrsId)
                     .All(dep => completedCrsIds.Contains(dep.CrsPreqId)) // all prereqs completed
                )
                && (
                    course.LvlId is not > 1
                    || (course.LvlId == 2 && unitRatio >= 0.25)
                    || (course.LvlId == 3 && unitRatio >= 0.50)
                    || (course.LvlId == 4 && unitRatio >= 0.75)
                )
            ).ToList();
        ViewBag.Categories = courseApi.GetCategories().Result;
        
        return View("~/Views/Enrolment/ChooseCourses.cshtml");
    }

    public IActionResult ChooseSchedules(int curId, int studId, List<int> crsIds)
    {
        ViewBag.CurId = curId;
        ViewBag.StudId = studId;
        ViewBag.CrsCount = crsIds.Count;
        
        ViewBag.Courses = courseApi.GetCourses().Result.Where(c => crsIds.Contains(c.CrsId)).ToList();
        ViewBag.Schedules = scheduleApi.GetSchedules().Result.Where(s => crsIds.Contains(s.CrsId)).ToList();
        var schedIds = scheduleApi.GetSchedules().Result.Where(s => crsIds.Contains(s.CrsId)).Select(s => s.SchedId).ToList();
        ViewBag.Sessions = scheduleApi.GetSessions().Result.Where(s => schedIds.Contains(s.SchedId)).ToList();
        ViewBag.TimeSlots = scheduleApi.GetTimeSlots().Result;
        return View("~/Views/Enrolment/ChooseSchedules.cshtml");
    }
    
    public IActionResult Confirm(int curId, int studId, List<int> schedIds)
    {
        ViewBag.CurId = curId;
        ViewBag.StudId = studId;
        ViewBag.SchedIds = schedIds;
        ViewBag.SemId = HttpContext.Session.GetInt32("SemId");
        return View("~/Views/Enrolment/ConfirmEnrolment.cshtml");
    }
}