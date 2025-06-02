using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

[ApiController]
public class FacultyApiController(IConfiguration config, ILogger<FacultyApiController> logger) : ControllerBase
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    [NonAction]
    public async Task<List<ProgramHead>> GetProgramHeads()
    {
        var list = new List<ProgramHead>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM program_head";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var head = new ProgramHead
            {
                HeadId = reader.GetInt32(reader.GetOrdinal("head_id")),
                HeadFirstName = reader.GetString(reader.GetOrdinal("head_first_name")),
                HeadLastName = reader.GetString(reader.GetOrdinal("head_last_name")),
                HeadEmail = reader.GetString(reader.GetOrdinal("head_email")),
                HeadPassword = reader.GetString(reader.GetOrdinal("head_password")),
                HeadIsActive = reader.GetBoolean(reader.GetOrdinal("head_is_active")),
                ProgId = reader["prog_id"] as int?
            };
        
            list.Add(head);
        }
    
        return list;
    }

    [NonAction]
    public async Task<List<Teacher>> GetTeachers()
    {
        var list = new List<Teacher>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM teacher";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var teacher = new Teacher
            {
                TchrId = reader.GetInt32(reader.GetOrdinal("tchr_id")),
                TchrFirstName = reader.GetString(reader.GetOrdinal("tchr_first_name")),
                TchrLastName = reader.GetString(reader.GetOrdinal("tchr_last_name")),
                TchrEmail = reader.GetString(reader.GetOrdinal("tchr_email")),
                TchrPassword = reader.GetString(reader.GetOrdinal("tchr_password")),
                TchrIsActive = reader.GetBoolean(reader.GetOrdinal("tchr_is_active")),
            };
        
            list.Add(teacher);
        }
    
        return list;
    }
    
    [HttpPost("Faculty/Add", Name = "Faculty.Add")]
    public async Task<IActionResult> AddFaculty([FromForm] FacultyCreateDto form)
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
                    SELECT 1 FROM program_head
                    WHERE head_email = @email
                    
                    UNION ALL
                    
                    SELECT 1 FROM teacher
                    WHERE tchr_email = @email";
                checkCmd.Parameters.AddWithValue("email", form.Email.Trim());

                var exists = await checkCmd.ExecuteScalarAsync();
                if (exists is not null)
                    return Conflict(new { success = false, message = "An account with this email already exists." });
            }

            var hasher = new PasswordHasher<FacultyCreateDto>();
            var hashedPassword = hasher.HashPassword(form, form.Password.Trim());

            await using var insertCmd = conn.CreateCommand();
            insertCmd.Transaction = tx;
            
            if (form.Type == 1)
            {
                insertCmd.CommandText = @"
                    INSERT INTO program_head (
                        head_first_name, head_last_name,
                        head_email, head_password, head_is_active
                    ) VALUES (
                        @firstName, @lastName,
                        @email, @password, true
                    ) RETURNING head_id";
            }
            
            else
            {
                insertCmd.CommandText = @"
                    INSERT INTO teacher (
                        tchr_first_name, tchr_last_name,
                        tchr_email, tchr_password, tchr_is_active
                    ) VALUES (
                        @firstName, @lastName,
                        @email, @password, true
                    ) RETURNING tchr_id";
            }
            
            insertCmd.Parameters.AddWithValue("firstName", form.FirstName.Trim());
            insertCmd.Parameters.AddWithValue("lastName", form.LastName.Trim());
            insertCmd.Parameters.AddWithValue("email", form.Email.Trim());
            insertCmd.Parameters.AddWithValue("password", hashedPassword);
            
            var newId = (int)(await insertCmd.ExecuteScalarAsync())!;
            
            await tx.CommitAsync();

            return Ok(new
            {
                success = true,
                data = new { Id = newId }
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
    
    [HttpPost("Faculty/Update", Name = "Faculty.Update")]
    public async Task<IActionResult> UpdateFaculty([FromForm] FacultyUpdateDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var tx = await conn.BeginTransactionAsync();

            int resultId = form.Id;
            
            if (form.OldType == form.Type)
            {
                await using (var checkCmd = conn.CreateCommand())
                {
                    checkCmd.Transaction = tx;

                    if (form.OldType == 1)
                    {
                        checkCmd.CommandText = @"
                            SELECT 1 FROM program_head
                            WHERE head_email = @email
                            AND head_id != @id
                            
                            UNION ALL
                            
                            SELECT 1 FROM teacher
                            WHERE tchr_email = @email";
                    }
                    else
                    {
                        checkCmd.CommandText = @"
                            SELECT 1 FROM program_head
                            WHERE head_email = @email
                            
                            UNION ALL
                            
                            SELECT 1 FROM teacher
                            WHERE tchr_email = @email
                            AND tchr_id != @id";
                    }
                    
                    checkCmd.Parameters.AddWithValue("email", form.Email.Trim());
                    checkCmd.Parameters.AddWithValue("id", form.Id);
                    
                    var exists = await checkCmd.ExecuteScalarAsync();
                    if (exists is not null)
                        return Conflict(new { success = false, message = "An account with this email already exists." });
                }
                
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;

                if (form.OldType == 1)
                {
                    cmd.CommandText = @"
                        UPDATE program_head
                        SET 
                            head_first_name = @firstName,
                            head_last_name = @lastName,
                            head_email = @email
                        WHERE head_id = @id";
                }

                else
                {
                    cmd.CommandText = @"
                        UPDATE teacher
                        SET 
                            tchr_first_name = @firstName,
                            tchr_last_name = @lastName,
                            tchr_email = @email
                        WHERE tchr_id = @id";
                }
                
                cmd.Parameters.AddWithValue("firstName", form.FirstName.Trim());
                cmd.Parameters.AddWithValue("lastName", form.LastName.Trim());
                cmd.Parameters.AddWithValue("email", form.Email.Trim());
                cmd.Parameters.AddWithValue("id", form.Id);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    return NotFound(new { success = false, message = "Faculty member not found" });
                }
            }
            
            else
            {
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;

                if (form.OldType == 1)
                {
                    cmd.CommandText = @"
                        DELETE FROM program_head
                        WHERE head_id = @id
                        RETURNING head_password";
                }
                else
                {
                    cmd.CommandText = @"
                        DELETE FROM teacher
                        WHERE tchr_id = @id
                        RETURNING tchr_password";
                }
                
                cmd.Parameters.AddWithValue("id", form.Id);

                var password = (string)(await cmd.ExecuteScalarAsync())!;
                
                await using (var checkCmd = conn.CreateCommand())
                {
                    checkCmd.Transaction = tx;
                    checkCmd.CommandText = @"
                        SELECT 1 FROM program_head
                        WHERE head_email = @email
                        
                        UNION ALL
                        
                        SELECT 1 FROM teacher
                        WHERE tchr_email = @email";
                    checkCmd.Parameters.AddWithValue("email", form.Email.Trim());

                    var exists = await checkCmd.ExecuteScalarAsync();
                    if (exists is not null)
                        return Conflict(new { success = false, message = "An account with this email already exists." });
                }

                await using var insertCmd = conn.CreateCommand();
                insertCmd.Transaction = tx;
                
                if (form.Type == 1)
                {
                    insertCmd.CommandText = @"
                        INSERT INTO program_head (
                            head_first_name, head_last_name,
                            head_email, head_password, head_is_active
                        ) VALUES (
                            @firstName, @lastName,
                            @email, @password, false
                        ) RETURNING head_id";
                }
                
                else
                {
                    insertCmd.CommandText = @"
                        INSERT INTO teacher (
                            tchr_first_name, tchr_last_name,
                            tchr_email, tchr_password, tchr_is_active
                        ) VALUES (
                            @firstName, @lastName,
                            @email, @password, false
                        ) RETURNING tchr_id";
                }
                
                insertCmd.Parameters.AddWithValue("firstName", form.FirstName.Trim());
                insertCmd.Parameters.AddWithValue("lastName", form.LastName.Trim());
                insertCmd.Parameters.AddWithValue("email", form.Email.Trim());
                insertCmd.Parameters.AddWithValue("password", password);
                
                resultId = (int)(await insertCmd.ExecuteScalarAsync())!;
            }

            await tx.CommitAsync();
            
            return Ok(new
            {
                success = true,
                data = new { Id = resultId }
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
    
    [HttpPost("Faculty/Delete", Name = "Faculty.Delete")]
    public async Task<IActionResult> DeleteFaculty([FromForm] DeleteDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var tx = await conn.BeginTransactionAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;

            if (form.TypeId == 1)
            {
                cmd.CommandText = @"
                    DELETE FROM program_head
                    WHERE head_id = @id";
            }
            else
            {
                cmd.CommandText = @"
                    DELETE FROM teacher
                    WHERE tchr_id = @id";
            }
                
            cmd.Parameters.AddWithValue("id", form.Id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Faculty member not found" });
            }
            
            await tx.CommitAsync();
            
            return Ok(new
            {
                success = true,
                data = new { form.Id }
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