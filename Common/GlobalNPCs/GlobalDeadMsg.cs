using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Common.GlobalNPCs
{
	class GlobalDeadMsg:GlobalNPC
	{
		public override bool CheckDead(NPC npc) {
			if (npc.type == NPCID.SkeletronHead) {
				Main.NewText("愿EZ诅咒你。", new Color(0, 255, 255));
			}
			return base.CheckDead(npc);
		}
	}
}
