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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NLog.Web;
using Quartz;
using System.Security.Claims;
using System.Security.Cryptography;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IStatisticsService, StatisticService>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();
builder.Services.AddScoped<IColorService, ColorServie>();
builder.Services.AddScoped<IColorRepository, ColorRepository>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<IKeycloakAdminClientService, KeycloakAdminClientService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var tokenEndpoint = configuration["KeycloakAdmin:TokenEndpoint"];
    var clientId = configuration["KeycloakAdmin:ClientId"];
    var clientSecret = configuration["KeycloakAdmin:ClientSecret"];
    var username = configuration["KeycloakAdmin:Username"];
    var password = configuration["KeycloakAdmin:Password"];
    var realm = configuration["KeycloakAdmin:Realm"];

    return new KeycloakAdminClientService(tokenEndpoint, clientId, clientSecret, username, password, realm);
});

builder.Services.AddSingleton<IFileManagerService, FileManagerService>();
builder.Services.AddSingleton<ProducerHostedService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<EntityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
string jsonFilePath = "jwks.json"; // Укажите путь к вашему файлу JSON
string jsonWebKeySet = System.IO.File.ReadAllText(jsonFilePath);
// Загрузка ключей из JSON
var signingKeys = GetSigningKeys(jsonWebKeySet);


builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
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
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" // Убедитесь, что это соответствует вашему Bearer токену
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Token failed Bearer: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated Bearer: " + context.SecurityToken);
                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    var roles = context.Principal.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(c => c.Value).ToArray();
                    claimsIdentity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddJwtBearer("Keycloak", o =>
    {
        o.RequireHttpsMetadata = false;
        o.Authority = builder.Configuration["Authentication:MetadataAddress"];
        o.Audience = builder.Configuration["Authentication:Audience"];
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = signingKeys,
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" // Убедитесь, что это соответствует вашему Keycloak токену
        };
        o.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Token failed Keycloak: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated: " + context.SecurityToken);
                var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    var realmAccessClaim = context.Principal.FindFirst("realm_access");
                    if (realmAccessClaim != null)
                    {
                        var realmAccess = JsonConvert.DeserializeObject<Dictionary<string, object>>(realmAccessClaim.Value);
                        if (realmAccess != null && realmAccess.TryGetValue("roles", out var rolesObj))
                        {
                            var roles = rolesObj as JArray;
                            if (roles != null)
                            {
                                foreach (var role in roles)
                                {
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                                }
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("CheckAddPhotoToCarJob");
    q.AddJob<CheckAddPhotoToCarJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
    .ForJob(jobKey)
    .WithIdentity("CheckAddPhotoToCarJob-trigger")
    .WithCronSchedule("0 * * ? * *"));
});

builder.Services.AddQuartzHostedService(q=> q.WaitForJobsToComplete = true);

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
    .EnableQueryFeatures()
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
    c.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(builder.Configuration["Keycloak:AuthorizationUrl"]!),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "openid" },
                    { "profile", "profile" }
                }
            }
        }
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
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Keycloak",
                    Type = ReferenceType.SecurityScheme,
                },
                In = ParameterLocation.Header,
                Name = "Bearer",
                Scheme = "Bearer"
            },
            []
        }
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManagerOrAdmin", policy =>
        policy.RequireAuthenticatedUser()
               .RequireRole("Manager", "Admin")
               .AddAuthenticationSchemes("Bearer", "Keycloak"));
    options.AddPolicy("Admin", policy =>
    {
        policy.RequireAuthenticatedUser()
        .RequireRole("Admin")
        .AddAuthenticationSchemes("Bearer", "Keycloak");
                
    });
    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin", "User")
              .AddAuthenticationSchemes("Bearer", "Keycloak"));
    options.AddPolicy("All", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin", "User", "Manager")
              .AddAuthenticationSchemes("Bearer", "Keycloak"));
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
app.UseCors("AllowAll");
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


// Метод для преобразования ключей из JSON в RsaSecurityKey
IEnumerable<SecurityKey> GetSigningKeys(string jsonWebKeySet)
{
    var keys = JsonWebKeySet.Create(jsonWebKeySet);
    return keys.Keys.Where(k => k.Use == "sig").Select(k =>
    {
        var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(k.N),
            Exponent = Base64UrlEncoder.DecodeBytes(k.E)
        });
        return new RsaSecurityKey(rsa);
    });
}