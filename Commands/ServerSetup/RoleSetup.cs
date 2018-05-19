using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using PassiveBOT.Configuration;
using PassiveBOT.Preconditions;
using Sparrow.Platform.Posix.macOS;

namespace PassiveBOT.Commands.ServerSetup
{
    public class RoleSetup : ModuleBase
    {
        [RequireAdmin]
        [Command("ToggleColourRoles")]
        [Summary("ToggleColourRoles")]
        [Remarks("Toggle the color roles commands")]
        public async Task Toggle()
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            guild.RoleConfigurations.ColorRoleList.AllowCustomColorRoles = !guild.RoleConfigurations.ColorRoleList.AllowCustomColorRoles;
            GuildConfig.SaveServer(guild);
            await ReplyAsync($"Color Roles Enabled: {guild.RoleConfigurations.ColorRoleList.AllowCustomColorRoles}");
        }
        [RequireAdmin]
        [Command("ClearColors")]
        [Summary("ClearColors")]
        [Remarks("clear all color roles from the server")]
        public async Task ClearColors()
        {
            var roles = Context.Guild.Roles.Where(x => x.Name.StartsWith("#")).ToList();
            foreach (var role in roles)
            {
                await  role.DeleteAsync();
            }
            await ReplyAsync($"#Roles removed: {roles.Count}");
        }

        [Command("GetColor")]
        [Summary("GetColor <color>")]
        [Remarks("gives the user a role with the specified color")]
        public async Task JoinC(string color)
        {
            var guild = GuildConfig.GetServer(Context.Guild);
            if (guild.RoleConfigurations.ColorRoleList.AllowCustomColorRoles)
            {
                CustomColor DCol;
                var Hexed = false;
                if (color.StartsWith("#"))
                {
                    color = color.Replace("#", "");
                    Hexed = true;
                }
                if (Enum.TryParse(color, out GuildConfig.roleConfigurations.ColorRoles.Colours ECol) && !Hexed)
                {
                    DCol = getCol(ECol);
                }
                else
                {
                    DCol = getCol(color);
                }
                var colorrole = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, $"#{DCol.ColorNameStripped.ToString()}", StringComparison.CurrentCultureIgnoreCase));
                if (colorrole == null)
                {
                    var newrole = await Context.Guild.CreateRoleAsync($"#{DCol.ColorNameStripped.ToLower()}");
                    var position = (Context.Guild as SocketGuild).Roles.Where(r => r.Members.Select(m => m.Id).Contains(Context.Client.CurrentUser.Id)).Max(x => x.Position);
                    await newrole.ModifyAsync(x => x.Position = position - 1);
                    await newrole.ModifyAsync(x => x.Color = DCol.Color);
                    colorrole = newrole;
                }

                var croles = (Context.User as SocketGuildUser).Roles.Where(x => x.Name.StartsWith("#")).ToList();
                if (croles.Any())
                {
                    await (Context.User as SocketGuildUser).RemoveRolesAsync(croles);
                }
                await (Context.User as IGuildUser).AddRoleAsync(colorrole);
                await ReplyAsync("", false, new EmbedBuilder
                {
                    Description = $"Success, you have been given the role {colorrole.Mention}",
                    Color = DCol.Color
                });
                
            }
            else
            {
                //Not Enabled
                await ReplyAsync("Color Error, Not enabled by administrator");
            }
        }

        public CustomColor getCol(GuildConfig.roleConfigurations.ColorRoles.Colours Color)
        {
            Color DCol;
            switch (Color)
            {
                case GuildConfig.roleConfigurations.ColorRoles.Colours.yellow:
                    DCol = new Color(255, 255, 0);
                    break;
                case GuildConfig.roleConfigurations.ColorRoles.Colours.blue:
                    DCol = Discord.Color.Blue;
                    break;
                case GuildConfig.roleConfigurations.ColorRoles.Colours.brown:
                    DCol = new Color(139, 69, 19);
                    break;
                case GuildConfig.roleConfigurations.ColorRoles.Colours.red:
                    DCol = Discord.Color.Red;
                    break;
                case GuildConfig.roleConfigurations.ColorRoles.Colours.purple:
                    DCol = Discord.Color.Purple;
                    break;
                case GuildConfig.roleConfigurations.ColorRoles.Colours.cyan:
                    DCol = new Color(0, 255, 255);
                    break;
                case GuildConfig.roleConfigurations.ColorRoles.Colours.green:
                    DCol = Discord.Color.Green;
                    break;
                case GuildConfig.roleConfigurations.ColorRoles.Colours.orange:
                    DCol = Discord.Color.Orange;
                    break;
                case GuildConfig.roleConfigurations.ColorRoles.Colours.pink:
                    DCol = new Color(255, 105, 180);
                    break;
                    default:
                        throw new InvalidOperationException("Invlid COlor Input");
            }
            return new CustomColor
            {
                Color = DCol,
                ColorNameStripped = Color.ToString()
            };

        }

        public CustomColor getCol(string Color)
        {
            Color = Color.Replace("#", "");
            if (Color.Length != 6)
            {
                throw new InvalidOperationException("Color Length must be 6 characters (not including the # out the front), ie. #FFFFFF");
            }

            try
            {
                var rgb = System.Drawing.Color.FromArgb(int.Parse(Color , System.Globalization.NumberStyles.AllowHexSpecifier));
                var discordcolor = new Color(rgb.R, rgb.G, rgb.B);
                return new CustomColor
                {
                    Color = discordcolor,
                    ColorNameStripped = Color
                };

            }
            catch
            {
                throw new InvalidOperationException("Invalid Color Conversion Please ensure you input a valid hex color, ie. #FFFFFF");
            }
        }

        public class CustomColor
        {
            public Color Color { get; set; }
            public string ColorNameStripped { get; set; }
        }

    }
}
