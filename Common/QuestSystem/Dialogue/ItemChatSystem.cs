using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.QuestItems;

namespace VerminLordMod.Common.QuestSystem.Dialogue
{
    public static class ItemChatSystem
    {
        private static readonly List<ItemChatLog> ChatLog = [];

        public static IReadOnlyList<ItemChatLog> Logs => ChatLog.AsReadOnly();

        public static void AnalyzeItem(Item item)
        {
            if (item == null || item.IsAir) return;

            string response = GenerateResponse(item);
            ChatLog.Add(new ItemChatLog
            {
                ItemType = item.type,
                ItemName = item.Name,
                SystemResponse = response
            });

            if (Main.LocalPlayer.active)
                Main.NewText($"[系统分析] {item.Name}: {response}", new Microsoft.Xna.Framework.Color(180, 210, 255));
        }

        private static string GenerateResponse(Item item)
        {
            int type = item.type;

            if (type == ModContent.ItemType<YuanHaiBox>())
                return "截取模拟的投影世界能量制成的物品。外观不影响功能。";

            if (type == ModContent.ItemType<YuanHaiScrap>())
                return "金属外壳残片。能量耗尽后的残留物。可以当作劣质武器使用。";

            if (type == ItemID.DirtBlock)
                return "泥土。成分：硅酸盐矿物混合物。投影世界的基本建材。你拿着它干什么？";

            if (type == ItemID.StoneBlock)
                return "岩石。碳酸钙含量偏高。和基准世界的地壳成分没有显著差异。无聊。";

            if (type == ItemID.Wood || type == ItemID.WoodPlatform)
                return "纤维素基结构材料。当地文明用其建造居所和工具。效率低下但普遍。";

            if (type == ItemID.Torch)
                return "简易照明工具。浸染了含磷化合物的布料包裹在木质握柄上。热辐射效率低，但在缺乏红外视觉的情况下够用。";

            if (type == ItemID.IronPickaxe || type == ItemID.IronAxe)
                return "铁质工具。基本的杠杆-楔形机构组合。用于破碎矿物和砍伐纤维素结构。你打算用它来做什么？";

            if (type == ItemID.SilverCoin || type == ItemID.GoldCoin || type == ItemID.PlatinumCoin)
                return "当地文明流通的贵金属货币。经济媒介。虽然技术层面毫无价值，但在社会层面有实际用途——建议保留。";

            if (type == ItemID.Rope)
                return "编织纤维绳。单股拉伸强度约为 200kg。可用于垂直移动和简易束缚。经典设计，历久弥新。";

            if (type == ItemID.RecallPotion)
                return "【警告：未解析成分】一种混合溶液，摄入后触发空间折叠效应，将使用者传送到预设锚点。基准世界尚未复现此技术。采样优先级：高。";

            if (type == ItemID.LifeCrystal)
                return "【警告：高能量反应】生命能量压缩晶体。可以永久提升载体的组织再生能力。当地文明称之为'心晶'。采样优先级：最高。";

            if (type == ItemID.ManaCrystal)
                return "【注意：能量波动】精神能量凝聚体。可以扩展元海的能量容量。对当前实验有直接价值。";

            if (type == ItemID.CopperOre || type == ItemID.IronOre || type == ItemID.GoldOre || type == ItemID.SilverOre)
                return "金属矿物。需要经过冶炼才能获得可用的金属材料。在这个世界，这些金属可能具有不同的物理特性——值得深入分析。";

            if (type == ItemID.IronBar || type == ItemID.GoldBar || type == ItemID.SilverBar)
                return "金属锭。已经过初步冶炼。纯度一般，但可用于基础工具和装备制作。";

            if (type == ItemID.Mushroom)
                return "真菌生物质。可食用，微毒性。在基准世界也有类似的物种，但这里的样本表现出更强的再生能力。";

            if (type == ItemID.Daybloom)
                return "药用植物。当地文明用来制作基础治疗药剂。光合效率是基准世界同类植物的 1.7 倍。有趣。";

            if (type == ItemID.IronPickaxe || type == ItemID.GoldPickaxe || type == ItemID.SilverPickaxe)
                return "挖掘工具。你拿着它的姿势表明你打算用它来敲什么东西。注意：过度用力可能导致工具断裂。";

            // Generic responses for unrecognized items
            return "未检测到新信息。";
        }
    }

    public class ItemChatLog
    {
        public int ItemType { get; init; }
        public string ItemName { get; init; }
        public string SystemResponse { get; init; }
    }
}
