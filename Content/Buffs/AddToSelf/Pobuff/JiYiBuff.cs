using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JiYiBuff : ModBuff
    {
        private int _infoTimer;

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            _infoTimer++;

            if (_infoTimer % 60 == 0)
            {
                int screenRange = 800;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly)
                        continue;

                    float dist = Vector2.Distance(player.Center, npc.Center);
                    if (dist > screenRange)
                        continue;

                    string hpText = $"HP:{npc.life}/{npc.lifeMax}";
                    CombatText.NewText(npc.Hitbox, Color.Cyan, hpText);

                    string defText = $"DEF:{npc.defense}";
                    CombatText.NewText(npc.Hitbox, Color.LightBlue, defText);
                }
            }

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.IceTorch);
                d.velocity *= 0.3f;
                d.noGravity = true;
            }
        }
    }
}
