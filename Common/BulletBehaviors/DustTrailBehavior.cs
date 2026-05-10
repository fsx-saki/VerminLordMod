using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 飞行粒子行为 — 弹幕飞行时持续生成粒子。
    /// 可配置粒子类型、生成概率、速度等。
    /// </summary>
    public class DustTrailBehavior : IBulletBehavior
    {
        public string Name => "DustTrail";

        /// <summary>粒子类型（DustID）</summary>
        public int DustType { get; set; } = DustID.YellowStarDust;

        /// <summary>每帧生成概率（1/N）</summary>
        public int SpawnChance { get; set; } = 2;

        /// <summary>粒子速度倍率（相对于弹幕速度）</summary>
        public float VelocityMultiplier { get; set; } = 0.2f;

        /// <summary>粒子缩放</summary>
        public float DustScale { get; set; } = 1.2f;

        /// <summary>粒子是否无重力</summary>
        public bool NoGravity { get; set; } = true;

        /// <summary>粒子颜色（null=默认）</summary>
        public Color? DustColor { get; set; } = null;

        /// <summary>粒子透明度（0~255）</summary>
        public int DustAlpha { get; set; } = 150;

        /// <summary>是否使用弹幕速度影响粒子方向</summary>
        public bool UseProjectileVelocity { get; set; } = true;

        /// <summary>随机速度范围</summary>
        public float RandomSpeed { get; set; } = 0f;

        public DustTrailBehavior() { }

        public DustTrailBehavior(int dustType, int spawnChance = 2)
        {
            DustType = dustType;
            SpawnChance = spawnChance;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            if (Main.rand.NextBool(SpawnChance))
            {
                Vector2 dustVel = Vector2.Zero;
                if (UseProjectileVelocity)
                {
                    dustVel = projectile.velocity * VelocityMultiplier;
                }
                if (RandomSpeed > 0f)
                {
                    dustVel += new Vector2(
                        Main.rand.NextFloat(-RandomSpeed, RandomSpeed),
                        Main.rand.NextFloat(-RandomSpeed, RandomSpeed)
                    );
                }

                int dustId = Dust.NewDust(
                    projectile.position, projectile.width, projectile.height,
                    DustType, dustVel.X, dustVel.Y,
                    DustAlpha, DustColor ?? default(Color), DustScale
                );

                if (NoGravity && dustId >= 0 && dustId < Main.dust.Length)
                {
                    Main.dust[dustId].noGravity = true;
                }
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
