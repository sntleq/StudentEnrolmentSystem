using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class CurriculumController(
    IConfiguration config, ILogger<CurriculumController> logger,
    CurriculumApiController curriculumApi 
) : Controller
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

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
        var program = curriculumApi.GetPrograms().Result.FirstOrDefault(x => x.ProgId == progId);
        return View("~/Views/Program/EditProgram.cshtml", program);
    }
    
    public IActionResult Delete(int progId)
    {
        var dto = new DeleteDto { Id = progId };
        return View("~/Views/Program/DeleteProgram.cshtml", dto);
    }
}