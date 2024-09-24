using Discord.Commands;
using Discord.WebSocket;
using KushBot;
using KushBot.Resources.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<DiscordBotService>();

//builder.Services.AddSingleton(new DiscordSocketClient());
builder.Services.AddSingleton(new CommandService());

builder.Services.AddDbContext<SqliteDbContext>();

builder.Services.AddQuartzInfrastructure(builder.Configuration);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

var host = builder.Build();
host.Run();
