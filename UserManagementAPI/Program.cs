using UserManagementAPI.Models;
using UserManagementAPI.Repositories;
using UserManagementAPI.Validation;

const string apiRoute = "/api/users"; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var problem = Results.Problem(
            detail: "An unexpected error occurred while processing your request.",
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Unexpected error"
        );

        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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
        return Results.BadRequest(errors);

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

app.Run();
