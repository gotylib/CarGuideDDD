using CarGuideDDD.MailService.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(49152);
//    options.ListenAnyIP(49153, listenOptions =>
//    {
//        listenOptions.UseHttps();
//    });
//});

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

// Get the lifetime and server
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var server = app.Services.GetRequiredService<IServer>();

lifetime.ApplicationStarted.Register(() =>
{
    var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
    if (addresses != null && addresses.Any())
    {
        Console.WriteLine("Application started on the following URLs:");
        foreach (var address in addresses)
        {
            Console.WriteLine(address);
        }
    }
    else
    {
        Console.WriteLine("No addresses found.");
    }
});

await app.RunAsync();
