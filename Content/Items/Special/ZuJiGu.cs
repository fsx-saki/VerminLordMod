using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.Buffs.AddToEnemy;
using VerminLordMod.Content.Items.Weapons.Daos;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转情报道一次性消耗蛊，揭示附近NPC足迹", "二转", "律")]
    public class ZuJiGu : InfoWeapon
    {
        protected override int _guLevel => 2;
        protected override int qiCost => 15;
        protected override int controlQiCost => 15;
        protected override int _useTime => 30;
        protected override int _useStyle => ItemUseStyleID.HoldUp;
        protected override bool needCtrl => false;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 30;
            Item.value = 5000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item4;
            Item.consumable = true;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool? UseItem(Player player)
        {
            base.UseItem(player);

            for (int i = 0; i < 40; i++)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-100, 100), Main.rand.NextFloat(-100, 100));
                var d = Dust.NewDustDirect(player.Center + offset, 0, 0, DustID.GreenFairy);
                d.velocity = new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-3, -0.5f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1f, 1.5f);
            }

            int buffType = ModContent.BuffType<ZuJiGubuff>();
            float range = 1000f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.Distance(player.Center) <= range)
                {
                    npc.AddBuff(buffType, 600);
                }
            }

            return true;
        }

        public override bool AltFunctionUse(Player player)
        {
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ZuJiGuEffect", "揭示附近NPC的足迹，使其显形10秒"));
            tooltips.Add(new TooltipLine(Mod, "ZuJiGuConsumable", "[c/ff6600:一次性消耗蛊]"));
        }
    }
}
