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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace SMAPIHealthBarMod
{
    /// <summary>The mod entry point.</summary>
    public class SMAPIHealthBarModMainClass : Mod
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
        public override void Entry(params object[] objects)
        {
            this.Config = new HealthBarConfig().InitializeConfig(BaseConfigPath);
            if (this.Config.ColourScheme < 0)
                this.Config.ColourScheme = 0;
            if (this.Config.ColourScheme >= this.ColorSchemes.Length)
                this.Config.ColourScheme = this.ColorSchemes.Length - 1;

            this.Pixel = this.GetPixel();
            GraphicsEvents.DrawTick += this.GraphicsEvents_DrawTick;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the game is drawing to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GraphicsEvents_DrawTick(object sender, EventArgs e)
        {
            if (Game1.currentLocation == null || Game1.gameMode == 11 || Game1.currentMinigame != null || Game1.showingEndOfNightStuff || Game1.gameMode == 6 || Game1.gameMode == 0 || Game1.menuUp || Game1.activeClickableMenu != null) return;

            Game1.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            foreach (NPC character in Game1.currentLocation.characters)
            {
                if (character is Monster monster)
                {
                    if (!monster.isInvisible && Utility.isOnScreen(monster.position, 3 * Game1.tileSize))
                    {
                        if (monster.health > monster.maxHealth) monster.maxHealth = monster.health;

                        var monsterKilledAmount = Game1.stats.specificMonstersKilled.ContainsKey(monster.name)
                            ? Game1.stats.specificMonstersKilled[monster.name]
                            : 0;

                        string healthText = "???";
                        Color barColor;
                        float barLengthPercent;
                        if (monsterKilledAmount + Game1.player.combatLevel > 15)
                        {
                            //basic stats
                            float monsterHealthPercent = monster.health / (float)monster.maxHealth;
                            barLengthPercent = 1f;
                            if (monsterHealthPercent > 0.9f) barColor = this.ColorSchemes[Config.ColourScheme][0];
                            else if (monsterHealthPercent > 0.65f) barColor = this.ColorSchemes[Config.ColourScheme][1];
                            else if (monsterHealthPercent > 0.35f) barColor = this.ColorSchemes[Config.ColourScheme][2];
                            else if (monsterHealthPercent > 0.15f) barColor = this.ColorSchemes[Config.ColourScheme][3];
                            else barColor = this.ColorSchemes[Config.ColourScheme][4];

                            if (monsterKilledAmount + Game1.player.combatLevel * 4 > 45)
                            {
                                barLengthPercent = monsterHealthPercent;
                                if (monster.health > 999) healthText = "!!!";
                                else healthText = $"{monster.health:000}";
                            }
                        }
                        else
                        {
                            barLengthPercent = 1f;
                            barColor = Color.DarkSlateGray;
                        }

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
                        monsterBox.X = (int)(monsterBox.X * Game1.options.zoomLevel);
                        monsterBox.Y = (int)(monsterBox.Y * Game1.options.zoomLevel);
                        monsterBox.Width = (int)(monsterBox.Width * Game1.options.zoomLevel);
                        monsterBox.Height = (int)(monsterBox.Height * Game1.options.zoomLevel);
                        Rectangle lifeBox = monsterBox;
                        ++lifeBox.X;
                        ++lifeBox.Y;
                        lifeBox.Height = monsterBox.Height - 2;
                        lifeBox.Width = monsterBox.Width - 2;
                        Game1.spriteBatch.Draw(this.Pixel, monsterBox, Color.BurlyWood);
                        Game1.spriteBatch.Draw(this.Pixel, lifeBox, Color.SaddleBrown);
                        lifeBox.Width = (int)(lifeBox.Width * barLengthPercent);
                        Game1.spriteBatch.Draw(this.Pixel, lifeBox, barColor);

                        Color textColor = barColor == Color.DarkSlateGray || barLengthPercent < 0.35f ? Color.AntiqueWhite : Color.DarkSlateGray;
                        Utility.drawTextWithShadow(Game1.spriteBatch, healthText, Game1.smallFont, new Vector2(monsterBox.X + (float)monsterBox.Width / 2 - 9 * Game1.options.zoomLevel, monsterBox.Y + 2), textColor, Game1.options.zoomLevel * 0.4f, -1, 0, 0, 0, 0);
                    }
                }
            }

            Game1.spriteBatch.End();
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
