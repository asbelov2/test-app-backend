using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using test_app.Models;
using test_app.Services;
namespace test_app.Controllers;

public static class PostController
{
  public static void MapPostEndpoints(this IEndpointRouteBuilder routes)
  {
    var group = routes.MapGroup("/api/Post");

    group.MapGet("/", async (ApplicationContext db) =>
    {
      return await db.Posts.ToListAsync();
    })
    .WithName("GetAllPosts");

    group.MapGet("/{id}", async Task<Results<Ok<Post>, NotFound>> (int id, ApplicationContext db) =>
    {
      return await db.Posts.AsNoTracking()
              .FirstOrDefaultAsync(model => model.Id == id)
              is Post model
                  ? TypedResults.Ok(model)
                  : TypedResults.NotFound();
    })
    .WithName("GetPostById");

    group.MapGet("/GetPostByUserEmail", async (string email, ApplicationContext db) =>
    {
      var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
      return await db.Posts
              .Where(model => model.UserId == user.Id).ToListAsync();
    })
    .WithName("GetPostByUserEmail");

    group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Post post, ApplicationContext db) =>
    {
      var affected = await db.Posts
              .Where(model => model.Id == id)
              .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.Id, post.Id)
                .SetProperty(m => m.UserId, post.UserId)
                .SetProperty(m => m.Heading, post.Heading)
                .SetProperty(m => m.Text, post.Text)
              );

      return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
    })
    .WithName("UpdatePost");


    group.MapPost("/", async (Post post, ApplicationContext db) =>
{
  db.Posts.Add(post);
  await db.SaveChangesAsync();
  return TypedResults.Created($"/api/Post/{post.Id}", post);
})
.WithName("CreatePost");

    group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, ApplicationContext db) =>
    {
      var affected = await db.Posts
              .Where(model => model.Id == id)
              .ExecuteDeleteAsync();

      return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
    })
    .WithName("DeletePost");
  }
}
