using API.Extensions;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(builder =>
    builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200")
);

app.UseAuthentication(); //checks for valid token(if its in the format of a token)
app.UseAuthorization(); // authorizes(if token data is correct) based on given token [think bearer token check for say accessing a user.]

// app.UseHttpsRedirection();


app.MapControllers();

app.Run();
