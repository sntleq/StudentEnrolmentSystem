using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class FacultyController(
    FacultyApiController facultyApi
) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Faculty", "Admin");
    }

    public IActionResult Add()
    {
        return View("~/Views/Faculty/AddFaculty.cshtml");
    }
    
    
    public IActionResult Update(int id, int typeId)
    {
        var head = null as ProgramHead;
        var teacher = null as Teacher;
        
        if (typeId == 1)
        {
            head = facultyApi.GetProgramHeads().Result.FirstOrDefault(x => x.HeadId == id);
        }
        else
        {
            teacher = facultyApi.GetTeachers().Result.FirstOrDefault(x => x.TchrId == id);
        }
        
        var dto = new FacultyUpdateDto
        {
            OldType = typeId,
            Id = id,
            
            Type = typeId,
            FirstName = typeId == 1 ? head!.HeadFirstName : teacher!.TchrFirstName,
            LastName = typeId == 1 ? head!.HeadLastName : teacher!.TchrLastName,
            Email = typeId == 1 ? head!.HeadEmail : teacher!.TchrEmail
        };
        
        ViewData["TypeSelectList"] = new SelectList(
            new[]
            {
                new { Value = "1", Text = "Program Head" },
                new { Value = "2", Text = "Teacher" }
            },
            "Value",   // the property to use as <option value="…">
            "Text",    // the property to use as <option>…</option>
            typeId // the currently selected value (1 or 2)
        );
        
        return View("~/Views/Faculty/EditFaculty.cshtml", dto);
    }
    
    public IActionResult Delete(int id, int typeId)
    {
        var dto = new DeleteDto { Id = id, TypeId = typeId };
        return View("~/Views/Faculty/DeleteFaculty.cshtml", dto);
    }
}