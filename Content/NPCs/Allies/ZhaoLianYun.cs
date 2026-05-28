using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.DialogueTreeUI;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.NPCs.Allies
{
    [AutoloadHead]
    public class ZhaoLianYun : GuMasterBase
    {
        private int _attackTimer;
        private int _stealCooldown;
        private bool _dialogueTreeRegistered;
        private static readonly HashSet<int> RegisteredDialogueTreeTypes = new();

        public override string Texture => "VerminLordMod/Content/NPCs/GuMasters/GuYuePatrolGuMaster";

        public override FactionID GetFaction() => FactionID.Scattered;
        public override GuRank GetRank() => GuRank.Zhuan6_Chu;
        public override GuPersonality GetPersonality() => GuPersonality.Clever;

        public override string GuMasterDisplayName => "赵怜云";
        public override int GuMasterDamage => 220;
        public override int GuMasterLife => 30000;
        public override int GuMasterDefense => 100;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 800;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 30;
            NPCID.Sets.AttackAverageChance[Type] = 20;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

            if (!NPCID.Sets.NPCBestiaryDrawOffset.ContainsKey(Type))
            {
                NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Velocity = 1f, Direction = 1 });
            }
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = GuMasterDamage;
            NPC.lifeMax = GuMasterLife;
            NPC.defense = GuMasterDefense;
            NPC.knockBackResist = 0.35f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(0, 60, 0, 0);
            NPC.townNPC = true;
            NPC.friendly = true;
            AnimationType = NPCID.Guide;
            ApplyRankBonuses();
            SetupGuMaster();
        }

        protected virtual void SetupGuMaster()
        {
            _attackTimer = 0;
            _stealCooldown = 0;
        }

        public override void AI()
        {
            base.AI();

            if (!_dialogueTreeRegistered)
            {
                _dialogueTreeRegistered = true;
                if (!RegisteredDialogueTreeTypes.Contains(Type))
                {
                    RegisteredDialogueTreeTypes.Add(Type);
                    RegisterDialogueTree();
                }
            }

            if (_stealCooldown > 0) _stealCooldown--;

            if (CurrentAttitude == GuAttitude.Hostile || (HasBeenHitByPlayer && AggroTimer > 0))
            {
                ExecuteCombatAI();
            }
        }

        private void ExecuteCombatAI()
        {
            NPC.TargetClosest(true);
            var target = Main.player[NPC.target];
            if (!target.active || target.dead) { NPC.velocity *= 0.95f; return; }

            float dist = Vector2.Distance(NPC.Center, target.Center);
            float dir = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = (int)dir;

            NPC.velocity.X = dir * 2.2f;

            if (NPC.collideX && NPC.velocity.Y == 0)
                NPC.velocity.Y = -6f;

            _attackTimer++;
            if (_attackTimer >= 40)
            {
                _attackTimer = 0;
                ExecuteAttack(target);
            }

            if (_stealCooldown <= 0 && dist < 300f)
            {
                StealAttack(target);
                _stealCooldown = 300;
            }
        }

        private void ExecuteAttack(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            Vector2 toTarget = target.Center - NPC.Center;
            int damage = NPC.damage / 3;

            int pattern = Main.rand.Next(3);
            switch (pattern)
            {
                case 0:
                    CombatText.NewText(NPC.getRect(), Color.Purple, "盗天真传！", true);
                    for (int i = -3; i <= 3; i++)
                    {
                        float angle = toTarget.ToRotation() + i * 0.1f;
                        Vector2 vel = angle.ToRotationVector2() * 9f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.ShadowFlame, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 1:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 vel = angle.ToRotationVector2() * 5f;
                        Projectile.NewProjectile(source, NPC.Center, vel, ProjectileID.ShadowFlameKnife, damage, 1f, Main.myPlayer);
                    }
                    break;
                case 2:
                    Vector2 spawnPos = target.Center + new Vector2(Main.rand.Next(-200, 200), -300);
                    Vector2 vel2 = (target.Center - spawnPos).SafeNormalize(Vector2.UnitY) * 8f;
                    Projectile.NewProjectile(source, spawnPos, vel2, ProjectileID.ShadowFlame, damage * 2, 2f, Main.myPlayer);
                    break;
            }
        }

        private void StealAttack(Player target)
        {
            CombatText.NewText(NPC.getRect(), Color.Purple, "偷！", true);

            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            var source = NPC.GetSource_FromAI();
            int damage = NPC.damage / 4;

            Vector2 toTarget = target.Center - NPC.Center;
            Projectile.NewProjectile(source, NPC.Center, toTarget.SafeNormalize(Vector2.UnitY) * 12f,
                ProjectileID.ShadowFlameKnife, damage, 1f, Main.myPlayer);

            target.AddBuff(BuffID.Weak, 120);
            target.AddBuff(BuffID.Slow, 90);

            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(NPC.Center, 20, 20, DustID.PurpleTorch,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                    100, Color.Purple, 1.2f);
            }
        }

        public override string GetDialogue(NPC npc, GuAttitude attitude)
        {
            NumberOfTimesTalkedTo++;
            return attitude switch
            {
                GuAttitude.Hostile => NumberOfTimesTalkedTo switch
                {
                    1 => "盗天前辈的传承，我一定会守护。",
                    2 => "你逼我出手？那就别怪我不客气！",
                    _ => HostileDialogue()
                },
                GuAttitude.Wary => "嘿嘿，你在打什么主意？我可是一眼就能看穿。",
                GuAttitude.Friendly => "哟，朋友！来来来，坐下聊聊，我请你喝酒。",
                GuAttitude.Respectful => "你有点本事！我赵怜云服了。",
                GuAttitude.Contemptuous => "就你？连我一根手指头都偷不过。",
                GuAttitude.Fearful => "你……太强了……我溜了！",
                _ => "盗天前辈的传承，我一定会守护。"
            };
        }

        private string HostileDialogue()
        {
            var lines = new List<string>
            {
                "偷天换日，你防不住我的！",
                "盗天魔尊的传承，岂是等闲？",
                "你的东西，我看上了就是我的！",
                "偷道无痕，你连自己丢了什么都不知道！",
                "赵怜云的名号，可不是白来的！",
            };
            return lines[Main.rand.Next(lines.Count)];
        }

        protected virtual void RegisterDialogueTree()
        {
            var b = new DialogueTreeBuilder("ZhaoLianYun", "greeting");

            b.StartNode("greeting", "赵怜云懒洋洋地靠在墙上，嘴角挂着一抹不羁的笑。")
                .AddOption("谈论偷道", "tou_dao", DialogueOptionType.Teach)
                .AddOption("关于盗天魔尊", "dao_tian", DialogueOptionType.Informative)
                .AddOption("你的故事", "my_story", DialogueOptionType.Informative)
                .AddOption("偷术展示", "steal_show", DialogueOptionType.Steal)
                .AddOption("交易", "trade", DialogueOptionType.Trade)
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            b.StartNode("tou_dao", "赵怜云：\"偷道？嘿嘿，这是最自由的道。偷天、偷地、偷人、偷心——万物皆可偷。但偷道的真谛不是偷，而是——取。\"")
                .AddOption("偷与取的区别？", "steal_vs_take")
                .AddOption("盗天术是什么？", "dao_tian_shu")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("steal_vs_take", "赵怜云：\"偷是暗中窃取，取是光明正大。偷道到了极致，便不再是偷，而是取——天地万物，为我所取。这就是盗天魔尊的道。\"")
                .AddOption("回到偷道", "tou_dao");

            b.StartNode("dao_tian_shu", "赵怜云：\"盗天术？这是盗天魔尊的绝技，我只会皮毛。真正的盗天术，可以偷取敌人的修为、蛊虫、甚至寿命。但那需要九转偷道蛊虫。\"")
                .AddOption("回到偷道", "tou_dao");

            b.StartNode("dao_tian", "赵怜云神色一正：\"盗天魔尊……他是偷道的开创者，也是我的偶像。他偷天偷地，最终偷到了天道的秘密。虽然他最终陨落，但他的传承，由我继承。\"")
                .AddOption("盗天魔尊怎么陨落的？", "dao_tian_fall")
                .AddOption("你继承了多少？", "inherit_how_much")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("dao_tian_fall", "赵怜云：\"盗天魔尊的陨落……他偷了太多不该偷的东西，最终引来了天道的反噬。但他从不后悔——他说，偷道的尽头，就是偷取天道本身。\"")
                .AddOption("回到盗天魔尊", "dao_tian");

            b.StartNode("inherit_how_much", "赵怜云苦笑：\"继承了多少？说实话，不到一成。盗天魔尊的真正传承，需要九转修为才能完全解锁。我现在才六转，差得远呢。\"")
                .AddOption("回到盗天魔尊", "dao_tian");

            b.StartNode("my_story", "赵怜云：\"我的故事？没什么好说的。从小就是孤儿，靠偷盗为生。后来偶然得到了盗天魔尊的传承，才算有了正经的修炼之路。\"")
                .AddOption("你后悔当小偷吗？", "regret_thief")
                .AddOption("回到主菜单", "greeting");

            b.StartNode("regret_thief", "赵怜云大笑：\"后悔？为什么要后悔？盗亦有道，我偷的都是不义之财。再说了，偷道就是我的道，我走我自己的路，管别人怎么说！\"")
                .AddOption("回到我的故事", "my_story");

            b.StartNode("steal_show", "赵怜云嘿嘿一笑：\"想看我的偷术？好，让你开开眼！\"")
                .AddOption("表演一个", "steal_perform")
                .AddOption("算了，我怕丢东西", "greeting");

            b.StartNode("steal_perform", "赵怜云手指一弹，一道紫光闪过：\"看到了吗？你的……嗯，算了，还给你吧。偷朋友的东西，不厚道。\"")
                .AddOption("你偷了什么？！", "greeting");

            b.StartNode("trade", "赵怜云：\"交易？可以。不过我这里的东西嘛……来路可能不太正，你懂的。\"")
                .OpensShop(GuMasterBase.ShopName)
                .AddOption("回到主菜单", "greeting");

            b.StartNode("bye", "赵怜云挥挥手：\"走了？下次见面，小心你的钱包！开玩笑的……大概。\"")
                .AddOption("告辞", "bye", DialogueOptionType.Exit);

            var tree = b.Build();
            tree.NPCType = Type;
            DialogueTreeManager.Instance.RegisterTree(tree);
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (DialogueTreeManager.Instance.HasTree(NPC))
            {
                button = "对话";
                button2 = CurrentAttitude != GuAttitude.Hostile ? "商店" : "";
                return;
            }
            button = "对话";
            button2 = CurrentAttitude != GuAttitude.Hostile ? "商店" : "";
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                if (DialogueTreeManager.Instance.HasTree(NPC))
                {
                    var mgr = DialogueTreeManager.Instance;
                    if (!mgr.HasActiveSession(Main.LocalPlayer))
                        mgr.StartDialogue(NPC, Main.LocalPlayer);

                    var currentText = mgr.GetCurrentNPCText(Main.LocalPlayer);
                    var options = mgr.GetCurrentOptions(Main.LocalPlayer);

                    if (options != null && options.Count > 0)
                    {
                        DialogueTreeUI.Instance.Open(NPC.GivenName, NPCHeadLoader.GetHeadSlot(HeadTexture), currentText ?? "", options);
                    }
                    else
                    {
                        mgr.EndDialogue(Main.LocalPlayer);
                        Main.npcChatText = currentText ?? GetDialogue(NPC, CurrentAttitude);
                    }
                    return;
                }
                Main.npcChatText = GetDialogue(NPC, CurrentAttitude);
            }
            else
            {
                shop = ShopName;
            }
        }

        public override bool CanChat() => CurrentAttitude != GuAttitude.Hostile || NPC.life > NPC.lifeMax * 0.5f;

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

        public override List<string> SetNPCNameList() => new List<string> { "赵怜云" };
    }
}
