using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons.One;
using VerminLordMod.Content.Items.Placeable;
using VerminLordMod.Content.Items.Placeable.Furniture;
using VerminLordMod.Content.Biomes;
using VerminLordMod.Content.Items.Weapons.Two;
using VerminLordMod.Content.NPCs.GuMasters;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.Town
{
    /// <summary>
    /// 学堂家老 — 古月家族的城镇NPC蛊师
    /// 继承 GuMasterBase 以使用信念系统，同时保留城镇NPC特性
    /// </summary>
    [AutoloadHead]
    class XueTangJiaLao : GuMasterBase
    {
        public const string ShopName = "Shop";
        private static Profiles.StackedNPCProfile NPCProfile;

        // ===== GuMasterBase 抽象实现 =====
        public override FactionID GetFaction() => FactionID.GuYue;
        public override GuRank GetRank() => GuRank.Zhuan3_Chu; // 三转初阶蛊师
        public override GuPersonality GetPersonality() => GuPersonality.Benevolent; // 仁慈

        public override string GuMasterDisplayName => "学堂家老";
        public override int GuMasterDamage => 100;
        public override int GuMasterLife => 250;
        public override int GuMasterDefense => 15;

        public override void SetStaticDefaults()
        {
            // 先调用基类设置（帧数、攻击参数等）
            base.SetStaticDefaults();

            // 学堂家老特定设置
            NPCID.Sets.DangerDetectRange[Type] = 700;
            NPCID.Sets.AttackType[Type] = 2; // 魔法攻击
            NPCID.Sets.AttackTime[Type] = 90;
            NPCID.Sets.AttackAverageChance[Type] = 30;

            var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Velocity = 1f,
                Direction = 1
            };
            // 基类已添加，这里覆盖
            if (NPCID.Sets.NPCBestiaryDrawOffset.ContainsKey(Type))
                NPCID.Sets.NPCBestiaryDrawOffset[Type] = drawModifiers;
            else
                NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

            NPC.Happiness
                .SetBiomeAffection<ForestBiome>(AffectionLevel.Like)
                .SetBiomeAffection<GuYueCompoundBiome>(AffectionLevel.Love)
                .SetBiomeAffection<DesertBiome>(AffectionLevel.Dislike)
                .SetBiomeAffection<DungeonBiome>(AffectionLevel.Hate)
                .SetNPCAffection(NPCID.Dryad, AffectionLevel.Love)
                .SetNPCAffection(NPCID.Guide, AffectionLevel.Like)
                .SetNPCAffection(NPCID.Angler, AffectionLevel.Dislike)
                .SetNPCAffection(ModContent.NPCType<BaiA>(), AffectionLevel.Hate);

            NPCProfile = new Profiles.StackedNPCProfile(
                new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"));
        }

        public override void SetDefaults()
        {
            // 基类 SetDefaults 设置基础属性
            base.SetDefaults();

            // 学堂家老覆盖为城镇NPC
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.aiStyle = 7; // 城镇NPC AI（覆盖基类的 -1）
            NPC.knockBackResist = 0.5f;

            // 覆盖基类设置的值
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("古月家族学堂家老"),
                new FlavorTextBestiaryInfoElement("Mods.VerminLordMod.Bestiary.XueTangJiaLao")
            });
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPCID.Sets.NPCBestiaryDrawOffset.TryGetValue(Type, out NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers))
            {
                drawModifiers.Rotation += 0.001f;
                NPCID.Sets.NPCBestiaryDrawOffset.Remove(Type);
                NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
            }
            return true;
        }

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            foreach (var player in Main.ActivePlayers)
            {
                QiPlayer qiPlayer = player.GetModPlayer<QiPlayer>();
                if (qiPlayer.qiEnabled)
                    return true;
            }
            return false;
        }

        public override ITownNPCProfile TownNPCProfile()
        {
            return NPCProfile;
        }

        public override List<string> SetNPCNameList()
        {
            return NameList;
        }

        // ===== 对话系统 =====
        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            var chat = new WeightedRandom<string>();

            chat.Add(Language.GetTextValue("人是万物之灵，蛊是天地之精。在这个世界上存在着成千上万种，数不胜数的蛊。它们就生活在我们的周围，在矿土里，在草丛里，甚至在野兽的体内。"));
            chat.Add(Language.GetTextValue("在人类繁衍生息的过程中，先贤们逐步发现了蛊虫的奥妙。已经开辟空窍，运用本身真元来喂养、炼化、操控这些蛊，达到各种目的的人，我们统称为蛊师。"));
            chat.Add(Language.GetTextValue("蛊师一共有九大境界，从下到上，分别是一转、二转、三转直至九转。每一转大境界中又分初阶、中阶、高阶、巅峰四个小境界。"));
            chat.Add(Language.GetTextValue("四代族长。天资卓越，一直修行到了五转蛊师的境界。要不是那个卑鄙无耻的魔头花酒行者偷袭的话，兴许能晋升成六转蛊师也说不定。唉……"), 0.2);

            return chat;
        }

        public override string GetChat()
        {
            return GetDialogue(NPC, CurrentAttitude);
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
            // 学堂家老不提供弹幕保护切换（他是友方NPC）
            // button2 留空
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                shop = ShopName;
            }
        }

        // ===== 商店 =====
        public override void AddShops()
        {
            var npcShop = new NPCShop(Type, ShopName)
                .Add(new Item(ModContent.ItemType<Moonlight>())
                {
                    shopCustomPrice = 100,
                    shopSpecialCurrency = VerminLordMod.YuanSId
                })
                .Add(new Item(ModContent.ItemType<Minilight>())
                {
                    shopCustomPrice = 50,
                    shopSpecialCurrency = VerminLordMod.YuanSId
                })
                .Add(new Item(ModContent.ItemType<WanShi>())
                {
                    shopCustomPrice = 5,
                    shopSpecialCurrency = VerminLordMod.YuanSId
                })
                .Add(new Item(ModContent.ItemType<ShiningGu>())
                {
                    shopCustomPrice = 1,
                    shopSpecialCurrency = VerminLordMod.YuanSId
                })
                .Add(new Item(ModContent.ItemType<QingMaoStoneBlock>())
                {
                    shopCustomPrice = 5
                })
                .Add(new Item(ModContent.ItemType<QingMaoStoneChair>()))
                .Add(new Item(ModContent.ItemType<QingMaoStoneTable>()))
                .Add(new Item(ModContent.ItemType<QingMaoStoneBed>()))
                .Add(new Item(ModContent.ItemType<QingMaoStoneClock>()))
                .Add(new Item(ModContent.ItemType<QingMaoStoneDoor>()))
                .Add(new Item(ModContent.ItemType<QingMaoStoneSink>()))
                .Add(new Item(ModContent.ItemType<QingMaoStoneToilet>()));
            npcShop.Register();
        }

        public override void ModifyActiveShop(string shopName, Item[] items)
        {
            foreach (Item item in items)
            {
                if (item == null || item.type == ItemID.None)
                    continue;
            }
        }

        public override bool CanGoToStatue(bool toKingStatue) => true;

        // ===== 城镇NPC攻击 =====
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 0;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ModContent.ProjectileType<MoonlightProj>();
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 12f;
            randomOffset = 0f;
        }

        // ===== 同家族保护 =====
        /// <summary>
        /// 防止学堂家老被同家族NPC攻击（如巡逻蛊师）
        /// </summary>
        public override bool CanBeHitByNPC(NPC attacker)
        {
            if (attacker.ModNPC is GuMasterBase otherMaster && otherMaster.GetFaction() == GetFaction())
                return false;
            return base.CanBeHitByNPC(attacker);
        }

        public static List<string> NameList = new List<string>() {
            "古月翰墨","古月庆雪","古月伶俐","古月新","古月昆皓","古月映雪","古月安娴","古月文乐","古月乐章","古月娅童","古月冷松","古月麦冬","古月碧春","古月觅露","古月嘉玉","古月妙晴","古月从筠","古月焱","古月锐锋","古月书萱","古月香春","古月采白","古月舒","古月馨兰","古月梦桐","古月宏壮","古月承","古月香彤","古月碧菡","古月寄南","古月绣文","古月大","古月问夏","古月吉玟","古月含桃","古月清韵","古月亦绿","古月阳阳","古月初阳","古月博厚","古月婉然","古月安荷","古月衍","古月秋蝶","古月思天","古月初兰","古月建树","古月景铄","古月慕蕊","古月晴画","古月绮山","古月绿海","古月浩言","古月晴波","古月思源","古月嘉云","古月秀竹","古月蔼","古月格","古月浩阔","古月懿轩","古月姣","古月访彤","古月轩","古月涵育","古月舞","古月斯琪","古月彦珺","古月晴曦","古月之玉","古月映寒","古月白容","古月乐蓉","古月悦恺","古月傲之","古月央","古月俊逸","古月婉淑","古月德辉","古月如","古月颜","古月芷雪","古月孤容","古月半雪","古月古韵","古月阳煦","古月嘉美","古月善静","古月元绿","古月筠溪","古月谷蕊","古月痴梅","古月荷","古月蔓菁","古月雪羽","古月宁","古月雅静","古月清怡","古月访曼","古月安吉","古月嘉熙","古月良奥","古月峻","古月景龙","古月涵易","古月夜梅","古月千儿","古月晴丽","古月建同","古月恨风"
        };
    }
}
