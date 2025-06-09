using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

[ApiController]
public class RoomApiController(IConfiguration config, ILogger<RoomApiController> logger) : ControllerBase
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    [NonAction]
    public async Task<List<Room>> GetRooms()
    {
        var list = new List<Room>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM room
            ORDER BY room_id";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var room = new Room
            {
                RoomId = reader.GetInt32(reader.GetOrdinal("room_id")),
                RoomCode = reader.GetString(reader.GetOrdinal("room_code")),
                ProgId = reader["prog_id"] as int?,
                RoomIsActive = reader.GetBoolean(reader.GetOrdinal("room_is_active")),
            };
        
            list.Add(room);
        }
    
        return list;
    }
    
    [HttpPost("Rooms/Add", Name = "Rooms.Add")]
    public async Task<IActionResult> AddRoom([FromForm] Room form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO room (
                    room_code, prog_id
                ) VALUES (
                    @roomCode, @progId
                ) RETURNING room_id";

            insertCmd.Parameters.AddWithValue("roomCode", form.RoomCode.Trim());
            if (form.ProgId.HasValue)
                insertCmd.Parameters.AddWithValue("progId", form.ProgId);
            else
                insertCmd.Parameters.AddWithValue("progId", DBNull.Value);
            
            var newId = (int)(await insertCmd.ExecuteScalarAsync())!;
            
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
    
    [HttpPost("Rooms/Update", Name = "Rooms.Update")]
    public async Task<IActionResult> UpdateRoom([FromForm] Room form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE room
                SET 
                    room_code = @roomCode,
                    prog_id = @progId
                WHERE room_id = @roomId";

            cmd.Parameters.AddWithValue("roomCode", form.RoomCode);
            if (form.ProgId.HasValue)
                cmd.Parameters.AddWithValue("progId", form.ProgId);
            else
                cmd.Parameters.AddWithValue("progId", DBNull.Value);
            cmd.Parameters.AddWithValue("roomId", form.RoomId);
            
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Room not found" });
            }
            
            return Ok(new
            {
                success = true,
                data = new { Id = form.RoomId }
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
    
    [HttpPost("Rooms/Delete", Name = "Rooms.Delete")]
    public async Task<IActionResult> DeleteProgram([FromForm] IdDto form)
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
                    SELECT 1 FROM course_schedule
                    WHERE room_id = @roomId";
                
                checkCmd.Parameters.AddWithValue("roomId", form.Id);

                var exists = await checkCmd.ExecuteScalarAsync();
                
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                
                if (exists is not null)
                {
                    cmd.CommandText = @"
                        UPDATE room
                        SET 
                            room_is_active = false
                        WHERE room_id = @roomId";
                }
                else
                {
                    cmd.CommandText = @"
                        DELETE FROM room
                        WHERE room_id = @roomId";
                }
                cmd.Parameters.AddWithValue("roomId", form.Id);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    return NotFound(new { success = false, message = "Room not found" });
                }
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