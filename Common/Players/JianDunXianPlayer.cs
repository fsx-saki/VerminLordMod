using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace VerminLordMod.Common.Players
{
    public class JianDunXianPlayer : ModPlayer
    {
        public bool JianDunXianActive { get; set; }
        private const float ParryChance = 0.15f;

        public override void ResetEffects()
        {
            JianDunXianActive = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (JianDunXianActive && Main.rand.NextFloat() < ParryChance)
            {
                modifiers.FinalDamage *= 0f;
                Player.immuneTime = (int)(60);

                for (int i = 0; i < 15; i++)
                {
                    var d = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                        ModContent.DustType<global::VerminLordMod.Content.Dusts.SwordDust>());
                    d.noGravity = true;
                    d.velocity = new Microsoft.Xna.Framework.Vector2(
                        Main.rand.NextFloat(-3f, 3f),
                        Main.rand.NextFloat(-3f, 3f));
                    d.scale = Main.rand.NextFloat(1.0f, 1.5f);
                }

                CombatText.NewText(Player.Hitbox, new Microsoft.Xna.Framework.Color(200, 200, 255), "格挡！");
            }
        }
    }
}
