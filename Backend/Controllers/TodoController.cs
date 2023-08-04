using PuzonnsThings.Models.Todo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuzonnsThings.Databases;
using PuzonnsThings.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PuzonnsThings.Repositories;

namespace PuzonnsThings.Controllers;

[Authorize]
[ApiController]
public class TodoController : ControllerBase
{
    private readonly DatabaseContext dbContext;
    private readonly IUserRepository userRepository;

    public TodoController(DatabaseContext context, IUserRepository _userRepository)
    {
        dbContext = context;
        userRepository = _userRepository;
    }

    [HttpPost("/api/[controller]/create")]
    public async Task<IActionResult> CreateUserTodo([FromBody] TaskModelCreation task)
    {
        User? user = await GetUser();

        if (user is null)
        {
            return Forbid();
        }

        if (!task.Validate(out string error))
        {
            return BadRequest(error);
        }

        TaskModel taskModel = new TaskModel()
        {
            TaskEndDateTime = task.TaskEndDateTime,
            TaskName = task.TaskName,
            TaskProgressId = 2, //Uncompleted 
            TaskPriority = task.TaskPriority,
            UserId = user.Id
        };

        await dbContext.TodoList.AddAsync(taskModel);

        await dbContext.SaveChangesAsync();

        return Ok(taskModel);
    }

    [HttpGet("/api/[controller]/fetch")]
    public async Task<ActionResult<List<TaskModel>>> FetchUserTodoList()
    {
        User? user = await GetUser();

        if (user is null)
        {
            return BadRequest();
        }

        return Ok(dbContext.TodoList.Where(x => x.UserId == user.Id).ToList());
    }

    [HttpDelete("/api/[controller]/delete")]
    public async Task<IActionResult> DeleteUserTodo([FromBody] TodoDeleteRequest todoDelete)
    {
        User? user = await GetUser();

        if (user is null)
        {
            return Forbid();
        }

        TaskModel? task = await dbContext.TodoList.Where(x => x.UserId == user.Id && x.Id == todoDelete.Id).FirstOrDefaultAsync();

        if (task is not null)
        {
            dbContext.TodoList.Remove(task);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        return BadRequest("Todo dose not exist");
    }

    [HttpPut("/api/[controller]/update")]
    public async Task<IActionResult> UpdateUserTodo([FromBody] TodoUpdateModel update)
    {
        User? user = await GetUser();

        if (user is null)
        {
            return Forbid();
        }

        TaskModel? task = await dbContext.TodoList.Where(x => x.UserId == user.Id && x.Id == update.TaskId).FirstOrDefaultAsync();

        if (task is null)
        {
            return BadRequest("Todo with given id dose not exist");
        }

        task.TaskPriority = update.TaskPriority;
        task.TaskName = update.TaskName;
        task.TaskProgressId = update.TaskProgressId;
        task.TaskEndDateTime = update.TaskEndDateTime;

        dbContext.TodoList.Update(task);

        await dbContext.SaveChangesAsync();

        return Ok(task);
    }

    private async Task<User?> GetUser()
    {
        Claim? userId = HttpContext.User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).FirstOrDefault();

        if (userId is null || userId.Value is null)
        {
            return null;
        }

        return await userRepository.GetByIdAsync(int.Parse(userId.Value));
    }
}