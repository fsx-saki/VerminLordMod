using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.GlobalNPCs;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class GuiGuBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<GuiGuPlayer>().HasGuiGuBuff = true;

            for (int i = 0; i < 2; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = 1.0f;
            }
        }
    }

    public class GuiGuPlayer : ModPlayer
    {
        public bool HasGuiGuBuff { get; set; }

        public override void ResetEffects()
        {
            HasGuiGuBuff = false;
        }
    }

    public class GuiGuDropNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.boss || npc.SpawnedFromStatue || npc.friendly)
                return;

            Player player = Main.LocalPlayer;
            var guiGuPlayer = player.GetModPlayer<GuiGuPlayer>();

            if (!guiGuPlayer.HasGuiGuBuff)
                return;

            if (!Main.rand.NextBool(2))
                return;

            var allDrops = GuDropRegistry.GetAllDropItems();
            if (allDrops == null || allDrops.Count == 0)
                return;

            int selectedItem = Main.rand.Next(allDrops.Count);
            int itemId = allDrops[selectedItem];

            int item = Item.NewItem(npc.GetSource_Loot(), npc.getRect(), itemId);
            if (Main.item[item] != null && !Main.item[item].IsAir)
            {
                Main.item[item].noGrabDelay = 0;
            }

            Text.ShowTextGreen(player, "规蛊捕获了一只蛊虫！");
        }
    }
}
