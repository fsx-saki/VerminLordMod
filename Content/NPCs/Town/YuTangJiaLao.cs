using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Content.NPCs.GuYue;

namespace VerminLordMod.Content.NPCs.Town
{
    /// <summary>
    /// 御堂家老 — 向后兼容包装类
    /// 实际逻辑委托给 GuYueVillager（统一 NPC 类）
    /// 保留此类以确保已有存档引用和 Mod.Call 不受影响
    /// </summary>
    [AutoloadHead]
    public class YuTangJiaLao : GuYueVillager
    {
        public override string HeadTexture => "VerminLordMod/Content/NPCs/Town/YuTangJiaLao_Head";
    }
}
