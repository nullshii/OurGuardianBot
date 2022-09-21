﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OurGuardian.Services;
using Serilog;
using Victoria;

internal class Program
{
    private readonly IServiceProvider _services;
    private readonly DiscordSocketConfig _socketConfig;
    private readonly IConfiguration _configuration;

    public Program()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _socketConfig = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
            AlwaysDownloadUsers = true,
        };

        _services = new ServiceCollection()
            .AddSingleton(_configuration)
            .AddLogging(builder => builder.AddSerilog(dispose: true))
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<LavaNode>()
            .AddSingleton<LavaConfig>()
            .AddSingleton<DiscordSocketClientHandler>()
            .AddSingleton<AudioHandler>()
            .AddSingleton<InteractionHandler>()
            .BuildServiceProvider();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File($"Logs/{DateTime.Now:yyyy-MM-dd}/{DateTime.Now:HH-mm-ss}.txt")
            .WriteTo.Console()
            .CreateLogger();
    }

    private static void Main()
        => new Program().RunAsync().GetAwaiter().GetResult();

    private async Task RunAsync()
    {
        await _services.GetRequiredService<DiscordSocketClientHandler>().InitializeAsync();
        await _services.GetRequiredService<AudioHandler>().InitializeAsync();
        await _services.GetRequiredService<InteractionHandler>().InitializeAsync();
        await Task.Delay(Timeout.Infinite);
    }

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}