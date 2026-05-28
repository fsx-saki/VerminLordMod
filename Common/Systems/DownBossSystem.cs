using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Systems
{
	public class DownBossSystem:ModSystem
	{
		public static bool downedMinionBoss = false;

		public static bool downedYuanShiXianZun = false;
		public static bool downedXingSuXianZun = false;
		public static bool downedWuJiMoZun = false;
		public static bool downedKuangManMoZun = false;
		public static bool downedHongLianMoZun = false;
		public static bool downedYuanLianXianZun = false;
		public static bool downedDaoTianMoZun = false;
		public static bool downedJuYangXianZun = false;
		public static bool downedYouHunMoZun = false;
		public static bool downedLeTuXianZun = false;

		public override void ClearWorld() {
			downedMinionBoss = false;
			downedYuanShiXianZun = false;
			downedXingSuXianZun = false;
			downedWuJiMoZun = false;
			downedKuangManMoZun = false;
			downedHongLianMoZun = false;
			downedYuanLianXianZun = false;
			downedDaoTianMoZun = false;
			downedJuYangXianZun = false;
			downedYouHunMoZun = false;
			downedLeTuXianZun = false;
		}

		public override void SaveWorldData(TagCompound tag) {
			if (downedMinionBoss) {
				tag["downedMinionBoss"] = true;
			}
			if (downedYuanShiXianZun) tag["downedYuanShiXianZun"] = true;
			if (downedXingSuXianZun) tag["downedXingSuXianZun"] = true;
			if (downedWuJiMoZun) tag["downedWuJiMoZun"] = true;
			if (downedKuangManMoZun) tag["downedKuangManMoZun"] = true;
			if (downedHongLianMoZun) tag["downedHongLianMoZun"] = true;
			if (downedYuanLianXianZun) tag["downedYuanLianXianZun"] = true;
			if (downedDaoTianMoZun) tag["downedDaoTianMoZun"] = true;
			if (downedJuYangXianZun) tag["downedJuYangXianZun"] = true;
			if (downedYouHunMoZun) tag["downedYouHunMoZun"] = true;
			if (downedLeTuXianZun) tag["downedLeTuXianZun"] = true;
		}

		public override void LoadWorldData(TagCompound tag) {
			downedMinionBoss = tag.ContainsKey("downedMinionBoss");
			downedYuanShiXianZun = tag.ContainsKey("downedYuanShiXianZun");
			downedXingSuXianZun = tag.ContainsKey("downedXingSuXianZun");
			downedWuJiMoZun = tag.ContainsKey("downedWuJiMoZun");
			downedKuangManMoZun = tag.ContainsKey("downedKuangManMoZun");
			downedHongLianMoZun = tag.ContainsKey("downedHongLianMoZun");
			downedYuanLianXianZun = tag.ContainsKey("downedYuanLianXianZun");
			downedDaoTianMoZun = tag.ContainsKey("downedDaoTianMoZun");
			downedJuYangXianZun = tag.ContainsKey("downedJuYangXianZun");
			downedYouHunMoZun = tag.ContainsKey("downedYouHunMoZun");
			downedLeTuXianZun = tag.ContainsKey("downedLeTuXianZun");
		}

		public override void NetSend(BinaryWriter writer) {
			var flags = new BitsByte();
			flags[0] = downedMinionBoss;
			flags[1] = downedYuanShiXianZun;
			flags[2] = downedXingSuXianZun;
			flags[3] = downedWuJiMoZun;
			flags[4] = downedKuangManMoZun;
			flags[5] = downedHongLianMoZun;
			flags[6] = downedYuanLianXianZun;
			flags[7] = downedDaoTianMoZun;
			writer.Write(flags);

			var flags2 = new BitsByte();
			flags2[0] = downedJuYangXianZun;
			flags2[1] = downedYouHunMoZun;
			flags2[2] = downedLeTuXianZun;
			writer.Write(flags2);
		}

		public override void NetReceive(BinaryReader reader) {
			BitsByte flags = reader.ReadByte();
			downedMinionBoss = flags[0];
			downedYuanShiXianZun = flags[1];
			downedXingSuXianZun = flags[2];
			downedWuJiMoZun = flags[3];
			downedKuangManMoZun = flags[4];
			downedHongLianMoZun = flags[5];
			downedYuanLianXianZun = flags[6];
			downedDaoTianMoZun = flags[7];

			BitsByte flags2 = reader.ReadByte();
			downedJuYangXianZun = flags2[0];
			downedYouHunMoZun = flags2[1];
			downedLeTuXianZun = flags2[2];
		}
	}
}
