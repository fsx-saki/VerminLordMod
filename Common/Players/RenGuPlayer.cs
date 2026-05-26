using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Content.Buffs.AddToEnemy;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using Terraria.GameContent;
using VerminLordMod.Content;

namespace VerminLordMod.Common.Players
{
    public class RenGuPlayer : ModPlayer
    {
        public int LinkTarget = -1;
        public int LinkTimer = 0;

        public bool IsLinked => LinkTimer > 0 && LinkTarget >= 0;

        private int _lastLife;
        private int _lastHealAccumulator;

        public override void ResetEffects()
        {
            if (LinkTimer > 0)
            {
                LinkTimer--;
                if (LinkTimer <= 0)
                {
                    BreakLink();
                }
            }
        }

        public override void PostUpdate()
        {
            if (!IsLinked)
            {
                _lastLife = Player.statLife;
                _lastHealAccumulator = 0;
                return;
            }

            NPC target = LinkTarget >= 0 && LinkTarget < Main.maxNPCs ? Main.npc[LinkTarget] : null;
            if (target == null || !target.active)
            {
                BreakLink();
                _lastLife = Player.statLife;
                return;
            }

            int lifeDelta = _lastLife - Player.statLife;
            if (lifeDelta > 0)
            {
                float damage = lifeDelta;
                target.SimpleStrikeNPC((int)damage, 0, false, 0f, DamageClass.Default);

                if (Player.whoAmI == Main.myPlayer)
                {
                    CombatText.NewText(target.Hitbox, Color.Pink, (int)damage, false, false);
                }
            }

            int healDelta = Player.statLife - _lastLife;
            if (healDelta > 0)
            {
                _lastHealAccumulator += healDelta;
                if (_lastHealAccumulator > 0)
                {
                    float healAsDamage = _lastHealAccumulator;
                    target.SimpleStrikeNPC((int)healAsDamage, 0, false, 0f, DamageClass.Default);

                    if (Player.whoAmI == Main.myPlayer)
                    {
                        CombatText.NewText(target.Hitbox, Color.HotPink, (int)healAsDamage, false, false);
                    }
                    _lastHealAccumulator = 0;
                }
            }

            _lastLife = Player.statLife;

            if (Player.whoAmI == Main.myPlayer)
            {
                Vector2 playerCenter = Player.Center;
                Vector2 targetCenter = target.Center;
                float dist = Vector2.Distance(playerCenter, targetCenter);
                for (int i = 0; i < 3; i++)
                {
                    float t = Main.rand.NextFloat();
                    Vector2 pos = Vector2.Lerp(playerCenter, targetCenter, t);
                    var d = Dust.NewDustDirect(pos, 0, 0, DustID.PinkTorch);
                    d.velocity *= 0.2f;
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                }
            }
        }

        private void BreakLink()
        {
            LinkTarget = -1;
            LinkTimer = 0;
            _lastHealAccumulator = 0;

            int linkBuffType = ModContent.BuffType<RenGuLinkBuff>();
            int linkedBuffType = ModContent.BuffType<RenGuLinkedBuff>();
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (Player.buffType[i] == linkBuffType)
                {
                    Player.DelBuff(i);
                    break;
                }
            }

            if (Player.whoAmI == Main.myPlayer)
            {
                Text.ShowTextRed(Player, "仁蛊链接已断开");
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!IsLinked) return;

            NPC target = LinkTarget >= 0 && LinkTarget < Main.maxNPCs ? Main.npc[LinkTarget] : null;
            if (target != null && target.active)
            {
                int mirrorDamage = info.Damage;
                target.SimpleStrikeNPC(mirrorDamage, 0, false, 0f, DamageClass.Default);

                if (Player.whoAmI == Main.myPlayer)
                {
                    CombatText.NewText(target.Hitbox, Color.Pink, mirrorDamage, false, false);
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["linkTarget"] = LinkTarget;
            tag["linkTimer"] = LinkTimer;
        }

        public override void LoadData(TagCompound tag)
        {
            LinkTarget = tag.GetInt("linkTarget");
            LinkTimer = tag.GetInt("linkTimer");
        }
    }
}
