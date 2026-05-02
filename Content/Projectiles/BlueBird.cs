using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.Buffs.AddToEnemy;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// 蓝鸟弹幕 — 追踪敌人，命中后在敌人位置生成冰霜法阵。
    /// 法阵旋转并召唤冰碎片汇聚向敌人，最终冻结敌人。
    /// </summary>
    public class BlueBird : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: 10f, trackingWeight: 1f / 11f)
            {
                Range = 8000f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });
            Behaviors.Add(new TrailBehavior()
            {
                AutoDraw = true,
                SuppressDefaultDraw = false,
            });
            Behaviors.Add(new DustKillBehavior(DustID.Ice, 20, 5f, 1.5f));
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 100;
            Projectile.alpha = 100;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 配置拖尾
            var trailBehavior = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trailBehavior != null)
            {
                trailBehavior.TrailManager.AddGhostTrail(
                    trailTex: Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value,
                    color: new Color(150, 200, 255),
                    maxPositions: 16,
                    widthScale: 0.5f,
                    lengthScale: 1.0f,
                    alpha: 0.5f,
                    recordInterval: 2);
            }
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 在敌人位置生成冰霜法阵
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                target.Center,
                Vector2.Zero,
                ModContent.ProjectileType<BlueBirdCircleProj>(),
                0, 0f, Projectile.owner,
                ai0: target.whoAmI  // 记录目标 NPC 索引
            );

            // 添加冻结 buff
            target.AddBuff(ModContent.BuffType<BlueBirdbuff>(), 300);
        }
    }
}
