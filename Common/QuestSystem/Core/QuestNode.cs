using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using InnoVault;

namespace VerminLordMod.Common.QuestSystem.Core
{
    public abstract class QuestNode : VaultType<QuestNode>, ILocalizedModType
    {
        private static readonly Dictionary<string, QuestNode> _quests = [];
        public static IReadOnlyCollection<QuestNode> AllQuests => _quests.Values;

        public virtual string ID => Name;

        public LocalizedText DisplayName { get; protected set; }
        public LocalizedText Description { get; protected set; }
        public LocalizedText DetailedDescription { get; protected set; }

        public Vector2 Position;

        public Vector2 CalculatedPosition
        {
            get
            {
                if (ParentIDs.Count > 0)
                {
                    var parent = GetQuest(ParentIDs[0]);
                    if (parent != null)
                        return parent.CalculatedPosition + Position;
                }
                return Position;
            }
        }

        public List<string> ParentIDs = [];
        public List<string> ChildIDs = [];
        public QuestIconType IconType = QuestIconType.Texture;
        public string IconTexturePath;
        public int IconItemType;
        public int IconNPCType;

        private Asset<Texture2D> _iconTextureCache;
        public List<QuestReward> Rewards = [];
        public List<QuestObjective> Objectives = [];
        public QuestType QuestType;
        public QuestDifficulty Difficulty;

        public bool IsCompleted
        {
            get
            {
                var player = Main.LocalPlayer.GetModPlayer<QLPlayer>();
                return player?.GetQuestData(ID).IsCompleted ?? false;
            }
            set
            {
                var player = Main.LocalPlayer.GetModPlayer<QLPlayer>();
                if (player == null) return;
                var data = player.GetQuestData(ID);
                if (data.IsCompleted != value)
                {
                    data.IsCompleted = value;
                    if (value) OnCompletion();
                }
            }
        }

        public bool HasUnclaimedRewards =>
            IsCompleted && Rewards.Count > 0 && Rewards.Exists(r => !r.Claimed);

        public bool AllRewardsClaimed =>
            !IsCompleted || Rewards.Count == 0 || Rewards.TrueForAll(r => r.Claimed);

        public bool IsUnlocked
        {
            get
            {
                var player = Main.LocalPlayer.GetModPlayer<QLPlayer>();
                return player?.GetQuestData(ID).IsUnlocked ?? false;
            }
            set
            {
                var player = Main.LocalPlayer.GetModPlayer<QLPlayer>();
                if (player == null) return;
                var data = player.GetQuestData(ID);
                if (data.IsUnlocked != value)
                {
                    data.IsUnlocked = value;
                    if (value) OnUnlock();
                }
            }
        }

        public string LocalizationCategory => "QuestLogs.QuestNode";

        protected virtual void OnCompletion()
        {
            if (Main.LocalPlayer.active)
                QuestNotificationSystem.AddNotification(this);

            foreach (var quest in AllQuests)
            {
                if (ChildIDs.Contains(quest.ID) || quest.ParentIDs.Contains(ID))
                    quest.CheckUnlock();
            }
        }

        protected virtual void OnUnlock() { }

        public void CheckUnlock()
        {
            if (IsUnlocked) return;

            bool allDone = true;
            foreach (var pid in ParentIDs)
            {
                var parent = GetQuest(pid);
                if (parent == null || !parent.IsCompleted)
                {
                    allDone = false;
                    break;
                }
            }

            if (allDone) IsUnlocked = true;
        }

        public Texture2D GetIconTexture()
        {
            switch (IconType)
            {
                case QuestIconType.Item when IconItemType > 0:
                    Main.instance.LoadItem(IconItemType);
                    return TextureAssets.Item[IconItemType]?.Value;

                case QuestIconType.NPC when IconNPCType > 0:
                    Main.instance.LoadNPC(IconNPCType);
                    return TextureAssets.Npc[IconNPCType]?.Value;

                case QuestIconType.Texture when !string.IsNullOrEmpty(IconTexturePath)
                    && ModContent.HasAsset(IconTexturePath):
                    if (_iconTextureCache == null || !_iconTextureCache.IsLoaded)
                        _iconTextureCache = ModContent.Request<Texture2D>(IconTexturePath);
                    return _iconTextureCache?.Value;
            }

            return VaultAsset.placeholder3.Value;
        }

        public Rectangle? GetIconSourceRect(Texture2D texture)
        {
            if (texture == null) return null;

            switch (IconType)
            {
                case QuestIconType.Item when IconItemType > 0 && Main.itemAnimations[IconItemType] != null:
                    return Main.itemAnimations[IconItemType].GetFrame(texture);
                case QuestIconType.NPC when IconNPCType > 0:
                    return texture.Frame(1, Main.npcFrameCount[IconNPCType], 0, 0);
                default:
                    return texture.Frame();
            }
        }

        public void SetItemIcon(int itemType)
        {
            IconType = QuestIconType.Item;
            IconItemType = itemType;
        }

        public void SetNPCIcon(int npcType)
        {
            IconType = QuestIconType.NPC;
            IconNPCType = npcType;
        }

        public void SetTextureIcon(string texturePath)
        {
            IconType = QuestIconType.Texture;
            IconTexturePath = texturePath;
        }

        protected void AddParent<T>() where T : QuestNode =>
            ParentIDs.Add(typeof(T).Name);

        protected void AddChild<T>() where T : QuestNode =>
            ChildIDs.Add(typeof(T).Name);

        public static QuestNode GetQuest(string id) =>
            _quests.TryGetValue(id, out var q) ? q : null;

        public static QuestNode GetQuest<T>() where T : QuestNode =>
            GetQuest(typeof(T).Name);

