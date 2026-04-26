using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.Items.Debuggers
{
    /// <summary>
    /// 开发者调试道具 - 清空玩家恶名和所有势力声望
    /// </summary>
    class ReputationReset : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Master;
            Item.maxStack = 1;
            Item.value = 100;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Guitar;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item1;
        }

        public override bool? UseItem(Player player)
        {
            var worldPlayer = player.GetModPlayer<GuWorldPlayer>();
            if (worldPlayer == null) return true;

            // 清空恶名和声望
            worldPlayer.InfamyPoints = 0;
            worldPlayer.FamePoints = 0;

            // 清空所有势力声望
            foreach (var fid in System.Enum.GetValues<FactionID>())
            {
                if (fid == FactionID.None || fid == FactionID.Scattered) continue;

                if (worldPlayer.FactionRelations.TryGetValue(fid, out var rel))
                {
                    rel.ReputationPoints = 0;
                    rel.HasBounty = false;
                    rel.IsAllied = false;
                }
            }

            // 清空通缉
            worldPlayer.ActiveBounties.Clear();
            worldPlayer.CurrentAlly = FactionID.None;

            Main.NewText("已清空所有恶名值和势力声望！", Color.Green);
            return true;
        }
    }
}
