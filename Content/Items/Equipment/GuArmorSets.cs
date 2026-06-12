using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Equipment
{
    // ==================== 一转蛊师套装 ====================
    [AutoloadEquip(EquipType.Head)]
    public class GuMasterHat : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24; Item.height = 24;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.White;
            Item.defense = 2;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.02f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
            => head.type == Type && body.type == ModContent.ItemType<GuMasterRobe>() && legs.type == ModContent.ItemType<GuMasterPants>();
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "蛊师入门：+5真元上限，+3%移动速度";
            player.statLifeMax2 += 5;
            player.moveSpeed += 0.03f;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class GuMasterRobe : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30; Item.height = 20;
            Item.value = Item.buyPrice(silver: 60);
            Item.rare = ItemRarityID.White;
            Item.defense = 3;
        }
        public override void UpdateEquip(Player player)
        {
            player.statDefense += 1;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class GuMasterPants : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22; Item.height = 18;
            Item.value = Item.buyPrice(silver: 40);
            Item.rare = ItemRarityID.White;
            Item.defense = 2;
        }
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.02f;
        }
    }

    // ==================== 二转蛊师套装 ====================
    [AutoloadEquip(EquipType.Head)]
    public class GuAdeptHood : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24; Item.height = 24;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 4;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.05f;
            player.statLifeMax2 += 5;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
            => head.type == Type && body.type == ModContent.ItemType<GuAdeptArmor>() && legs.type == ModContent.ItemType<GuAdeptBoots>();
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "蛊师精进：+15真元上限，+5%暴击率";
            player.statLifeMax2 += 15;
            player.GetCritChance(DamageClass.Generic) += 5;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class GuAdeptArmor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30; Item.height = 20;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 6;
        }
        public override void UpdateEquip(Player player)
        {
            player.statDefense += 2;
            player.endurance += 0.03f;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class GuAdeptBoots : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22; Item.height = 18;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 4;
        }
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.04f;
        }
    }

    // ==================== 三转蛊师套装 ====================
    [AutoloadEquip(EquipType.Head)]
    public class GuExpertCrown : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24; Item.height = 24;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Green;
            Item.defense = 7;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.08f;
            player.statLifeMax2 += 10;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
            => head.type == Type && body.type == ModContent.ItemType<GuExpertArmor>() && legs.type == ModContent.ItemType<GuExpertLeggings>();
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "蛊师大成：+30真元上限，+8%暴击率，+5%伤害减免";
            player.statLifeMax2 += 30;
            player.GetCritChance(DamageClass.Generic) += 8;
            player.endurance += 0.05f;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class GuExpertArmor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30; Item.height = 20;
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Green;
            Item.defense = 10;
        }
        public override void UpdateEquip(Player player)
        {
            player.statDefense += 3;
            player.endurance += 0.05f;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class GuExpertLeggings : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22; Item.height = 18;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Green;
            Item.defense = 7;
        }
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.06f;
            player.statLifeMax2 += 5;
        }
    }

    // ==================== 四转蛊师套装 ====================
    [AutoloadEquip(EquipType.Head)]
    public class GuMasterWarHelm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24; Item.height = 24;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.LightRed;
            Item.defense = 12;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.12f;
            player.statLifeMax2 += 20;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
            => head.type == Type && body.type == ModContent.ItemType<GuMasterWarPlate>() && legs.type == ModContent.ItemType<GuMasterWarGreaves>();
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "蛊师战意：+50真元上限，+12%暴击率，+8%伤害减免\n击杀敌人恢复5真元";
            player.statLifeMax2 += 50;
            player.GetCritChance(DamageClass.Generic) += 12;
            player.endurance += 0.08f;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class GuMasterWarPlate : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30; Item.height = 20;
            Item.value = Item.buyPrice(gold: 20);
            Item.rare = ItemRarityID.LightRed;
            Item.defense = 16;
        }
        public override void UpdateEquip(Player player)
        {
            player.statDefense += 5;
            player.endurance += 0.08f;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class GuMasterWarGreaves : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22; Item.height = 18;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.LightRed;
            Item.defense = 10;
        }
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.08f;
            player.statLifeMax2 += 10;
        }
    }

    // ==================== 五转蛊师套装 ====================
    [AutoloadEquip(EquipType.Head)]
    public class GuLordDiadem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24; Item.height = 24;
            Item.value = Item.buyPrice(gold: 30);
            Item.rare = ItemRarityID.Pink;
            Item.defense = 18;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.statLifeMax2 += 40;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
            => head.type == Type && body.type == ModContent.ItemType<GuLordBattleArmor>() && legs.type == ModContent.ItemType<GuLordBattleLeggings>();
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "蛊师霸主：+80真元上限，+18%暴击率，+12%伤害减免\n生命低于30%时防御+20";
            player.statLifeMax2 += 80;
            player.GetCritChance(DamageClass.Generic) += 18;
            player.endurance += 0.12f;
            if (player.statLife < player.statLifeMax2 * 0.3f)
                player.statDefense += 20;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class GuLordBattleArmor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30; Item.height = 20;
            Item.value = Item.buyPrice(gold: 40);
            Item.rare = ItemRarityID.Pink;
            Item.defense = 24;
        }
        public override void UpdateEquip(Player player)
        {
            player.statDefense += 8;
            player.endurance += 0.12f;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class GuLordBattleLeggings : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22; Item.height = 18;
            Item.value = Item.buyPrice(gold: 30);
            Item.rare = ItemRarityID.Pink;
            Item.defense = 16;
        }
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.10f;
            player.statLifeMax2 += 20;
        }
    }

    // ==================== 六转蛊仙套装 ====================
    [AutoloadEquip(EquipType.Head)]
    public class GuImmortalCrown : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24; Item.height = 24;
            Item.value = Item.buyPrice(gold: 50);
            Item.rare = ItemRarityID.LightPurple;
            Item.defense = 25;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.25f;
            player.statLifeMax2 += 80;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
            => head.type == Type && body.type == ModContent.ItemType<GuImmortalRobe>() && legs.type == ModContent.ItemType<GuImmortalBoots>();
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "蛊仙之威：+150真元上限，+25%暴击率，+15%伤害减免\n免疫岩浆，+2最大召唤数";
            player.statLifeMax2 += 150;
            player.GetCritChance(DamageClass.Generic) += 25;
            player.endurance += 0.15f;
            player.lavaMax += 600;
            player.maxMinions += 2;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class GuImmortalRobe : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30; Item.height = 20;
            Item.value = Item.buyPrice(gold: 60);
            Item.rare = ItemRarityID.LightPurple;
            Item.defense = 32;
        }
        public override void UpdateEquip(Player player)
        {
            player.statDefense += 12;
            player.endurance += 0.15f;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class GuImmortalBoots : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22; Item.height = 18;
            Item.value = Item.buyPrice(gold: 50);
            Item.rare = ItemRarityID.LightPurple;
            Item.defense = 22;
        }
        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.15f;
            player.statLifeMax2 += 40;
            player.rocketBoots = 2;
        }
    }
}