        public override void Unload()
        {
            _quests.Clear();
            _iconTextureCache = null;
        }

        protected sealed override void VaultRegister()
        {
            ModTypeLookup<QuestNode>.Register(this);
            Instances.Add(this);
            _quests.TryAdd(ID, this);
        }

        public override void VaultSetup()
        {
            try { SetStaticDefaults(); }
            catch (Exception ex)
            {
                ModContent.GetInstance<VerminLordMod>().Logger.Error(
                    $"[QuestNode:VaultSetup] {ex.Message}");
            }

            DisplayName ??= this.GetLocalization(nameof(DisplayName), () => Name);
            Description ??= this.GetLocalization(nameof(Description), () => " ");
            DetailedDescription ??= this.GetLocalization(nameof(DetailedDescription), () => " ");

            InitializeRewards();
            for (int i = 0; i < Objectives.Count; i++)
            {
                if (Objectives[i].TargetItemID == 0 && IconType == QuestIconType.Item && IconItemType > 0)
                    Objectives[i].TargetItemID = IconItemType;
                if (Objectives[i].TargetNpcID == 0
                    && Objectives[i].DescriptionStyle == QuestObjectiveDescriptionStyle.DefeatNpc
                    && IconType == QuestIconType.NPC && IconNPCType > 0)
                    Objectives[i].TargetNpcID = IconNPCType;
                Objectives[i].Initialize(this, i);
            }

            PostSetup();
        }

        public void AddReward(int itemType, int amount = 1, LocalizedText text = null)
        {
            if (itemType <= ItemID.None || amount <= 0) return;
            if (Rewards.Any(r => r.ItemType == itemType)) return;
            Rewards.Add(new QuestReward
            {
                ItemType = itemType,
                Amount = amount,
                Description = text
            });
            InitializeRewards();
        }

        public void InitializeRewards()
        {
            for (int i = 0; i < Rewards.Count; i++)
                Rewards[i].Initialize(this, i);
        }

        public void AddDefeatObjective(int npcType = 0)
        {
            Objectives.Add(new QuestObjective
            {
                DescriptionStyle = QuestObjectiveDescriptionStyle.DefeatNpc,
                TargetNpcID = npcType,
                RequiredProgress = 1
            });
        }

        public void AddObtainObjective(int itemType = 0)
        {
            Objectives.Add(new QuestObjective
            {
                DescriptionStyle = QuestObjectiveDescriptionStyle.ObtainItem,
                TargetItemID = itemType,
                RequiredProgress = 1
            });
        }

        public void AddCollectObjective(int amount, int itemType = 0)
        {
            if (amount <= 0) return;
            Objectives.Add(new QuestObjective
            {
                DescriptionStyle = QuestObjectiveDescriptionStyle.CollectItem,
                TargetItemID = itemType,
                RequiredProgress = amount
            });
        }

        public virtual void PostSetup() { }
        public virtual void UpdateByPlayer() { }
        public virtual void CraftedItem(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) { }
        public virtual void OnKillByNPC(NPC npc) { }
        public virtual void OnWorldEnter() { }
        public virtual bool PreDraw(SpriteBatch spriteBatch, Vector2 drawPos, float scale, bool isHovered, float alpha) => true;
        public virtual void PostDraw(SpriteBatch spriteBatch, Vector2 drawPos, float scale, bool isHovered, float alpha) { }
    }

    public enum QuestIconType { Texture, Item, NPC }

    public class QuestReward
    {
        public int ItemType;
        public int Amount;
        public LocalizedText Description;

        private QuestNode _node;
        private int _index;

        public void Initialize(QuestNode node, int index)
        {
            _node = node;
            _index = index;
        }

        public bool Claimed
        {
            get
            {
                if (_node == null) return false;
                var data = Main.LocalPlayer.GetModPlayer<QLPlayer>()?.GetQuestData(_node.ID);
                return data != null && data.RewardsClaimed.Count > _index && data.RewardsClaimed[_index];
            }
            set
            {
                if (_node == null) return;
                var data = Main.LocalPlayer.GetModPlayer<QLPlayer>()?.GetQuestData(_node.ID);
                if (data == null) return;
                while (data.RewardsClaimed.Count <= _index)
                    data.RewardsClaimed.Add(false);
                data.RewardsClaimed[_index] = value;
            }
        }
    }

    public class QuestObjective
    {
        public LocalizedText Description;
        public QuestObjectiveDescriptionStyle DescriptionStyle = QuestObjectiveDescriptionStyle.Custom;
        public int RequiredProgress;
        public int TargetItemID;
        public int TargetNpcID;

        private QuestNode _node;
        private int _index;

        public void Initialize(QuestNode node, int index)
        {
            _node = node;
            _index = index;
        }

        public string GetDisplayText() => QuestObjectiveTemplates.Format(this);

        public int CurrentProgress
        {
            get
            {
                if (_node == null) return 0;
                var data = Main.LocalPlayer.GetModPlayer<QLPlayer>()?.GetQuestData(_node.ID);
                return data != null && data.ObjectiveProgress.Count > _index ? data.ObjectiveProgress[_index] : 0;
            }
            set
            {
                if (_node == null) return;
                var data = Main.LocalPlayer.GetModPlayer<QLPlayer>()?.GetQuestData(_node.ID);
                if (data == null) return;
                while (data.ObjectiveProgress.Count <= _index)
                    data.ObjectiveProgress.Add(0);
                data.ObjectiveProgress[_index] = value;
            }
        }

        public bool IsCompleted => CurrentProgress >= RequiredProgress;
    }

    public enum QuestType { Main, Side, Daily, Achievement }

    public enum QuestDifficulty { Easy, Normal, Hard, Expert, Master }
}
