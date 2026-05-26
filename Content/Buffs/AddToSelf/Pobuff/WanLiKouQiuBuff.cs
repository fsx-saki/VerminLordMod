using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class WanLiKouQiuBuff : ModBuff
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
            player.GetModPlayer<WanLiKouQiuPlayer>().HasWanLiKouQiu = true;
        }
    }

    public class WanLiKouQiuPlayer : ModPlayer
    {
        public bool HasWanLiKouQiu { get; set; }

        public override void ResetEffects()
        {
            HasWanLiKouQiu = false;
        }

        public override void PostUpdateEquips()
        {
            if (!HasWanLiKouQiu)
                return;

            Player.moveSpeed *= 0.5f;
            Player.maxRunSpeed *= 0.5f;
            Player.runAcceleration *= 0.5f;

            Player.wallSpeed += 3f;
            Player.spikedBoots = 2;
            Player.lavaImmune = true;
            Player.buffImmune[BuffID.Suffocation] = true;

            Player.gravity = Player.defaultGravity;
        }

        public override void PostUpdate()
        {
            if (!HasWanLiKouQiu || Player.whoAmI != Main.myPlayer)
                return;

            Point playerTile = Player.Center.ToTileCoordinates();
            if (playerTile.X >= 0 && playerTile.X < Main.maxTilesX &&
                playerTile.Y >= 0 && playerTile.Y < Main.maxTilesY)
            {
                Tile tile = Main.tile[playerTile.X, playerTile.Y];
                if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    Player.position += Player.velocity;
                }
            }

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Mud);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }
}
