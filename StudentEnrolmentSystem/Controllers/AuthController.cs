using Microsoft.AspNetCore.Mvc;
namespace StudentEnrolmentSystem.Controllers;

public class AuthController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Student");
    }

    public IActionResult Student()
    {
        return View("~/Views/Auth/StudentLogin.cshtml");
    }

    public IActionResult Teacher()
    {
        return View("~/Views/Auth/TeacherLogin.cshtml");
    }
    
    public IActionResult ProgramHead()
    {
        return View("~/Views/Auth/ProgramHeadLogin.cshtml");
    }
    
    public IActionResult Admin()
    {
        return View("~/Views/Auth/AdminLogin.cshtml");
    }
    
    public IActionResult SignUp()
    {
        return View("~/Views/Auth/SignUp.cshtml");
    }

    public IActionResult ForgotPassword()
    {
        return View("~/Views/Auth/ForgotPassword.cshtml");
    }

    public IActionResult ResetPasswordViaCode()
    {
        return View("~/Views/Auth/ResetPasswordViaCode.cshtml");
    }

    public IActionResult ResetPasswordViaOld()
    {
        return View("~/Views/Auth/ResetPasswordViaOld.cshtml");
    }
    
    public IActionResult Logout()
    {
        HttpContext.Session.SetInt32("StudId", 0);
        HttpContext.Session.SetString("StudFirstName", string.Empty);
        HttpContext.Session.SetInt32("AdmId", 0);
        return RedirectToAction("Index");   
    }
}