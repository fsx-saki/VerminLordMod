using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 范围伤害行为 — 弹幕每帧/间隔检测范围内的敌人并造成伤害。
    /// 适用于火焰墙、光环、漩涡、毒雾等范围持续伤害型弹幕。
    /// 
    /// 使用方式：
    ///   Behaviors.Add(new AreaDamageBehavior
    ///   {
    ///       HitRadius = 60f,
    ///       HitInterval = 15,       // 每 15 帧检测一次
    ///       Knockback = 0f,
    ///       UseLocalNPCHitCooldown = true
    ///   });
    /// </summary>
    public class AreaDamageBehavior : IBulletBehavior
    {
        public string Name => "AreaDamage";

        /// <summary>伤害检测半径（像素）</summary>
        public float HitRadius { get; set; } = 60f;

        /// <summary>伤害检测间隔（帧），每 N 帧检测一次</summary>
        public int HitInterval { get; set; } = 15;

        /// <summary>击退力</summary>
        public float Knockback { get; set; } = 0f;

        /// <summary>
        /// 是否使用弹幕的 localNPCImmunity 机制。
        /// true = 使用 projectile.localNPCHitCooldown 控制免疫间隔
        /// false = 使用内部 HashSet 记录已命中敌人（弹幕销毁时重置）
        /// </summary>
        public bool UseLocalNPCHitCooldown { get; set; } = true;

        /// <summary>
        /// 是否自动设置弹幕的 localNPCHitCooldown。
        /// 仅在 UseLocalNPCHitCooldown=true 且 AutoSetCooldown=true 时生效。
        /// </summary>
        public bool AutoSetCooldown { get; set; } = true;

        /// <summary>伤害来源位置偏移</summary>
        public Vector2 PositionOffset { get; set; } = Vector2.Zero;

        /// <summary>是否在命中时产生击退方向（基于敌人相对于弹幕的位置）</summary>
        public bool DirectionalKnockback { get; set; } = true;

        // 内部状态（仅在不使用 localNPCImmunity 时使用）
        private System.Collections.Generic.HashSet<int> _hitNPCs;
        private int _timer;

        public AreaDamageBehavior() { }

        public AreaDamageBehavior(float hitRadius, int hitInterval = 15)
        {
            HitRadius = hitRadius;
            HitInterval = hitInterval;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _timer = 0;
            _hitNPCs = new System.Collections.Generic.HashSet<int>();

            if (UseLocalNPCHitCooldown && AutoSetCooldown)
            {
                projectile.usesLocalNPCImmunity = true;
                projectile.localNPCHitCooldown = HitInterval * 2;
            }
        }

        public void Update(Projectile projectile)
        {
            _timer++;

            if (_timer % HitInterval != 0) return;

            Vector2 center = projectile.Center + PositionOffset;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy() || npc.friendly)
                    continue;

                float dist = Vector2.Distance(center, npc.Center);
                if (dist > HitRadius)
                    continue;

                // 使用 localNPCImmunity 时，引擎自动处理免疫
                if (UseLocalNPCHitCooldown)
                {
                    int damage = projectile.damage;
                    Player owner = Main.player[projectile.owner];
                    if (owner != null && owner.active)
                    {
                        int hitDir = DirectionalKnockback
                            ? (npc.Center.X > projectile.Center.X ? 1 : -1)
                            : 0;
                        bool crit = Main.rand.Next(100) < projectile.CritChance;
                        npc.StrikeNPC(new NPC.HitInfo
                        {
                            Damage = damage,
                            Knockback = Knockback,
                            HitDirection = hitDir,
                            Crit = crit
                        });
                    }
                }
                else
                {
                    // 使用 HashSet 手动控制免疫
                    if (_hitNPCs.Contains(i))
                        continue;

                    _hitNPCs.Add(i);

                    int damage = projectile.damage;
                    Player owner = Main.player[projectile.owner];
                    if (owner != null && owner.active)
                    {
                        int hitDir = DirectionalKnockback
                            ? (npc.Center.X > projectile.Center.X ? 1 : -1)
                            : 0;
                        bool crit = Main.rand.Next(100) < projectile.CritChance;
                        npc.StrikeNPC(new NPC.HitInfo
                        {
                            Damage = damage,
                            Knockback = Knockback,
                            HitDirection = hitDir,
                            Crit = crit
                        });
                    }
                }
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            _hitNPCs?.Clear();
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
