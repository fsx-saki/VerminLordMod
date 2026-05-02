using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.Buffs.AddToEnemy;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// 冰碎片弹幕 — 从法阵外围飞向目标并贴住敌人。
    /// 
    /// 行为：
    /// 1. 生成时 ai[0], ai[1] 记录目标位置（法阵中心 = 敌人位置）
    /// 2. 飞向目标位置，到达后停止并贴住
    /// 3. 贴住后持续给目标叠加 BlueBirdbuff
    /// 4. 一段时间后消失
    /// </summary>
    public class IceShardProj : ModProjectile
    {
        // ===== 配置常量 =====
        private const float ApproachSpeed = 6f;     // 飞行速度
        private const float StopDistance = 10f;     // 停止距离（到达目标多远算贴住）
        private const int StickLifetime = 120;      // 贴住后存活 2 秒
        private const int BuffInterval = 20;        // 每 20 帧刷新一次 buff

        // ===== 运行时状态 =====
        private bool _isStuck;          // 是否已贴住
        private int _stickTimer;        // 贴住计时
        private int _buffTimer;         // buff 刷新计时
        private Vector2 _targetPos;     // 目标位置
        private NPC _targetNPC;         // 目标 NPC

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.scale = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;  // 最大存活 5 秒
            Projectile.alpha = 50;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.aiStyle = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 从 ai 中读取目标位置
            _targetPos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            _isStuck = false;
            _stickTimer = 0;
            _buffTimer = 0;

            // 尝试找到目标 NPC（法阵中心附近的敌人）
            _targetNPC = FindNearestEnemy(Projectile.Center, 100f);
        }

        public override void AI()
        {
            if (_isStuck)
            {
                // === 贴住状态 ===
                _stickTimer++;

                // 跟随目标
                if (_targetNPC != null && _targetNPC.active)
                {
                    Projectile.Center = _targetNPC.Center + _localOffset;
                }

                // 闪烁效果（快消失时）
                if (_stickTimer > StickLifetime - 30)
                {
                    Projectile.alpha = (int)(50 + ((float)System.Math.Sin(_stickTimer * 0.3f) * 0.5f + 0.5f) * 150);
                }

                // 持续刷新 buff
                _buffTimer++;
                if (_buffTimer >= BuffInterval && _targetNPC != null && _targetNPC.active)
                {
                    _buffTimer = 0;
                    _targetNPC.AddBuff(ModContent.BuffType<BlueBirdbuff>(), 120);
                }

                // 冰霜粒子
                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustDirect(
                        Projectile.position, Projectile.width, Projectile.height,
                        DustID.Ice, 0f, 0f, 80, default, 0.6f);
                    d.noGravity = true;
                    d.velocity *= 0.2f;
                }

                // 超时消失
                if (_stickTimer >= StickLifetime)
                {
                    Projectile.Kill();
                }
                return;
            }

            // === 飞行状态 ===
            Vector2 toTarget = _targetPos - Projectile.Center;
            float dist = toTarget.Length();

            // 到达目标位置 → 贴住
            if (dist < StopDistance)
            {
                _isStuck = true;
                _stickTimer = 0;
                Projectile.velocity = Vector2.Zero;

                // 重新查找目标 NPC
                if (_targetNPC == null || !_targetNPC.active)
                {
                    _targetNPC = FindNearestEnemy(Projectile.Center, 50f);
                }

                // 记录相对偏移
                if (_targetNPC != null && _targetNPC.active)
                {
                    _localOffset = Projectile.Center - _targetNPC.Center;
                }
                return;
            }

            // 飞向目标
            Vector2 desiredVel = toTarget.SafeNormalize(Vector2.Zero) * ApproachSpeed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.15f);

            // 旋转到速度方向
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 飞行粒子
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Ice, 0f, 0f, 50, default, 0.5f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }

        private Vector2 _localOffset;  // 贴住时相对于 NPC 的偏移

        private NPC FindNearestEnemy(Vector2 center, float range)
        {
            NPC nearest = null;
            float nearestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(center, npc.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = npc;
                    }
                }
            }

            return nearest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用默认绘制（小冰晶贴图）
            return true;
        }
    }
}
