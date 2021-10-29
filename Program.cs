using AutoMapper;
using Ahm.DiscordBot.Services;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Commands;
using System;
using NLog.Web;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Ahm.DiscordBot.EventHandlers;
using Ahm.DiscordBot.TypeReaders;

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

            HandleReactionAddedAsync();

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
                .AddSingleton<IFileIOService, FileIOService>()
                .AddSingleton<IDestinyService, DestinyService>()
                .AddSingleton<IAccountConnectionService, AccountConnectionService>()
                .AddSingleton<IDestinyManifestService, DestinyManifestService>()
                .AddSingleton<IRoleReactionService, RoleReactionService>()
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
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _commands.AddTypeReader(typeof(Emote), new EmoteTypeReader());
            _commands.AddTypeReader(typeof(Emoji), new EmojiTypeReader());
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            int argPos = 0;
            if (message.HasStringPrefix("Ahm, ", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                {
                    // When there is an error processing a command a DM will be sent
                    // to the bot owner.
                    // TODO: Make this toggleable 
                    // TODO: differentiate between user error and code error. 
                    //      Send user error to channel, code error in DM.
                    _logger.LogInformation(result.ErrorReason);
                    var applicationInfo = await _client.GetApplicationInfoAsync();
                    var botOwner = applicationInfo.Owner;
                    await botOwner.SendMessageAsync(result.ErrorReason);
                }
            }
        }

        private void HandleReactionAddedAsync()
        {
            new ReactionAddedHandler(_client, _services.GetService<IRoleReactionService>());
            new ReactionRemovedHandler(_client, _services.GetService<IRoleReactionService>());
        }
    }
}
