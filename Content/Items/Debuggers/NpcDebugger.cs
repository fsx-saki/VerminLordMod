using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.UIUtils;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.Items.Debuggers
{
    /// <summary>
    /// 开发者调试道具 - 查看NPC的各种数据
    /// 使用后自动检测鼠标悬停的NPC，显示其信念状态、态度、修为等详细信息
    /// </summary>
    class NpcDebugger : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Master;
            Item.maxStack = 1;
            Item.value = 100;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Guitar;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item1;
        }

        public override bool? UseItem(Player player)
        {
            // 检测鼠标悬停的NPC
            NPC target = GetHoveredNPC();
            if (target == null)
            {
                Main.NewText("请将鼠标对准一个NPC后使用此道具。", Color.Yellow);
                return true;
            }

            DisplayNpcInfo(target, player);
            return true;
        }

        /// <summary>
        /// 获取鼠标悬停的NPC
        /// </summary>
        private static NPC GetHoveredNPC()
        {
            // 获取鼠标在游戏世界中的位置
            Vector2 mouseWorld = Main.MouseWorld;

            // 遍历所有NPC，检测鼠标是否在其碰撞箱内
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.getRect().Contains((int)mouseWorld.X, (int)mouseWorld.Y))
                {
                    return npc;
                }
            }

            return null;
        }

        /// <summary>
        /// 显示NPC的详细调试信息
        /// </summary>
        private void DisplayNpcInfo(NPC npc, Player player)
        {
            Main.NewText($"===== NPC 调试信息: {npc.TypeName} (ID:{npc.whoAmI}) =====", Color.Cyan);

            // 基础信息
            Main.NewText($"生命: {npc.life}/{npc.lifeMax} | 伤害: {npc.damage} | 防御: {npc.defense}", Color.White);
            Main.NewText($"位置: ({npc.Center.X:F0}, {npc.Center.Y:F0}) | 方向: {npc.spriteDirection}", Color.White);
            Main.NewText($"友好: {npc.friendly} | 城镇NPC: {npc.townNPC} | AI类型: {npc.aiStyle}", Color.White);

            // 如果是 GuMasterBase 类型
            if (npc.ModNPC is GuMasterBase guMaster)
            {
                DisplayGuMasterInfo(guMaster, npc, player);
            }
            else
            {
                Main.NewText("此NPC不是蛊师类型，无法显示更多数据。", Color.Gray);
            }

            // 玩家侧声望信息
            var worldPlayer = player.GetModPlayer<GuWorldPlayer>();
            Main.NewText($"--- 玩家声望数据 ---", Color.Cyan);
            Main.NewText($"恶名值: {worldPlayer.InfamyPoints} | 声望值: {worldPlayer.FamePoints}", Color.White);

            foreach (var (fid, rel) in worldPlayer.FactionRelations)
            {
                string displayName = WorldStateMachine.GetFactionDisplayName(fid);
                string levelName = GuiEnumHelper.GetRepLevelName(rel.GetLevel());
                Color color = rel.GetLevel() switch
                {
                    RepLevel.Hostile => Color.Red,
                    RepLevel.Unfriendly => Color.Orange,
                    RepLevel.Neutral => Color.Gray,
                    RepLevel.Friendly => Color.Green,
                    RepLevel.Allied => Color.Gold,
                    _ => Color.White
                };
                Main.NewText($"  {displayName}: {levelName} ({rel.ReputationPoints})", color);
            }
        }

        /// <summary>
        /// 显示 GuMasterBase 类型的详细数据
        /// </summary>
        private void DisplayGuMasterInfo(GuMasterBase guMaster, NPC npc, Player player)
        {
            Main.NewText($"--- 蛊师数据 ---", Color.Cyan);

            // 基础属性
            string factionName = WorldStateMachine.GetFactionDisplayName(guMaster.GetFaction());
            string rankName = GuiEnumHelper.GetRankName(guMaster.GetRank());
            string personalityName = GuiEnumHelper.GetPersonalityName(guMaster.GetPersonality());
            Main.NewText($"势力: {factionName} | 修为: {rankName} | 性格: {personalityName}", Color.White);

            // 运行时状态
            string stateName = guMaster.CurrentAIState switch
            {
                GuMasterAIState.Idle => "闲逛",
                GuMasterAIState.Approach => "接近",
                GuMasterAIState.Combat => "战斗",
                GuMasterAIState.Flee => "逃跑(已废弃)",
                GuMasterAIState.Talk => "对话",
                GuMasterAIState.Patrol => "巡逻",
                GuMasterAIState.CallForHelp => "呼叫支援",
                _ => "未知"
            };
            string attitudeName = GuiEnumHelper.GetAttitudeName(guMaster.CurrentAttitude);
            Main.NewText($"当前状态: {stateName} | 态度: {attitudeName}", Color.White);
            Main.NewText($"被攻击标记: {guMaster.HasBeenHitByPlayer} | 仇恨计时: {guMaster.AggroTimer}", Color.White);
            Main.NewText($"弹幕保护: {(guMaster.ProjectileProtectionEnabled ? "开启" : "关闭")}", Color.White);

            // 信念数据
            string playerName = player.name;
            var belief = guMaster.GetBelief(playerName);
            Main.NewText($"--- 信念数据 (对 {playerName}) ---", Color.Cyan);
            Main.NewText($"风险阈值: {belief.RiskThreshold:F2} (0=自信, 1=恐惧)", Color.White);
            Main.NewText($"信心等级: {belief.ConfidenceLevel:F2} | 观察次数: {belief.ObservationCount}", Color.White);
            Main.NewText($"预估实力: {belief.EstimatedPower:F2}", Color.White);
            Main.NewText($"被击败过: {belief.WasDefeated} | 击败过玩家: {belief.HasDefeatedPlayer}", Color.White);
            Main.NewText($"交易过: {belief.HasTraded} | 战斗过: {belief.HasFought}", Color.White);
            Main.NewText($"上次交互日: {belief.LastInteractionDay}", Color.White);
        }

    }
}
