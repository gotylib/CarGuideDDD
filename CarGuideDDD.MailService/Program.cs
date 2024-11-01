using CarGuideDDD.MailService.Services;
using CarGuideDDD.MailService.Services.KafkaAdmin;
using CarGuideDDD.MailService.Services.Producers;
using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton((IServiceProvider provider) =>
{
    var config = builder.Configuration.GetSection("KafkaRederect").Get<ProducerConfig>() ??
                 throw new Exception("No kafka producer config section: 'Kafka'.");
    var topic = builder.Configuration.GetValue<string>("PUBLISHING_TOPIC_2") ??
                throw new Exception("No publishing kafka topic: 'PUBLISHING_TOPIC'.");
    KafkaAdmin kafkaAdmin = new
    (
        config: config,
        topic: topic
    );
    return kafkaAdmin;
});

builder.Services.AddHostedService((IServiceProvider provider) =>
{
    var config = builder.Configuration.GetSection("Kafka").Get<ConsumerConfig>() ??throw new Exception("No kafka consumer config section: 'Kafka'.");
    var consumer = new ConsumerBuilder<int, string>(config).Build();
    var topic = builder.Configuration.GetValue<string>("LISTENING_TOPIC") ?? throw new Exception("No listening kafka topic: 'LISTENING_TOPIC'.");
    var logger = provider.GetRequiredService<ILogger<ConsumerHostedService>>();
    var mailService = new MailServices();
    var massageProducer = provider.GetRequiredService<KafkaMessageProducer>();
    var rederectMessageProducer = provider.GetRequiredService<RederectMessageProducer>();
    var kafkaAdmin = provider.GetRequiredService<KafkaAdmin>();
    ConsumerHostedService backbroundService = new
    (
        consumer: consumer,
        topic: topic,
        logger: logger,
        mailServices: mailService,
        kafkaMessageProducer: massageProducer,
        rederectMessageProducer: rederectMessageProducer,
        kafkaAdmin: kafkaAdmin
    );

    return backbroundService;
});


builder.Services.AddSingleton((IServiceProvider provider) =>
{
    var config = builder.Configuration.GetSection("KafkaProduce").Get<ProducerConfig>() ??
                 throw new Exception("No kafka producer config section: 'Kafka'.");
    var producer = new ProducerBuilder<int, string>(config).Build();
    var topic = builder.Configuration.GetValue<string>("PUBLISHING_TOPIC") ??
                throw new Exception("No publishing kafka topic: 'PUBLISHING_TOPIC'.");
    var logger = provider.GetRequiredService<ILogger<KafkaMessageProducer>>();
    KafkaMessageProducer messageProducer = new
    (
        producer: producer,
        topic: topic,
        logger: logger,
        partition: 2
    );

    return messageProducer;
});

builder.Services.AddSingleton((IServiceProvider provider) =>
{
    var config = builder.Configuration.GetSection("KafkaRederect").Get<ProducerConfig>() ??
                 throw new Exception("No kafka producer config section: 'Kafka'.");
    var producer = new ProducerBuilder<int, string>(config).Build();
    var topic = builder.Configuration.GetValue<string>("PUBLISHING_TOPIC_2") ??
                throw new Exception("No publishing kafka topic: 'PUBLISHING_TOPIC'.");
    var logger = provider.GetRequiredService<ILogger<ProducerConfig>>();
    RederectMessageProducer messageProducer = new
    (
        producer: producer,
        topic: topic,
        logger: logger
    );

    return messageProducer;
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
