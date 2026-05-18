using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{    /// <summary>
    /// 四转火道蛊虫 — 油龙蛊
    /// 四转蛊，化作黑色油龙，喷吐黑油辅助火焰攻击。
    /// </summary>

    public class YouLongGu : DarkWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 42;
        protected override int _useTime => 26;
        protected override int _guLevel => 4;
        protected override int controlQiCost => 21;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
        public float DoTDuration => 5f;
        public float DoTDamage => 6f;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.damage = 60;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 3f;
            Item.crit = 4;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 13500;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 26;
            Item.useTime = 26;
            Item.UseSound = SoundID.Item20;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<YouLongProj>();
            Item.shootSpeed = 7f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
