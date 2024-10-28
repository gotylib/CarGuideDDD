using CarGuideDDD.MailService.Services;
using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHostedService((IServiceProvider provider) =>
{
    ConsumerConfig config = builder.Configuration.GetSection("Kafka").Get<ConsumerConfig>() ??
                            throw new Exception("No kafka consumer config section: 'Kafka'.");
    IConsumer<Null, string> consumer = new ConsumerBuilder<Null, string>(config).Build();
    string topic = builder.Configuration.GetValue<string>("LISTENING_TOPIC") ??
                    throw new Exception("No listening kafka topic: 'LISTENING_TOPIC'.");
    ILogger<ConsumerHostedService> logger = provider.GetRequiredService<ILogger<ConsumerHostedService>>();
    ConsumerHostedService backbroundService = new
    (
      consumer: consumer,
      topic: topic,
      logger: logger
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

app.Run();
