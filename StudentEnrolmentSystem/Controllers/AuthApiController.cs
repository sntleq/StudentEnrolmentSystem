using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

[ApiController]
public class AuthApiController(IConfiguration config, ILogger<AuthApiController> logger) : ControllerBase
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

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
            
            HttpContext.Session.SetInt32("StudId", studentId);
            HttpContext.Session.SetString("StudFirstName", firstName);
            
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
    
    [HttpPost("Admin/Login", Name = "Admin.Login")]
    public async Task<IActionResult> AdminLogin([FromForm] AdminLoginDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT adm_id, adm_password
                FROM admin";

            await using var reader = await cmd.ExecuteReaderAsync();
            var hasher = new PasswordHasher<AdminLoginDto>();
            int? adminMatchId = null;
            
            while (await reader.ReadAsync())
            {
                var id    = reader.GetInt32(reader.GetOrdinal("adm_id"));
                var hash  = reader.GetString(reader.GetOrdinal("adm_password")).Trim();

                if (hasher.VerifyHashedPassword(form, hash, form.AdmPassword.Trim()) != PasswordVerificationResult.Failed)
                {
                    adminMatchId = id;
                    break;
                }
            }
            
            if (adminMatchId == null)
                return Unauthorized(new { success = false, message = "Invalid credentials." });
            
            HttpContext.Session.SetInt32("AdmId", adminMatchId.Value);
            
            return Ok(new
            {
                success = true,
                data = new { Id = adminMatchId.Value }
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
    
    [HttpPost("ProgramHead/Login", Name = "ProgramHead.Login")]
    public async Task<IActionResult> ProgramHeadLogin([FromForm] FacultyLoginDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT *
                FROM program_head
                WHERE head_email = @email";
            cmd.Parameters.AddWithValue("email", form.Email.Trim());

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows)
                return Unauthorized(new { success = false, message = "Invalid credentials." });

            await reader.ReadAsync();
            var hashed = reader.GetString(reader.GetOrdinal("head_password")).Trim();
            
            var hasher = new PasswordHasher<FacultyLoginDto>();
            if (hasher.VerifyHashedPassword(form, hashed, form.Password.Trim()) == PasswordVerificationResult.Failed)
                return Unauthorized(new { success = false, message = "Wrong password. Try again." });

            if (reader.GetBoolean(reader.GetOrdinal("head_is_active")) == false)
                return Unauthorized(new { success = false, message = "Your account is inactive. Contact the administrator." });
            
            var headId = reader.GetInt32(reader.GetOrdinal("head_id"));
            var progId = reader["prog_id"] as int?;
            
            HttpContext.Session.SetInt32("HeadId", headId);
            if (progId != null)
                HttpContext.Session.SetInt32("ProgId", (int) progId);
            
            return Ok(new
            {
                success = true,
                data = new { Id = headId, ProgId = progId }
            });
        }
        catch (NpgsqlException ex)
        {
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { success = false, message = "Unexpected error" });
        }
    }
    
    [HttpPost("Teacher/Login", Name = "Teacher.Login")]
    public async Task<IActionResult> TeacherLogin([FromForm] FacultyLoginDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT *
                FROM teacher
                WHERE tchr_email = @email";
            cmd.Parameters.AddWithValue("email", form.Email.Trim());

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows)
                return Unauthorized(new { success = false, message = "Invalid credentials." });

            await reader.ReadAsync();
            var hashed = reader.GetString(reader.GetOrdinal("tchr_password")).Trim();
            
            var hasher = new PasswordHasher<FacultyLoginDto>();
            if (hasher.VerifyHashedPassword(form, hashed, form.Password.Trim()) == PasswordVerificationResult.Failed)
                return Unauthorized(new { success = false, message = "Wrong password. Try again." });

            if (reader.GetBoolean(reader.GetOrdinal("tchr_is_active")) == false)
                return Unauthorized(new { success = false, message = "Your account is inactive. Contact the administrator." });
            
            var tchrId = reader.GetInt32(reader.GetOrdinal("tchr_id"));
            
            HttpContext.Session.SetInt32("TchrId", tchrId);
            
            return Ok(new
            {
                success = true,
                data = new { Id = tchrId }
            });
        }
        catch (NpgsqlException ex)
        {
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
            
            await using var tx = await conn.BeginTransactionAsync();

            await using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.Transaction = tx;
                checkCmd.CommandText = @"
                    SELECT 1 FROM student 
                    WHERE stud_code = @code OR stud_email = @email";
                checkCmd.Parameters.AddWithValue("code", form.StudCode.Trim());
                checkCmd.Parameters.AddWithValue("email", form.StudEmail.Trim());

                var exists = await checkCmd.ExecuteScalarAsync();
                if (exists is not null)
                    return Conflict(new { success = false, message = "An account with this ID number or email already exists." });
            }

            var hasher = new PasswordHasher<SignUpDto>();
            var hashedPassword = hasher.HashPassword(form, form.StudPassword.Trim());

            await using var insertCmd = conn.CreateCommand();
            insertCmd.Transaction = tx;
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

            await tx.CommitAsync();
            
            return Ok(new
            {
                success = true,
                data = new { Id = newId, FirstName = form.StudFirstName.Trim() }
            });
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }
}