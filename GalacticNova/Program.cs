using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Rest;
using Discord.Commands;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace GalacticNova
{
    class Program
    {
        static void Main(string[] args) =>
            new Program().RunAsync().GetAwaiter().GetResult();

        private DiscordSocketClient socketClient;
        private DiscordRestClient restClient;
        private Config config;
        private CommandHandler handler;
        private Dictionary<string, ulong> RoleColors = new Dictionary<string, ulong>();
        private ulong updateChannel = 0;

        private async Task RunAsync()
        {
            socketClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100,
                GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.Guilds | GatewayIntents.GuildMessageReactions
            });
            socketClient.Log += Log;

            restClient = new DiscordRestClient(new DiscordRestConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            restClient.Log += Log;

            if (File.Exists("./update"))
            {
                var temp = File.ReadAllText("./update");
                ulong.TryParse(temp, out updateChannel);
                File.Delete("./update");
                Console.WriteLine($"Found an update file! It contained [{temp}] and we got [{updateChannel}] from it!");
            }

            config = await Config.Load();

            var map = new ServiceCollection().AddSingleton(socketClient).AddSingleton(config).AddSingleton(restClient).BuildServiceProvider();

            await socketClient.LoginAsync(TokenType.Bot, config.Token);
            await socketClient.StartAsync();

            await restClient.LoginAsync(TokenType.Bot, config.Token);

            if (File.Exists("./deadlock"))
            {
                Console.WriteLine("We're recovering from a deadlock.");
                File.Delete("./deadlock");
                foreach (var u in config.OwnerIds)
                {
                    (await restClient.GetUserAsync(u))?
                        .SendMessageAsync($"I recovered from a deadlock.\n`{DateTime.Now.ToShortDateString()}` `{DateTime.Now.ToLongTimeString()}`");
                }
            }

            socketClient.GuildAvailable += Client_GuildAvailable;
            socketClient.Disconnected += SocketClient_Disconnected;

            RoleColors = JsonStorage.DeserializeObjectFromFile<Dictionary<string, ulong>>("colors.json");

            handler = new CommandHandler();
            await handler.Install(map);

            try
            {
                socketClient.ReactionAdded += Client_ReactionAdded;
                socketClient.ReactionRemoved += Client_ReactionRemoved;

                await socketClient.SetStatusAsync(UserStatus.Idle);
                await socketClient.SetGameAsync("READY ->");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Source}\n{ex.Message}\n{ex.StackTrace}");
            }

            await Task.Delay(-1);
        }

        private async Task Client_GuildAvailable(SocketGuild guild)
        {
            if (updateChannel != 0 && guild.GetTextChannel(updateChannel) != null)
            {
                await Task.Delay(3000); // wait 3 seconds just to ensure we can actually send it. this might not do anything.
                await guild.GetTextChannel(updateChannel).SendMessageAsync("Successfully reconnected.");
                updateChannel = 0;
            }

            if (guild.Id != config.HomeGuildId && guild.Id != 783783142737182720)
            {
                await guild.LeaveAsync();
            }
        }

        private async Task SocketClient_Disconnected(Exception ex)
        {
            // If we disconnect, wait 3 minutes and see if we regained the connection.
            // If we did, great, exit out and continue. If not, check again 3 minutes later
            // just to be safe, and restart to exit a deadlock.
            var task = Task.Run(async () =>
            {
                for (int i = 0; i < 2; i++)
                {
                    await Task.Delay(1000 * 60 * 3);

                    if (socketClient.ConnectionState == ConnectionState.Connected)
                        break;
                    else if (i == 1)
                    {
                        File.Create("./deadlock");
                        await config.Save();
                        Environment.Exit((int)ExitCodes.ExitCode.DeadlockEscape);
                    }
                }
            });
        }

        private async Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> messageCache, Cacheable<IMessageChannel, ulong> channelCache, SocketReaction reaction)
        {
            if (reaction.Channel.Id == 995622112079392849 && RoleColors.ContainsKey(reaction.Emote.Name))
            {
                var user = ((SocketGuildUser)reaction.User);

                if (user.Roles.Contains(user.Guild.GetRole(RoleColors[reaction.Emote.Name])))
                {
                    var restUser = await restClient.GetGuildUserAsync(user.Guild.Id, user.Id);
                    var roles = restUser.RoleIds.ToList();

                    roles = roles.Where(x => !RoleColors.ContainsValue(x) && x != restUser.GuildId).ToList();
                    roles.Remove(RoleColors[reaction.Emote.Name]);

                    await restUser.ModifyAsync(x => x.RoleIds = roles);
                }
            }
            else if (reaction.Channel.Id == 995622112079392849)
            {
                SocketRole role = null;
                var user = ((SocketGuildUser)reaction.User);

                switch (reaction.Emote.Name)
                {
                    case "friendheartred":
                        role = user.Guild.GetRole(996710668440977509);
                        break;
                    case "friendheartteal":
                        role = user.Guild.GetRole(996710629710770196);
                        break;
                    case "friendheartorange":
                        role = user.Guild.GetRole(996710695531986984);
                        break;
                    case "friendheartgrey":
                        role = user.Guild.GetRole(997570032248832132);
                        break;
                    case "friendheartbrown":
                        role = user.Guild.GetRole(997950291989250119);
                        break;
                    case "🔐":
                        role = user.Guild.GetRole(1029279897505501246);
                        break;
                    case "🏘":
                        role = user.Guild.GetRole(1051161366058246174);
                        break;
                }

                if (role == null)
                    return;

                if (user.Roles.Contains(role))
                    await user.RemoveRoleAsync(role);
            }
        }

        private async Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channelCache, SocketReaction reaction)
        {
            if (reaction.Channel.Id == 995622112079392849 && RoleColors.ContainsKey(reaction.Emote.Name))
            {
                var user = ((SocketGuildUser)reaction.User);

                var restUser = await restClient.GetGuildUserAsync(user.Guild.Id, user.Id);
                var roles = restUser.RoleIds.ToList();

                roles = roles.Where(x => !RoleColors.ContainsValue(x) && x != restUser.GuildId).ToList();
                roles.Add(RoleColors[reaction.Emote.Name]);

                await restUser.ModifyAsync(x => x.RoleIds = roles);
            }
            else if (reaction.Channel.Id == 995622112079392849)
            {
                SocketRole role = null;
                var user = ((SocketGuildUser)reaction.User);

                switch (reaction.Emote.Name)
                {
                    case "friendheartred":
                        role = user.Guild.GetRole(996710668440977509);
                        break;
                    case "friendheartteal":
                        role = user.Guild.GetRole(996710629710770196);
                        break;
                    case "friendheartorange":
                        role = user.Guild.GetRole(996710695531986984);
                        break;
                    case "friendheartgrey":
                        role = user.Guild.GetRole(997570032248832132);
                        break;
                    case "friendheartbrown":
                        role = user.Guild.GetRole(997950291989250119);
                        break;
                    case "🔐":
                        role = user.Guild.GetRole(1029279897505501246);
                        break;
                    case "🏘":
                        role = user.Guild.GetRole(1051161366058246174);
                        break;
                }

                if (role == null)
                    return;

                if (!user.Roles.Contains(role))
                    await user.AddRoleAsync(role);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
