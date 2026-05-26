using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class SongShengBuff : ModBuff
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
            player.GetModPlayer<SongShengPlayer>().SongShengActive = true;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.AncientLight);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }

    public class SongShengPlayer : ModPlayer
    {
        public bool SongShengActive { get; set; }
        public Vector2 StoredPosition { get; set; }
        public int StoredHP { get; set; }
        public bool HasStoredState { get; set; }
        private bool _hasRevivedThisBuff;

        public override void ResetEffects()
        {
            SongShengActive = false;
        }

        public void StoreState(Player player)
        {
            StoredPosition = player.Center;
            StoredHP = player.statLife;
            HasStoredState = true;
            _hasRevivedThisBuff = false;
        }

        public override void UpdateDead()
        {
            if (!SongShengActive || !HasStoredState || _hasRevivedThisBuff)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            _hasRevivedThisBuff = true;
            HasStoredState = false;

            Player.Center = StoredPosition;
            Player.statLife = Player.statLifeMax2 / 2;
            Player.dead = false;
            Player.respawnTimer = 0;

            for (int i = 0; i < 30; i++)
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.AncientLight,
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 0, default, 1.5f);
                d.noGravity = true;
            }

            CombatText.NewText(Player.Hitbox, new Color(255, 215, 0), "送生庙庇佑复活！");
        }

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["HasStoredState"] = HasStoredState;
            tag["StoredHP"] = StoredHP;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            HasStoredState = tag.GetBool("HasStoredState");
            StoredHP = tag.GetInt("StoredHP");
        }
    }
}
