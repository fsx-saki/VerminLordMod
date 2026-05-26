using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Players
{
    public class XuQingPlayer : ModPlayer
    {
        public bool IsInvisible { get; set; }
        public bool HasDeceptiveStrike { get; set; }

        public override void ResetEffects()
        {
        }

        public override void Initialize()
        {
            IsInvisible = false;
            HasDeceptiveStrike = false;
        }

        public override void PostUpdate()
        {
            if (!Player.HasBuff(ModContent.BuffType<global::VerminLordMod.Content.Buffs.AddToSelf.Pobuff.XuQingBuff>()))
            {
                IsInvisible = false;
                HasDeceptiveStrike = false;
            }

            if (IsInvisible)
            {
                Player.stealth = 0f;
                Player.aggro -= 750;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (HasDeceptiveStrike)
            {
                modifiers.FinalDamage *= 1.3f;
                HasDeceptiveStrike = false;
                IsInvisible = false;

                for (int i = 0; i < 15; i++)
                {
                    var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Shadowflame);
                    d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.8f, 1.5f);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (IsInvisible)
            {
                IsInvisible = false;
            }
        }
    }
}
