using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.GlobalNPCs;

namespace VerminLordMod.Common.GuBehaviors
{
    public static class DaoEffectSystem
    {
        public static void ApplyEffects(NPC target, Player player, Projectile proj, DaoEffectTags tags,
            float doTDamage = 0, float doTDuration = 0, float slowPercent = 0, int slowDuration = 0,
            float armorShred = 0, int armorShredDuration = 0, float weakenPercent = 0,
            float lifeSteal = 0, float chainRange = 200f, int chainCount = 2,
            float markDuration = 0, float healAmount = 0, float shieldAmount = 0,
            int shieldDuration = 0, float fearDuration = 0, float charmDuration = 0,
            float pullForce = 0, float pushForce = 0)
        {
            if (tags == DaoEffectTags.None)
                return;

            var npcInfo = target.GetGlobalNPC<GuNPCInfo>();

            // 基础效果
            if ((tags & DaoEffectTags.DoT) != 0)
                ApplyDoT(npcInfo, doTDamage, doTDuration);

            if ((tags & DaoEffectTags.StrongDoT) != 0)
                ApplyDoT(npcInfo, doTDamage * 1.5f, doTDuration);

            if ((tags & DaoEffectTags.Slow) != 0)
                ApplySlow(npcInfo, slowPercent, slowDuration);

            if ((tags & DaoEffectTags.Freeze) != 0)
                ApplySlow(npcInfo, 0.9f, slowDuration); // 冻结 = 90% 减速

            if ((tags & DaoEffectTags.ArmorShred) != 0)
                ApplyArmorShred(npcInfo, armorShred, armorShredDuration);

            if ((tags & DaoEffectTags.Weaken) != 0)
                ApplyWeaken(npcInfo, weakenPercent, slowDuration);

            if ((tags & DaoEffectTags.LifeSteal) != 0)
                ApplyLifeSteal(player, proj.damage, lifeSteal);

            // D-25: 复杂效果实现
            if ((tags & DaoEffectTags.Chain) != 0)
                ApplyChain(target, player, proj, chainRange, chainCount);

            if ((tags & DaoEffectTags.Mark) != 0)
                ApplyMark(npcInfo, markDuration, player);

            if ((tags & DaoEffectTags.MoonMark) != 0)
                ApplyMark(npcInfo, markDuration * 1.5f, player); // 月光标记持续更久

            if ((tags & DaoEffectTags.Heal) != 0)
                ApplyHeal(player, healAmount);

            if ((tags & DaoEffectTags.Shield) != 0)
                ApplyShield(player, shieldAmount, shieldDuration);

            if ((tags & DaoEffectTags.Fear) != 0)
                ApplyFear(npcInfo, fearDuration, player);

            if ((tags & DaoEffectTags.Charm) != 0)
                ApplyCharm(npcInfo, charmDuration, player);

            if ((tags & DaoEffectTags.Pull) != 0)
                ApplyPull(target, player, pullForce);

            if ((tags & DaoEffectTags.Push) != 0)
                ApplyPush(target, player, pushForce);

            if ((tags & DaoEffectTags.Blind) != 0)
                ApplyBlind(npcInfo, slowDuration);

            if ((tags & DaoEffectTags.Silence) != 0)
                ApplySilence(npcInfo, slowDuration);

            if ((tags & DaoEffectTags.Disarm) != 0)
                ApplyDisarm(npcInfo, slowDuration);

            if ((tags & DaoEffectTags.QiRestore) != 0)
                ApplyQiRestore(player, healAmount);
        }

        // ===== 基础效果 =====

        private static void ApplyDoT(GuNPCInfo npcInfo, float damage, float duration)
        {
            if (damage <= 0 || duration <= 0) return;
            npcInfo.DoTDamage = damage;
            npcInfo.DoTTimer = (int)(duration * 60);
        }

        private static void ApplySlow(GuNPCInfo npcInfo, float percent, int duration)
        {
            if (percent <= 0 || duration <= 0) return;
            npcInfo.SlowPercent = percent;
            npcInfo.SlowTimer = duration;
        }

        private static void ApplyArmorShred(GuNPCInfo npcInfo, float amount, int duration)
        {
            if (amount <= 0 || duration <= 0) return;
            npcInfo.ArmorShredAmount = amount;
            npcInfo.ArmorShredTimer = duration;
        }

        private static void ApplyWeaken(GuNPCInfo npcInfo, float percent, int duration)
        {
            if (percent <= 0 || duration <= 0) return;
            npcInfo.WeakenPercent = percent;
            npcInfo.WeakenTimer = duration;
        }

        private static void ApplyLifeSteal(Player player, int damage, float percent)
        {
            if (percent <= 0) return;
            int healAmount = (int)(damage * percent);
            if (healAmount > 0)
            {
                player.statLife += healAmount;
                player.HealEffect(healAmount);
            }
        }

        // ===== D-25: 复杂效果 =====

