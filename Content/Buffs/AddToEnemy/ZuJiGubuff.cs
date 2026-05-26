using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToEnemy
{
	class ZuJiGubuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = false;
			Main.buffNoTimeDisplay[Type] = false;
			Main.persistentBuff[Type] = false;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			Lighting.AddLight(npc.Center, 0.3f, 0.7f, 0.1f);

			if (Main.rand.NextBool(5))
			{
				var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GreenFairy);
				d.velocity *= 0.3f;
				d.velocity.Y -= 1f;
				d.noGravity = true;
				d.scale = Main.rand.NextFloat(0.8f, 1.2f);
			}
		}
	}
}
