using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using KushBot;
using KushBot.Resources.Database;
using KushBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<DiscordBotService>();

//builder.Services.AddSingleton(new DiscordSocketClient());
builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    LogLevel = LogSeverity.Info,
    GatewayIntents = GatewayIntents.All
}));

builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));

builder.Services.AddDbContext<SqliteDbContext>();

builder.Services.AddSingleton<QuestRequirementFactory>();
builder.Services.AddTransient<PortraitManager>();
builder.Services.AddSingleton<VendorService>();
builder.Services.AddSingleton<TutorialManager>();
builder.Services.AddSingleton<BossService>();
builder.Services.AddSingleton<MessageHandler>();

builder.Services.AddQuartzInfrastructure(builder.Configuration);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

var host = builder.Build();
host.Run();
