/*
    Copyright 2016 Maurício Gomes (Speeder)

    Storm is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Speeder's SDV Mods are distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Speeder's SDV Mods.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace SMAPIHealthBarMod
{
    /// <summary>The mod entry point.</summary>
    public class HealthBarMod : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>A blank pixel for drawing.</summary>
        private Texture2D Pixel;

        /// <summary>The mod settings.</summary>
        private HealthBarConfig Config;

        /// <summary>The available color schemes.</summary>
        private readonly Color[][] ColorSchemes =
        {
            new[] { Color.LawnGreen, Color.YellowGreen, Color.Gold, Color.DarkOrange, Color.Crimson },
            new[] { Color.Crimson, Color.DarkOrange, Color.Gold, Color.YellowGreen, Color.LawnGreen },
        };


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<HealthBarConfig>();
            if (this.Config.ColourScheme < 0)
                this.Config.ColourScheme = 0;
            if (this.Config.ColourScheme >= this.ColorSchemes.Length)
                this.Config.ColourScheme = this.ColorSchemes.Length - 1;

            GraphicsEvents.OnPostRenderEvent += this.GraphicsEvents_DrawTick;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the game is drawing to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GraphicsEvents_DrawTick(object sender, EventArgs e)
        {
            if (!Game1.hasLoadedGame || Game1.currentMinigame != null || Game1.activeClickableMenu != null)
                return;
            if (this.Pixel == null)
                this.Pixel = this.GetPixel();

            foreach (Monster monster in Game1.currentLocation.characters.OfType<Monster>())
            {
                // skip if not visible
                if (monster.isInvisible || !Utility.isOnScreen(monster.position, 3 * Game1.tileSize))
                    continue;

                // get monster data
                int health = monster.health;
                int maxHealth = Math.Max(monster.maxHealth, monster.health);
                int numberKilled = Game1.stats.specificMonstersKilled.ContainsKey(monster.name)
                    ? Game1.stats.specificMonstersKilled[monster.name]
                    : 0;
                string label = "???";

                // get bar data
                Color barColor;
                float barLengthPercent;
                if (numberKilled + Game1.player.combatLevel > 15)
                {
                    float monsterHealthPercent = health / (float)maxHealth;
                    barLengthPercent = 1f;
                    if (monsterHealthPercent > 0.9f)
                        barColor = this.ColorSchemes[this.Config.ColourScheme][0];
                    else if (monsterHealthPercent > 0.65f)
                        barColor = this.ColorSchemes[this.Config.ColourScheme][1];
                    else if (monsterHealthPercent > 0.35f)
                        barColor = this.ColorSchemes[this.Config.ColourScheme][2];
                    else if (monsterHealthPercent > 0.15f)
                        barColor = this.ColorSchemes[this.Config.ColourScheme][3];
                    else
                        barColor = this.ColorSchemes[this.Config.ColourScheme][4];

                    if (numberKilled + Game1.player.combatLevel * 4 > 45)
                    {
                        barLengthPercent = monsterHealthPercent;
                        label = monster.health > 999
                            ? "!!!"
                            : $"{monster.health:000}";
                    }
                }
                else
                {
                    barLengthPercent = 1f;
                    barColor = Color.DarkSlateGray;
                }

                // get monster position
                Vector2 monsterLocalPosition = monster.getLocalPosition(Game1.viewport);
                Rectangle monsterBox = new Rectangle((int)monsterLocalPosition.X, (int)monsterLocalPosition.Y - monster.sprite.spriteHeight / 2 * Game1.pixelZoom, monster.sprite.spriteWidth * Game1.pixelZoom, 12);
                if (monster is GreenSlime slime)
                {
                    if (slime.hasSpecialItem)
                    {
                        monsterBox.X -= 5;
                        monsterBox.Width += 10;
                    }
                    else if (slime.cute)
                    {
                        monsterBox.X -= 2;
                        monsterBox.Width += 4;
                    }
                    else
                        monsterBox.Y += 5 * Game1.pixelZoom;
                }
                else if (monster is RockCrab || monster is LavaCrab)
                {
                    if (monster.sprite.CurrentFrame % 4 == 0)
                        continue;
                }
                else if (monster is RockGolem)
                {
                    if (monster.health == monster.maxHealth)
                        continue;
                    monsterBox.Y = (int)monsterLocalPosition.Y - monster.sprite.spriteHeight * Game1.pixelZoom * 3 / 4;
                }
                else if (monster is Bug bug)
                {
                    if (bug.isArmoredBug)
                        continue;
                    monsterBox.Y -= 15 * Game1.pixelZoom;
                }
                else if (monster is Grub)
                {
                    if (monster.sprite.CurrentFrame == 19)
                        continue;
                    monsterBox.Y = (int)monsterLocalPosition.Y - monster.sprite.spriteHeight * Game1.pixelZoom * 4 / 7;
                }
                else if (monster is Fly)
                    monsterBox.Y = (int)monsterLocalPosition.Y - monster.sprite.spriteHeight * Game1.pixelZoom * 5 / 7;
                else if (monster is DustSpirit)
                {
                    monsterBox.X += 3;
                    monsterBox.Width -= 6;
                    monsterBox.Y += 5 * Game1.pixelZoom;
                }
                else if (monster is Bat)
                {
                    if (monster.sprite.CurrentFrame == 4)
                        continue;
                    monsterBox.X -= 1;
                    monsterBox.Width -= 2;
                    monsterBox.Y += 1 * Game1.pixelZoom;
                }
                else if (monster is MetalHead || monster is Mummy)
                    monsterBox.Y -= 2 * Game1.pixelZoom;
                else if (monster is Skeleton || monster is ShadowBrute || monster is ShadowShaman || monster is SquidKid)
                {
                    if (monster.health == monster.maxHealth)
                        continue;
                    monsterBox.Y -= 7 * Game1.pixelZoom;
                }

                // get health bar position
                Rectangle healthBox = monsterBox;
                ++healthBox.X;
                ++healthBox.Y;
                healthBox.Height = monsterBox.Height - 2;
                healthBox.Width = monsterBox.Width - 2;

                // draw health bar
                Game1.spriteBatch.Draw(this.Pixel, monsterBox, Color.BurlyWood);
                Game1.spriteBatch.Draw(this.Pixel, healthBox, Color.SaddleBrown);
                healthBox.Width = (int)(healthBox.Width * barLengthPercent);
                Game1.spriteBatch.Draw(this.Pixel, healthBox, barColor);

                // draw label
                Color textColor = barColor == Color.DarkSlateGray || barLengthPercent < 0.35f ? Color.AntiqueWhite : Color.DarkSlateGray;
                Utility.drawTextWithShadow(Game1.spriteBatch, label, Game1.smallFont, new Vector2(monsterBox.X + (float)monsterBox.Width / 2 - 9 * Game1.options.zoomLevel, monsterBox.Y + 2), textColor, Game1.options.zoomLevel * 0.4f, -1, 0, 0, 0, 0);
            }
        }

        /// <summary>Get a blank pixel.</summary>
        private Texture2D GetPixel()
        {
            Texture2D pixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            return pixel;
        }
    }
}
