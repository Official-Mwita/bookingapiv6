using Azure.Core;
using BookingApi;
using BookingApi.Commons;
using BookingApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();

////Add microsoft authenticatio
//builder.Services.AddAuthentication()
//    .AddMicrosoftIdentityWebApi(builder.Configuration);

//builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

// Add services to the container.

//Add Jwt authentication and validation middleware
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    
}).AddJwtBearer(ops =>
{
    //Add on message received event that decrpyt, then replaces authorization token with plain text one
    ops.Events = new JwtBearerEvents()
    {
        //Fired before a token can be verified
        OnMessageReceived = context =>
         {
             try 
             {
                 var authorization = context.HttpContext.Request.Headers.Authorization;
                 string decryptedtoken;

                 if (authorization.Count > 0)
                 {
                     decryptedtoken = authorization[0]?.Substring("Bearer ".Length).Trim() ?? "";
                     decryptedtoken = new AesEncryption(builder.Configuration).Decrypt(decryptedtoken);

                     //Write back the token httpcontext header
                     context.HttpContext.Request.Headers.Authorization = "Bearer " + decryptedtoken;
                 }

             }
             catch
             {
                 //Nothing to process. The rest is handled by default
             }
             
             return Task.CompletedTask;
         }

    };
    ops.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))

    };

    //ops.ForwardSignIn = "/api/Account/Login";

});

//Add authorization here. Can be added using a middleware function
builder.Services.AddAuthorization(ops =>
{
    ops.AddPolicy("AdminUser", policy => policy.RequireClaim(MUser.ADMIN_TYPE, "admin"));
});


builder.Services.AddControllers();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(ops =>
    {
        ops.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
}
else
{
    app.UseCors(ops =>
    {

        string[] origins = {
                        "https://ibusiness-git-main-moryno.vercel.app", //Dashboard frontend link
                        "https://i-business-ui-git-main-moryno.vercel.app", //login frontend link
                        "https://i-business-ui-git-main-moryno.vercel.app/",//login frontend link
                        "https://ibusiness-git-main-moryno.vercel.app/", //Dashboard frontend link
                        "http://localhost:3000",
                        "http://localhost:3000/",
                        "http://192.168.1.200:3000/",
                        "http://localhost:3000/dashboard"
                    };

        ops.WithOrigins(origins).AllowCredentials().AllowAnyMethod().WithHeaders("Cookie", "Content-Type", "X-Custom-Header", "set-Cookie", "Authorization");
    });
}



app.UseAuthentication();
app.UseAuthorization();

//Redirect back if user is not authenticated
//This middleware ensures that, a user is authenticated before atleast going further into the system
app.Use(async (Context, next) =>
{
    if (!Context.User.Identity?.IsAuthenticated??false)
    {
        Context.Response.StatusCode = 401;
        await Context.Response.WriteAsync("Unauthorized. Try logging in");
    }
    else
    {
        await next();

    }

});


app.MapControllers();

app.Run();
