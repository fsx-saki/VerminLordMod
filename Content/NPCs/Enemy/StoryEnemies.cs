using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.DialogueTree;

namespace VerminLordMod.Content.NPCs.Enemy
{
    /// <summary> 荒兽——青茅山荒野中的基础敌人 </summary>
    public class WildBeast : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
        }

        public override void SetDefaults()
        {
            NPC.width = 36; NPC.height = 28;
            NPC.damage = 18; NPC.defense = 6; NPC.lifeMax = 80;
            NPC.HitSound = SoundID.NPCHit1; NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.4f;
            NPC.value = Item.buyPrice(copper: 50);
            NPC.aiStyle = NPCAIStyleID.Fighter;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("青茅山荒野中常见的野兽，体内偶尔蕴含蛊虫。")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qi = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qi.GuLevel <= 0) return 0f;
            var phase = StoryManager.Instance.GetPhase(spawnInfo.Player);
            if ((int)phase < (int)StoryPhase.Arrival) return 0f;
            if ((int)phase > (int)StoryPhase.LeftQingMao) return 0.02f;
            return 0.15f;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
            if (NPC.frameCounter >= 6) { NPC.frameCounter = 0; NPC.frame.Y += frameHeight; }
            if (NPC.frame.Y >= Main.npcFrameCount[Type] * frameHeight) NPC.frame.Y = 0;
        }

        public override void OnKill()
        {
            if (Main.rand.NextBool(5))
                Item.NewItem(null, NPC.getRect(), ModContent.ItemType<Content.Items.Consumables.YuanS>(), Main.rand.Next(1, 3));
        }
    }

    /// <summary> 毒蛇——南疆荒野中的常见敌人 </summary>
    public class PoisonSnake : ModNPC
    {
        public override void SetStaticDefaults() { Main.npcFrameCount[Type] = 4; }

        public override void SetDefaults()
        {
            NPC.width = 20; NPC.height = 16;
            NPC.damage = 25; NPC.defense = 4; NPC.lifeMax = 60;
            NPC.HitSound = SoundID.NPCHit18; NPC.DeathSound = SoundID.NPCDeath18;
            NPC.knockBackResist = 0.3f;
            NPC.value = Item.buyPrice(copper: 80);
            NPC.aiStyle = NPCAIStyleID.Worm;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Jungle,
                new FlavorTextBestiaryInfoElement("南疆毒蛇，咬伤会导致中毒。")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qi = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qi.GuLevel <= 0) return 0f;
            var phase = StoryManager.Instance.GetPhase(spawnInfo.Player);
            if ((int)phase < (int)StoryPhase.SouthBorderArrival) return 0f;
            return spawnInfo.Player.ZoneJungle ? 0.2f : 0f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Poisoned, 300);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
            if (NPC.frameCounter >= 8) { NPC.frameCounter = 0; NPC.frame.Y += frameHeight; }
            if (NPC.frame.Y >= Main.npcFrameCount[Type] * frameHeight) NPC.frame.Y = 0;
        }

        public override void OnKill()
        {
            if (Main.rand.NextBool(3))
                Item.NewItem(null, NPC.getRect(), ModContent.ItemType<Content.Items.Consumables.YuanS>(), Main.rand.Next(1, 4));
        }
    }

    /// <summary> 冰原狼——北原的凶猛猎食者 </summary>
    public class IceWolf : ModNPC
    {
        public override void SetStaticDefaults() { Main.npcFrameCount[Type] = 6; }

        public override void SetDefaults()
        {
            NPC.width = 36; NPC.height = 30;
            NPC.damage = 55; NPC.defense = 18; NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit5; NPC.DeathSound = SoundID.NPCDeath5;
            NPC.knockBackResist = 0.5f;
            NPC.value = Item.buyPrice(silver: 2);
            NPC.aiStyle = NPCAIStyleID.Fighter;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
                new FlavorTextBestiaryInfoElement("北原冰原狼，群体行动，速度极快。")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qi = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qi.GuLevel <= 0) return 0f;
            var phase = StoryManager.Instance.GetPhase(spawnInfo.Player);
            if ((int)phase < (int)StoryPhase.NorthDesertArrival) return 0f;
            return spawnInfo.Player.ZoneSnow ? 0.18f : 0f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Chilled, 180);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
            if (NPC.frameCounter >= 6) { NPC.frameCounter = 0; NPC.frame.Y += frameHeight; }
            if (NPC.frame.Y >= Main.npcFrameCount[Type] * frameHeight) NPC.frame.Y = 0;
        }

        public override void OnKill()
        {
            Item.NewItem(null, NPC.getRect(), ModContent.ItemType<Content.Items.Consumables.YuanS>(), Main.rand.Next(2, 6));
        }
    }

    /// <summary> 散修劫匪——南疆的敌对蛊师 </summary>
    public class RogueGuMaster : ModNPC
    {
        public override void SetStaticDefaults() { Main.npcFrameCount[Type] = 6; }

        public override void SetDefaults()
        {
            NPC.width = 22; NPC.height = 40;
            NPC.damage = 35; NPC.defense = 10; NPC.lifeMax = 180;
            NPC.HitSound = SoundID.NPCHit1; NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.3f;
            NPC.value = Item.buyPrice(silver: 5);
            NPC.aiStyle = NPCAIStyleID.Fighter;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Jungle,
                new FlavorTextBestiaryInfoElement("南疆散修中的败类，专门劫掠过路蛊师。")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qi = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qi.GuLevel <= 0) return 0f;
            var phase = StoryManager.Instance.GetPhase(spawnInfo.Player);
            if ((int)phase < (int)StoryPhase.SouthBorderArrival) return 0f;
            return spawnInfo.Player.ZoneJungle ? 0.08f : 0f;
        }

        public override void AI()
        {
            NPC.TargetClosest();
            Player target = Main.player[NPC.target];
            if (target != null && target.active && !target.dead)
            {
                float dist = Vector2.Distance(NPC.Center, target.Center);
                if (dist < 400f && NPC.ai[2] <= 0)
                {
                    Vector2 shootDir = target.Center - NPC.Center;
                    shootDir.Normalize();
                    Projectile.NewProjectile(null, NPC.Center, shootDir * 7f, ProjectileID.ThrowingKnife, 20, 2f);
                    NPC.ai[2] = 90;
                }
            }
            if (NPC.ai[2] > 0) NPC.ai[2]--;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
            if (NPC.frameCounter >= 6) { NPC.frameCounter = 0; NPC.frame.Y += frameHeight; }
            if (NPC.frame.Y >= Main.npcFrameCount[Type] * frameHeight) NPC.frame.Y = 0;
        }

        public override void OnKill()
        {
            Item.NewItem(null, NPC.getRect(), ModContent.ItemType<Content.Items.Consumables.YuanS>(), Main.rand.Next(5, 15));
        }
    }

    /// <summary> 天庭巡逻兵——宿命大战阶段的敌人 </summary>
    public class HeavenPatrol : ModNPC
    {
        public override void SetStaticDefaults() { Main.npcFrameCount[Type] = 6; }

        public override void SetDefaults()
        {
            NPC.width = 22; NPC.height = 42;
            NPC.damage = 70; NPC.defense = 25; NPC.lifeMax = 400;
            NPC.HitSound = SoundID.NPCHit1; NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.4f;
            NPC.value = Item.buyPrice(silver: 10);
            NPC.aiStyle = NPCAIStyleID.Fighter;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("天庭巡逻兵，执行宿命的意志。")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qi = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qi.GuLevel <= 0) return 0f;
            var phase = StoryManager.Instance.GetPhase(spawnInfo.Player);
            if ((int)phase < (int)StoryPhase.DestinyWarBegin) return 0f;
            return 0.1f;
        }

        public override void AI()
        {
            NPC.TargetClosest();
            Player target = Main.player[NPC.target];
            if (target != null && target.active && !target.dead)
            {
                float dist = Vector2.Distance(NPC.Center, target.Center);
                if (dist < 500f && NPC.ai[2] <= 0)
                {
                    Vector2 shootDir = target.Center - NPC.Center;
                    shootDir.Normalize();
                    Projectile.NewProjectile(null, NPC.Center, shootDir * 9f, ProjectileID.HolyWater, 45, 3f);
                    NPC.ai[2] = 70;
                }
            }
            if (NPC.ai[2] > 0) NPC.ai[2]--;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
            if (NPC.frameCounter >= 5) { NPC.frameCounter = 0; NPC.frame.Y += frameHeight; }
            if (NPC.frame.Y >= Main.npcFrameCount[Type] * frameHeight) NPC.frame.Y = 0;
        }

        public override void OnKill()
        {
            Item.NewItem(null, NPC.getRect(), ModContent.ItemType<Content.Items.Consumables.YuanS>(), Main.rand.Next(10, 25));
        }
    }

    /// <summary> 混沌蛊虫——七转后的强力敌人 </summary>
    public class ChaosGuWorm : ModNPC
    {
        public override void SetStaticDefaults() { Main.npcFrameCount[Type] = 4; }

        public override void SetDefaults()
        {
            NPC.width = 30; NPC.height = 24;
            NPC.damage = 90; NPC.defense = 30; NPC.lifeMax = 600;
            NPC.HitSound = SoundID.NPCHit8; NPC.DeathSound = SoundID.NPCDeath8;
            NPC.knockBackResist = 0.6f;
            NPC.value = Item.buyPrice(silver: 20);
            NPC.aiStyle = NPCAIStyleID.Worm;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("混沌蛊虫——升仙后出现的危险生物，蕴含混沌之力。")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            var qi = spawnInfo.Player.GetModPlayer<QiRealmPlayer>();
            if (qi.GuLevel < 7) return 0f;
            var phase = StoryManager.Instance.GetPhase(spawnInfo.Player);
            if ((int)phase < (int)StoryPhase.SevenTurnBegin) return 0f;
            return 0.08f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.Cursed, 120);
            target.AddBuff(BuffID.Ichor, 180);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1;
            if (NPC.frameCounter >= 8) { NPC.frameCounter = 0; NPC.frame.Y += frameHeight; }
            if (NPC.frame.Y >= Main.npcFrameCount[Type] * frameHeight) NPC.frame.Y = 0;
        }

        public override void OnKill()
        {
            Item.NewItem(null, NPC.getRect(), ModContent.ItemType<Content.Items.Consumables.YuanS>(), Main.rand.Next(20, 40));
        }
    }
}
