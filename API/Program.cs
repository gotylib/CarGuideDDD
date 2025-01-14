using API.CustomMiddleware;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Core.Token;
using CarGuideDDD.Infrastructure.Data;
using CarGuideDDD.Infrastructure.Repositories;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using CarGuideDDD.Infrastructure.Services;
using CarGuideDDD.Infrastructure.Services.Hosted_Services;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IStatisticsService, StatisticService>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();

builder.Services.AddSingleton<IFileManagerService, FileManagerService>();
builder.Services.AddSingleton<ProducerHostedService>();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<EntityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = AuthOptions.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddSingleton<IJobFactory, JobFactory>();

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Настройка вашего задания с параметрами
    var jobKey = new JobKey("checkAddPhotoToCarJob");
    q.AddJob<CheckAddPhotoToCarJob>(opts => opts.WithIdentity(jobKey));

    // Настройка триггера
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("checkAddPhotoToCarJob-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithInterval(TimeSpan.FromMinutes(1))  // Пример интервала
            .RepeatForever()));

});

// Добавление сервисов
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddControllers().AddOData(opt => opt
    .AddRouteComponents("odata", GetEdmModel())
    .Select()
    .Filter()
    .OrderBy()
    .Expand()
    .SetMaxTop(20)
    .Count()
    );


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter prefix Bearer",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{ }
        }
    });
});

builder.Services.AddControllers();

builder.Services.AddHttpClient();



var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await InitializeRoles(app);

app.UseMiddleware<StatisticsMiddleware>();

app.UseMiddleware<ErrorHandlingMiddleware>();


await app.RunAsync();


static async Task InitializeRoles(IHost app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRoles(roleManager);
    }
    catch (Exception ex)
    {

        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles.");
    }
}

static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "Manager", "Admin", "User" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole { Name = roleName });
        }
    }
}


static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<PriorityCarDto>("Cars");
    return builder.GetEdmModel();
}