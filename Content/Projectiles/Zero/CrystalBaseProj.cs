using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 晶道基础弹幕 — 棱晶碎片。
    ///
    /// 设计哲学：
    /// 晶的本质是"分裂 + 折射 + 璀璨"。弹幕以中速直线飞行，
    /// 命中后产生径向晶片爆散（SplashBehavior Radial 模式），
    /// 销毁时分裂为 3 个小型晶片弹幕继续攻击，
    /// 视觉上以晶系拖尾（共振环 + 棱晶碎片 + 晶格节点）模拟水晶的折射璀璨。
    ///
    /// 运动方式：
    /// - 中速直线飞行（AimBehavior）
    /// - 命中后径向晶片爆散
    /// - 销毁时分裂为 3 个小型晶片
    ///
    /// 视觉效果：
    /// - 晶系拖尾：共振环 + 棱晶碎片 + 晶格节点（CrystalTrailBehavior）
    /// - 命中时径向晶片爆散（SplashBehavior Radial 模式）
    ///
    /// 行为组合：
    /// - AimBehavior: 中速直线飞行
    /// - CrystalTrailBehavior: 晶系拖尾（共振环 + 棱晶碎片 + 晶格节点）
    /// - SplashBehavior(Radial): 命中时径向晶片爆散
    /// </summary>
    public class CrystalBaseProj : BaseBullet
    {
        private const float FlySpeed = 9f;
        private const int MaxLife = 150;
        private const int SplitCount = 3;
        private const float SplitSpeed = 5f;
        private const float SplitSpread = 0.4f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.6f, 0.9f)
            });

            Behaviors.Add(new CrystalTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(180, 180, 255, 160),
                ResonanceRingColor = new Color(160, 180, 255, 200),
                PrismShardColor = new Color(200, 200, 255, 220),
                LatticeNodeColor = new Color(180, 200, 255, 200),
            });

            Behaviors.Add(new SplashBehavior(SplashMode.Radial)
            {
                Count = 10,
                SpeedMin = 2f,
                SpeedMax = 5f,
                SpreadRadius = 4f,
                SpawnExtraDust = true,
                ExtraDustCount = 12,
                DustType = DustID.Clentaminator_Cyan,
                DustColorStart = new Color(180, 200, 255, 220),
                DustColorEnd = new Color(80, 100, 200, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.6f,
                DustSpeedMin = 1f,
                DustSpeedMax = 3f,
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
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 120);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < SplitCount; i++)
            {
                float angleOffset = (i - (SplitCount - 1) / 2f) * SplitSpread;
                Vector2 splitVel = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(angleOffset) * SplitSpeed;

                int childType = ModContent.ProjectileType<CrystalShardProj>();
                if (childType > 0)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        splitVel,
                        childType,
                        Projectile.damage / 2,
                        Projectile.knockBack * 0.5f,
                        Projectile.owner
                    );
                }
            }

            for (int i = 0; i < 12; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 3f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Clentaminator_Cyan,
                    vel,
                    0,
                    new Color(180, 200, 255, 200),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }

    /// <summary>
    /// 晶片碎片 — CrystalBaseProj 销毁时分裂出的小型弹幕。
    /// 短寿命、低伤害、纯视觉效果。
    /// </summary>
    public class CrystalShardProj : BaseBullet
    {
        private const int MaxLife = 40;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new DustTrailBehavior(DustID.Clentaminator_Cyan, spawnChance: 1)
            {
                DustScale = 0.3f,
                VelocityMultiplier = 0.03f,
                NoGravity = true,
                DustAlpha = 150,
                RandomSpeed = 0.2f
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(150, 180, 255, 150),
                GlowBaseScale = 0.8f,
                GlowLayers = 1,
                GlowAlphaMultiplier = 0.2f,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.4f, 0.7f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.scale = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(0.5f, 1.5f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Clentaminator_Cyan,
                    vel,
                    0,
                    new Color(150, 180, 255, 150),
                    Main.rand.NextFloat(0.2f, 0.4f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}