using System.Collections.Generic;
using Terraria;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.DialogueTree
{
    /// <summary>
    /// 剧情对话提供者基类 — 为剧情驱动的对话提供便捷基类。
    ///
    /// 设计意图：
    /// - 剧情对话通常有固定的对话树结构，不需要动态生成
    /// - 提供 Builder 模式的便捷封装
    /// - 子类只需实现 BuildTree() 和少量配置属性
    ///
    /// 使用方式：
    /// <code>
    /// public class MyStory : StoryDialogueProvider
    /// {
    ///     protected override string TreeID => "MyStory";
    ///     protected override string DisplayName => "神秘人";
    ///
    ///     protected override DialogueTree BuildTree()
    ///     {
    ///         var b = NewBuilder("greeting");
    ///         b.StartNode("greeting", "你好，旅行者。")
    ///          .AddOption("你是谁？", "who_are_you")
    ///          .AddOption("再见", "bye", DialogueOptionType.Exit);
    ///         // ...
    ///         return b.Build();
    ///     }
    /// }
    /// </code>
    /// </summary>
    public abstract class StoryDialogueProvider : IDialogueProvider
    {
        protected abstract string TreeID { get; }
        protected abstract string DisplayName { get; }
        protected virtual int HeadType => -1;
        protected virtual string GreetingText => $"「{DisplayName}」看着你，等待你开口。";

        private DialogueTree _cachedTree;
        private NPC _npc;

        public void BindToNPC(NPC npc)
        {
            _npc = npc;
        }

        public string GetDisplayName() => DisplayName;

        public int GetHeadType() => HeadType;

        public virtual bool CanTalk(Player player) => true;

        public virtual string GetGreetingText(Player player) => GreetingText;

        public virtual DialogueTree GetDialogueTree(Player player)
        {
            if (_cachedTree == null)
                _cachedTree = BuildTree();
            return _cachedTree;
        }

        public virtual List<DialogueOption> GetDefaultOptions(Player player) => null;

        public virtual BeliefState GetBelief(Player player) => null;

        public NPC GetNPC() => _npc;

        /// <summary>
        /// 子类实现此方法构建对话树。
        /// 只在首次访问时调用一次，结果会被缓存。
        /// </summary>
        protected abstract DialogueTree BuildTree();

        /// <summary>
        /// 创建预配置了 TreeID 的 Builder
        /// </summary>
        protected DialogueTreeBuilder NewBuilder(string rootNodeID)
        {
            return new DialogueTreeBuilder(TreeID, rootNodeID);
        }
    }
}