using API.CustomMiddleware;
using CarGuideDDD.Core.DtObjects;
using CarGuideDDD.Core.EntityObjects;
using CarGuideDDD.Core.Token;
using CarGuideDDD.Infrastructure.Data;
using CarGuideDDD.Infrastructure.Repositories;
using CarGuideDDD.Infrastructure.Repositories.Interfaces;
using CarGuideDDD.Infrastructure.Services;
using CarGuideDDD.Infrastructure.Services.Interfaces;
using CarGuideDDD.Infrastructure.Services.Producers;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using NLog.Web;
using static CarGuideDDD.Infrastructure.Services.Interfaces.ICarServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IStatisticsService, StatisticService>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();

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

builder.Services.AddSingleton((IServiceProvider provider) =>
{
    var config = builder.Configuration.GetSection("Kafka").Get<ProducerConfig>() ??
                 throw new Exception("No kafka producer config section: 'Kafka'.");
    var producer = new ProducerBuilder<int, string>(config).Build();
    var topic = builder.Configuration.GetValue<string>("PUBLISHING_TOPIC") ??
                throw new Exception("No publishing kafka topic: 'PUBLISHING_TOPIC'.");
    
    using var adminClient = new AdminClientBuilder(new AdminClientConfig
    {
        BootstrapServers = config.BootstrapServers,
    }).Build();
    var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(10));
    if (metadata.Topics.Count == 0)
    {
            
        const int numPartitions = 2;
        const int replicationFactor = 1; 

        var topicSpecification = new TopicSpecification
        {
            Name = topic,
            NumPartitions = numPartitions,
            ReplicationFactor = replicationFactor
        };

        adminClient.CreateTopicsAsync(new[] { topicSpecification }).GetAwaiter().GetResult();
    }

    var logger = provider.GetRequiredService<ILogger<KafkaMessageProducer>>();
    KafkaMessageProducer messageProducer = new
    (
        producer: producer,
        topic: topic,
        logger: logger
    );

    return messageProducer;
});

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