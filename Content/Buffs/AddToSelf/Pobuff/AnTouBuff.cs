using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class AnTouBuff : ModBuff
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
            player.GetModPlayer<AnTouPlayer>().HasAnTou = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, ModContent.DustType<DarkDust>());
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class AnTouPlayer : ModPlayer
    {
        public bool HasAnTou { get; set; }
        private const int DarkRange = 400;

        public override void ResetEffects()
        {
            HasAnTou = false;
        }

        private bool IsInDarkness()
        {
            return Lighting.GetColor(Player.Center.ToTileCoordinates()).R < 0.3f;
        }

        public override void PostUpdate()
        {
            if (!HasAnTou)
                return;

            if (IsInDarkness())
            {
                Player.GetCritChance(DamageClass.Generic) += 15f;

                if (Player.whoAmI == Main.myPlayer)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.active || npc.friendly)
                            continue;

                        float dist = Vector2.Distance(Player.Center, npc.Center);
                        if (dist <= DarkRange)
                        {
                            npc.GetGlobalNPC<AnTouNPC>().IsInDarkness = true;
                        }
                    }
                }
            }
        }
    }

    public class AnTouNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool IsInDarkness { get; set; }

        public override void ResetEffects(NPC npc)
        {
            IsInDarkness = false;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (IsInDarkness)
            {
                modifiers.FinalDamage *= 1.10f;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (IsInDarkness)
            {
                modifiers.FinalDamage *= 1.10f;
            }
        }
    }
}
