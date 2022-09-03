using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Rest;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using GalacticNova.Preconditions;
using System.Net;
using System.Security.Cryptography;

namespace GalacticNova.Modules.Standard
{
    public class Standard : MinitoriModule
    {
        private CommandService commands;
        private IServiceProvider services;
        private Config config;
        private DiscordRestClient restClient;

        public Standard(CommandService _commands, IServiceProvider _services, Config _config, DiscordRestClient _restClient)
        {
            commands = _commands;
            services = _services;
            config = _config;
            restClient = _restClient;
        }

        [Command("help")]
        public async Task HelpCommand()
        {
            Context.IsHelp = true;

            StringBuilder output = new StringBuilder();
            StringBuilder module = new StringBuilder();
            var SeenModules = new List<string>();
            int i = 0;

            output.Append("These are the commands you can use:");

            foreach (var c in commands.Commands)
            {
                if (!SeenModules.Contains(c.Module.Name))
                {
                    if (i > 0)
                        output.Append(module.ToString());

                    module.Clear();

                    module.Append($"\n**{c.Module.Name}:**");
                    SeenModules.Add(c.Module.Name);
                    i = 0;
                }

                if ((await c.CheckPreconditionsAsync(Context, services)).IsSuccess)
                {
                    if (i == 0)
                        module.Append(" ");
                    else
                        module.Append(", ");

                    i++;

                    module.Append($"`{c.Name}`");
                }
            }

            if (i > 0)
                output.AppendLine(module.ToString());

            await RespondAsync(output.ToString());
        }

