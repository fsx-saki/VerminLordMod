using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 接触治疗行为 — 弹幕靠近友方单位时自动治疗并销毁。
    ///
    /// 每帧检测范围内是否有友方玩家（自身/同队），
    /// 若距离小于 ContactRange 则回复生命并 Kill 弹幕。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new HealOnContactBehavior
    /// {
    ///     HealAmount = 5,
    ///     ContactRange = 30f,
    /// });
    /// </code>
    /// </summary>
    public class HealOnContactBehavior : IBulletBehavior
    {
        public string Name => "HealOnContact";

        /// <summary>治疗量</summary>
        public int HealAmount { get; set; } = 5;

        /// <summary>接触判定距离（像素）</summary>
        public float ContactRange { get; set; } = 30f;

        /// <summary>治疗粒子数量</summary>
        public int HealDustCount { get; set; } = 5;

        /// <summary>治疗粒子类型</summary>
        public int DustType { get; set; } = DustID.HealingPlus;

        /// <summary>治疗粒子颜色</summary>
        public Color DustColor { get; set; } = new Color(80, 220, 255, 200);

        /// <summary>治疗粒子大小最小值</summary>
        public float DustScaleMin { get; set; } = 0.5f;

        /// <summary>治疗粒子大小最大值</summary>
        public float DustScaleMax { get; set; } = 1.0f;

        /// <summary>治疗粒子速度</summary>
        public float DustSpeed { get; set; } = 2f;

        private bool _hasHealed = false;

        public HealOnContactBehavior() { }

        public HealOnContactBehavior(int healAmount, float contactRange = 30f)
        {
            HealAmount = healAmount;
            ContactRange = contactRange;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            if (_hasHealed) return;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player == null || !player.active || player.dead) continue;

                Player owner = Main.player[projectile.owner];
                if (i != projectile.owner && owner != null && owner.active && player.team != owner.team)
                    continue;

                float dist = Vector2.Distance(projectile.Center, player.Center);
                if (dist < ContactRange)
                {
                    player.statLife += HealAmount;
                    player.HealEffect(HealAmount);

                    for (int j = 0; j < HealDustCount; j++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(DustSpeed, DustSpeed);
                        Dust d = Dust.NewDustPerfect(
                            player.Center + Main.rand.NextVector2Circular(10f, 10f),
                            DustType,
                            vel,
                            0,
                            DustColor,
                            Main.rand.NextFloat(DustScaleMin, DustScaleMax)
                        );
                        d.noGravity = true;
                    }

                    _hasHealed = true;
                    projectile.Kill();
                    return;
                }
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}