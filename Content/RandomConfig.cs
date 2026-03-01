using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace DamageItem
{
    public class RandomConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("Items")]
        [DefaultValue(true)]
        public bool IncludeModdedItems;

        [DefaultValue(false)]
        public bool UltraHardcore; // ВКЛЮЧАЕТ И ДРОП, И ЖЕСТКИЕ СОБЫТИЯ

        [DefaultValue(1)]
        [Range(1, 100)]
        [Slider]
        public int LuckMultiplier;

        [Header("Challenges")]
        [DefaultValue(false)]
        public bool EveryTickDamage;

        [DefaultValue(true)]
        public bool BossSpawnChallenge;

        [DefaultValue(true)]
        public bool EnableEvents;

        [DefaultValue(true)]
        public bool GravityChaos;

        [DefaultValue(true)]
        public bool ProjectileCounter;

        [Header("Buffs")]
        [DefaultValue(true)]
        public bool SpeedBoost;

        [Header("Effects")]
        [DefaultValue(false)]
        public bool AutoDeleteOnFull;

        [DefaultValue(true)]
        public bool EnableJackpots;

        [DefaultValue(true)]
        public bool VisualEffects;

        [DefaultValue(true)]
        public bool ShowCounter;

        [DefaultValue(true)]
        public bool PlaySoundOnLoot;

        [DefaultValue(false)]
        public bool UseCustomSound;

        [Header("Other")]
        [DefaultValue(true)]
        public bool ShowWelcomeMessage; 
    }
}