        [Command("addnewline")]
        [Priority(1000)]
        [Hide]
        [RequireOwner]
        public async Task AddNewLine()
        {
            if (Context.Guild.Id != config.HomeGuildId)
                return;

            var channel = Context.Guild.GetChannel(995622112079392849) as SocketTextChannel;
            var emoteServer = await Context.Client.GetGuild(783783142737182720).GetEmotesAsync();

            await channel.SendMessageAsync("CLICK A REACTION TO GET A ROLE. ->");

            var msg1 = await channel.SendMessageAsync("<@&996331740605984778> <@&996331634276188200> <@&996331821333745766> <@&996331869694078997>", allowedMentions: null);

            var msg2 = await channel.SendMessageAsync("<@&996331907748991046> <@&996331946084941834> <@&996331996034900029> <@&996332079539298386>", allowedMentions: null);

            await Task.Delay(2000);

            await msg1.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "Grey"));
            await msg1.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "Black"));
            await msg1.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "Cyan"));
            await msg1.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "Blue"));

            await msg2.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "Red"));
            await msg2.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "Yellow"));
            await msg2.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "Green"));
            await msg2.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "Orange"));

            Task.Run(async () =>
            {
                await Task.Delay(1000 * 60 * 11);

                var msg3 = await channel.SendMessageAsync("PRONOUN ROLES ALSO AVAILABLE, IF YOU WISH. ->\n" +
                    "<:friendheartred:997557216917868614> - SHE / HER\n" +
                    "<:friendheartteal:997557217911914586> - HE / HIM\n" +
                    "<:friendheartorange:997557216225796106> - THEY / THEM\n" +
                    "<:friendheartgrey:997557214401265695> - OTHER / NO PREFERENCE");

                await Task.Delay(2000);

                await msg3.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "friendheartred"));
                await msg3.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "friendheartteal"));
                await msg3.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "friendheartorange"));
                await msg3.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "friendheartgrey"));
            });
        }

        [Command("editroles")]
        [Hide]
        [RequireOwner]
        public async Task EditStuff()
        {
            var guild = await restClient.GetGuildAsync(config.HomeGuildId);
            var channel = await guild.GetChannelAsync(995622112079392849) as RestTextChannel;
            var emoteGuild = await Context.Client.GetGuild(783783142737182720).GetEmotesAsync();

            //var msg1 = await channel.GetMessageAsync(997566857122947172) as RestUserMessage;
            //var msg2 = await channel.GetMessageAsync(997566857810813008) as RestUserMessage;
            var msg3 = await channel.GetMessageAsync(997569672864088196) as RestUserMessage;



            //await msg2.ModifyAsync(x => { x.Content = "<@&996331907748991046> <@&997813700654288957> <@&996331946084941834> <@&996331996034900029> <@&996332079539298386>"; x.AllowedMentions = null; });

            //await msg2.AddReactionAsync(emoteGuild.Emotes.FirstOrDefault(x => x.Name == "Pink"));

            //var channel = Context.Guild.GetChannel(995622112079392849) as SocketTextChannel;
            //var emoteServer = await Context.Client.GetGuild(783783142737182720).GetEmotesAsync();

            //var msg1 = await channel.GetMessageAsync(997566857122947172) as SocketUserMessage;
            //var msg2 = await channel.GetMessageAsync(997566857810813008) as SocketUserMessage;

            //await Task.Delay(3000);

            ////await msg1.ModifyAsync(x => { x.Content = "<@&996331740605984778> <@&996331634276188200> <@&996331821333745766> <@&996331869694078997>"; x.AllowedMentions = null; });
            //await msg2.ModifyAsync(x => { x.Content = "<@&996331907748991046> <@&997813700654288957> <@&996331946084941834> <@&996331996034900029> <@&996332079539298386>"; x.AllowedMentions = null; });

            //await msg2.AddReactionAsync(emoteServer.FirstOrDefault(x => x.Name == "Pink"));

            await msg3.ModifyAsync(x => x.Content = "PRONOUN ROLES ALSO AVAILABLE, IF YOU WISH. ->\n" +
                    "<:friendheartred:997557216917868614> - SHE / HER\n" +
                    "<:friendheartteal:997557217911914586> - HE / HIM\n" +
                    "<:friendheartorange:997557216225796106> - THEY / THEM\n" +
                    "<:friendheartbrown:1010719561101881384> - IT / IT'S\n" +
                    "<:friendheartgrey:997557214401265695> - OTHER / NO PREFERENCE");

            await msg3.AddReactionAsync(emoteGuild.FirstOrDefault(x => x.Name == "friendheartbrown"));
        }

        [Command("ping")]
        [Summary("Pong!")]
        [Priority(1000)]
        public async Task Blah()
        {
            await RespondAsync($"Pong {Context.User.Mention}!");
        }

        [Command("quit", RunMode = RunMode.Async)]
        [Hide]
        [Priority(1000)]
        public async Task ShutDown()
        {
            if (!config.OwnerIds.Contains(Context.User.Id))
            {
                await RespondAsync(":no_good::skin-tone-3: You don't have permission to run this command!");
                return;
            }

            await RespondAsync("Disconnecting...");
            await config.Save();
            await Context.Client.LogoutAsync();
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.ExitCode.Success);
        }

        [Command("restart", RunMode = RunMode.Async)]
        [Hide]
        [Priority(1000)]
        public async Task Restart()
        {
            if (!config.OwnerIds.Contains(Context.User.Id))
            {
                await RespondAsync(":no_good::skin-tone-3: You don't have permission to run this command!");
                return;
            }

            await RespondAsync("Restarting...");
            await config.Save();
            await File.WriteAllTextAsync("./update", Context.Channel.Id.ToString());

            await Context.Client.LogoutAsync();
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.ExitCode.Restart);
        }

        [Command("update", RunMode = RunMode.Async)]
        [Hide]
        [Priority(1000)]
        public async Task UpdateAndRestart()
        {
            if (!config.OwnerIds.Contains(Context.User.Id))
            {
                await RespondAsync(":no_good::skin-tone-3: You don't have permission to run this command!");
                return;
            }

            await File.WriteAllTextAsync("./update", Context.Channel.Id.ToString());

            await RespondAsync("Pulling latest code and rebuilding from source, I'll be back in a bit.");
            await config.Save();
            await Context.Client.LogoutAsync();
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.ExitCode.RestartAndUpdate);
        }

        [Command("deadlocksim", RunMode = RunMode.Async)]
        [Hide]
        [Priority(1000)]
        public async Task DeadlockSimulation()
        {
            if (!config.OwnerIds.Contains(Context.User.Id))
            {
                await RespondAsync(":no_good::skin-tone-3: You don't have permission to run this command!");
                return;
            }

            File.Create("./deadlock");

            await RespondAsync("Restarting...");
            await config.Save();
            await Context.Client.LogoutAsync();
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.ExitCode.DeadlockEscape);
        }
    }
}
