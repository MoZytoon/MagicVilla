//using Serilog;
//using MagicVilla_VillaAPI.Logging;

using MagicVilla_API;
using MagicVilla_API.Data;
using MagicVilla_API.Repository;
using MagicVilla_API.Repository.IRepository;
using MagicVilla_API.Repository.IRepostiory;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(o => {
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

//Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File("log/villalog.txt",rollingInterval: RollingInterval.Day).CreateLogger();
//builder.Host.UseSerilog();

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

//builder.Services.AddApiVersioning(options =>
//{
//    options.AssumeDefaultVersionWhenUnspecified = true;
//    options.DefaultApiVersion = new ApiVersion(1, 0);
//});

builder.Services.AddControllers(o => {
    //o.ReturnHttpNotAcceptable = true; 
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();

builder.Services.AddEndpointsApiExplorer();

//get secret key from appSittings.json
var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

//add Authentication Configurations
builder.Services.AddAuthentication(x =>
{
    //configure the default authentication scheme and the default challenge scheme. Both of them we want to use JWT bearer defaults
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    //we have something called as add JWT bearer and then we can configure options on that.
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false; //we have the required https metadata
        x.SaveToken = true; //save the token
        x.Authority = "https://localhost:7003/";
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
        };
    });

//add authorization
//builder.Services.AddAuthorization();

//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//         .AddCookie(options =>
//         {

//             options.Events.OnRedirectToLogin = context =>
//             {
//                 context.Response.Headers["Location"] = context.RedirectUri;
//                 context.Response.StatusCode = 401;
//                 return Task.CompletedTask;
//             };


//         })
//         .AddJwtBearer(options =>    //.AddJwtBearer("bearer", options =>
//         {
//             options.TokenValidationParameters = new TokenValidationParameters
//             {
//                 ValidateAudience = false,
//                 ValidateIssuer = false,
//                 ValidateIssuerSigningKey = true,
//                 //ValidIssuer = Configuration["Jwt:Issuer"],
//                 //ValidAudience = Configuration["Jwt:Audience"],
//                 //TODO: Add ["Jwt:Key"] to key vault 
//                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
//                 ValidateLifetime = true,
//                 ClockSkew = TimeSpan.Zero //the default for this setting is 5 minutes
//             };
//             options.Events = new JwtBearerEvents
//             {
//                 OnAuthenticationFailed = context =>
//                 {
//                     if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
//                     {
//                         context.Response.Headers.Add("Token-Expired", "true");
//                     }
//                     return Task.CompletedTask;
//                 }
//             };
//         });

//to enable bearer in swagger
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
            "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

//builder.Services.AddSingleton<ILogging, Logging>();

//app => configured application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();