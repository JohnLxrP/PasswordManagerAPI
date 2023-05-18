using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using PasswordManagerAPI.Configurations;
using PasswordManagerAPI.CustomMiddleware;
using PasswordManagerAPI.Data;
using PasswordManagerAPI.DTO;
using PasswordManagerAPI.Model;
using PasswordManagerAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<apiDBContext>(opt => opt.UseInMemoryDatabase("pwDB"));
builder.Services.AddAutoMapper(typeof(AutoMapperConfig));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<apiDBContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 1;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
});

// configure the token decoding logic verify the token 
// algorithm to decode the token
var issuer = builder.Configuration["JWT:Issuer"];
var audience = builder.Configuration["JWT:Audience"];
var key = builder.Configuration["JWT:Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy("admin_greetings", policy => policy.RequireAuthenticatedUser());});

// policy who can access it 

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(opt =>
{
    opt.AddPolicy( name: MyAllowSpecificOrigins, policy =>
    {
        //policy.AllowAnyOrigin()
        policy.WithOrigins("https://localhost:44360", "mydomain.com")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// [Authorize]
app.MapGet("/passwordmngrs", async (apiDBContext db) => Results.Ok(await db.passwordmngrs.ToListAsync()))
    .RequireAuthorization("admin_greetings");

app.MapGet("/passwordmngrs/{id}", async (apiDBContext db, int id) =>
            await db.passwordmngrs.FindAsync(id)
                is Passwordmngr passwordmngr ? Results.Ok(passwordmngr) : Results.NotFound());

app.MapPost("/passwordmngrs", async (apiDBContext db, Passwordmngr passwordmngr) =>
{
    db.passwordmngrs.Add(passwordmngr);
    await db.SaveChangesAsync();
    return Results.Created($"/tpasswordmngrs/{passwordmngr.Id}", passwordmngr);
});

app.MapPut("/passwordmngrs/{id}", async (apiDBContext db, Passwordmngr passwordmngr, int id) =>
{
    var oldPasswordmngr = await db.passwordmngrs.FindAsync(id);
    if(passwordmngr is null) return Results.NotFound();
    // automapper
    oldPasswordmngr.Accountr = passwordmngr.Accountr;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/passwordmngrs/{id}", async (apiDBContext db, int id) =>
{
    if (await db.passwordmngrs.FindAsync(id) is Passwordmngr passwordmngr)
    {
        db.passwordmngrs.Remove(passwordmngr);
        await db.SaveChangesAsync();
        return Results.Ok(passwordmngr);
    }
    return Results.NotFound();
});

app.MapPost("/signup", async (apiDBContext db, IMapper mapper, UserManager <ApplicationUser> userManager, SignUpDTO userDTO) =>
{
    var user = mapper.Map<ApplicationUser>(userDTO);

    var newUser = await userManager.CreateAsync(user, userDTO.Password);
    if (newUser.Succeeded)
        return user;
    return null;
});

app.MapPost("/login", async (apiDBContext db, 
                            SignInManager < ApplicationUser > signInManager,
                            UserManager < ApplicationUser > userManager,
                            IConfiguration appConfig,
                            LoginDTO loginDTO) =>
{
    // generate a token and return a token
    var issuer = appConfig["JWT:Issuer"];
    var audience = appConfig["JWT:Audience"];
    var key = appConfig["JWT:Key"];

    if (loginDTO is not null)
    {
        var loginResult = await signInManager.PasswordSignInAsync(loginDTO.UserName, loginDTO.Password, loginDTO.RememberMe, false);
        if (loginResult.Succeeded)
        {
            // generate a token
            var user = await userManager.FindByEmailAsync(loginDTO.UserName);
            if (user != null)
            {
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var theKey = new SymmetricSecurityKey(keyBytes); // 256 bits of key
                var creds = new SigningCredentials(theKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(issuer, audience, null, expires: DateTime.Now.AddMinutes(30), signingCredentials: creds);
                return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) }); // token 
            }
        }
    }
    return Results.BadRequest();
});

app.UseCors(MyAllowSpecificOrigins);
// has a valid api key if it is valid then allow to access the endpoint if not deny
//app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.Run();

