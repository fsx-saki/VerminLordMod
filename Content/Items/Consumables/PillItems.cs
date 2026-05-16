using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Consumables
{
    public class HealingPill : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 17; Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(silver: 10);
            Item.healLife = 50;
        }
    }

    public class QiRecoveryPill : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 17; Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(silver: 15);
        }
    }

    public class DetoxPill : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 17; Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 20);
        }
    }

    public class PerceptionPill : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 17; Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 25);
        }
    }

    public class DefensePill : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 17; Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 20);
        }
    }

    public class StrengthPill : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 17; Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 20);
        }
    }

    public class SpeedPill : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 17; Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 20);
        }
    }

    public class VisionPill : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 17; Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(silver: 30);
        }
    }

    public class AwakeningPill : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20; Item.height = 20; Item.maxStack = 99;
            Item.useTurn = true; Item.autoReuse = true;
            Item.useAnimation = 17; Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 1);
        }
    }
}