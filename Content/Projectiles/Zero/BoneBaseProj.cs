using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 骨道基础弹幕 — 骨刺穿透。
    ///
    /// 设计哲学：
    /// 骨道的本质是"穿透 + 碎裂 + 锋利"。弹幕以高速穿透多个敌人，
    /// 命中后向前方锥形散射骨碎片（SplashBehavior Cone 模式），
    /// 视觉上以白色骨粉拖尾和锐利感模拟骨刺的穿透力。
    ///
    /// 运动方式：
    /// - 高速直线飞行，高穿透
    /// - 命中后前方锥形散射骨碎片
    ///
    /// 视觉效果：
    /// - 骨系拖尾：肋骨 + 髓光 + 骨刺（BoneTrailBehavior）
    /// - 近战挥舞特效：挥砍弧 + 穿刺冲击 + 砸击环（MeleeSwingTrailBehavior）
    /// - 命中时前方锥形骨碎片散射（SplashBehavior Cone 模式）
    ///
    /// 行为组合：
    /// - AimBehavior: 高速直线飞行
    /// - BoneTrailBehavior: 骨系拖尾（肋骨 + 髓光 + 骨刺）
    /// - MeleeSwingTrailBehavior: 近战挥舞特效（挥砍 + 穿刺 + 砸击）
    /// - SplashBehavior(Cone): 命中时前方锥形骨碎片散射
    /// </summary>
    public class BoneBaseProj : BaseBullet
    {
        private const float FlySpeed = 14f;
        private const int MaxLife = 120;

        protected override void RegisterBehaviors()
        {
            // 1. 高速直线飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.3f, 0.3f)
            });

            Behaviors.Add(new BoneTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(180, 175, 160, 140),

                RibCageColor = new Color(200, 195, 180, 200),
                MarrowGlowColor = new Color(140, 200, 180, 180),
                BoneSpikeColor = new Color(220, 215, 200, 220),
            });
            Behaviors.Add(new MeleeSwingTrailBehavior
            {
                SuppressDefaultDraw = true,
                SwingArcColor = new Color(200, 195, 180, 200),
                StabImpactColor = new Color(220, 215, 200, 220),
                SmashRingColor = new Color(180, 175, 160, 180),
                SwingArcLength = 36f,
                SwingArcWidth = 0.3f,
                SwingArcCurlAmount = 3f,
                StabImpactSpread = 0.35f,
                StabImpactStretch = 3.5f,
                SmashRingEndRadius = 44f,
                SmashRingCrackCount = 7,
            });

            // 4. 命中时前方锥形骨碎片散射
            Behaviors.Add(new SplashBehavior(SplashMode.Cone)
            {
                Count = 8,
                SpeedMin = 3f,
                SpeedMax = 8f,
                SpreadRadius = 4f,
                ConeAngle = 0.5f,
                SpawnExtraDust = true,
                ExtraDustCount = 10,
                DustType = DustID.Bone,
                DustColorStart = new Color(220, 210, 190, 200),
                DustColorEnd = new Color(150, 140, 120, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.7f,
                DustSpeedMin = 1f,
                DustSpeedMax = 4f,
                DustNoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 4;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.BrokenArmor, 180);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 3f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Bone,
                    vel,
                    0,
                    new Color(200, 190, 170, 150),
                    Main.rand.NextFloat(0.4f, 0.7f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}