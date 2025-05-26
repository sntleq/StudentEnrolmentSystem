using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

[ApiController]
public class AuthApiController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<AuthApiController> _logger;
    
    public AuthApiController(IConfiguration config, ILogger<AuthApiController> logger)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        _logger = logger;
    }
    
    [HttpPost("Student/Login", Name = "Student.Login")]
    public async Task<IActionResult> StudentLogin([FromForm] StudentLoginDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT stud_id, stud_first_name, stud_password
                FROM student
                WHERE stud_code = @code";
            cmd.Parameters.AddWithValue("code", form.StudCode.Trim());

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows)
                return Unauthorized(new { success = false, message = "Invalid credentials." });

            await reader.ReadAsync();
            var hashed = reader.GetString(reader.GetOrdinal("stud_password")).Trim();
            
            var hasher = new PasswordHasher<StudentLoginDto>();
            if (hasher.VerifyHashedPassword(form, hashed, form.StudPassword.Trim()) == PasswordVerificationResult.Failed)
                return Unauthorized(new { success = false, message = "Wrong password. Try again." });

            var studentId = reader.GetInt32(reader.GetOrdinal("stud_id"));
            var firstName = reader.GetString(reader.GetOrdinal("stud_first_name"));

            return Ok(new
            {
                success = true,
                data = new { Id = studentId, FirstName = firstName }
            });
        }
        catch (NpgsqlException ex)
        {
            // log if you have a logger, then:
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { success = false, message = "Unexpected error" });
        }
    }
    
    [HttpPost("SignUp", Name = "SignUp")]
    public async Task<IActionResult> SignUp([FromForm] SignUpDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Check for existing student by stud_code or stud_email
            await using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = @"
                    SELECT 1 FROM student 
                    WHERE stud_code = @code OR stud_email = @email";
                checkCmd.Parameters.AddWithValue("code", form.StudCode.Trim());
                checkCmd.Parameters.AddWithValue("email", form.StudEmail.Trim());

                var exists = await checkCmd.ExecuteScalarAsync();
                if (exists is not null)
                    return Conflict(new { success = false, message = "An account with this ID number or email already exists." });
            }

            // Hash the password
            var hasher = new PasswordHasher<SignUpDto>();
            var hashedPassword = hasher.HashPassword(form, form.StudPassword.Trim());

            // Insert new student
            await using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO student (
                    stud_code, stud_first_name, stud_last_name,
                    stud_email, stud_password, stud_dob
                ) VALUES (
                    @code, @firstName, @lastName,
                    @email, @password, @dob
                ) RETURNING stud_id";

            insertCmd.Parameters.AddWithValue("code", form.StudCode.Trim());
            insertCmd.Parameters.AddWithValue("firstName", form.StudFirstName.Trim());
            insertCmd.Parameters.AddWithValue("lastName", form.StudLastName.Trim());
            insertCmd.Parameters.AddWithValue("email", form.StudEmail.Trim());
            insertCmd.Parameters.AddWithValue("password", hashedPassword);
            insertCmd.Parameters.AddWithValue("dob", form.StudDob);

            var newId = (int)(await insertCmd.ExecuteScalarAsync())!;

            return Ok(new
            {
                success = true,
                data = new { Id = newId, FirstName = form.StudFirstName.Trim() }
            });
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-up.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }

}