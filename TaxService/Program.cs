using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaxService.Data;
using TaxService.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var secretKey = builder.Configuration["JwtSettings:SecretKey"];

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<AuthService>(provider => 
    new AuthService(builder.Configuration["JwtSettings:SecretKey"], provider.GetRequiredService<ApplicationDbContext>()));
builder.Services.AddScoped<UserService>(provider => 
    new UserService(builder.Configuration["JwtSettings:SecretKey"], provider.GetRequiredService<ApplicationDbContext>()));
builder.Services.AddScoped<ReportService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddDirectoryBrowser();
builder.Services.AddControllersWithViews();


var app = builder.Build();
app.UseStaticFiles();

app.MapControllers();   

app.MapGet("/", async context =>
{
    context.Response.Redirect("/login.html");
});

app.Run();
