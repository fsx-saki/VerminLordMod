using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles.Zero;

namespace VerminLordMod.Content.Items.Weapons.Zero
{
    public class LightningBaseGu : LightningWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 8;
        protected override int _useTime => 20;
        protected override int _guLevel => 1;
        protected override int controlQiCost => 5;
        protected override float unitConntrolRate => 25;

        public int attackMode = 0;

        private static readonly string[] AttackModeNames = new[]
        {
            "电芒",
            "连锁闪电",
            "雷暴",
            "电磁脉冲",
            "雷霆万钧",
        };

        private int[] _modeProjectileTypes;

        private readonly float[] _modeShootSpeeds = new[]
        {
            18f,
            14f,
            0f,
            10f,
            0f,
        };

        private readonly float[] _modeDamageMultipliers = new[]
        {
            1.0f,
            0.8f,
            0.5f,
            1.5f,
            2.0f,
        };

        private readonly int[] _modeUseTimes = new[]
        {
            18,
            22,
            35,
            30,
            45,
        };

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow };
        public float DoTDuration => 3f;
        public float DoTDamage => 4f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 120;
        public float ArmorShredAmount => 5f;
        public int ArmorShredDuration => 180;
        public float WeakenPercent => 0.15f;
        public float LifeStealPercent => 0.1f;
        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item1;
            Item.damage = 18;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 4;
            Item.noMelee = true;

            _modeProjectileTypes = new[]
            {
                ModContent.ProjectileType<LightningBaseProj>(),
                ModContent.ProjectileType<LightningChainProj>(),
                ModContent.ProjectileType<LightningStormProj>(),
                ModContent.ProjectileType<LightningEMPProj>(),
                ModContent.ProjectileType<LightningBoltProj>(),
            };

            Item.shoot = _modeProjectileTypes[0];
            Item.shootSpeed = _modeShootSpeeds[0];
        }

        private bool _lastRKeyState = false;

        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);

            if (player.HeldItem.type == Item.type)
            {
                bool currentRState = Main.keyState.IsKeyDown(Keys.R);
                if (currentRState && !_lastRKeyState)
                {
                    SwitchAttackMode(player);
                }
                _lastRKeyState = currentRState;
            }
            else
            {
                _lastRKeyState = false;
            }
        }

        private void SwitchAttackMode(Player player)
        {
            attackMode = (attackMode + 1) % _modeProjectileTypes.Length;

            Item.shoot = _modeProjectileTypes[attackMode];
            Item.shootSpeed = _modeShootSpeeds[attackMode];
            Item.damage = (int)(Item.OriginalDamage * _modeDamageMultipliers[attackMode]);

            string modeName = AttackModeNames[attackMode];
            CombatText.NewText(player.getRect(), Color.Yellow, $"切换至：{modeName}");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            string modeName = AttackModeNames[attackMode];
            tooltips.Add(new TooltipLine(Mod, "AttackMode", $"当前攻击方式：[c/FFFF00:{modeName}]"));
            tooltips.Add(new TooltipLine(Mod, "SwitchHint", "手持时按 [c/66CCFF:R] 键切换攻击方式"));
        }

        public override bool CanUseItem(Player player)
        {
            if (attackMode >= 0 && attackMode < _modeProjectileTypes.Length)
            {
                Item.shoot = _modeProjectileTypes[attackMode];
                Item.shootSpeed = _modeShootSpeeds[attackMode];
            }

            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (attackMode == 2)
            {
                Vector2 mousePos = Main.MouseWorld;
                Projectile.NewProjectile(source, mousePos, Vector2.Zero, type, damage, 0f, player.whoAmI);
                return false;
            }

            if (attackMode == 4)
            {
                Vector2 mousePos = Main.MouseWorld;
                float offsetY = Main.rand.NextFloat(500f, 700f);
                float offsetX = Main.rand.NextFloat(-60f, 60f);
                Vector2 spawnPos = new Vector2(mousePos.X + offsetX, mousePos.Y - offsetY);
                Vector2 initVel = new Vector2(0f, 0f);

                Projectile.NewProjectile(source, spawnPos, initVel, type, damage, knockback, player.whoAmI);
                return false;
            }

            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag["attackMode"] = attackMode;
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            attackMode = tag.GetInt("attackMode");

            if (_modeProjectileTypes != null && attackMode >= 0 && attackMode < _modeProjectileTypes.Length)
            {
                Item.shoot = _modeProjectileTypes[attackMode];
                Item.shootSpeed = _modeShootSpeeds[attackMode];
                Item.damage = (int)(Item.OriginalDamage * _modeDamageMultipliers[attackMode]);
            }
        }
    }
}