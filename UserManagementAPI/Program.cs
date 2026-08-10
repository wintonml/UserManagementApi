using UserManagementAPI.Models;
using UserManagementAPI.Repositories;
using UserManagementAPI.Validation;
using UserManagementAPI.Middleware;

const string apiRoute = "/api/users";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Assignment-mandated middleware order:
// 1. Error handling
// 2. Authentication
// 3. Logging
// Ideally logging would be the first middleware,
// but since we are logging the user id,
// we need to authenticate first.
// In a real-world application,
// we would log the request id and correlate it with
// the user id in a later step.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<AuditMiddleware>();
app.UseHttpsRedirection();

app.MapGet(apiRoute, async (IUserRepository repository) =>
{
    var users = await repository.GetAllAsync();
    return Results.Ok(users);
})
.WithName("GetUsers");

app.MapGet($"{apiRoute}/{{id:guid}}", async (Guid id, IUserRepository repository) =>
{
    var user = await repository.GetByIdAsync(id);
    return user is null ? Results.NotFound() : Results.Ok(user);
})
.WithName("GetUserById");

app.MapPost(apiRoute, async (CreateUserRequest user, IUserRepository repository) =>
{
    var errors = ValidateUser.ValidateUserInput(user);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    var createdUser = await repository.CreateAsync(user);
    return Results.Created($"/api/users/{createdUser.Id}", createdUser);
})
.WithName("CreateUser");

app.MapPut($"{apiRoute}/{{id:guid}}", async (Guid id, UpdateUserRequest user, IUserRepository repository) =>
{
    var updatedUser = await repository.UpdateAsync(id, user);
    return updatedUser is null ? Results.NotFound() : Results.Ok(updatedUser);
})
.WithName("UpdateUser");

app.MapDelete($"{apiRoute}/{{id:guid}}", async (Guid id, IUserRepository repository) =>
{
    var deleted = await repository.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteUser");

app.MapGet("/api/test/throw", () =>
{
    throw new Exception("test");
});

app.Run();
