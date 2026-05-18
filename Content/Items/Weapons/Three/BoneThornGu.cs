using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Weapons.Three
{    /// <summary>
    /// 三转道蛊虫 — BoneThornGu
    /// 蛊虫
    /// </summary>

    public class BoneThornGu : BoneWeapon, IOnHitEffectProvider
    {
        protected override int qiCost => 28;
        protected override int _useTime => 32;
        protected override int _guLevel => 3;
        protected override int controlQiCost => 14;
        protected override float unitConntrolRate => 12;

        public DaoEffectTags[] OnHitEffects => new[] { DaoEffectTags.ArmorShred, DaoEffectTags.DoT };
        public float DoTDuration => 3f;
        public float DoTDamage => 5f;
        public float SlowPercent => 0;
        public int SlowDuration => 0;
        public float ArmorShredAmount => 6f;
        public int ArmorShredDuration => 180;
        public float WeakenPercent => 0;
        public float LifeStealPercent => 0;

        public void CustomOnHitNPC(Terraria.NPC target, Terraria.Player player, Terraria.Projectile projectile, int damage) { }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.damage = 40;
            Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Item.knockBack = 5f;
            Item.crit = 6;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 5000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.UseSound = SoundID.Item71;
            Item.scale = 1f;
            Item.shoot = ModContent.ProjectileType<BoneThornProj>();
            Item.shootSpeed = 0f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Microsoft.Xna.Framework.Vector2 position, Microsoft.Xna.Framework.Vector2 velocity,
            int type, int damage, float knockback)
        {
            int hpCost = 8;
            if (player.statLife <= hpCost)
            {
                SoundEngine.PlaySound(SoundID.NPCHit13, player.Center);
                return false;
            }

            player.statLife -= hpCost;
            player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(
                Terraria.Localization.NetworkText.FromLiteral(player.name + "的骨刺破体而出")),
                hpCost, 0);

            Projectile.NewProjectile(source, player.Center, Microsoft.Xna.Framework.Vector2.Zero,
                type, damage, knockback, player.whoAmI);
            return false;
        }
    }
}