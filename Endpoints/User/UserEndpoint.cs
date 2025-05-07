using dotnet.Database;
using dotnet.Extensions;
using dotnet.Filters;
using dotnet.Models;
using dotnet.Records;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace dotnet.Endpoints;

internal sealed class UserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder endPointGroup = app.MapGroup("users/").WithTags(Tags.Users);

        endPointGroup.MapGet("/", async (AppDbContext dbContext) =>
        {
            var users = await dbContext.Users.ToListAsync();
            var resp = Result<object>.Success(users);

            return Results.Ok(resp);

        }).MapToApiVersion(1);

        endPointGroup.MapGet("/{id:int}", async (int id, AppDbContext dbContext) =>
        {
            var user = await dbContext.Users.FindAsync(id);
            if (user is null)
            {
                return Results.NotFound(Result<string>.Failure(new Error("User.NotFound", $"User with ID {id} does not exits")));
            }
            return Results.Ok(Result<object>.Success(user));

        }).MapToApiVersion(1);

        endPointGroup.MapPost("", async (User user, AppDbContext dbContext, IValidator<User> validator) =>
        {
            var validationResult = validator.Validate(user);

            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            await dbContext.Users.AddAsync(new UserModel()
            {
                Name = user.Name,
                Address = user.Address
            });

            await dbContext.SaveChangesAsync();

            return Results.Created("User added", user);

        }).MapToApiVersion(1);

        endPointGroup.MapPut("/{id:int}", async (int id, User user, AppDbContext dbContext) =>
        {
            var existingUser = await dbContext.Users.FindAsync(id);

            if (existingUser is null)
            {
                var resp = Result<string>.Failure(new Error("User.NotFound", $"User with Id {id} not found"));
                return Results.NotFound(resp);
            }

            existingUser.Address = user.Address;
            existingUser.Name = user.Name;
            await dbContext.SaveChangesAsync();
            return Results.Ok(Result<object>.Success(existingUser));
        }).AddEndpointFilter<ValidationFilter<User>>()
        .MapToApiVersion(1);

        endPointGroup.MapDelete("/{id:int}", async (int id, AppDbContext dbContext) =>
        {

            var user = await dbContext.Users.FindAsync(id);
            if (user is null)
            {
                return Results.NotFound(Result<string>.Failure(new Error("User.NotFound", $"User with ID {id} not found")));
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();

            return Results.Ok(Result<string>.Success($"User {id} is deleted successfully"));

        }).MapToApiVersion(1);

        endPointGroup.MapGet("/", async (AppDbContext dbContext) =>
            {
                var userList = await dbContext.Users.ToListAsync();

                if (userList is not null)
                {
                    userList.Add(new UserModel
                    {
                        Name = $"{userList.FirstOrDefault()?.Name} V2",
                        Address = $"{userList.FirstOrDefault()?.Address} V2"
                    });

                    return Results.Ok(Result<object>.Success(userList));
                }
                return Results.Ok(Result<object>.Success(userList!));
            }).MapToApiVersion(2);
    }
}