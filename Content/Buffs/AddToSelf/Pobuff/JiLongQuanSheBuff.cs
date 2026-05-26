using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JiLongQuanSheBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<JiLongQuanShePlayer>().HasJiLongQuanShe = true;
            player.GetDamage(DamageClass.Summon) += 0.05f;
        }
    }

    public class JiLongQuanShePlayer : ModPlayer
    {
        public bool HasJiLongQuanShe { get; set; }
        private int _bunny1 = -1;
        private int _bunny2 = -1;
        private int _bird = -1;

        public override void ResetEffects()
        {
            HasJiLongQuanShe = false;
        }

        public override void PostUpdate()
        {
            if (!HasJiLongQuanShe)
            {
                _bunny1 = -1;
                _bunny2 = -1;
                _bird = -1;
                return;
            }

            if (Player.whoAmI != Main.myPlayer)
                return;

            if (_bunny1 == -1 || !Main.npc[_bunny1].active)
            {
                _bunny1 = NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)Player.Center.X - 30, (int)Player.Center.Y, NPCID.Bunny);
                Main.npc[_bunny1].friendly = true;
                Main.npc[_bunny1].netUpdate = true;
            }

            if (_bunny2 == -1 || !Main.npc[_bunny2].active)
            {
                _bunny2 = NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)Player.Center.X + 30, (int)Player.Center.Y, NPCID.Bunny);
                Main.npc[_bunny2].friendly = true;
                Main.npc[_bunny2].netUpdate = true;
            }

            if (_bird == -1 || !Main.npc[_bird].active)
            {
                _bird = NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)Player.Center.X, (int)Player.Center.Y - 40, NPCID.Bird);
                Main.npc[_bird].friendly = true;
                Main.npc[_bird].netUpdate = true;
            }

            FollowPlayer(_bunny1, -30, 0);
            FollowPlayer(_bunny2, 30, 0);
            FollowPlayer(_bird, 0, -40);
        }

        private void FollowPlayer(int npcIndex, float offsetX, float offsetY)
        {
            if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
                return;

            NPC npc = Main.npc[npcIndex];
            if (!npc.active)
                return;

            Vector2 target = Player.Center + new Vector2(offsetX, offsetY);
            float dist = Vector2.Distance(npc.Center, target);

            if (dist > 100f)
            {
                npc.Center = target;
                npc.netUpdate = true;
            }
            else if (dist > 20f)
            {
                Vector2 dir = target - npc.Center;
                dir.Normalize();
                npc.velocity = dir * 2f;
            }
            else
            {
                npc.velocity *= 0.8f;
            }
        }
    }
}
