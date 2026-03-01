using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;

namespace DamageItem
{
    public class RandomOnHitPlayer : ModPlayer
    {
        private int lastHealth;
        public int totalItemsReceived = 0;
        private int spawnTimer = 0; 
        private int globalCooldown = 0; 

        public override void Initialize() {
            lastHealth = Player.statLife;
            totalItemsReceived = 0;
            spawnTimer = 120;
            globalCooldown = 0;
        }

        public override void OnEnterWorld() {
            lastHealth = Player.statLife;
            spawnTimer = 120;
            globalCooldown = 0;

            var config = ModContent.GetInstance<RandomConfig>();
            if (config.ShowWelcomeMessage) {
                bool isRussian = Language.ActiveCulture.Name == "ru-RU";
                string rainbowNick = "[c/FF0000:M][c/FF7F00:u][c/FFFF00:r][c/00FF00:Z][c/00FFFF:i][c/0000FF:k][c/8B00FF:k]";
                string welcome = isRussian ? $"Автор мода: {rainbowNick}. Удачи в выживании!" : $"Mod Author: {rainbowNick}. Good luck!";
                Main.NewText(welcome, Color.White);
            }
        }

        public override void PostUpdateRunSpeeds() {
            var config = ModContent.GetInstance<RandomConfig>();
            if (config.SpeedBoost) {
                Player.maxRunSpeed *= 1.5f;
                Player.runAcceleration *= 1.5f; 
            }
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
            var config = ModContent.GetInstance<RandomConfig>();
            if (config.UltraHardcore) {
                bool isRussian = Language.ActiveCulture.Name == "ru-RU";
                Main.NewText(isRussian ? "УЛЬТРАХАРДКОР: Вещи потеряны!" : "ULTRAHARDCORE: Items lost!", Color.Red);
                for (int i = 0; i < 58; i++) {
                    Item invItem = Player.inventory[i];
                    if (invItem.type > 0 && !invItem.favorited) {
                        Player.QuickSpawnItem(Player.GetSource_Death(), invItem.type, invItem.stack);
                        invItem.TurnToAir();
                    }
                }
            }
        }

        public override void PostUpdate() {
            if (spawnTimer > 0) { spawnTimer--; lastHealth = Player.statLife; return; }
            if (globalCooldown > 0) globalCooldown--;

            var config = ModContent.GetInstance<RandomConfig>();
            if (config.EveryTickDamage && Player.statLife < lastHealth && !Player.dead && globalCooldown <= 0) {
                GiveRandomItem();
                globalCooldown = 15; 
            }
            lastHealth = Player.statLife;
        }

        public override void OnHurt(Player.HurtInfo info) {
            if (spawnTimer > 0 || globalCooldown > 0) return;
            var config = ModContent.GetInstance<RandomConfig>();
            if (!config.EveryTickDamage) { GiveRandomItem(); globalCooldown = 25; }
            
            if (config.GravityChaos && Main.rand.NextBool(33)) { Player.gravDir *= -1f; }
            if (config.ProjectileCounter && Main.rand.NextBool(10)) SpawnProjectiles();
            if (config.BossSpawnChallenge && Main.rand.NextBool(200)) SpawnRandomBoss();
            
            // Если включен UltraHardcore, события срабатывают чаще и они жестче
            if (config.EnableEvents && Main.rand.NextBool(config.UltraHardcore ? 25 : 50)) {
                TriggerRandomEvent(config.UltraHardcore);
            }
        }

