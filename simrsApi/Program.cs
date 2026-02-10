using cpcApi.Data;
using cpcApi.Data.Seeders;
using cpcApi.Filter;
using cpcApi.Model;
using cpcApi.Services;
using cpcApi.Services.Bank;
using cpcApi.Services.Cabang;
using cpcApi.Services.Combo;
using cpcApi.Services.Cpc;
using cpcApi.Services.Mesin;
using cpcApi.Services.Order;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

//var cs = builder.Configuration.GetConnectionString("MonitoringConnection");
//Console.WriteLine(cs);

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer("name=DefaultConnection"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    );
});

// For Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(p =>
{
    p.Password.RequireDigit = false;
    p.Password.RequireLowercase = false;
    p.Password.RequireUppercase = false;
    p.Password.RequireNonAlphanumeric = false;
    p.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var secretKey = builder.Configuration["Jwt:Secret"]
                ?? throw new ArgumentNullException("Jwt:Secret is missing in configuration.");

// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})

// Adding Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
        ValidAudience = builder.Configuration["Jwt:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    options.UseSecurityTokenValidators = true;
});

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// Add CORS service with policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOn8085", policy =>
        policy.WithOrigins("https://localhost:3000", "http://localhost:3000", "http://157.66.34.74", "http://157.66.34.74:82")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()); // jika pakai cookie/auth
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddHttpClient();

builder.Services.AddScoped<DapperSistagContext>();

builder.Services.AddScoped<ApiKeyAuthorizeAsyncFilter>();

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<IRepoCabang, RepoCabang>();

builder.Services.AddScoped<IRepoMesin, RepoMesin>();

builder.Services.AddScoped<IRepoCombo, RepoCombo>();

builder.Services.AddScoped<IRepoBank, RepoBank>();

builder.Services.AddScoped<IRepoOrder, RepoOrder>();

builder.Services.AddScoped<CpcReportService>();


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // ===============================
    // 🔑 RUN IDENTITY SEEDER (DI SINI)
    // ===============================
    await IdentitySeeder.SeedAsync(app.Services);

    app.MapOpenApi();
    app.UseSwagger(); // Enable Swagger UI
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.RoutePrefix = string.Empty;  // Make Swagger UI accessible at the root
    });
}

app.UseCors("AllowFrontendOn8085");

//app.UseHttpsRedirection();

//app.UseMiddleware<HeaderValidationMiddleware>();

app.UseAuthorization();

// Enable static files
app.UseStaticFiles(); // default: wwwroot

// Jika folder berada di wwwroot/uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads")),
    RequestPath = "/upload" // URL prefix harus sama dengan yang dipanggil
});


app.MapControllers();

app.Run();
