using CarGuideDDD.MailService.Services;
using CarGuideDDD.MailService.Services.Interfaces;
using Confluent.Kafka;
using MailKit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IMailServices, MailServices>();


builder.Services.AddHostedService((IServiceProvider provider) =>
{
    var config = builder.Configuration.GetSection("Kafka").Get<ConsumerConfig>() ??
                 throw new Exception("No kafka consumer config section: 'Kafka'.");
    var consumer = new ConsumerBuilder<int, string>(config).Build();
    var topic = builder.Configuration.GetValue<string>("LISTENING_TOPIC") ??
                throw new Exception("No listening kafka topic: 'LISTENING_TOPIC'.");
    var logger = provider.GetRequiredService<ILogger<ConsumerHostedService>>();
    ConsumerHostedService backbroundService = new
    (
        consumer: consumer,
        topic: topic,
        logger: logger,
        mailServices: IMailServices
    );

    return backbroundService;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
