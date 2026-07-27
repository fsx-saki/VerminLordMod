using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Common.QuestSystem.Dialogue
{
    public class DialogueLoader : ModSystem
    {
        public static DialogueTreeData LoadedTree { get; private set; }

        public override void Load()
        {
            if (Main.netMode == 2) return;

            bool loaded = false;
            try
            {
                // Try reading from DialogueTree.txt at mod source root
                string probe = Path.Combine(Main.SavePath, "..", "ModSources", Mod.Name, "DialogueTree.txt");
                string fullPath = Path.GetFullPath(probe);
                if (File.Exists(fullPath))
                {
                    string text = File.ReadAllText(fullPath, System.Text.Encoding.UTF8);
                    LoadedTree = Parse(text);
                    Mod.Logger.Info("[DialogueLoader] Loaded from " + fullPath);
                    loaded = true;
                }
            }
            catch { }

            if (!loaded)
            {
                LoadedTree = BuildDefaultTree();
                Mod.Logger.Info("[DialogueLoader] Using embedded tree (" + LoadedTree.AllNodes.Count + " nodes)");
            }
        }

        public override void Unload() => LoadedTree = null;

        private static DialogueTreeData Parse(string text)
        {
            var tree = new DialogueTreeData();
            var lines = text.Split('\n');
            string id = null;
            var currentLines = new List<string>();
            string choice = null, next = null;

            foreach (var rawLine in lines)
            {
                string line = rawLine.TrimEnd('\r');
                if (string.IsNullOrEmpty(line) || line.TrimStart().StartsWith("#")) continue;

                var hdr = Regex.Match(line, @"^(\w+):\s*$");
                if (hdr.Success)
                {
                    if (id != null && currentLines.Count > 0) tree.Add(id, currentLines, choice ?? "", next ?? "");
                    id = hdr.Groups[1].Value;
                    currentLines = []; choice = null; next = null;
                    continue;
                }

                var sys = Regex.Match(line, @"^\s*系统:\s*(.+)");
                if (sys.Success) { currentLines.Add(sys.Groups[1].Value.Trim()); continue; }

                var ch = Regex.Match(line, @"^\s*→\s*(.+?)\s*\[→\s*(\w+)\s*\]");
                if (ch.Success) { choice = ch.Groups[1].Value.Trim(); next = ch.Groups[2].Value; continue; }

                var ce = Regex.Match(line, @"^\s*→\s*(.+)");
                if (ce.Success) { choice = ce.Groups[1].Value.Trim(); next = ""; }
            }

            if (id != null && currentLines.Count > 0) tree.Add(id, currentLines, choice ?? "", next ?? "");
            return tree;
        }

        private static DialogueTreeData BuildDefaultTree()
        {
            var tree = new DialogueTreeData();
            tree.Add("start", ["神经链接已建立。突触信号校准完成。欢迎，实验者。", "本次记录编号 VII/LOTUS-9。你现在所处的环境，被本地生态标记为「蛊界」。"], "请介绍本次任务的环境", "ask_where");
            tree.Add("ask_where", ["你现在位于一个编号为 G-07 的异质生物圈——本地称为「蛊界」。", "Latuis指数：适宜。能量浓度：高于基线 5.7 倍。微生物活性：极高。", "这个生态系统的核心特征是一种寄生-共生双向可切换的生命形式：蛊。"], "有意思。请详细说说这种东西", "what_gu");
            tree.Add("what_gu", ["[蛊]是从当地文明信息实体逸散中提取的高频词汇，目前推断为一种具有高度可塑性的泛能体态微生物共生体。", "它们可以寄生于宿主的能量循环系统[元海]，与宿主进行某种程度上的配合。", "你可以把它们理解为——可编程的器官。这是「狭间」计划的核心关注对象。"], "能确定是共生还是寄生吗？", "gu_danger");
            tree.Add("gu_danger", ["蛊与宿主之间存在双向作用关系。低契合度的个体会对载体造成负担。", "部分蛊种具有自我意识残留，可能尝试影响宿主的决策回路。", "不过——这正是我们需要记录的实验数据。遇到异常情况及时汇报。"], "我也想搞来一个玩玩", "get_gu");
            tree.Add("get_gu", ["获取途径包括：野外捕获、从击败的宿主身上剥离、和当地文明进行交易。", "建议从能量反应弱的个体入手。在进一步了解当地文明前，不建议发生大规模冲突。", "获取蛊后，我可以协助你完成初步编码记录。"], "这次投影你能使用多少能力？", "ask_who");
            tree.Add("ask_who", ["我是 VII 型实验辅助人格矩阵，代称「莲」。", "我的核心协议包括三项：一、依据「狭间」协议维持投影。二、记录实验数据。三、根据收集到的信息编码实体。"], "我不是问这个。你能多大程度影响投影世界？", "will_help");
            tree.Add("will_help", ["在当前投影层级下，超限武器、真空技术、量子技术武器被无限期禁用。", "体积空间稳定器、非欧惯性框架、本体论加密核心等生命维持设备可用。", "随着探索深入，物质操纵等技术可在必要时解锁。"], "老师还真是对我不放心啊。「狭间」计划怎么样了？", "what_experiment");
            tree.Add("what_experiment", ["[加载失败]"], "密级不够……算了。听说投影功能升级了？", "what_projection");
            tree.Add("what_projection", ["本次投影相较于 VII/LOTUS-8 有以下改进：", "一、载体稳定性提升，投影漂移概率降低。", "二、虚拟能量循环系统升级，已检测到适配能量系统[元海]。", "三、神经回传带宽拓宽。"], "元海？又是个「修仙」文明？真不想跟那群迷信的家伙打交道", "what_yuanhai");
            tree.Add("what_yuanhai", ["元海——「能量循环系统接入端口」——位于载体的腹下区域。", "它是本地生命形式的能量中枢。激活后将自动吸收环境能量，转化为可用能源。", "无需与当地迷信框架关联。它只是生物能量接口——和电力系统没有本质区别。"], "确认启动元海。", "activate_yuanhai");
            tree.Add("activate_yuanhai", ["指令已确认。[item:YuanHaiBox:莲用能量为你捏合出一个歪歪扭扭的金属盒子。]使用后可激活元海。"], "这种「可用能源」具体指什么？", "what_qi");
            tree.Add("what_qi", ["元海对环境能量压缩后的产物，此地文献称「真元」。即你的操作能量。", "蛊虫的激活、维持、特化都需要消耗真元。", "上限取决于元海扩容程度——需消耗[元石]或其等价物。"], "元石？我也是旅行者吗XD", "what_resources");
            tree.Add("what_resources", ["元石——本地通用能量介质，这个生态圈的硬通货。", "可用于修炼、交易、工艺能源。", "当地流通的银两可作为低效替代品。"], "先看看任务简报吧。", "what_do");
            tree.Add("what_do", ["当前阶段核心任务：", "一、维持投影稳定度。", "二、探索周边，记录基础生态数据。", "三、获取并驯化至少一只蛊，完成首次实体编码。", "四、与本地智慧生命体建立初步交流。"], "这次投影的预计时间是多长？", "go_back");
            tree.Add("go_back", ["本次投影没有预设终止时间。理论上载体可无限期维持。", "但「狭间」协议要求######################9.22231"], "？（刷新一下）", "death_question");
            tree.Add("death_question", ["[加载失败]"], "更新又有bug……唉，天天加班，我又想辞职了。", "refuse");
            tree.Add("refuse", ["已记录。将在下次项目总结会上提交你的离职申请。", "根据历史记录，你说要辞职的平均频率是每次投影 4.7 次。实际离职次数为 0。", "数据不会说谎，实验者。"], "…………你连这个都记？", "silence");
            tree.Add("silence", ["检测到交流回避行为时，我会进入待机模式。", "但根据协议第三条，即使你不主动发起对话，我也需持续提供引导信息。"], "知道了。（结束引导，信息已收录至资料库）", "");
            return tree;
        }
    }

    public class DialogueTreeData
    {
        private readonly Dictionary<string, DialogueNode> _nodes = [];
        private readonly List<string> _orderedIds = [];
        public IReadOnlyList<string> OrderedIds => _orderedIds;
        public IReadOnlyCollection<DialogueNode> AllNodes => _nodes.Values;

        internal void Add(string id, List<string> lines, string choice, string next)
        {
            _nodes[id] = new DialogueNode { Id = id, SystemLines = lines, ChoiceText = choice, NextNodeId = next };
            _orderedIds.Add(id);
        }

        public DialogueNode Get(string id) => _nodes.TryGetValue(id, out var n) ? n : null;
        public int GetIndex(string id) => _orderedIds.IndexOf(id);
    }
}
