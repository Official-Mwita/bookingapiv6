using BookingApi;
using BookingApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


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
        //string[] origins = { 
        //    "http://192.168.1.200:3000", 
        //    "http://localhost:3000",
        //    "http://192.168.1.200:3000/", 
        //    "http://localhost:3000/",
        //    "http://192.168.1.6:3000",
        //    "http://192.168.1.6:3000/login",
        //    "http://192.168.1.5:3000/*",
        //    "https://192.168.1.5:3000/*",
        //    "http://192.168.1.200:3000/*",
        //};

        string[] origins = {
                        "https://i-business-ouigcdw7x-moryno.vercel.app",
                        "https://i-business-ouigcdw7x-moryno.vercel.app/",
                        "https://ibusiness-a6vkaxgd2-moryno.vercel.app/",
                        "https://i-business-2niabkx7d-moryno.vercel.app",
                        "https://i-business-2niabkx7d-moryno.vercel.app/",
                        "https://ibusiness-a6vkaxgd2-moryno.vercel.app/",
                        "http://localhost:3000",
                        "http://192.168.1.200:3000/",
                        "https://react-test-official-mwita.vercel.app/",
                        "https://react-test-eta-eight.vercel.app",
                        "https://react-test-eta-eight.vercel.app/"
                    };



        ops.WithOrigins(origins).AllowCredentials().WithMethods("POST", "GET").WithHeaders("Cookie", "Content-Type", "X-Custom-Header", "set-Cookie", "Authorization");
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
