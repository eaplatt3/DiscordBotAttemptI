using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBotAttemptI
{
    public class Program
    {
       public static void Main(string[] args)
        
            //Used to establish an async context
           => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;

        public async Task MainAsync()                                    
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            var token = File.ReadAllText("token.txt");
            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            //Block this task until the program is closed
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public class CommandHandler
        {
            private readonly DiscordSocketClient _client;
            private readonly CommandService _commands;

            // Retrieve client and CommandService instance via ctor
            public CommandHandler(DiscordSocketClient client, CommandService commands)
            {
                _commands = commands;
                _client = client;
            }

            public async Task InstallCommandsAsync()
            {
                // Hook the MessageReceived event into our command handler
                _client.MessageReceived += HandleCommandAsync;

                // Here we discover all of the command modules in the entry 
                // assembly and load them. Starting from Discord.NET 2.0, a
                // service provider is required to be passed into the
                // module registration method to inject the 
                // required dependencies.
                //
                // If you do not use Dependency Injection, pass null.
                // See Dependency Injection guide for more information.
                await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
            }

            private async Task HandleCommandAsync(SocketMessage messageParam)
            {
                // Don't process the command if it was a system message
                var message = messageParam as SocketUserMessage;
                if (message == null) return;

                // Create a number to track where the prefix ends and the command begins

                int argPos = 0;

                // Determine if the message is a command based on the prefix and make sure no bots trigger commands
                if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                    message.Author.IsBot) return;

                // Create a WebSocket-based command context based on the message
                var context = new SocketCommandContext(_client, message);

                // Execute the command with the command context we just
                // created, along with the service provider for precondition checks.
                await _commands.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: null);
            }
        }

        public class InfoModule : ModuleBase<SocketCommandContext>
        {
            [Command("say")]
            [Summary("Echoes a message.")]
            public Task SayAsync([Remainder][Summary("the text to echo")] string echo) => ReplyAsync(echo);
        }

        [Group("sample")]
        public class SampleModule : ModuleBase<SocketCommandContext>
        {
            [Command("square")]
            [Summary("Squares a number.")]
            public async Task SquareAsync([Summary("the nuber to square.")] int num)
            {
                await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
            }
        }




    }
}
