using StardewModdingAPI;

namespace SMAPIHealthBarMod
{
    /// <summary>The mod configuration model.</summary>
    public class HealthBarConfig : Config
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The color scheme to apply.</summary>
        public int ColourScheme { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the default config values.</summary>
        public override T GenerateDefaultConfig<T>()
        {
            ColourScheme = 0;
            return this as T;
        }
    }
}
