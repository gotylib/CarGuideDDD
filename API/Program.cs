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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IStatisticsService, StatisticService>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();
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
// JSON с ключами
string jsonWebKeySet = @"
{
    ""keys"": [
        {
            ""kid"": ""XealZKgd5k4s14KxRZ_brOMlsVY8h3Pb0fTosAKvjXo"",
            ""kty"": ""RSA"",
            ""alg"": ""RSA-OAEP"",
            ""use"": ""enc"",
            ""n"": ""0vvgnQj83NicRdVIOh7uIkOFKAprs0Nu0foWwBolKysXgAxNC0JfHqYjJ0-91tnMNkLNI3xHyC1w6iNGHY6_hq0boA9x31_-Ua7vWyc0vgr-FDT-8jG5Pwe6GPoC_vEsz9-c3Hi4z7PRZTW4TWhP9MabPuhRB5owasZKoqoqO0PTe06x4EohDWV7y3qDHI9OB8sNZqhkhs7kSS6KIjTH9u6d44nlb3X42fn8JxevJfL6giGfOBZXJawS5c1SC99E9J3uFCBh7R_hRNmqckINRqnglh97fl27fThMd4FooYRU1SMMy2KQNhHRvelb2rqx2LNanGTd7xKMM9oPEpAriQ"",
            ""e"": ""AQAB"",
            ""x5c"": [
                ""MIIClTCCAX0CBgGUbodhOjANBgkqhkiG9w0BAQsFADAOMQwwCgYDVQQDDANkZXYwHhcNMjUwMTE2MDk1MDUzWhcNMzUwMTE2MDk1MjMzWjAOMQwwCgYDVQQDDANkZXYwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDS++CdCPzc2JxF1Ug6Hu4iQ4UoCmuzQ27R+hbAGiUrKxeADE0LQl8epiMnT73W2cw2Qs0jfEfILXDqI0Ydjr+GrRugD3HfX/5Rru9bJzS+Cv4UNP7yMbk/B7oY+gL+8SzP35zceLjPs9FlNbhNaE/0xps+6FEHmjBqxkqiqio7Q9N7TrHgSiENZXvLeoMcj04Hyw1mqGSGzuRJLooiNMf27p3jieVvdfjZ+fwnF68l8vqCIZ84FlclrBLlzVIL30T0ne4UIGHtH+FE2apyQg1GqeCWH3t+Xbt9OEx3gWihhFTVIwzLYpA2EdG96VvaurHYs1qcZN3vEowz2g8SkCuJAgMBAAEwDQYJKoZIhvcNAQELBQADggEBAC8GU+15dZsDigJXtJYya42li5t4dzGEG36NiMPjcbHtxM7eD28NOiHkGOO/NYoayqVrXzNY8fAdXEfSiUKcmZUmciN9gNwxNbucE7lSVK0qIgzfCm7F0f8+VrHxkDbCrwWH64s5oHOSSaHSjZc5SB4mp2k+joy/eE20OBRanLb+kruHeFLnm2RIHYSmWdvoP+ZeVKyrNXEDm1GMFzJeTTBN8n1nmtT6zVRW0dSV2Ha0NnMwBRTKcSWF/sEwYfws6mHRgAGq6kQ3N5MIbQQkaMwC+JJXoS0M073dfaaK4C/rI6GIIHZYiYCbumQ+WE6ANVBG8B/ubS8Ri+eQHM36BSQ=""
            ],
            ""x5t"": ""BJZvCoTDVEghnmCTL8aQWRRKTI0"",
            ""x5t#S256"": ""8pdIFR-mgOU1a_PZXNF9nwEEoZDPvEe3k6Mpneg_deo""
        },
        {
            ""kid"": ""pK0rZ6UPYxbphCHGeRdHjWM9UZAT-Wea0D422ZuaL1w"",
            ""kty"": ""RSA"",
            ""alg"": ""RS256"",
            ""use"": ""sig"",
            ""n"": ""y_zV70NCXw_EEvE3iPWTFXAZXLldbideUtfrs1OCqcmg1aYHTnTVMfaNev9y5f23OiC997iTK__eCGQkLJ9vzON904PWrUY6neo2nWz4qVh8QDQUo-lSOtnNSeAqjHVsZLLQME92Vh3rdTOPv0RvXBXEk8Jn18twpphr_Dsi2oyxqaaqt-ovGzguwEcdjn9J-tOapid0F9EIxdUH5f4yKCBO5fGp7PLmJW7BbeP_gaGfpB_JGSteus7h3np8HN3gQlO_v5IyAdOD9fp7eQVd9P7oQYDDmlO5-8lRmfM2so3gcHdkG6gSjkhiu43oqRCysinHVeK5XXvScK3sqiA1yQ"",
            ""e"": ""AQAB"",
            ""x5c"": [
                ""MIIClTCCAX0CBgGUbodglDANBgkqhkiG9w0BAQsFADAOMQwwCgYDVQQDDANkZXYwHhcNMjUwMTE2MDk1MDUzWhcNMzUwMTE2MDk1MjMzWjAOMQwwCgYDVQQDDANkZXYwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDL/NXvQ0JfD8QS8TeI9ZMVcBlcuV1uJ15S1+uzU4KpyaDVpgdOdNUx9o16/3Ll/bc6IL33uJMr/94IZCQsn2/M433Tg9atRjqd6jadbPipWHxANBSj6VI62c1J4CqMdWxkstAwT3ZWHet1M4+/RG9cFcSTwmfXy3CmmGv8OyLajLGppqq36i8bOC7ARx2Of0n605qmJ3QX0QjF1Qfl/jIoIE7l8ans8uYlbsFt4/+BoZ+kH8kZK166zuHeenwc3eBCU7+/kjIB04P1+nt5BV30/uhBgMOaU7n7yVGZ8zayjeBwd2QbqBKOSGK7jeipELKyKcdV4rlde9JwreyqIDXJAgMBAAEwDQYJKoZIhvcNAQELBQADggEBADooizPNsFZ53qe9XdecZ/i9FfUbdDpn9htzbTinc8Z9hAfTqxJkJTtLMFj6h/RiCdjOjGs0dQlhwcAo0MlioVTlrsOOvF2qaMtV1JD7eSEj3SSGlTMdzdnlOvGI8mw5Qro7kwpO/T/fsEP4zwKCMzHjfqZYReh4aSQ6fTN7lIkITSgdG+HRsnVcQZX1I92OKCMubZH6VUv/KH8NcIbUwDJBtZZ9Oy9Nw+CI/Aunf49B82Ykkccvq9HtyjNfD0OOE6UREAxFUiDRjTNHav/xW+/0keehQevadvqWOX/96U3o8avH8BzwUaB45ibJ0Ya9GI5wM1n2B0FVOw3Fb5zSi8E=""
            ],
            ""x5t"": ""yRlOEAqBN6pE5np2eXCUwQ8UO6k"",
            ""x5t#S256"": ""i_BlyziljtIqQ6VCm6KIAd1dBSMCyfdQs4aOnrXQ_w8""
        }
    ]
}";
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
            RoleClaimType = "realm_access/roles" // Убедитесь, что это соответствует вашему Keycloak токену
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
                    var roles = context.Principal.FindAll("realm_access/roles").Select(c => c.Value).ToArray();
                    claimsIdentity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));
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