using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class WanNianDouBuff : ModBuff
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
            player.GetModPlayer<WanNianDouPlayer>().HasWanNianDou = true;

            player.moveSpeed += 0.30f;
            player.GetDamage(DamageClass.Generic) += 0.15f;
            player.statDefense += 10;

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Torch);
                d.noGravity = true;
                d.velocity.Y -= 1f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }

    public class WanNianDouPlayer : ModPlayer
    {
        public bool HasWanNianDou { get; set; }
        private int _fireTimer;

        public override void ResetEffects()
        {
            HasWanNianDou = false;
        }

        public override void PostUpdate()
        {
            if (!HasWanNianDou || Player.whoAmI != Main.myPlayer)
            {
                _fireTimer = 0;
                return;
            }

            _fireTimer++;
            if (_fireTimer % 5 != 0)
                return;

            float speed = Player.velocity.Length();
            if (speed < 1f)
                return;

            Vector2 firePos = Player.Center - Player.velocity.SafeNormalize(Vector2.Zero) * 20f;
            firePos += new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-4f, 12f));

            int fireProj = Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                firePos,
                Vector2.Zero,
                ProjectileID.Fireball,
                20,
                2f,
                Player.whoAmI
            );

            if (fireProj >= 0 && fireProj < Main.maxProjectiles)
            {
                Main.projectile[fireProj].friendly = true;
                Main.projectile[fireProj].hostile = false;
                Main.projectile[fireProj].timeLeft = 60;
                Main.projectile[fireProj].penetrate = 1;
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(firePos, npc.Center);
                if (dist < 30f)
                {
                    npc.AddBuff(BuffID.OnFire, 300);
                }
            }
        }
    }
}
