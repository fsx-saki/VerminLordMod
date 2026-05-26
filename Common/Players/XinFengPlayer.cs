using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Players
{
    public class XinFengPlayer : ModPlayer
    {
        private const float RevealRange = 800f;
        private int _revealTimer;

        public override void PostUpdate()
        {
            if (Player.HasBuff(ModContent.BuffType<global::VerminLordMod.Content.Buffs.AddToSelf.Pobuff.XinFengBuff>()))
            {
                _revealTimer++;
                if (_revealTimer % 10 == 0)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                        {
                            float dist = Vector2.Distance(Player.Center, npc.Center);
                            if (dist <= RevealRange)
                            {
                                var d = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                                    ModContent.DustType<global::VerminLordMod.Content.Dusts.WindDust>());
                                d.velocity = new Vector2(0, -1.5f);
                                d.noGravity = true;
                                d.scale = 0.7f;
                                d.alpha = 80;
                            }
                        }
                    }
                }
            }
            else
            {
                _revealTimer = 0;
            }
        }
    }
}
