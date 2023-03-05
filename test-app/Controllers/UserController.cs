using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using test_app.Models;
using test_app.Services;

namespace test_app.Controllers;

public static class UserController
{
  public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
  {
    var group = routes.MapGroup("/api/User");

    group.MapGet("/", async (ApplicationContext db) =>
    {
      return await db.Users.ToListAsync();
    })
    .WithName("GetAllUsers");

    group.MapGet("/{id}", async Task<Results<Ok<User>, NotFound>> (int id, ApplicationContext db) =>
    {
      return await db.Users.AsNoTracking()
              .FirstOrDefaultAsync(model => model.Id == id)
              is User model
                  ? TypedResults.Ok(model)
                  : TypedResults.NotFound();
    })
    .WithName("GetUserById");

    group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, User user, ApplicationContext db) =>
    {
      var affected = await db.Users
              .Where(model => model.Id == id)
              .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.Id, user.Id)
                .SetProperty(m => m.Email, user.Email)
                .SetProperty(m => m.Username, user.Username)
                .SetProperty(m => m.PasswordHash, user.PasswordHash)
              );

      return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
    })
    .WithName("UpdateUser");

    group.MapPost("/", async Task<Results<Created<User>, Conflict<string>>> (User user, ApplicationContext db) =>
    {
      if (db.Users.FirstOrDefault(x => x.Email == user.Email) != null)
      {
        return TypedResults.Conflict("User with this email already registered");
      }
      db.Users.Add(user);
      await db.SaveChangesAsync();
      return TypedResults.Created($"/api/User/{user.Id}", user);
    })
    .WithName("CreateUser");

    group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, ApplicationContext db) =>
    {
      var affected = await db.Users
              .Where(model => model.Id == id)
              .ExecuteDeleteAsync();

      return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
    })
    .WithName("DeleteUser");

    group.MapGet("/CheckEmail", async Task<Results<Ok<User>, NotFound>> (string email, ApplicationContext db) =>
    {
      return await db.Users.AsNoTracking()
                  .FirstOrDefaultAsync(model => model.Email == email)
               is User model
                  ? TypedResults.Ok(model)
                  : TypedResults.NotFound();

    })
    .WithName("CheckEmail");
  }
}
