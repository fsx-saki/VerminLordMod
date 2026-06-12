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
		public static bool downedEarthVeinGuardian = false;
		public static bool downedHuaJiuXingZhe = false;
		public static bool downedIronBoneKing = false;
		public static bool downedPoisonSnakeKing = false;
		public static bool downedPhantomKing = false;
		public static bool downedFengJiuGe = false;
		public static bool downedQinBaiSheng = false;
		public static bool downedHeiLouLan = false;
		public static bool downedBingSaiChuan = false;
		public static bool downedWuXingDaFaShi = false;
		public static bool downedLongGong = false;
		public static bool defeatedTianHeShangRen = false;
		public static bool defeatedDiMaiGuardian = false;
		public static bool defeatedLongGong = false;
		public static bool downedBloodSeaPhantom = false;
		public static bool destinyShattered = false;
		public static bool hasAscended = false;

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
			downedEarthVeinGuardian = false;
			downedHuaJiuXingZhe = false;
			downedIronBoneKing = false;
			downedPoisonSnakeKing = false;
			downedPhantomKing = false;
			downedFengJiuGe = false;
			downedQinBaiSheng = false;
			downedHeiLouLan = false;
			downedBingSaiChuan = false;
			downedWuXingDaFaShi = false;
			downedLongGong = false;
			defeatedTianHeShangRen = false;
			defeatedDiMaiGuardian = false;
			defeatedLongGong = false;
			downedBloodSeaPhantom = false;
			hasAscended = false;
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
			if (downedEarthVeinGuardian) tag["downedEarthVeinGuardian"] = true;
			if (downedHuaJiuXingZhe) tag["downedHuaJiuXingZhe"] = true;
			if (downedIronBoneKing) tag["downedIronBoneKing"] = true;
			if (downedPoisonSnakeKing) tag["downedPoisonSnakeKing"] = true;
			if (downedPhantomKing) tag["downedPhantomKing"] = true;
			if (downedFengJiuGe) tag["downedFengJiuGe"] = true;
			if (downedQinBaiSheng) tag["downedQinBaiSheng"] = true;
			if (downedHeiLouLan) tag["downedHeiLouLan"] = true;
			if (downedBingSaiChuan) tag["downedBingSaiChuan"] = true;
			if (downedWuXingDaFaShi) tag["downedWuXingDaFaShi"] = true;
			if (downedLongGong) tag["downedLongGong"] = true;
			if (defeatedTianHeShangRen) tag["defeatedTianHeShangRen"] = true;
			if (defeatedDiMaiGuardian) tag["defeatedDiMaiGuardian"] = true;
			if (defeatedLongGong) tag["defeatedLongGong"] = true;
			if (downedBloodSeaPhantom) tag["downedBloodSeaPhantom"] = true;
			if (destinyShattered) tag["destinyShattered"] = true;
			if (hasAscended) tag["hasAscended"] = true;
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
			downedEarthVeinGuardian = tag.ContainsKey("downedEarthVeinGuardian");
			downedHuaJiuXingZhe = tag.ContainsKey("downedHuaJiuXingZhe");
			downedIronBoneKing = tag.ContainsKey("downedIronBoneKing");
			downedPoisonSnakeKing = tag.ContainsKey("downedPoisonSnakeKing");
			downedPhantomKing = tag.ContainsKey("downedPhantomKing");
			downedFengJiuGe = tag.ContainsKey("downedFengJiuGe");
			downedQinBaiSheng = tag.ContainsKey("downedQinBaiSheng");
			downedHeiLouLan = tag.ContainsKey("downedHeiLouLan");
			downedBingSaiChuan = tag.ContainsKey("downedBingSaiChuan");
			downedWuXingDaFaShi = tag.ContainsKey("downedWuXingDaFaShi");
			downedLongGong = tag.ContainsKey("downedLongGong");
			defeatedTianHeShangRen = tag.ContainsKey("defeatedTianHeShangRen");
			defeatedDiMaiGuardian = tag.ContainsKey("defeatedDiMaiGuardian");
			defeatedLongGong = tag.ContainsKey("defeatedLongGong");
			downedBloodSeaPhantom = tag.ContainsKey("downedBloodSeaPhantom");
			destinyShattered = tag.ContainsKey("destinyShattered");
			hasAscended = tag.ContainsKey("hasAscended");
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
			flags2[3] = downedEarthVeinGuardian;
			flags2[4] = downedHuaJiuXingZhe;
			flags2[5] = downedIronBoneKing;
			flags2[6] = downedPoisonSnakeKing;
			flags2[7] = downedPhantomKing;
			writer.Write(flags2);

			var flags3 = new BitsByte();
			flags3[0] = downedFengJiuGe;
			flags3[1] = downedQinBaiSheng;
			flags3[2] = downedHeiLouLan;
			flags3[3] = downedBingSaiChuan;
			flags3[4] = downedWuXingDaFaShi;
			flags3[5] = downedLongGong;
			flags3[6] = downedBloodSeaPhantom;
			flags3[7] = destinyShattered;
			writer.Write(flags3);

			var flags4 = new BitsByte();
			flags4[0] = defeatedTianHeShangRen;
			flags4[1] = defeatedDiMaiGuardian;
			flags4[2] = defeatedLongGong;
			writer.Write(flags4);

			writer.Write(hasAscended);
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
			downedEarthVeinGuardian = flags2[3];
			downedHuaJiuXingZhe = flags2[4];
			downedIronBoneKing = flags2[5];
			downedPoisonSnakeKing = flags2[6];
			downedPhantomKing = flags2[7];

			BitsByte flags3 = reader.ReadByte();
			downedFengJiuGe = flags3[0];
			downedQinBaiSheng = flags3[1];
			downedHeiLouLan = flags3[2];
			downedBingSaiChuan = flags3[3];
			downedWuXingDaFaShi = flags3[4];
			downedLongGong = flags3[5];
			downedBloodSeaPhantom = flags3[6];
			destinyShattered = flags3[7];

			BitsByte flags4 = reader.ReadByte();
			defeatedTianHeShangRen = flags4[0];
			defeatedDiMaiGuardian = flags4[1];
			defeatedLongGong = flags4[2];

			hasAscended = reader.ReadBoolean();
		}
	}
}
