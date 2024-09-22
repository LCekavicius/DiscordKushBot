using KushBot;
using KushBot.Resources.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<DiscordBotService>();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

builder.Services.AddDbContext<SqliteDbContext>();

builder.Services.AddQuartzInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
