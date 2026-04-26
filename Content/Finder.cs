using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace VerminLordMod.Content
{
	public class Finder
	{
		public static NPC FindCloestEnemy(Vector2 position, float maxDistance, Func<NPC, bool> predicate) {
			float maxDis = maxDistance;
			NPC res = null;
			foreach (var npc in Main.npc.Where(n => n.active && !n.friendly && predicate(n))) {
				float dis = Vector2.Distance(position, npc.Center);
				if (dis < maxDis) {
					maxDis = dis;
					res = npc;
				}
			}
			return res;
		}
		public static NPC FindCloestEnemy(Vector2 position, float maxDistance) {
			float maxDis = maxDistance;
			NPC res = null;
			foreach (var npc in Main.npc.Where(n => n.active && !n.friendly)) {
				float dis = Vector2.Distance(position, npc.Center);
				if (dis < maxDis) {
					maxDis = dis;
					res = npc;
				}
			}
			return res;
		}
	}
}
