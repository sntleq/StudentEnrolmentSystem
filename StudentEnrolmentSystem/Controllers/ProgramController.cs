using Microsoft.AspNetCore.Mvc;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class ProgramController(
    ProgramApiController programApi 
) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Programs", "Admin");
    }

    public IActionResult Add()
    {
        return View("~/Views/Program/AddProgram.cshtml");
    }
    
    
    public IActionResult Update(int progId)
    {
        var program = programApi.GetPrograms().Result.FirstOrDefault(x => x.ProgId == progId);
        return View("~/Views/Program/EditProgram.cshtml", program);
    }
    
    public IActionResult AssignHead(int progId)
    {
        ViewBag.Program = programApi.GetPrograms().Result.FirstOrDefault(x => x.ProgId == progId);
        ViewBag.ProgramHeads = programApi.GetProgramHeads().Result;
        return View("~/Views/Program/AssignProgramHead.cshtml");
    }
    
    public IActionResult AddHead(int progId)
    {
        ViewBag.Program = programApi.GetPrograms().Result.FirstOrDefault(x => x.ProgId == progId);
        ViewBag.ProgramHeads = programApi.GetProgramHeads().Result;
        return View("~/Views/Program/AddProgramHead.cshtml");
    }
    
    public IActionResult Delete(int progId)
    {
        var dto = new DeleteDto { Id = progId };
        return View("~/Views/Program/DeleteProgram.cshtml", dto);
    }
}