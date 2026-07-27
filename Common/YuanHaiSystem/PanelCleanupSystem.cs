using Terraria;
using Terraria.ModLoader;
using InnoVault.UIHandles;
using VerminLordMod.Common.QuestSystem;
using VerminLordMod.Common.QuestSystem.Dialogue;

namespace VerminLordMod.Common.YuanHaiSystem
{
    public class PanelCleanupSystem : ModSystem
    {
        public override void OnWorldUnload()
        {
            // 关闭所有面板并退回暂存物品
            try
            {
                var refining = UIHandleLoader.GetUIHandleOfType<RefiningUI>();
                if (refining != null)
                {
                    refining.ReturnStoredItems();
                    refining.Visible = false;
                }
            }
            catch { }

            try
            {
                var yuanHai = UIHandleLoader.GetUIHandleOfType<YuanHaiUI>();
                if (yuanHai != null) yuanHai.Visible = false;
            }
            catch { }

            try
            {
                var questLog = UIHandleLoader.GetUIHandleOfType<QuestLog>();
                if (questLog != null)
                {
                    var dlg = Main.LocalPlayer?.GetModPlayer<global::VerminLordMod.Common.QuestSystem.Dialogue.DialoguePlayer>();
                    if (dlg != null && dlg.AnalysisSlot != null && !dlg.AnalysisSlot.IsAir)
                    {
                        Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), dlg.AnalysisSlot, dlg.AnalysisSlot.stack);
                        dlg.AnalysisSlot.TurnToAir();
                    }
                    questLog.visible = false;
                }
            }
            catch { }
        }
    }
}
