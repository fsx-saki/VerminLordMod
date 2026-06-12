using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.NPCs.Boss;

namespace VerminLordMod.Content.Items.Consumables
{
    public class WolfKingSummon : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20;
            Item.maxStack = 20;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.LightRed;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.consumable = true;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<ElectricWolfKing>()) && player.ZoneSnow;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                NPC.NewNPC(null, (int)player.Center.X + 200, (int)player.Center.Y - 300, ModContent.NPCType<ElectricWolfKing>());
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.Center);
            }
            return true;
        }
    }

    public class TianHeSummon : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20;
            Item.maxStack = 20;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.LightRed;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.consumable = true;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<TianHeShangRenBoss>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 400, ModContent.NPCType<TianHeShangRenBoss>());
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.Center);
            }
            return true;
        }
    }

    public class DiMaiSummon : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20;
            Item.maxStack = 20;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Orange;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.consumable = true;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<DiMaiGuardian>()) && player.ZoneUnderworldHeight;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 100, ModContent.NPCType<DiMaiGuardian>());
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Roar, player.Center);
            }
            return true;
        }
    }

    public class LongGongSummon : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20;
            Item.maxStack = 20;
            Item.value = Item.buyPrice(gold: 30);
            Item.rare = ItemRarityID.LightPurple;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.consumable = true;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<LongGongBoss>()) && Main.dayTime;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 500, ModContent.NPCType<LongGongBoss>());
                Terraria.Audio.SoundEngine.PlaySound(SoundID.ForceRoarPitched, player.Center);
            }
            return true;
        }
    }
}
