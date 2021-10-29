using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ahm.DiscordBot.Services;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Ahm.DiscordBot.Modules.RoleModules
{
    public class TitleRoleModule : ModuleBase<SocketCommandContext>
    {
        private readonly Dictionary<string, ulong> _roleNames;
        private Dictionary<string, List<string>> _titleHashes;
        private readonly IDestinyService _destinyService;

        private readonly ILogger<TitleRoleModule> _logger;

        public TitleRoleModule(IDestinyService destinyService, ILogger<TitleRoleModule> logger)
        {
            _destinyService = destinyService;
            _roleNames = new Dictionary<string, ulong>();
            _titleHashes = _destinyService.GetTitleHashsFromManifest();

            _logger = logger;
        }

        
        [Command("TitleAdd")]
        [Alias("Title Add", "title add")]
        [Summary("Checks if the user has the title unlocked in Destiny 2 and gives the role associated with it.")]
        public async Task TitleAdd([Remainder] string titleWanted)
        {
            // TODO: send response for if title does not exist (spelling error)
            LoadRoles();
            SocketGuildUser user = (SocketGuildUser)Context.User;
            if (_destinyService.HasTitle(titleWanted, user.Id))
            {
                if (_roleNames.TryGetValue(titleWanted.ToLower(), out var titleToGive))
                    await user.AddRoleAsync(titleToGive);
            }
            else
            {
                var noTitleMessage = await Context.Channel.SendMessageAsync(string.Format("{0} not unlocked for {1}", titleWanted, user.Mention));
                _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(task => noTitleMessage.DeleteAsync());
            }
            _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(task => Context.Message.DeleteAsync());
        }

        [Command("TitleRemove")]
        [Alias("Title Remove", "title remove")]
        [Summary("Removes the title role from the user.")]
        public async Task TitleRemove([Remainder] string titleWanted)
        {
            LoadRoles();
            SocketGuildUser user = (SocketGuildUser)Context.User;
            if (_roleNames.TryGetValue(titleWanted.ToLower(), out var titleToRemove))
                await user.RemoveRoleAsync(titleToRemove);
            else
            {
                var noTitleMessage = await Context.Channel.SendMessageAsync(string.Format("{0} does not exist", titleWanted));
                _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(task => noTitleMessage.DeleteAsync());
            }
            _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(task => Context.Message.DeleteAsync());

        }

        /*
         * Currently Discord's API doesn't batch add/remove roles and using
         * AddRoles/RemoveRoles can result in rate limited.
         * TODO: Allow commands when a work around is made or an api update is made.
         */

        //[Command("TitleAddAll")]
        //[Alias("add all titles", "Title Add All")]
        //[Summary("Checks the users progress on every title and gives roles for the titles unlocked.")]
        //public async Task TitleAddAll()
        //{
        //    LoadRoles();
        //    SocketGuildUser user = (SocketGuildUser)Context.User;
        //    var titlesUserHas = _destinyService.GetUserTitles(user.Id);
        //    List<ulong> rolesUserHas = GetTitleUlongs(titlesUserHas);
        //    await user.AddRolesAsync(rolesUserHas);
        //}

        //[Command("TitleRemoveAll")]
        //[Alias("remove all titles", "Title Remove All")]
        //[Summary("Removes all title roles from the user.")]
        //public async Task TitleRemoveAll()
        //{
        //    LoadRoles();
        //    SocketGuildUser user = (SocketGuildUser)Context.User;
        //    var rolesToRemove = _roleNames.Select(x => x.Value).ToList();
        //    await user.RemoveRolesAsync(rolesToRemove);
        //}
       
        public void LoadRoles()
        {
            var guild = Context.Guild;
            var roles = guild.Roles;
            List<string> roleNames = new List<string>();
            foreach (var key in _titleHashes.Keys)
            {
                roleNames.Add(key.ToLower());
            }
            foreach (var role in roles)
            {
                if (roleNames.Contains(role.Name.ToLower()))
                    _roleNames.Add(role.Name.ToLower(), role.Id);
            }
        }

        public List<ulong> GetTitleUlongs(List<string> userTitleHashes)
        {
            List<ulong> titleNames = new List<ulong>();
            foreach (var userTitleHash in userTitleHashes)
            {
                foreach (var title in _titleHashes)
                {
                    if (title.Value.Contains(userTitleHash))
                    {
                        titleNames.Add(_roleNames[title.Key.ToLower()]);
                    }
                }
            }

            return titleNames;
        }
    }
}
