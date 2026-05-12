using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.NPCBehaviors;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    public class GuYueVillager : NPCBehaviorHost
    {
        private GuYueNPCType _npcType;
        private GuYueNPCConfig _config;

        public GuYueNPCType NPCType => _npcType;
        public GuYueNPCConfig Config => _config;

        public override string Texture => "VerminLordMod/Content/NPCs/Town/XueTangJiaLao";

        public GuYueVillager()
        {
            _npcType = GuYueNPCType.Commoner;
        }

        public void SetNPCType(GuYueNPCType type)
        {
            _npcType = type;
            _config = GuYueNPCConfig.GetDefaultConfig(type);
        }

        protected override void RegisterBehaviors()
        {
            _config = GuYueNPCConfig.GetDefaultConfig(_npcType);
            var data = NPCDataRegistry.Get(_npcType);

            RegisterDialogue(data);
            RegisterCombat(data);
            RegisterShop(data);
            RegisterSpawn(data);
            RegisterLoot(data);
        }

        private void RegisterDialogue(NPCDataEntry data)
        {
            string roleName = data.DisplayName;

            Behaviors.Add(new DialogueBehavior((talkCount) =>
            {
                if (talkCount == 1) return roleName + "：" + data.FirstMeeting;
                if (talkCount <= 3) return roleName + "：" + data.CasualTalk;
                return roleName + "：" + data.Farewell;
            }, data.ShopItems.Count > 0, _npcType.ToString() + "Shop"));
        }

        private void RegisterCombat(NPCDataEntry data)
        {
            Behaviors.Add(new CombatBehavior(
                damage: data.Damage,
                knockback: data.Knockback,
                cooldown: data.Cooldown,
                randExtraCooldown: data.RandExtraCooldown,
                projType: data.ProjType,
                attackDelay: data.AttackDelay,
                projSpeed: data.ProjSpeed
            ));
        }

        private void RegisterShop(NPCDataEntry data)
        {
            if (data.ShopItems.Count == 0) return;

            var shop = new ShopBehavior(_npcType.ToString() + "Shop");
            foreach (var item in data.ShopItems)
            {
                shop.AddItem(item.ItemType, item.CustomPrice, item.SpecialCurrency);
            }
            Behaviors.Add(shop);
        }

        private void RegisterSpawn(NPCDataEntry data)
        {
            Behaviors.Add(new SpawnBehavior(data.MinGuLevel, data.MinLife));
        }

        private void RegisterLoot(NPCDataEntry data)
        {
            if (data.LootItems.Count == 0 && data.CoinType == 0) return;

            var loot = new LootBehavior(data.CoinType, data.CoinMin, data.CoinMax);
            foreach (var item in data.LootItems)
            {
                loot.AddItem(item.ItemType, item.MinStack, item.MaxStack, item.Chance);
            }
            Behaviors.Add(loot);
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 40;
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4;

            if (!NPCID.Sets.NPCBestiaryDrawOffset.ContainsKey(Type))
            {
                var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers
                {
                    Velocity = 1f,
                    Direction = 1
                };
                NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
            }
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = _config?.BaseDamage ?? 10;
            NPC.lifeMax = _config?.BaseLife ?? 100;
            NPC.defense = _config?.BaseDefense ?? 5;
            NPC.knockBackResist = 0.5f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = NPCAIStyleID.Passive;
            NPC.townNPC = _config?.IsTownNPC ?? true;
            NPC.friendly = _config?.IsFriendly ?? true;
            AnimationType = NPCID.Guide;
        }

        public override bool CanBeHitByNPC(NPC attacker)
        {
            if (attacker.ModNPC is GuYueVillager other && other._npcType == _npcType)
                return false;
            return base.CanBeHitByNPC(attacker);
        }

        public override bool CanHitNPC(NPC target)
        {
            if (target.ModNPC is GuYueVillager other && other._npcType == _npcType)
                return false;
            return base.CanHitNPC(target);
        }

        public override List<string> SetNPCNameList()
        {
            return GuYueNPCBase.GuYueNameList;
        }
    }
}