        /// <summary>
        /// 连锁弹射：弹射到附近其他 NPC。
        /// </summary>
        private static void ApplyChain(NPC target, Player player, Projectile proj, float range, int count)
        {
            if (count <= 0 || range <= 0) return;

            var nearbyNPCs = Main.npc.Where(n => n.active && !n.friendly
                && n.Distance(target.Center) < range && n.whoAmI != target.whoAmI
                && n.Distance(player.Center) < range * 2).Take(count);

            foreach (var npc in nearbyNPCs)
            {
                Vector2 direction = (npc.Center - target.Center).SafeNormalize(Vector2.Zero);
                Projectile.NewProjectile(
                    proj.GetSource_FromThis(),
                    target.Center,
                    direction * 12f,
                    proj.type,
                    proj.damage / 2,
                    proj.knockBack * 0.5f,
                    proj.owner);
            }
        }

        /// <summary>
        /// 标记：叠加标记层数，后续攻击可引爆。
        /// </summary>
        private static void ApplyMark(GuNPCInfo npcInfo, float duration, Player player)
        {
            if (duration <= 0) return;
            npcInfo.MarkTimer = (int)(duration * 60);
            npcInfo.MarkCount++;
            npcInfo.MarkSourcePlayer = player.whoAmI;
        }

        /// <summary>
        /// 治疗：直接回复玩家生命。
        /// </summary>
        private static void ApplyHeal(Player player, float amount)
        {
            if (amount <= 0) return;
            int heal = (int)amount;
            player.statLife += heal;
            if (player.statLife > player.statLifeMax2)
                player.statLife = player.statLifeMax2;
            player.HealEffect(heal);
        }

        /// <summary>
        /// 护盾：临时减伤。
        /// </summary>
        private static void ApplyShield(Player player, float amount, int duration)
        {
            if (amount <= 0 || duration <= 0) return;
            // 护盾效果：通过 ModPlayer 存储护盾值，在 ModifyHurt 中减伤
            // 使用 ShieldPlayer 存储护盾数据
            var shieldPlayer = player.GetModPlayer<Players.ShieldPlayer>();
            if (shieldPlayer != null)
            {
                shieldPlayer.ShieldAmount += (int)amount;
                shieldPlayer.ShieldTimer = duration * 60;
            }
        }

        /// <summary>
        /// 恐惧：NPC 逃离玩家。
        /// </summary>
        private static void ApplyFear(GuNPCInfo npcInfo, float duration, Player player)
        {
            if (duration <= 0) return;
            npcInfo.FearTimer = (int)(duration * 60);
            npcInfo.FearSourcePlayer = player.whoAmI;
        }

        /// <summary>
        /// 魅惑：NPC 为玩家而战。
        /// </summary>
        private static void ApplyCharm(GuNPCInfo npcInfo, float duration, Player player)
        {
            if (duration <= 0) return;
            npcInfo.CharmTimer = (int)(duration * 60);
            npcInfo.CharmSourcePlayer = player.whoAmI;
        }

        /// <summary>
        /// 拉近：将 NPC 拉向玩家。
        /// </summary>
        private static void ApplyPull(NPC target, Player player, float force)
        {
            if (force <= 0) return;
            Vector2 pullDir = player.Center - target.Center;
            if (pullDir != Vector2.Zero)
            {
                pullDir.Normalize();
                target.velocity += pullDir * force;
            }
        }

        /// <summary>
        /// 击退：将 NPC 推离玩家。
        /// </summary>
        private static void ApplyPush(NPC target, Player player, float force)
        {
            if (force <= 0) return;
            Vector2 pushDir = target.Center - player.Center;
            if (pushDir != Vector2.Zero)
            {
                pushDir.Normalize();
                target.velocity += pushDir * force;
            }
        }

        /// <summary>
        /// 致盲：降低 NPC 命中率（通过降低攻击频率模拟）。
        /// </summary>
        private static void ApplyBlind(GuNPCInfo npcInfo, int duration)
        {
            if (duration <= 0) return;
            // 致盲效果通过降低 NPC 的 AI 更新频率实现
            // 简化：使用沉默计时器来降低伤害
            npcInfo.SilenceTimer = duration;
        }

        /// <summary>
        /// 沉默：禁用 NPC 特殊能力。
        /// </summary>
        private static void ApplySilence(GuNPCInfo npcInfo, int duration)
        {
            if (duration <= 0) return;
            npcInfo.SilenceTimer = duration;
        }

        /// <summary>
        /// 缴械：降低 NPC 攻击力。
        /// </summary>
        private static void ApplyDisarm(GuNPCInfo npcInfo, int duration)
        {
            if (duration <= 0) return;
            npcInfo.DisarmTimer = duration;
        }

        /// <summary>
        /// 回复真元。
        /// </summary>
        private static void ApplyQiRestore(Player player, float amount)
        {
            if (amount <= 0) return;
            // 通过 QiResourcePlayer 回复真元
            var qiPlayer = player.GetModPlayer<Players.QiResourcePlayer>();
            if (qiPlayer != null)
            {
                qiPlayer.RefundQi(amount);
            }
        }
    }
}
