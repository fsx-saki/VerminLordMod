using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Events;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 选择后果系统 — 关键选择影响NPC态度和世界状态
    ///
    /// 当玩家做出关键选择时，此系统：
    /// 1. 修改相关NPC的好感度/态度
    /// 2. 修改玩家的正道/魔道/中立倾向
    /// 3. 修改因果业力
    /// 4. 触发后续事件
    /// </summary>
    public class ChoiceConsequenceSystem : ModSystem
    {
        public static ChoiceConsequenceSystem Instance => ModContent.GetInstance<ChoiceConsequenceSystem>();

        private bool _subscribed = false;

        /// <summary> 玩家倾向值（正>0=正道，<0=魔道，≈0=中立） </summary>
        private int _playerAlignment;

        /// <summary> 因果业力值 </summary>
        private int _karma;

        /// <summary> NPC好感度修正（NPC类型名→修正值） </summary>
        private readonly Dictionary<string, int> _npcAttitudeModifiers = new();

        public override void PostUpdateWorld()
        {
            if (!_subscribed)
            {
                EventBus.Subscribe<ChoiceMadeEvent>(OnChoiceMade);
                _subscribed = true;
            }
        }

        public override void OnWorldUnload()
        {
            _subscribed = false;
            _playerAlignment = 0;
            _karma = 0;
            _npcAttitudeModifiers.Clear();
        }

        /// <summary> 获取玩家倾向值 </summary>
        public static int GetAlignment() => Instance._playerAlignment;

        /// <summary> 获取倾向描述 </summary>
        public static string GetAlignmentDescription()
        {
            int a = Instance._playerAlignment;
            if (a >= 60) return "正道·坚定";
            if (a >= 30) return "正道·倾向";
            if (a > -30) return "中立";
            if (a > -60) return "魔道·倾向";
            return "魔道·坚定";
        }

        /// <summary> 获取因果业力 </summary>
        public static int GetKarma() => Instance._karma;

        /// <summary> 获取NPC好感度修正 </summary>
        public static int GetNPCAttitudeModifier(string npcTypeName)
        {
            return Instance._npcAttitudeModifiers.TryGetValue(npcTypeName, out var v) ? v : 0;
        }

        /// <summary> 获取所有NPC好感度修正 </summary>
        public static Dictionary<string, int> GetAllModifiers() => new(Instance._npcAttitudeModifiers);

        private void OnChoiceMade(ChoiceMadeEvent e)
        {
            string id = e.ChoiceID;
            int val = e.ChoiceValue;

            switch (id)
            {
                // === Stage1: 贾金生之死 ===
                case "S1_JiaJinSheng":
                    ApplyConsequence(val,
                        positiveNPCs: new[] { ("GuYueFangYuan", 20) },
                        negativeNPCs: val == 1 ? new[] { ("TieXueLeng", -20) } : null,
                        alignment: val == 0 ? 5 : val == 1 ? -5 : 0,
                        karma: 0);
                    break;

                // === Stage2: 血祭之夜 ===
                case "S2_BloodSacrifice":
                    ApplyConsequence(val,
                        positiveNPCs: val == 0 ? new[] { ("GuYueFangYuan", 20) } : null,
                        negativeNPCs: val == 1 ? new[] { ("GuYueFangYuan", -20) } : null,
                        alignment: val == 0 ? -30 : val == 1 ? 30 : 0,
                        karma: val == 0 ? 20 : val == 1 ? -10 : 5);
                    break;

                // === Stage2: 白凝冰 ===
                case "S2_BaiNingBing":
                    ApplyConsequence(val,
                        positiveNPCs: val == 0 ? new[] { ("BaiNingBing", 30) } : null,
                        negativeNPCs: val == 2 ? new[] { ("BaiNingBing", -30) } : null,
                        alignment: val == 0 ? 10 : val == 2 ? -10 : 0,
                        karma: val == 0 ? -10 : 0);
                    break;

                // === Stage3: 商心慈 ===
                case "S3_ShangXinCi":
                    ApplyConsequence(val,
                        positiveNPCs: val == 0 ? new[] { ("ShangXinCi", 15) } : null,
                        negativeNPCs: val == 1 ? new[] { ("ShangXinCi", -5) } : null,
                        alignment: val == 0 ? 5 : val == 1 ? -5 : 0,
                        karma: val == 0 ? -5 : val == 1 ? 5 : 0);
                    break;

                // === Stage4: 方源暴露 ===
                case "S4_FangYuanReveal":
                    ApplyConsequence(val,
                        positiveNPCs: val == 2 ? new[] { ("GuYueFangYuan", 30) } :
                                      val == 1 ? new[] { ("GuYueFangYuan", 10) } : null,
                        negativeNPCs: val == 0 ? new[] { ("GuYueFangYuan", -10) } : null,
                        alignment: val == 0 ? 20 : val == 1 ? -10 : -30,
                        karma: 0);
                    break;

                // === Stage5: 太白云生 ===
                case "S5_TaiBaiYunSheng":
                    ApplyConsequence(val,
                        positiveNPCs: val == 0 ? new[] { ("TaiBaiYunSheng", 30), ("ShangXinCi", 20) } : null,
                        negativeNPCs: val == 2 ? new[] { ("GuYueFangYuan", -30), ("ShangXinCi", -40) } :
                                      val == 1 ? new[] { ("ShangXinCi", -20) } : null,
                        alignment: val == 0 ? 20 : val == 2 ? -30 : 0,
                        karma: val == 0 ? -30 : val == 1 ? 50 : 80);
                    break;

                // === Stage6: 阵营选择 ===
                case "S6_FactionChoice":
                    ApplyConsequence(val,
                        positiveNPCs: val == 0 ? new[] { ("TongGong", 20), ("MeiGong", 20) } :
                                      val == 1 ? new[] { ("YingWuXie", 20), ("QinBaiSheng", 20) } : null,
                        alignment: val == 0 ? 50 : val == 1 ? -50 : 0,
                        karma: 0);
                    break;

                // === Stage7: 结局 ===
                case "S7_Ending":
                    // 结局选择不修改倾向，但影响最终世界状态
                    break;
            }

            // 显示倾向变化通知
            string alignDesc = GetAlignmentDescription();
            Main.NewText($"[蛊世界] 当前倾向：{alignDesc}（因果业力：{_karma}）",
                _playerAlignment > 0 ? Color.Cyan : _playerAlignment < 0 ? Color.OrangeRed : Color.Gray);
        }

        /// <summary>
        /// 应用选择后果
        /// </summary>
        private void ApplyConsequence(int choiceValue,
            (string, int)[] positiveNPCs = null,
            (string, int)[] negativeNPCs = null,
            int alignment = 0,
            int karma = 0)
        {
            // 修改倾向
            _playerAlignment += alignment;
            _playerAlignment = Math.Clamp(_playerAlignment, -100, 100);

            // 修改因果业力
            _karma += karma;

            // 修改NPC好感度
            if (positiveNPCs != null)
            {
                foreach (var (npcName, value) in positiveNPCs)
                {
                    if (!_npcAttitudeModifiers.ContainsKey(npcName))
                        _npcAttitudeModifiers[npcName] = 0;
                    _npcAttitudeModifiers[npcName] += value;
                }
            }

            if (negativeNPCs != null)
            {
                foreach (var (npcName, value) in negativeNPCs)
                {
                    if (!_npcAttitudeModifiers.ContainsKey(npcName))
                        _npcAttitudeModifiers[npcName] = 0;
                    _npcAttitudeModifiers[npcName] += value;
                }
            }
        }
    }
}
