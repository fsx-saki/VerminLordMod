using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Four
{    /// <summary>
    /// 四转火道蛊虫 — 火龙蛊
    /// 四转蛊，化作火龙，能在火海中恢复体型。
    /// </summary>
    public class HuoLongGu : FireWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 45;
        protected override int _useTime => 28;
        protected override int _guLevel => 4;
        protected override int controlQiCost => 22;
        protected override float unitConntrolRate => 10;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.DoT };
        public float DoTDuration => 4f;
        public float DoTDamage => 9f;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 0;
        public int ArmorShredDuration => 0;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.damage = 80;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 4f;
            Item.crit = 4;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 15000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 28;
            Item.useTime = 28;
            Item.UseSound = SoundID.Item20;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<HuoLongProj>();
            Item.shootSpeed = 8f;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
        }
    }
}
