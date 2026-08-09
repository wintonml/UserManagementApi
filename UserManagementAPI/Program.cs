using UserManagementAPI.Models;
using UserManagementAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/users", async (IUserRepository repository) =>
{
    var users = await repository.GetAllAsync();
    return Results.Ok(users);
})
.WithName("GetUsers");

app.MapGet("/api/users/{id:guid}", async (Guid id, IUserRepository repository) =>
{
    var user = await repository.GetByIdAsync(id);
    return user is null ? Results.NotFound() : Results.Ok(user);
})
.WithName("GetUserById");

app.MapPost("/api/users", async (User user, IUserRepository repository) =>
{
    var createdUser = await repository.CreateAsync(user);
    return Results.Created($"/api/users/{createdUser.Id}", createdUser);
})
.WithName("CreateUser");

app.MapPut("/api/users/{id:guid}", async (Guid id, User user, IUserRepository repository) =>
{
    var updatedUser = await repository.UpdateAsync(id, user);
    return updatedUser is null ? Results.NotFound() : Results.Ok(updatedUser);
})
.WithName("UpdateUser");

app.MapDelete("/api/users/{id:guid}", async (Guid id, IUserRepository repository) =>
{
    var deleted = await repository.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteUser");

app.Run();
