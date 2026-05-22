using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Five
{
    /// <summary>
    /// 五转时间道蛊虫 — 激突蛊
    /// 五转蛊虫，能让蛊师短时间内变身激流向前冲刺，但易被炎道克制。
    /// </summary>
    public class JiTuGu : TimeWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 55;
        protected override int _useTime => 15;
        protected override int _guLevel => 5;
        protected override int controlQiCost => 25;
        protected override float unitConntrolRate => 8;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.Slow };
        public float DoTDuration => 3f;
        public float DoTDamage => 14f;
        public float SlowPercent => 0.3f;
        public int SlowDuration => 210;
        public float ArmorShredAmount => 13f;
        public int ArmorShredDuration => 210;
        public float WeakenPercent => 0.15f;
        public float LifeStealPercent => 0.1f;

        public void CustomOnHitNPC(NPC target, Player player, Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.damage = 85;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 6f;
            Item.crit = 8;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 30000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.UseSound = SoundID.Item4;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<JiTuProj>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }
    }
}
