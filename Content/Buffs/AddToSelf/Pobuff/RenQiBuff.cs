using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class RenQiBuff : ModBuff
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
            player.GetModPlayer<RenQiPlayer>().HasRenQi = true;
            player.GetDamage(DamageClass.Generic) += 0.05f;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class RenQiPlayer : ModPlayer
    {
        public bool HasRenQi { get; set; }

        public override void ResetEffects()
        {
            HasRenQi = false;
        }
    }

    public class RenQiNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void OnKill(NPC npc)
        {
            Player closestPlayer = null;
            float minDist = float.MaxValue;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && p.GetModPlayer<RenQiPlayer>().HasRenQi)
                {
                    float dist = (p.Center - npc.Center).Length();
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestPlayer = p;
                    }
                }
            }

            if (closestPlayer != null && npc.value > 0f)
            {
                int bonusCoins = (int)(npc.value * 0.10f / 100f);
                if (bonusCoins > 0)
                {
                    for (int i = 0; i < bonusCoins; i++)
                    {
                        Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ItemID.GoldCoin);
                    }
                }
                else
                {
                    int copperBonus = (int)(npc.value * 0.10f);
                    if (copperBonus > 0)
                    {
                        Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ItemID.CopperCoin, copperBonus);
                    }
                }
            }
        }
    }
}
