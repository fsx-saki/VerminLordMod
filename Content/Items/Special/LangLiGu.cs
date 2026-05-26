using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转力道辅助蛊", "一转", "力")]
    public class LangLiGu : ModItem
    {
        private const int QiCostPerUse = 8;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 99;
            Item.value = 20000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            var langLi = player.GetModPlayer<LangLiPlayer>();
            langLi.MeleeDamageBonus += 0.02f;

            CombatText.NewText(player.Hitbox, new Color(255, 140, 50), "+2% 近战伤害", true);

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, 174);
                d.velocity *= 0.5f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "LangLiEffect", "永久增加2%近战伤害"));
            tooltips.Add(new TooltipLine(Mod, "LangLiQiCost", $"消耗真元：{QiCostPerUse}"));
            tooltips.Add(new TooltipLine(Mod, "LangLiConsumable", "一次性消耗品"));
        }
    }

    public class LangLiPlayer : ModPlayer
    {
        public float MeleeDamageBonus { get; set; }

        public override void ResetEffects()
        {
        }

        public override void UpdateEquips()
        {
            Player.GetDamage(DamageClass.Melee) += MeleeDamageBonus;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["MeleeDamageBonus"] = MeleeDamageBonus;
        }

        public override void LoadData(TagCompound tag)
        {
            MeleeDamageBonus = tag.GetFloat("MeleeDamageBonus");
        }
    }
}
