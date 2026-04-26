using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons;

namespace VerminLordMod.Content.Projectiles.Minions
{
	// This file contains all the code necessary for a minion
	// - ModItem - the weapon which you use to summon the minion with
	// - ModBuff - the icon you can click on to despawn the minion
	// - ModProjectile - the minion itself

	// It is not recommended to put all these classes in the same file. For demonstrations sake they are all compacted together so you get a better overview.
	// To get a better understanding of how everything works together, and how to code minion AI, read the guide: https://github.com/tModLoader/tModLoader/wiki/Basic-Minion-Guide
	// This is NOT an in-depth guide to advanced minion AI

	public class DogControlGuBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true; // This buff won't save when you exit the world
			Main.buffNoTimeDisplay[Type] = true; // The time remaining won't display on this buff
		}

		public override void Update(Player player, ref int buffIndex) {
			// If the minions exist reset the buff time, otherwise remove the buff from the player
			if (player.ownedProjectileCounts[ModContent.ProjectileType<DogControlGuMinion>()] > 0) {
				player.buffTime[buffIndex] = 18000;
			}
			else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}

	class DogControlGuItem : GuWeaponItem
	{
		protected override int controlQiCost => 20;
		protected override int qiCost => 50;
		protected override int _useTime => 26;
		protected override int _guLevel => 1;

		public static LocalizedText UsesXQiText { get; private set; }
		public static LocalizedText ControlRate { get; private set; }
		public static LocalizedText GuLevel { get; private set; }
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			
			tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
			tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
			if (controlRate > 0f) {
				tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
			}
			else {
				tooltips.Add(new TooltipLine(Mod, "ControlRate", "\u53f3\u952e\u4f7f\u7528\u5f00\u59cb\u70bc\u5316"));
			}
		}
		public override void SetStaticDefaults() {

			UsesXQiText = this.GetLocalization("UsesXQi");
			ControlRate = this.GetLocalization("ControlRate");
			GuLevel = this.GetLocalization("GuLevel");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;

			ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f; // The default value is 1, but other values are supported. See the docs for more guidance. 
		}

		public override void SetDefaults() {
			//静态属性
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = 50000;

			//使用属性
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.scale = 1f;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 0f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;

			Item.damage = 20;
			Item.knockBack = 3f;
			Item.rare = ItemRarityID.Cyan;
			Item.UseSound = SoundID.Item44; // What sound should play when using the item
			Item.buffType = ModContent.BuffType<DogControlGuBuff>();
			// No buffTime because otherwise the item tooltip would say something like "1 minute duration"
			Item.shoot = ModContent.ProjectileType<DogControlGuMinion>(); // This item creates the minion projectile
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position
			position = Main.MouseWorld;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(Item.buffType, 2);

			// Minions have to be spawned manually, then have originalDamage assigned to the damage of the summon item
			var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
			projectile.originalDamage = Item.damage;

			// Since we spawned the projectile manually already, we do not need the game to spawn it for ourselves anymore, so return false
			return false;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		//public override void AddRecipes() {
		//	CreateRecipe()
		//		.AddIngredient(ModContent.ItemType<ExampleItem>())
		//		.AddTile(ModContent.TileType<ExampleWorkbench>())
		//		.Register();
		//}
	}

	// This minion shows a few mandatory things that make it behave properly.
	// Its attack pattern is simple: If an enemy is in range of 43 tiles, it will fly to it and deal contact damage
	// If the player targets a certain NPC with right-click, it will fly through tiles to it
	// If it isn't attacking, it will float near the player with minimal movement
	public class DogControlGuMinion : ModProjectile
	{
		public override void SetStaticDefaults() {
			// Sets the amount of Projectile.frames this minion has on its spritesheet
			//Main.projProjectile.frames[Projectile.type] = 4;
			Main.projFrames[Projectile.type] = 15;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			Main.projPet[Projectile.type] = true; // Denotes that this projectile is a pet or minion

			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Make the cultist resistant to this projectile, as it's resistant to all homing projectiles.
		}

		public sealed override void SetDefaults() {
			Projectile.width = 24;
			Projectile.height = 36;
			Projectile.tileCollide = true; // Makes the minion go through tiles freely

			// These below are needed for a minion weapon
			Projectile.friendly = true; // Only controls if it deals damage to enemies on contact (more on that later)
			Projectile.minion = true; // Declares this as a minion (has many effects)
			Projectile.DamageType = DamageClass.Summon; // Declares the damage type (needed for it to deal damage)
			Projectile.minionSlots = 1f; // Amount of slots this minion occupies from the total minion slots available to the player (more on that later)
			Projectile.penetrate = -1; // Needed so the minion doesn't despawn on collision with enemies or tiles

		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}

		// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)
		public override bool MinionContactDamage() {
			return true;
		}

		// The AI of this minion is split into multiple methods to avoid bloat. This method just passes values between calls actual parts of the AI.
		public override void AI() {
			Player owner = Main.player[Projectile.owner];

			if (!CheckActive(owner)) {
				return;
			}

			//GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
			//SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
			//Movement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
			//Visuals();

			Player player4 = Main.player[Projectile.owner];
			if (!player4.active) {
				Projectile.active = false;
				return;
			}
			bool flag19 = true;
			if (flag19) {
				if (player4.dead) {
					player4.pirateMinion = false;
				}
				if (player4.pirateMinion) {
					Projectile.timeLeft = 2;
				}
			}
			Vector2 vector96 = player4.Center;
			if (flag19) {
				vector96.X -= (15 + player4.width / 2) * player4.direction;
				vector96.X -= Projectile.minionPos * 40 * player4.direction;
			}


			bool flag20 = true;
			int num915 = -1;
			float num916 = 450f;
			if (flag19) {
				num916 = 800f;
			}
			int num917 = 15;
			if (Projectile.ai[0] == 0f && flag20) {
				NPC ownerMinionAttackTargetNPC4 = Projectile.OwnerMinionAttackTargetNPC;
				if (ownerMinionAttackTargetNPC4 != null && ownerMinionAttackTargetNPC4.CanBeChasedBy(this)) {
					float num918 = (ownerMinionAttackTargetNPC4.Center - Projectile.Center).Length();
					if (num918 < num916) {
						num915 = ownerMinionAttackTargetNPC4.whoAmI;
						num916 = num918;
					}
				}
				if (num915 < 0) {
					for (int num919 = 0; num919 < 200; num919++) {
						NPC nPC12 = Main.npc[num919];
						if (nPC12.CanBeChasedBy(this)) {
							float num920 = (nPC12.Center - Projectile.Center).Length();
							if (num920 < num916) {
								num915 = num919;
								num916 = num920;
							}
						}
					}
				}
			}
			if (Projectile.ai[0] == 1f) {
				Projectile.tileCollide = false;
				float num921 = 0.2f;
				float num922 = 10f;
				int num923 = 200;
				if (num922 < Math.Abs(player4.velocity.X) + Math.Abs(player4.velocity.Y)) {
					num922 = Math.Abs(player4.velocity.X) + Math.Abs(player4.velocity.Y);
				}
				Vector2 value15 = player4.Center - Projectile.Center;
				float num925 = value15.Length();
				if (num925 > 2000f) {
					Projectile.position = player4.Center - new Vector2(Projectile.width, Projectile.height) / 2f;
				}
				if (num925 < (float)num923 && player4.velocity.Y == 0f && Projectile.position.Y + (float)Projectile.height <= player4.position.Y + (float)player4.height && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
					Projectile.ai[0] = 0f;
					Projectile.netUpdate = true;
					if (Projectile.velocity.Y < -6f) {
						Projectile.velocity.Y = -6f;
					}
				}
				if (num925 >= 60f) {
					value15.Normalize();
					value15 *= num922;
					if (Projectile.velocity.X < value15.X) {
						Projectile.velocity.X = Projectile.velocity.X + num921;
						if (Projectile.velocity.X < 0f) {
							Projectile.velocity.X = Projectile.velocity.X + num921 * 1.5f;
						}
					}
					if (Projectile.velocity.X > value15.X) {
						Projectile.velocity.X = Projectile.velocity.X - num921;
						if (Projectile.velocity.X > 0f) {
							Projectile.velocity.X = Projectile.velocity.X - num921 * 1.5f;
						}
					}
					if (Projectile.velocity.Y < value15.Y) {
						Projectile.velocity.Y = Projectile.velocity.Y + num921;
						if (Projectile.velocity.Y < 0f) {
							Projectile.velocity.Y = Projectile.velocity.Y + num921 * 1.5f;
						}
					}
					if (Projectile.velocity.Y > value15.Y) {
						Projectile.velocity.Y = Projectile.velocity.Y - num921;
						if (Projectile.velocity.Y > 0f) {
							Projectile.velocity.Y = Projectile.velocity.Y - num921 * 1.5f;
						}
					}
				}
				if (Projectile.velocity.X != 0f) {
					Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
				}
				if (flag19) {
					Projectile.frameCounter++;
					if (Projectile.frameCounter > 3) {
						Projectile.frame++;
						Projectile.frameCounter = 0;
					}
					if ((Projectile.frame < 10) | (Projectile.frame > 13)) {
						Projectile.frame = 10;
					}
					Projectile.rotation = Projectile.velocity.X * 0.1f;
				}
			}
			if (Projectile.ai[0] == 2f) {
				Projectile.friendly = true;
				Projectile.spriteDirection = Projectile.direction;
				Projectile.rotation = 0f;
				Projectile.frame = 4 + (int)((float)num917 - Projectile.ai[1]) / (num917 / 3);
				if (Projectile.velocity.Y != 0f) {
					Projectile.frame += 3;
				}
				Projectile.velocity.Y = Projectile.velocity.Y + 0.4f;
				if (Projectile.velocity.Y > 10f) {
					Projectile.velocity.Y = 10f;
				}
				Projectile.ai[1] -= 1f;
				if (Projectile.ai[1] <= 0f) {
					Projectile.ai[1] = 0f;
					Projectile.ai[0] = 0f;
					Projectile.friendly = false;
					Projectile.netUpdate = true;
					return;
				}
			}
			if (num915 >= 0) {
				float num926 = 400f;
				float num927 = 20f;
				if (flag19) {
					num926 = 700f;
				}
				if ((double)Projectile.position.Y > Main.worldSurface * 16.0) {
					num926 *= 0.7f;
				}
				NPC nPC13 = Main.npc[num915];
				Vector2 center10 = nPC13.Center;
				float num928 = (center10 - Projectile.Center).Length();
				Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, nPC13.position, nPC13.width, nPC13.height);
				if (num928 < num926) {
					vector96 = center10;
					if (center10.Y < Projectile.Center.Y - 30f && Projectile.velocity.Y == 0f) {
						float num929 = Math.Abs(center10.Y - Projectile.Center.Y);
						if (num929 < 120f) {
							Projectile.velocity.Y = -10f;
						}
						else if (num929 < 210f) {
							Projectile.velocity.Y = -13f;
						}
						else if (num929 < 270f) {
							Projectile.velocity.Y = -15f;
						}
						else if (num929 < 310f) {
							Projectile.velocity.Y = -17f;
						}
						else if (num929 < 380f) {
							Projectile.velocity.Y = -18f;
						}
					}
				}
				if (num928 < num927) {
					Projectile.ai[0] = 2f;
					Projectile.ai[1] = num917;
					Projectile.netUpdate = true;
				}
			}
			if (Projectile.ai[0] == 0f && num915 < 0) {
				float num930 = 500f;
				if (Main.player[Projectile.owner].rocketDelay2 > 0) {
					Projectile.ai[0] = 1f;
					Projectile.netUpdate = true;
				}
				Vector2 vector101 = player4.Center - Projectile.Center;
				if (vector101.Length() > 2000f) {
					Projectile.position = player4.Center - new Vector2(Projectile.width, Projectile.height) / 2f;
				}
				else if (vector101.Length() > num930 || Math.Abs(vector101.Y) > 300f) {
					Projectile.ai[0] = 1f;
					Projectile.netUpdate = true;
					if (Projectile.velocity.Y > 0f && vector101.Y < 0f) {
						Projectile.velocity.Y = 0f;
					}
					if (Projectile.velocity.Y < 0f && vector101.Y > 0f) {
						Projectile.velocity.Y = 0f;
					}
				}
			}
			if (Projectile.ai[0] == 0f) {
				Projectile.tileCollide = true;
				float num931 = 0.5f;
				float num932 = 4f;
				float num933 = 4f;
				float num934 = 0.1f;
				if (num933 < Math.Abs(player4.velocity.X) + Math.Abs(player4.velocity.Y)) {
					num933 = Math.Abs(player4.velocity.X) + Math.Abs(player4.velocity.Y);
					num931 = 0.7f;
				}
				int num936 = 0;
				bool flag21 = false;
				float num937 = vector96.X - Projectile.Center.X;
				if (Math.Abs(num937) > 5f) {
					if (num937 < 0f) {
						num936 = -1;
						if (Projectile.velocity.X > 0f - num932) {
							Projectile.velocity.X = Projectile.velocity.X - num931;
						}
						else {
							Projectile.velocity.X = Projectile.velocity.X - num934;
						}
					}
					else {
						num936 = 1;
						if (Projectile.velocity.X < num932) {
							Projectile.velocity.X = Projectile.velocity.X + num931;
						}
						else {
							Projectile.velocity.X = Projectile.velocity.X + num934;
						}
					}
					if (!flag19) {
						flag21 = true;
					}
				}
				else {
					Projectile.velocity.X = Projectile.velocity.X * 0.9f;
					if (Math.Abs(Projectile.velocity.X) < num931 * 2f) {
						Projectile.velocity.X = 0f;
					}
				}
				if (num936 != 0) {
					int num940 = (int)(Projectile.position.X + (float)(Projectile.width / 2)) / 16;
					int num941 = (int)Projectile.position.Y / 16;
					num940 += num936;
					num940 += (int)Projectile.velocity.X;
					for (int num942 = num941; num942 < num941 + Projectile.height / 16 + 1; num942++) {
						if (WorldGen.SolidTile(num940, num942)) {
							flag21 = true;
						}
					}
				}
				Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
				if (Projectile.velocity.Y == 0f && flag21) {
					for (int num943 = 0; num943 < 3; num943++) {
						int num947 = (int)(Projectile.position.X + (float)(Projectile.width / 2)) / 16;
						if (num943 == 0) {
							num947 = (int)Projectile.position.X / 16;
						}
						if (num943 == 2) {
							num947 = (int)(Projectile.position.X + (float)Projectile.width) / 16;
						}
						int num949 = (int)(Projectile.position.Y + (float)Projectile.height) / 16;
						//if (WorldGen.SolidTile(num947, num949) || Main.tile[num947, num949].halfBrick() || Main.tile[num947, num949].slope() > 0 || (TileID.Sets.Platforms[Main.tile[num947, num949].type] && Main.tile[num947, num949].active() && !Main.tile[num947, num949].inActive())) {
						if (WorldGen.SolidTile(num947, num949) || Main.tile[num947, num949].IsHalfBlock || Main.tile[num947, num949].Slope > 0 || (TileID.Sets.Platforms[Main.tile[num947, num949].TileType])) {
							try {
								num947 = (int)(Projectile.position.X + (float)(Projectile.width / 2)) / 16;
								num949 = (int)(Projectile.position.Y + (float)(Projectile.height / 2)) / 16;
								num947 += num936;
								num947 += (int)Projectile.velocity.X;
								if (!WorldGen.SolidTile(num947, num949 - 1) && !WorldGen.SolidTile(num947, num949 - 2)) {
									Projectile.velocity.Y = -5.1f;
								}
								else if (!WorldGen.SolidTile(num947, num949 - 2)) {
									Projectile.velocity.Y = -7.1f;
								}
								else if (WorldGen.SolidTile(num947, num949 - 5)) {
									Projectile.velocity.Y = -11.1f;
								}
								else if (WorldGen.SolidTile(num947, num949 - 4)) {
									Projectile.velocity.Y = -10.1f;
								}
								else {
									Projectile.velocity.Y = -9.1f;
								}
							}
							catch {
								Projectile.velocity.Y = -9.1f;
							}
						}
					}
				}
				if (Projectile.velocity.X > num933) {
					Projectile.velocity.X = num933;
				}
				if (Projectile.velocity.X < 0f - num933) {
					Projectile.velocity.X = 0f - num933;
				}
				if (Projectile.velocity.X < 0f) {
					Projectile.direction = -1;
				}
				if (Projectile.velocity.X > 0f) {
					Projectile.direction = 1;
				}
				if (Projectile.velocity.X > num931 && num936 == 1) {
					Projectile.direction = 1;
				}
				if (Projectile.velocity.X < 0f - num931 && num936 == -1) {
					Projectile.direction = -1;
				}
				Projectile.spriteDirection = Projectile.direction;
				if (flag19) {
					Projectile.rotation = 0f;
					if (Projectile.velocity.Y == 0f) {
						if (Projectile.velocity.X == 0f) {
							Projectile.frame = 0;
							Projectile.frameCounter = 0;
						}
						else if (Math.Abs(Projectile.velocity.X) >= 0.5f) {
							Projectile.frameCounter += (int)Math.Abs(Projectile.velocity.X);
							Projectile.frameCounter++;
							if (Projectile.frameCounter > 10) {
								Projectile.frame++;
								Projectile.frameCounter = 0;
							}
							if (Projectile.frame >= 4) {
								Projectile.frame = 0;
							}
						}
						else {
							Projectile.frame = 0;
							Projectile.frameCounter = 0;
						}
					}
					else if (Projectile.velocity.Y != 0f) {
						Projectile.frameCounter = 0;
						Projectile.frame = 14;
					}
				}
				Projectile.velocity.Y = Projectile.velocity.Y + 0.4f;
				if (Projectile.velocity.Y > 10f) {
					Projectile.velocity.Y = 10f;
				}
			}
			if (!flag19) {
				return;
			}
			Projectile.localAI[0] += 1f;
			if (Projectile.velocity.X == 0f) {
				Projectile.localAI[0] += 1f;
			}
			if (Projectile.localAI[0] >= (float)Main.rand.Next(900, 1200)) {
				Projectile.localAI[0] = 0f;
				for (int num950 = 0; num950 < 6; num950++) {
					int num951 = Dust.NewDust(Projectile.Center + Vector2.UnitX * (0f - (float)Projectile.direction) * 8f - Vector2.One * 5f + Vector2.UnitY * 8f, 3, 6, 216, 0f - (float)Projectile.direction, 1f);
					Dust dust43 = Main.dust[num951];
					dust43.velocity /= 2f;
					Main.dust[num951].scale = 0.8f;
				}
				//int num953 = Gore.NewGore(Projectile.Center + Vector2.UnitX * (0f - (float)Projectile.direction) * 8f, Vector2.Zero, Main.rand.Next(580, 583));
				//Gore gore2 = Main.gore[num953];
				//gore2.velocity /= 2f;
				//Main.gore[num953].velocity.Y = Math.Abs(Main.gore[num953].velocity.Y);
				//Main.gore[num953].velocity.X = (0f - Math.Abs(Main.gore[num953].velocity.X)) * (float)Projectile.direction;
			}

		}


		private bool CheckActive(Player owner) {
			if (owner.dead || !owner.active) {
				owner.ClearBuff(ModContent.BuffType<DogControlGuBuff>());

				return false;
			}

			if (owner.HasBuff(ModContent.BuffType<DogControlGuBuff>())) {
				Projectile.timeLeft = 2;
			}

			return true;
		}
	}
}
