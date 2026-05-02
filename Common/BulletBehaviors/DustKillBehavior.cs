using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 死亡粒子行为 — 弹幕销毁时生成粒子效果。
    /// 可配置粒子类型、数量、速度等。
    /// </summary>
    public class DustKillBehavior : IBulletBehavior
    {
        public string Name => "DustKill";

        /// <summary>粒子类型（DustID）</summary>
        public int DustType { get; set; } = DustID.BlueFairy;

        /// <summary>粒子数量</summary>
        public int DustCount { get; set; } = 30;

        /// <summary>粒子速度范围</summary>
        public float DustSpeed { get; set; } = 5f;

        /// <summary>粒子缩放</summary>
        public float DustScale { get; set; } = 1.5f;

        /// <summary>粒子是否无重力</summary>
        public bool NoGravity { get; set; } = true;

        public DustKillBehavior() { }

        public DustKillBehavior(int dustType, int dustCount = 30, float dustSpeed = 5f, float dustScale = 1.5f)
        {
            DustType = dustType;
            DustCount = dustCount;
            DustSpeed = dustSpeed;
            DustScale = dustScale;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < DustCount; i++)
            {
                int dustId = Dust.NewDust(
                    projectile.position, projectile.width, projectile.height,
                    DustType,
                    Main.rand.NextFloat(-DustSpeed, DustSpeed),
                    Main.rand.NextFloat(-DustSpeed, DustSpeed),
                    100, default, DustScale
                );
                if (NoGravity && dustId >= 0 && dustId < Main.dust.Length)
                {
                    Main.dust[dustId].noGravity = true;
                }
            }
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }
    }
}
