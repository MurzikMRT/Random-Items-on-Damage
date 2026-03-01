using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace DamageItem
{
    public class StatsCommand : ModCommand
    {
        public override string Command => "stats"; // Сама команда
        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args) {
            var player = caller.Player.GetModPlayer<RandomOnHitPlayer>();
            bool isRussian = Language.ActiveCulture.Name == "ru-RU";
            
            string message = isRussian ? 
                $"Статистика {caller.Player.name}: получено {player.totalItemsReceived} предметов!" : 
                $"Stats for {caller.Player.name}: received {player.totalItemsReceived} items!";
                
            caller.Reply(message, Color.Yellow);
        }
    }
}
