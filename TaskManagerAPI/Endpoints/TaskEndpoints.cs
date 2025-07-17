using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TaskManagerAPI.Data;
using TaskManagerAPI.Dtos;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var taskGroup = app.MapGroup("/tasks");

        // GET /tasks - Gets all tasks
        taskGroup.MapGet("/", async (AppDbContext db, IConnectionMultiplexer redis) =>
        {
            var cache = redis.GetDatabase();
            const string cacheKey = "tasks:all";

            var cachedTasks = await cache.StringGetAsync(cacheKey);
            if (!cachedTasks.IsNullOrEmpty)
            {
                return Results.Ok(JsonSerializer.Deserialize<List<TaskItem>>(cachedTasks!));
            }

            var tasks = await db.TaskItems.ToListAsync();
            await cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(tasks), TimeSpan.FromMinutes(2));
            return Results.Ok(tasks);
        });
        
        // GET /tasks/{id} - Gets a single task
        taskGroup.MapGet("/{id:int}", async (int id, AppDbContext db, IConnectionMultiplexer redis) =>
        {
            var cache = redis.GetDatabase();
            var cacheKey = $"task:{id}";

            var cachedTask = await cache.StringGetAsync(cacheKey);
            if (!cachedTask.IsNullOrEmpty)
            {
                return Results.Ok(JsonSerializer.Deserialize<TaskItem>(cachedTask!));
            }

            var task = await db.TaskItems.FindAsync(id);
            if (task is null) return Results.NotFound();

            await cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(task), TimeSpan.FromMinutes(5));
            return Results.Ok(task);
        });

        // POST /tasks - Creates a new task
        taskGroup.MapPost("/", async (CreateTaskDto inputDto, AppDbContext db, IConnectionMultiplexer redis) =>
        {
            var task = new TaskItem
            {
                Title = inputDto.Title,
                Description = inputDto.Description,
                IsCompleted = inputDto.IsCompleted,
                Priority = inputDto.Priority,
                DueDate = inputDto.DueDate,
            };
            db.TaskItems.Add(task);
            await db.SaveChangesAsync();

            // When a new task is added, the list of all tasks is no longer valid.
            // We remove it from the cache.
            var cache = redis.GetDatabase();
            await cache.KeyDeleteAsync("tasks:all");

            return Results.Created($"/tasks/{task.Id}", task);
        });

        // PUT /tasks/{id} - Updates a task
        taskGroup.MapPut("/{id:int}", async (int id, TaskItem inputTask, AppDbContext db, IConnectionMultiplexer redis) =>
        {
            var task = await db.TaskItems.FindAsync(id);
            if (task is null) return Results.NotFound();

            task.Title = inputTask.Title;
            task.Description = inputTask.Description;
            task.IsCompleted = inputTask.IsCompleted;
            task.Priority = inputTask.Priority;
            task.DueDate = inputTask.DueDate;

            await db.SaveChangesAsync();

            // The data for this specific task and the full list are now invalid.
            // Remove both from the cache.
            var cache = redis.GetDatabase();
            await cache.KeyDeleteAsync($"task:{id}");
            await cache.KeyDeleteAsync("tasks:all");

            return Results.NoContent();
        });

        // DELETE /tasks/{id} - Deletes a task
        taskGroup.MapDelete("/{id:int}", async (int id, AppDbContext db, IConnectionMultiplexer redis) =>
        {
            var task = await db.TaskItems.FindAsync(id);
            if (task is null) return Results.NotFound();

            db.TaskItems.Remove(task);
            await db.SaveChangesAsync();

            // Invalidate caches
            var cache = redis.GetDatabase();
            await cache.KeyDeleteAsync($"task:{id}");
            await cache.KeyDeleteAsync("tasks:all");

            return Results.NoContent();
        });
    }
}