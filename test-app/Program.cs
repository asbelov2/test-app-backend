using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using test_app.Controllers;
using test_app.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
builder.Services.AddDbContext<ApplicationContext>();
var app = builder.Build();


app.Map("/login", (string email, string password) =>
{
  var db = new ApplicationContext();
  if (email == null)
    return "Email is not provided";
  if (password == null)
    return "Password is not provided";
  var users = db.Users.ToList();
  if (db.Users.First(x => x.Email == email) == null)
    return "User is not registered";
  else
  {
    if (users.FirstOrDefault(x => (x.Email == email) && BCrypt.BCryptHelper.CheckPassword(password, x.PasswordHash)) == null)
      return "Incorrect email or password";
  }
  var user = users.FirstOrDefault(x => (x.Email == email) && BCrypt.BCryptHelper.CheckPassword(password, x.PasswordHash));
  var claims = new List<Claim> { new Claim("email", user.Email), new Claim("name", user.Username) };
  var jwt = new JwtSecurityToken(
          issuer: AuthOptions.ISSUER,
          audience: AuthOptions.AUDIENCE,
          claims: claims,
          expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(60)),
          signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

  return new JwtSecurityTokenHandler().WriteToken(jwt);
});

app.UseCors(builder => builder.WithOrigins("http://localhost:4200", "https://asbelov2.github.io/").AllowAnyMethod().AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();
//app.MapControllers();

app.MapUserEndpoints();

app.MapPostEndpoints();

app.Run();

public class AuthOptions
{
  public const string ISSUER = "TestAppServer";
  public const string AUDIENCE = "TestAppClient";
  const string KEY = "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";
  public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
      new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}