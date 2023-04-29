using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PuzonnsThings.Databases;
using PuzonnsThings.Models.Todo;
using PuzonnsThings.Models;
using TodoApp.Repositories;

namespace PuzonnsThings.Controllers;

[Authorize]
[ApiController]
public class TodoController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly UserRepository _respository;

    public TodoController(DatabaseContext context, UserRepository respository)
    {
        _context = context;
        _respository = respository;
    }

    [HttpPost("/api/[controller]/create")]
    public async Task<IActionResult> CreateUserTodo([FromBody] TodoModel todo)
    {
        User? user = await GetUser();

        if (user is null)
        {
            return Forbid();
        }

        TodoModel dbTodo = new TodoModel()
        {
            Date = todo.Date,
            UserId = user.Id,
            Name = todo.Name,
            ProgressId = todo.ProgressId,
        };

        await _context.TodoList.AddAsync(dbTodo);

        _context.TodoList.Entry(dbTodo).State = EntityState.Added;

        await _context.SaveChangesAsync();

        return Ok(dbTodo);
    }

    [HttpGet("/api/[controller]/fetch")]
    public async Task<ActionResult<List<TodoModel>>> FetchUserTodoList()
    {
        User? user = await GetUser();

        if (user is null)
        {
            return BadRequest();
        }

        return Ok(_context.TodoList.Where(x => x.UserId == user.Id).ToList());
    }

    [HttpDelete("/api/[controller]/delete")]
    public async Task<IActionResult> DeleteUserTodo([FromBody] TodoDeleteRequest todoDelete)
    {
        User? user = await GetUser();

        if (user is null)
        {
            return Forbid();
        }

        TodoModel? todo = await _context.TodoList.Where(x => x.UserId == user.Id && x.Id == todoDelete.Id).FirstOrDefaultAsync();

        if (todo is not null)
        {
            _context.TodoList.Remove(todo);
            await _context.SaveChangesAsync();

            return Ok();
        }

        return BadRequest("Todo dose not exist");
    }

    [HttpPut("/api/[controller]/update")]
    public async Task<IActionResult> UpdateUserTodo([FromBody] TodoUpdateModel update)
    {
        User? user = await GetUser();

        TodoModel? model = await _context.TodoList.Where(x => x.UserId == user.Id && x.Id == update.TodoId).FirstOrDefaultAsync();

        if (model is null)
        {
            return BadRequest("Todo with given id dose not exist");
        }

        if (model.ProgressId == update.ProgressId)
        {
            return Ok();
        }

        model.ProgressId = update.ProgressId;

        _context.TodoList.Entry(model).State = EntityState.Modified;

        _context.TodoList.Update(model);

        await _context.SaveChangesAsync();

        return Ok();
    }

    private async Task<User?> GetUser()
    {
        Claim? userId = HttpContext.User.Claims.Where(x => x.Type == JwtRegisteredClaimNames.Jti).FirstOrDefault();

        if (userId is null || userId.Value is null)
        {
            return null;
        }

        return await _respository.GetByIdAsync(int.Parse(userId.Value));
    }
}