using AutoMapper;
using Ahm.DiscordBot.Services;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using NLog.Web;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Ahm.DiscordBot
{
    class Program
    {
        private IConfiguration _configuration;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private Microsoft.Extensions.Logging.ILogger _logger;
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {
            string localPath = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                localPath = Environment.CurrentDirectory + "\\";
            }
            var jsonPath = localPath + "appsettings.json";
            _configuration = new ConfigurationBuilder()
                .AddJsonFile(jsonPath)
                .Build();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                ExclusiveBulkDelete = false
            });

            _client.Log += LogAsync;

            _commands = new CommandService();

            // Configuring services for DI
            ConfigureServices();

            // Configuring log path
            ConfigureLogging();

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, _configuration["Discord:ApiToken"]);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private void ConfigureLogging()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                GlobalDiagnosticsContext.Set("logPath", _configuration["SavedLogPaths:Windows"]);
            else
                GlobalDiagnosticsContext.Set("logPath", _configuration["SavedLogPaths:Linux"]);

            var nlogLoggerProvider = new NLogLoggerProvider();

            _logger = nlogLoggerProvider.CreateLogger(typeof(Program).FullName);
        }

        private void ConfigureServices()
        {
            var mapperConfig = new MapperConfiguration(mapConfig =>
            {
                mapConfig.AddProfile(new MappingProfile());
            });
            IMapper mapper = mapperConfig.CreateMapper();

            _services = new ServiceCollection()
                .AddSingleton(mapper)
                .AddSingleton(_commands)
                .AddSingleton(_configuration)
                .AddSingleton<IDestinyService, DestinyService>()
                .AddSingleton<IAccountConnectionsService, AccountConnectionsService>()
                .AddSingleton<IDestinyManifestService, DestinyManifestService>()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                    builder.AddNLogWeb("nlog.config");
                })
                .BuildServiceProvider();
        }

        private Task LogAsync(LogMessage arg)
        {
            _logger.LogInformation(arg.ToString());
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            _logger.LogInformation(message.ToString());
            int argPos = 0;
            if (message.HasStringPrefix("~", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                {
                    var errorResponse = await context.Channel.SendMessageAsync(result.ErrorReason);
                    _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(task => errorResponse.DeleteAsync());
                }
            }
        }
    }
}
