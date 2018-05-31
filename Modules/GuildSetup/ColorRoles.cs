using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PassiveBOT.Discord.Context;
using PassiveBOT.Discord.Preconditions;

namespace PassiveBOT.Modules.GuildSetup
{
    public class ColorRoles : Base
    {
        public enum Colours
        {
            blue,
            green,
            red,
            purple,
            yellow,
            cyan,
            pink,
            orange,
            brown
        }

        [RequireAdmin]
        [Command("ToggleColorRoles")]
        [Alias("ToggleColourRoles")]
        [Summary("ToggleColourRoles")]
        [Remarks("Toggle the color roles commands")]
        public async Task Toggle()
        {
            Context.Server.Settings.ColorRoles.Enabled = !Context.Server.Settings.ColorRoles.Enabled;
            Context.Server.Save();
            await SimpleEmbedAsync($"Color Roles Enabled: {Context.Server.Settings.ColorRoles.Enabled}");
        }

        [RequireAdmin]
        [Command("ClearColors")]
        [Summary("ClearColors")]
        [Alias("ClearColours")]
        [Remarks("clear all color roles from the server")]
        public async Task ClearColors()
        {
            var roles = Context.Guild.Roles.Where(x => x.Name.StartsWith("#")).ToList();
            foreach (var role in roles)
            {
                await role.DeleteAsync();
            }

            await SimpleEmbedAsync($"#Roles removed: {roles.Count}");
        }

        [Command("GetColor")]
        [Summary("GetColor <color>")]
        [Alias("GetColour")]
        [Remarks("gives the user a role with the specified color")]
        public async Task JoinC(string color)
        {
            if (Context.Server.Settings.ColorRoles.Enabled)
            {
                CustomColor DCol;
                var Hexed = false;
                if (color.StartsWith("#"))
                {
                    color = color.Replace("#", "");
                    Hexed = true;
                }

                if (Enum.TryParse(color, out Colours ECol) && !Hexed)
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
                await ReplyAsync(new EmbedBuilder
                {
                    Description = $"Success, you have been given the role {colorrole.Mention}",
                    Color = DCol.Color
                });
            }
            else
            {
                //Not Enabled
                await SimpleEmbedAsync("Color Error, Not enabled by administrator");
            }
        }

        public CustomColor getCol(Colours cColor)
        {
            Color DCol;
            switch (cColor)
            {
                case Colours.yellow:
                    DCol = new Color(255, 255, 0);
                    break;
                case Colours.blue:
                    DCol = Color.Blue;
                    break;
                case Colours.brown:
                    DCol = new Color(139, 69, 19);
                    break;
                case Colours.red:
                    DCol = Color.Red;
                    break;
                case Colours.purple:
                    DCol = Color.Purple;
                    break;
                case Colours.cyan:
                    DCol = new Color(0, 255, 255);
                    break;
                case Colours.green:
                    DCol = Color.Green;
                    break;
                case Colours.orange:
                    DCol = Color.Orange;
                    break;
                case Colours.pink:
                    DCol = new Color(255, 105, 180);
                    break;
                default:
                    throw new Exception("Invlid Color Input");
            }

            return new CustomColor
            {
                Color = DCol,
                ColorNameStripped = cColor.ToString()
            };
        }

        public CustomColor getCol(string Color)
        {
            Color = Color.Replace("#", "");
            if (Color.Length != 6)
            {
                throw new Exception("Color Length must be 6 characters (not including the # out the front), ie. #FFFFFF");
            }

            try
            {
                var rgb = System.Drawing.Color.FromArgb(int.Parse(Color, NumberStyles.AllowHexSpecifier));
                var discordcolor = new Color(rgb.R, rgb.G, rgb.B);
                return new CustomColor
                {
                    Color = discordcolor,
                    ColorNameStripped = Color
                };
            }
            catch
            {
                throw new Exception("Invalid Color Conversion Please ensure you input a valid hex color, ie. #FFFFFF");
            }
        }

        public class CustomColor
        {
            public Color Color { get; set; }
            public string ColorNameStripped { get; set; }
        }
    }
}