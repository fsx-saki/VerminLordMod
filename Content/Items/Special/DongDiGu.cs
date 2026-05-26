using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;
using VerminLordMod.Content;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转泥道功能蛊", "三转", "泥")]
    public class DongDiGu : ModItem
    {
        private const int QiCostPerUse = 20;
        private const float MaxTeleportDistance = 500f;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 10000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
                return false;

            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 mouseWorld = Main.MouseWorld;
                float dist = Vector2.Distance(player.Center, mouseWorld);
                if (dist > MaxTeleportDistance)
                {
                    Text.ShowTextRed(player, "目标距离过远，洞地蛊无法到达");
                    return false;
                }

                if (player.position.Y < Main.worldSurface * 16f)
                {
                    Text.ShowTextRed(player, "必须在地下或靠近地面才能使用洞地蛊");
                    return false;
                }
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            Vector2 mouseWorld = Main.MouseWorld;
            Vector2 targetPos = mouseWorld - new Vector2(player.width / 2f, player.height / 2f);

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Dirt, 0f, 0f);
                dust.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
            }

            player.Teleport(targetPos, 1);
            player.velocity = Vector2.Zero;

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Dirt, 0f, 0f);
                dust.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
            }

            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, targetPos.X, targetPos.Y, 1);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DongDiEffect", "遁地传送至鼠标位置（最远500像素）"));
            tooltips.Add(new TooltipLine(Mod, "DongDiCondition", "必须在地下或靠近地面使用"));
            tooltips.Add(new TooltipLine(Mod, "DongDiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
