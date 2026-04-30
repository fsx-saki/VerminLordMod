using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons.Daos;
using VerminLordMod.Content.Projectiles;


namespace VerminLordMod.Content.Items.Weapons.Four
{
	class BloodSkullGu : BloodWeapon//必要继承moditem
	{
		protected override int controlQiCost => 20;
		protected override int qiCost => 10;
		protected override int _useTime => 14;
		protected override int _guLevel => 4;

		//物品属性
		public int deads = 0;
		public override void SetDefaults() {
			//静态属性
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = 50000;

			//使用属性
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = false;
			Item.useTurn = false;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.damage = 100 + deads;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 0;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shoot = ModContent.ProjectileType<BloodSkullProj>();
			Item.shootSpeed = 8f;
		}
		public override bool CanReforge() {
			return false;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				if (hasBeenControlled) {
					var qiTalent = player.GetModPlayer<QiTalentPlayer>();
					var qiRealm = player.GetModPlayer<QiRealmPlayer>();
					if (deads >= 100 && qiTalent.Grade == QiTalentPlayer.TalentGrade.Ding) {
						qiTalent.Grade = QiTalentPlayer.TalentGrade.Bing;
						qiTalent.CalculateEffects();
						qiRealm.OnAwakening();
						for (int i = 0; i <= 50; i++) {
							Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood);
							dust.velocity = Vector2.Normalize(dust.position - player.Center) * 7;
						}
						PunchCameraModifier modifier = new PunchCameraModifier(player.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
						Main.instance.CameraModifiers.Add(modifier);
						Text.ShowTextGreen(player, "资质提升成功！");
					}
					else if (deads >= 200 && qiTalent.Grade == QiTalentPlayer.TalentGrade.Bing) {
						qiTalent.Grade = QiTalentPlayer.TalentGrade.Yi;
						qiTalent.CalculateEffects();
						qiRealm.OnAwakening();
						for (int i = 0; i <= 50; i++) {
							Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood);
							dust.velocity = Vector2.Normalize(dust.position - player.Center) * 7;
						}
						PunchCameraModifier modifier = new PunchCameraModifier(player.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
						Main.instance.CameraModifiers.Add(modifier);
						Text.ShowTextGreen(player, "资质提升成功！");
					}
					else if (deads >= 500 && qiTalent.Grade == QiTalentPlayer.TalentGrade.Yi) {
						qiTalent.Grade = QiTalentPlayer.TalentGrade.Jia;
						qiTalent.CalculateEffects();
						qiRealm.OnAwakening();
						for (int i = 0; i <= 50; i++) {
							Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood);
							dust.velocity = Vector2.Normalize(dust.position - player.Center) * 7;
						}
						PunchCameraModifier modifier = new PunchCameraModifier(player.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 20f, 6f, 20, 1000f, FullName);
						Main.instance.CameraModifiers.Add(modifier);
						Text.ShowTextGreen(player, "资质提升成功！");
					}
					else {
						for (int i = 0; i <= 50; i++) {
							Dust.NewDustDirect(player.position, player.width, player.height, DustID.Dirt);
							Text.ShowTextRed(player, "资质提升失败！当前击杀数可能不足");
						}
					}
				}
				return false;
			}
				
			return true;
		}

		public override void SaveData(TagCompound tag) {
			tag["deads"] = deads;
			tag["controlRate"] = controlRate;
			tag["hasBeenControlled"] = hasBeenControlled;
		}
		public override void LoadData(TagCompound tag) {
			deads = tag.GetInt("deads");
			this.SetDefaults();
			controlRate = tag.GetFloat("controlRate");
			hasBeenControlled = tag.GetBool("hasBeenControlled");
		}
	}
}
