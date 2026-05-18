using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{    /// <summary>
    /// 五转道蛊虫 — BloodDropperGu
    /// 蛊虫
    /// </summary>
    public class BloodDropperGu : ChargeWeaponTemplate
    {
        protected override int qiCost => 350;
        protected override int _useTime => 28;
        protected override int _guLevel => 5;

        protected override int ProjType => ModContent.ProjectileType<BloodDropperProj>();
        protected override int BloodCostInterval => 50;

        protected override void SetupItemDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.damage = 55;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 4f;
            Item.crit = 8;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 18000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item71;
            Item.scale = 1f;
        }

        protected override void OnBloodCost(Player player)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    player.Center + Main.rand.NextVector2Circular(30f, 30f),
                    DustID.Blood,
                    Main.rand.NextVector2Circular(5f, 5f),
                    100, Color.Crimson, Main.rand.NextFloat(1f, 2.5f)
                );
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
        }

        protected override SoundStyle? BloodCostSound => SoundID.NPCHit13;
    }
}