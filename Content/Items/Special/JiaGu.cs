using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;
using VerminLordMod.Content.Items.Weapons;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "炼道特殊蛊-模仿", "不定", "炼")]
    public class JiaGu : ModItem
    {
        private const int QiCostPerUse = 30;
        private const int CopyDuration = 1800;
        private const float DamageMultiplier = 0.8f;

        private int _copiedItemType = -1;
        private int _copiedProjectileType = -1;
        private int _copiedDamage = 0;
        private float _copiedKnockback = 0f;
        private float _copiedShootSpeed = 0f;
        private int _copyTimer = 0;

        public bool HasCopy => _copiedItemType > 0 && _copyTimer > 0;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void RightClick(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return;

            if (HasCopy)
            {
                Text.ShowTextRed(player, "假蛊已有模仿目标，等待当前模仿消失");
                return;
            }

            Item otherItem = FindOtherHeldGuWeapon(player);
            if (otherItem == null || otherItem.IsAir)
            {
                Text.ShowTextRed(player, "另一只手未持有蛊虫武器，无法模仿");
                return;
            }

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足，无法催动假蛊");
                return;
            }

            qiResource.ConsumeQi(QiCostPerUse);

            _copiedItemType = otherItem.type;
            _copiedProjectileType = otherItem.shoot > 0 ? otherItem.shoot : ProjectileID.WoodenArrowFriendly;
            _copiedDamage = (int)(otherItem.damage * DamageMultiplier);
            _copiedKnockback = otherItem.knockBack;
            _copiedShootSpeed = otherItem.shootSpeed;
            _copyTimer = CopyDuration;

            Text.ShowTextGreen(player, $"假蛊开始模仿：{otherItem.Name}（80%威力，30秒后自毁）");
        }

        public override bool CanUseItem(Player player)
        {
            if (!HasCopy)
            {
                Text.ShowTextRed(player, "假蛊尚未模仿任何蛊虫，右键模仿");
                return false;
            }

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足");
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            if (!HasCopy)
                return false;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            Vector2 position = player.Center + new Vector2(0f, -10f);
            Vector2 velocity = Vector2.Normalize(Main.MouseWorld - position) * _copiedShootSpeed;

            Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                position,
                velocity,
                _copiedProjectileType,
                _copiedDamage,
                _copiedKnockback,
                player.whoAmI
            );

            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (_copyTimer > 0)
            {
                _copyTimer--;
                if (_copyTimer <= 0)
                {
                    ClearCopy(player);
                }
            }
        }

        private void ClearCopy(Player player)
        {
            _copiedItemType = -1;
            _copiedProjectileType = -1;
            _copiedDamage = 0;
            _copiedKnockback = 0f;
            _copiedShootSpeed = 0f;
            _copyTimer = 0;

            if (player.whoAmI == Main.myPlayer)
            {
                Text.ShowTextRed(player, "假蛊模仿结束，假体自毁消散");
            }
        }

        private Item FindOtherHeldGuWeapon(Player player)
        {
            for (int i = 0; i < 10; i++)
            {
                if (i == player.selectedItem) continue;
                Item item = player.inventory[i];
                if (item != null && !item.IsAir && item.ModItem is GuWeaponItem)
                {
                    return item;
                }
            }
            return null;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JiaGuDesc", "炼道特殊蛊：模仿"));
            tooltips.Add(new TooltipLine(Mod, "JiaGuRightClick", "右键：模仿另一只手持的蛊虫武器"));
            tooltips.Add(new TooltipLine(Mod, "JiaGuLeftClick", "左键：发射模仿的蛊虫弹幕（80%伤害）"));
            tooltips.Add(new TooltipLine(Mod, "JiaGuDuration", $"模仿持续：{CopyDuration / 60}秒，之后假体自毁"));

            if (HasCopy)
            {
                string copyName = "未知蛊虫";
                if (_copiedItemType > 0)
                {
                    Item tmp = new Item();
                    tmp.SetDefaults(_copiedItemType);
                    copyName = tmp.Name;
                }
                tooltips.Add(new TooltipLine(Mod, "JiaGuCopy", $"[c/FFD700:当前模仿：{copyName}]"));
                tooltips.Add(new TooltipLine(Mod, "JiaGuTimer", $"[c/FFD700:剩余时间：{_copyTimer / 60}秒]"));
            }

            tooltips.Add(new TooltipLine(Mod, "JiaGuQiCost", $"消耗真元：{QiCostPerUse}"));
        }

        public override void SaveData(TagCompound tag)
        {
            tag["copiedItemType"] = _copiedItemType;
            tag["copiedProjectileType"] = _copiedProjectileType;
            tag["copiedDamage"] = _copiedDamage;
            tag["copiedKnockback"] = _copiedKnockback;
            tag["copiedShootSpeed"] = _copiedShootSpeed;
            tag["copyTimer"] = _copyTimer;
        }

        public override void LoadData(TagCompound tag)
        {
            _copiedItemType = tag.GetInt("copiedItemType");
            _copiedProjectileType = tag.GetInt("copiedProjectileType");
            _copiedDamage = tag.GetInt("copiedDamage");
            _copiedKnockback = tag.GetFloat("copiedKnockback");
            _copiedShootSpeed = tag.GetFloat("copiedShootSpeed");
            _copyTimer = tag.GetInt("copyTimer");
        }
    }
}