        private void TriggerRandomEvent(bool isUltra) {
            bool isRussian = Language.ActiveCulture.Name == "ru-RU";
            // Если UltraHardcore включен, выбираем из 10 событий, иначе только из первых 4
            int eventType = Main.rand.Next(isUltra ? 10 : 4);

            switch (eventType) {
                case 0: Main.NewText("DISCO!", Color.Magenta); break;
                case 1: Player.AddBuff(BuffID.Blackout, 600); break;
                case 2: Player.velocity.Y -= 12f; break;
                case 3: Main.dayTime = !Main.dayTime; break;
                
                // ЖЕСТКИЕ СОБЫТИЯ (только при UltraHardcore)
                case 4: 
                    for (int i = 0; i < 9; i++) {
                        int target = Main.rand.Next(10);
                        Item temp = Player.inventory[i];
                        Player.inventory[i] = Player.inventory[target];
                        Player.inventory[target] = temp;
                    }
                    Main.NewText(isRussian ? "ИНВЕНТАРЬ ПЕРЕМЕШАН!" : "INVENTORY SHUFFLE!", Color.Orange);
                    break;
                case 5: Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ProjectileID.Bomb, 100, 10f, Player.whoAmI); break;
                case 6: Player.AddBuff(BuffID.Confused, 420); break;
                case 7: Player.AddBuff(BuffID.OnFire, 300); break;
                case 8: Player.AddBuff(BuffID.Frozen, 60); break;
                case 9: 
                    Player.velocity.Y -= 35f; 
                    Main.NewText(isRussian ? "ПРИЯТНОГО ПОЛЕТА!" : "HAVE A NICE FLIGHT!", Color.Cyan);
                    break;
            }
        }

        private void GiveRandomItem() {
            var config = ModContent.GetInstance<RandomConfig>();
            bool isJackpot = config.EnableJackpots && Main.rand.NextBool(100);
            int count = isJackpot ? Main.rand.Next(20, 51) : 1;
            bool isRussian = Language.ActiveCulture.Name == "ru-RU";

            if (isJackpot) {
                Main.NewText(isRussian ? "!!! ДЖЕКПОТ !!!" : "!!! JACKPOT !!!", Color.Gold);
                SoundEngine.PlaySound(SoundID.Coins, Player.position);
            } 
            else if (config.PlaySoundOnLoot) {
                if (config.UseCustomSound) SoundEngine.PlaySound(new SoundStyle("DamageItem/Sounds/CustomLootSound"), Player.position);
                else SoundEngine.PlaySound(SoundID.Grab, Player.position);
            }

            for (int i = 0; i < count; i++) {
                int id = GetRandomItemID(config);
                if (id > 0) {
                    if (!(config.AutoDeleteOnFull && IsInventoryFull())) {
                        Item sample = ContentSamples.ItemsByType[id];
                        if (sample.rare >= ItemRarityID.Expert && config.VisualEffects) SpawnLegendaryEffects();

                        if (!isJackpot) {
                            string action = isRussian ? "получил" : "received";
                            Main.NewText($"{Player.name} {action} {Lang.GetItemNameValue(id)}!", Color.Cyan);
                        }
                        Player.QuickSpawnItem(Player.GetSource_FromThis(), id);
                        totalItemsReceived++;
                    }
                }
            }
        }

        private void SpawnLegendaryEffects() {
            SoundEngine.PlaySound(SoundID.Item4, Player.position);
            for (int i = 0; i < 20; i++) {
                Dust d = Dust.NewDustPerfect(Player.Center, DustID.RainbowRod, Main.rand.NextVector2Circular(6f, 6f), 0, Color.White, 1.2f);
                d.noGravity = true;
            }
        }

        private int GetRandomItemID(RandomConfig config) {
            int max = config.IncludeModdedItems ? ItemLoader.ItemCount : ItemID.Count;
            int rolls = config.LuckMultiplier; 
            int bestID = 0;
            int highestRarity = -11;
            for (int i = 0; i < rolls; i++) {
                int currentID = Main.rand.Next(1, max);
                Item testItem = ContentSamples.ItemsByType[currentID];
                if (testItem.type > 0 && testItem.rare >= highestRarity) { highestRarity = testItem.rare; bestID = currentID; }
            }
            return bestID == 0 ? ItemID.DirtBlock : bestID;
        }

        private void SpawnProjectiles() {
            for (int i = 0; i < 8; i++) {
                Vector2 vel = new Vector2(0, 7).RotatedBy(MathHelper.ToRadians(i * 45));
                Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, vel, Main.rand.Next(1, ProjectileID.Count), 20, 2f, Player.whoAmI);
            }
        }

        private void SpawnRandomBoss() {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            NPC.SpawnOnPlayer(Player.whoAmI, Main.hardMode ? NPCID.TheDestroyer : NPCID.KingSlime);
        }

        private bool IsInventoryFull() {
            for (int i = 0; i < 50; i++) if (Player.inventory[i].IsAir) return false;
            return true;
        }
    }
}
