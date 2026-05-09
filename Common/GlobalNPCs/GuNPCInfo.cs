using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.GlobalNPCs
{
    public class GuNPCInfo : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // DoT
        public float DoTDamage;
        public int DoTTimer;

        // Slow
        public float SlowPercent;
        public int SlowTimer;

        // Armor Shred
        public float ArmorShredAmount;
        public int ArmorShredTimer;

        // Weaken
        public float WeakenPercent;
        public int WeakenTimer;

        // Mark（标记引爆）
        public int MarkTimer;
        public int MarkCount;
        public int MarkSourcePlayer;

        // Shield（护盾）
        public float ShieldAmount;
        public int ShieldTimer;

        // Fear（恐惧 — NPC 逃离玩家）
        public int FearTimer;
        public int FearSourcePlayer;

        // Charm（魅惑 — NPC 为玩家而战）
        public int CharmTimer;
        public int CharmSourcePlayer;

        // Silence（沉默 — 禁用特殊能力）
        public int SilenceTimer;

        // Disarm（缴械 — 降低攻击力）
        public int DisarmTimer;

        public override void ResetEffects(NPC npc)
        {
            // 每帧递减计时器
            if (DoTTimer > 0) DoTTimer--;
            if (SlowTimer > 0) SlowTimer--;
            if (ArmorShredTimer > 0) ArmorShredTimer--;
            if (WeakenTimer > 0) WeakenTimer--;
            if (MarkTimer > 0) MarkTimer--;
            if (ShieldTimer > 0) ShieldTimer--;
            if (FearTimer > 0) FearTimer--;
            if (CharmTimer > 0) CharmTimer--;
            if (SilenceTimer > 0) SilenceTimer--;
            if (DisarmTimer > 0) DisarmTimer--;
        }

        public override void PostAI(NPC npc)
        {
            // DoT 伤害
            if (DoTTimer > 0 && DoTDamage > 0)
            {
                if (DoTTimer % 60 == 0)
                {
                    npc.SimpleStrikeNPC((int)DoTDamage, 0);
                }
            }

            // 减速效果
            if (SlowTimer > 0 && SlowPercent > 0)
            {
                npc.velocity *= (1f - SlowPercent * 0.5f);
            }

            // 恐惧效果：NPC 远离玩家
            if (FearTimer > 0)
            {
                Player fearSource = Main.player[FearSourcePlayer];
                if (fearSource != null && fearSource.active)
                {
                    Vector2 fleeDir = npc.Center - fearSource.Center;
                    if (fleeDir != Vector2.Zero)
                    {
                        fleeDir.Normalize();
                        npc.velocity += fleeDir * 0.5f;
                    }
                }
            }

            // 魅惑效果：NPC 攻击其他敌人
            if (CharmTimer > 0)
            {
                // 寻找最近的敌对 NPC 并攻击
                NPC target = null;
                float minDist = 600f;
                foreach (var other in Main.npc)
                {
                    if (other.active && !other.friendly && other.whoAmI != npc.whoAmI)
                    {
                        float dist = npc.Distance(other.Center);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            target = other;
                        }
                    }
                }
                if (target != null)
                {
                    Vector2 dir = target.Center - npc.Center;
                    if (dir != Vector2.Zero)
                    {
                        dir.Normalize();
                        npc.velocity += dir * 0.3f;
                    }
                }
            }

            // 沉默效果：禁用特殊攻击（通过降低弹幕速度模拟）
            if (SilenceTimer > 0)
            {
                npc.damage = (int)(npc.defDamage * 0.5f);
            }

            // 缴械效果：降低攻击力
            if (DisarmTimer > 0)
            {
                npc.damage = (int)(npc.defDamage * 0.3f);
            }
        }

        /// <summary>
        /// 修改 NPC 受到的伤害（用于碎甲/虚弱/护盾效果）。
        /// </summary>
        public void ModifyDamage(ref NPC.HitModifiers modifiers)
        {
            if (ArmorShredTimer > 0 && ArmorShredAmount > 0)
            {
                modifiers.Defense -= (int)ArmorShredAmount;
            }

            if (WeakenTimer > 0 && WeakenPercent > 0)
            {
                modifiers.FinalDamage *= (1f - WeakenPercent);
            }

            // 护盾减伤
            if (ShieldTimer > 0 && ShieldAmount > 0)
            {
                modifiers.FinalDamage *= (1f - ShieldAmount);
            }
        }

        /// <summary>
        /// 标记引爆：对带有标记的 NPC 造成额外伤害。
        /// </summary>
        public int ApplyMarkDetonation(int baseDamage)
        {
            if (MarkTimer > 0 && MarkCount > 0)
            {
                int bonusDamage = baseDamage / 2 * MarkCount;
                MarkTimer = 0;
                MarkCount = 0;
                return bonusDamage;
            }
            return 0;
        }
    }
}
