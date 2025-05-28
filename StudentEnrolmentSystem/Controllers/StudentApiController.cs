using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class StudentApiController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<StudentApiController> _logger;
    
    public StudentApiController(IConfiguration config, ILogger<StudentApiController> logger)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        _logger = logger;
    }
    
    

}