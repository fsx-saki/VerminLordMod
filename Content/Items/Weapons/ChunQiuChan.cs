using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Items.Weapons
{
	/// <summary>
	/// 春秋蝉 - 六转宙道本命蛊
	/// <para>核心机制：即死回溯（旋转滑行回记录点）</para>
	/// <para>效果：玩家受到即死伤害时自动触发，防止死亡，旋转着无视方块滑行回记录点</para>
	/// <para>能量：春秋蝉拥有时光能量（0~100），缓慢恢复（约5分钟回满）</para>
	/// <para>回溯时消耗全部能量，恢复对应百分比的生命值和真元</para>
	/// </summary>
	public class ChunQiuChan : GuWeaponItem
	{
		// ===== 蛊虫基础属性 =====
		protected override int qiCost => 50;
		protected override int controlQiCost => 200;
		protected override float unitConntrolRate => 5;
		protected override float uncontrolRate => 0.005f;
		protected override int _useTime => 30;
		protected override int _useStyle => ItemUseStyleID.HoldUp;
		protected override int _guLevel => 6;

		// ===== 时光能量系统 =====

		/// <summary>
		/// 最大时光能量值
		/// </summary>
		public const int MAX_REBIRTH_ENERGY = 100;

		/// <summary>
		/// 能量恢复速率：每 tick 恢复量，300秒（18000 ticks）回满
		/// </summary>
		public const float ENERGY_RECOVERY_RATE = MAX_REBIRTH_ENERGY / 18000f; // ≈ 0.00556/tick

		/// <summary>
		/// 当前时光能量值（0~100），决定回溯时恢复生命和真元的百分比
		/// </summary>
		public float rebirthEnergy = 0f;

		// ===== 位置历史系统（替代固定记录点） =====

		/// <summary>
		/// 位置历史记录缓冲区，存储过去4分钟内玩家经过的随机位置
		/// </summary>
		public List<Vector2> positionHistory = new List<Vector2>();

		/// <summary>
		/// 位置记录计时器，每60 tick（1秒）记录一次
		/// </summary>
		private int positionRecordTimer = 0;

		/// <summary>
		/// 位置历史保留时长（4分钟 = 240秒 = 14400 ticks）
		/// </summary>
		private const int HISTORY_DURATION_TICKS = 14400;

		/// <summary>
		/// 位置记录间隔（1秒 = 60 ticks）
		/// </summary>
		private const int RECORD_INTERVAL = 60;

		/// <summary>
		/// 最大历史记录数（14400 / 60 = 240条）
		/// </summary>
		private const int MAX_HISTORY_COUNT = HISTORY_DURATION_TICKS / RECORD_INTERVAL;

		// ===== 回溯滑行动画状态 =====

		/// <summary>
		/// 是否正在执行回溯滑行
		/// </summary>
		public bool isRebirthSliding = false;

		/// <summary>
		/// 回溯滑行的目标位置
		/// </summary>
		private Vector2 slideTarget = Vector2.Zero;

		/// <summary>
		/// 回溯滑行的起始位置
		/// </summary>
		private Vector2 slideStart = Vector2.Zero;

		/// <summary>
		/// 回溯滑行的总时长（刻），5秒慢动作飞行，展示穿越时间的感觉
		/// </summary>
		private const int SLIDE_DURATION = 300; // 5秒

		/// <summary>
		/// 回溯滑行的当前进度（0~1）
		/// </summary>
		private float slideProgress = 0f;

		/// <summary>
		/// 回溯滑行的旋转角度累积
		/// </summary>
		private float slideRotation = 0f;

		/// <summary>
		/// 回溯滑行的旋转速度（弧度/刻），慢速旋转更明显
		/// </summary>
		private const float ROTATION_SPEED = 0.12f;

		/// <summary>
		/// 回溯滑行的最大速度（像素/刻）
		/// </summary>
		private const float MAX_SLIDE_SPEED = 12f;

		/// <summary>
		/// 垂直浮动幅度（像素），产生旋转飞行感
		/// </summary>
		private const float VERTICAL_BOUNCE_AMPLITUDE = 8f;

		/// <summary>
		/// 时光倒流粒子计时器
		/// </summary>
		private int timeRewindTimer = 0;

		// ===== 物品基础设置 =====
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.rare = ItemRarityID.Purple;
			Item.maxStack = 1;
			Item.value = 1000000;

			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item4;

			Item.damage = 1;
			Item.DamageType = ModContent.GetInstance<InsectDamageClass>();
			Item.knockBack = 0f;
			Item.crit = 0;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.shootSpeed = 0f;
		}

		// ===== 本地化文本 =====
		public static LocalizedText EnergyText { get; private set; }
		public static LocalizedText RebirthTriggeredText { get; private set; }
		public static LocalizedText EnergyLowText { get; private set; }
		public static LocalizedText PositionHistoryText { get; private set; }

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();

			EnergyText = this.GetLocalization("Energy");
			RebirthTriggeredText = this.GetLocalization("RebirthTriggered");
			EnergyLowText = this.GetLocalization("EnergyLow");
			PositionHistoryText = this.GetLocalization("PositionHistory");
		}

		// ===== 工具提示 =====
		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			base.ModifyTooltips(tooltips);

			// 显示时光能量条
			int energyPercent = (int)(rebirthEnergy / MAX_REBIRTH_ENERGY * 100f);
			tooltips.Add(new TooltipLine(Mod, "EnergyStatus",
				EnergyText.Format((int)rebirthEnergy, MAX_REBIRTH_ENERGY, energyPercent)));

			// 显示已记录的位置数量
			tooltips.Add(new TooltipLine(Mod, "PositionHistoryStatus",
				PositionHistoryText.Format(positionHistory.Count)));
		}

		// ===== 右键功能：炼化控制 =====
		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				if (!hasBeenControlled && needCtrl)
				{
					return base.CanUseItem(player);
				}
				return false;
			}

			if (rebirthEnergy < 1f)
			{
				Text.ShowTextRed(player, EnergyLowText.Value);
				return false;
			}

			return base.CanUseItem(player);
		}

		// ===== 使用物品 =====
		public override bool? UseItem(Player player)
		{
			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			qiResource.ConsumeQi(qiCost);

			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(
					player.position, player.width, player.height,
					DustID.MagicMirror, 0f, 0f, 100, default, 1.2f);
				dust.noGravity = true;
				dust.velocity = new Vector2(
					Main.rand.NextFloat(-1f, 1f),
					Main.rand.NextFloat(-1f, 1f));
			}

			return true;
		}

		// ===== 死亡回溯核心逻辑 =====

		/// <summary>
		/// 回溯成功时的死亡模拟消息（金色，消息栏显示）
		/// 意境深刻，含有春秋蝉、时光之河等元素，每次随机显示一句
		/// </summary>
		public static readonly string[] RebirthDeathMessages = new string[]
		{
			"春秋蝉振翅，时光之河倒流，你的命运在此刻被改写……",
			"蝉鸣一声，光阴逆转，你从死亡的缝隙中挣脱而出。",
			"时光之河泛起涟漪，春秋蝉带你溯流而上，重返人间。",
			"岁月长河中的一枚落叶，被春秋蝉轻轻托起，重归枝头。",
			"光阴的丝线断裂又重连，春秋蝉以己身为梭，织就新生。",
			"你听见了时光深处的蝉鸣，那是命运被重新编织的声音。",
			"春秋蝉展开双翼，载着你穿越时光的洪流，回到生之彼岸。",
			"时光之河在此处打了个旋，春秋蝉将你从漩涡中捞起。",
			"蝉翼划过光阴的帷幕，死亡的阴影被生生撕开一道裂隙。",
			"春秋蝉以千年道行，换你一次逆流而上的机会。",
		};

		/// <summary>
		/// 时光能量不足时的哀鸣文本（金色，消息栏显示，追加在正常死亡消息后）
		/// 10选1，意境悲凉
		/// </summary>
		public static readonly string[] EnergyDepletedMessages = new string[]
		{
			"春秋蝉发出一声哀鸣，时光之力已然枯竭，无力回天……",
			"蝉翼垂落，时光之河在此处断流，你坠入永恒的黑暗。",
			"春秋蝉的悲鸣回荡在时光长河中，但再也无法逆转分毫。",
			"蝉声渐息，光阴的齿轮卡死在这一刻，命运无法倒转。",
			"春秋蝉蜷缩在时光的灰烬中，再也无力振翅。",
			"时光之河干涸，春秋蝉的歌声永远沉寂在了岁月的尽头。",
			"蝉翼破碎，光阴的碎片散落一地，再也拼不回你的来路。",
			"春秋蝉低垂着头，时光的余晖从它身上缓缓流逝殆尽。",
			"你感到春秋蝉的脉搏渐渐停止，时光之力已彻底枯竭。",
			"蝉鸣断绝，时光之河改道而去，将你遗弃在死亡的荒原。",
		};

		/// <summary>
		/// 执行死亡回溯
		/// <para>1. 消耗全部时光能量</para>
		/// <para>2. 按能量百分比恢复生命值和真元</para>
		/// <para>3. 清除有害Debuff</para>
		/// <para>4. 从位置历史中随机选取一个位置，启动旋转滑行</para>
		/// </summary>
		public void TriggerRebirth(Player player)
		{
			var qiResource = player.GetModPlayer<QiResourcePlayer>();

			// 1. 获取当前能量百分比
			float energyPercent = rebirthEnergy / MAX_REBIRTH_ENERGY; // 0.0 ~ 1.0

			// 2. 按能量百分比设置生命值（直接设为最大值的 energyPercent 比例）
			int targetLife = (int)(player.statLifeMax2 * energyPercent);
			player.statLife = Math.Min(player.statLifeMax2, targetLife);
			if (targetLife > 0)
			{
				player.HealEffect(targetLife);
			}

			// 3. 按能量百分比设置真元（直接设为最大值的 energyPercent 比例）
			int targetQi = (int)(qiResource.QiMaxCurrent * energyPercent);
			qiResource.QiCurrent = Math.Min(qiResource.QiMaxCurrent, targetQi);

			// 4. 清除有害Debuff
			ClearHarmfulBuffs(player);

			// 5. 从位置历史中随机选取一个目标位置
			Vector2 targetPos;
			if (positionHistory.Count > 0)
			{
				// 随机选取一个历史位置
				int randomIndex = Main.rand.Next(positionHistory.Count);
				targetPos = positionHistory[randomIndex];
			}
			else
			{
				// 无历史记录时回到出生点
				targetPos = new Vector2(
					Main.spawnTileX * 16f,
					Main.spawnTileY * 16f);
			}

			StartRebirthSlide(player, targetPos);

			// 6. 消耗全部时光能量
			rebirthEnergy = 0f;

			// 显示回溯信息
			Text.ShowTextGreen(player, RebirthTriggeredText.Format((int)(energyPercent * 100f)));
		}

		/// <summary>
		/// 启动回溯滑行
		/// </summary>
		private void StartRebirthSlide(Player player, Vector2 target)
		{
			isRebirthSliding = true;
			slideStart = player.position;
			slideTarget = target;
			slideProgress = 0f;
			slideRotation = 0f;
			timeRewindTimer = 0;

			// 播放启动特效
			PlayRebirthEffect(player);
		}

		/// <summary>
		/// 每帧更新回溯滑行状态（由 UpdateInventory 调用）
		/// 直接设置 player.position 实现平滑飞行，不使用 velocity（避免物理干扰）
		/// </summary>
		private void UpdateRebirthSlide(Player player)
		{
			if (!isRebirthSliding || player != Main.LocalPlayer)
				return;

			timeRewindTimer++;

			// 滑行期间保持无敌，防止撞墙/摔落/环境伤害致死
			player.immune = true;
			player.immuneTime = Math.Max(player.immuneTime, 2);
			player.SetImmuneTimeForAllTypes(2);

			// 增加进度
			slideProgress += 1f / SLIDE_DURATION;

			if (slideProgress >= 1f)
			{
				// 滑行结束：精确归位
				isRebirthSliding = false;
				player.position = slideTarget;
				player.velocity = Vector2.Zero;
				player.fallStart = (int)(player.position.Y / 16f);

				// 到达后继续保持0.5秒无敌（30 ticks）
				player.immune = true;
				player.immuneTime = Math.Max(player.immuneTime, 30);
				player.SetImmuneTimeForAllTypes(30);

				// 到达特效
				PlayArrivalEffect(player);
				return;
			}

			// 使用缓动函数：先加速后减速（ease-in-out cubic）
			float eased = EaseInOutCubic(slideProgress);

			// 计算当前位置（线性插值），保持平稳飞行，不添加位置偏移避免视角颠簸
			Vector2 targetPos = Vector2.Lerp(slideStart, slideTarget, eased);

			// 旋转角度累积（仅用于粒子特效，不影响玩家位置）
			slideRotation += ROTATION_SPEED;

			// 直接设置玩家位置（无视碰撞），不使用 velocity，保持视角平稳
			player.position = targetPos;
			player.velocity = Vector2.Zero;
			player.fallStart = (int)(player.position.Y / 16f);

			// 无视碰撞：强制关闭碰撞检测
			player.noKnockback = true;
			player.noFallDmg = true;

			// 计算速度方向用于粒子特效
			Vector2 slideDir = Vector2.Normalize(slideTarget - slideStart);
			float speedFactor = GetSlideSpeedFactor(slideProgress);
			float currentSpeed = MathHelper.Lerp(1.5f, MAX_SLIDE_SPEED, speedFactor);

			// 每帧生成炫酷的时光倒流粒子特效（粒子围绕玩家旋转，视觉上产生飞行感）
			SpawnTimeRewindDust(player, slideDir, currentSpeed);
		}

		/// <summary>
		/// 获取滑行速度因子（0~1），用于先加速后减速
		/// </summary>
		private float GetSlideSpeedFactor(float progress)
		{
			// 前40%加速到最大，后60%减速到0
			if (progress < 0.4f)
			{
				// 加速阶段
				return progress / 0.4f;
			}
			else
			{
				// 减速阶段
				return 1f - (progress - 0.4f) / 0.6f;
			}
		}

		/// <summary>
		/// 缓动函数 ease-in-out cubic
		/// </summary>
		private float EaseInOutCubic(float t)
		{
			return t < 0.5f
				? 4f * t * t * t
				: 1f - (float)Math.Pow(-2f * t + 2f, 3f) / 2f;
		}

		/// <summary>
		/// 生成炫酷的时光倒流粒子特效
		/// <para>包含：金色时光螺旋、蓝色时间残影、彩色时光碎片、路径拖尾</para>
		/// </summary>
		private void SpawnTimeRewindDust(Player player, Vector2 direction, float speed)
		{
			float progress = slideProgress;
			float rot = slideRotation;

			// ===== 1. 金色时光螺旋（每帧生成，环绕玩家旋转） =====
			// 双层螺旋：内层金色火焰，外层蓝色水晶
			for (int ring = 0; ring < 2; ring++)
			{
				int particleCount = (ring == 0) ? 4 : 3;
				float baseRadius = (ring == 0) ? 22f : 38f;
				int dustType = (ring == 0) ? DustID.GoldFlame : DustID.BlueCrystalShard;
				float scale = (ring == 0) ? 1.4f : 1.0f;

				for (int i = 0; i < particleCount; i++)
				{
					float offsetAngle = rot * (ring == 0 ? 2f : 1.5f) + MathHelper.TwoPi * i / particleCount;
					float pulseRadius = baseRadius + (float)Math.Sin(rot * 4f + i * 1.5f) * 8f;
					Vector2 dustPos = player.Center + new Vector2(
						(float)Math.Cos(offsetAngle) * pulseRadius,
						(float)Math.Sin(offsetAngle) * pulseRadius);

					Dust dust = Dust.NewDustPerfect(dustPos, dustType,
						Vector2.Zero, 100, default, scale);
					dust.noGravity = true;
					dust.velocity = direction * -(1.5f + speed * 0.05f)
						+ new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.5f, 0.5f));
					dust.fadeIn = 0.6f;
					dust.rotation = rot + i;
				}
			}

			// ===== 2. 时光倒流残影轨迹（沿路径留下金色粒子带） =====
			if (timeRewindTimer % 2 == 0)
			{
				// 在玩家后方沿路径散布粒子，形成"时间线回缩"效果
				for (int j = 0; j < 3; j++)
				{
					float trailDist = 10f + j * 12f + Main.rand.NextFloat(5f);
					Vector2 trailPos = player.Center - direction * trailDist
						+ new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f));

					Dust trailDust = Dust.NewDustPerfect(trailPos, DustID.MagicMirror,
						Vector2.Zero, 80, default, 0.9f + Main.rand.NextFloat() * 0.4f);
					trailDust.noGravity = true;
					trailDust.velocity = direction * -(2f + Main.rand.NextFloat() * 2f);
					trailDust.fadeIn = 0.4f;
				}
			}

			// ===== 3. 彩色时光碎片（随机闪烁的彩色粒子） =====
			if (timeRewindTimer % 3 == 0)
			{
				for (int k = 0; k < 2; k++)
				{
					float sparkAngle = Main.rand.NextFloat(MathHelper.TwoPi);
					float sparkRadius = Main.rand.NextFloat(15f, 35f);
					Vector2 sparkPos = player.Center + new Vector2(
						(float)Math.Cos(sparkAngle) * sparkRadius,
						(float)Math.Sin(sparkAngle) * sparkRadius);

					Color sparkColor = Color.Lerp(
						new Color(255, 215, 0),  // 金色
						new Color(100, 200, 255), // 天蓝
						Main.rand.NextFloat());

					Dust sparkDust = Dust.NewDustPerfect(sparkPos, DustID.FireworksRGB,
						Vector2.Zero, 100, sparkColor, 0.8f + Main.rand.NextFloat() * 0.5f);
					sparkDust.noGravity = true;
					sparkDust.velocity = new Vector2(
						Main.rand.NextFloat(-1f, 1f),
						Main.rand.NextFloat(-1f, 1f));
					sparkDust.fadeIn = 0.5f;
				}
			}

			// ===== 4. 时间波纹（每5帧在玩家脚下生成扩散波纹） =====
			if (timeRewindTimer % 5 == 0)
			{
				for (int w = 0; w < 6; w++)
				{
					float waveAngle = MathHelper.TwoPi * w / 6f + rot * 0.5f;
					float waveRadius = 15f + progress * 20f;
					Vector2 wavePos = player.Center + new Vector2(
						(float)Math.Cos(waveAngle) * waveRadius,
						(float)Math.Sin(waveAngle) * waveRadius);

					Dust waveDust = Dust.NewDustPerfect(wavePos, DustID.TintableDustLighted,
						Vector2.Zero, 50, new Color(255, 215, 0) * 0.6f, 0.6f);
					waveDust.noGravity = true;
					waveDust.velocity = new Vector2(
						(float)Math.Cos(waveAngle) * 1.5f,
						(float)Math.Sin(waveAngle) * 1.5f);
				}
			}

			// ===== 5. 加速/减速时的速度线特效 =====
			float speedFactor = GetSlideSpeedFactor(progress);
			if (speedFactor > 0.6f && timeRewindTimer % 2 == 0)
			{
				// 高速时生成速度线
				Vector2 speedLinePos = player.Center - direction * Main.rand.NextFloat(20f, 40f)
					+ new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f));
				Dust speedLine = Dust.NewDustPerfect(speedLinePos, DustID.WhiteTorch,
					direction * -(3f + Main.rand.NextFloat() * 3f), 50, default, 0.5f + Main.rand.NextFloat() * 0.3f);
				speedLine.noGravity = true;
			}
		}

		/// <summary>
		/// 播放到达特效 — 时光倒流完成时的华丽爆炸
		/// </summary>
		private void PlayArrivalEffect(Player player)
		{
			// ===== 1. 金色时光粒子大爆发 =====
			for (int i = 0; i < 60; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(4f, 14f);
				Vector2 velocity = new Vector2(
					(float)Math.Cos(angle) * speed,
					(float)Math.Sin(angle) * speed);

				Dust dust = Dust.NewDustPerfect(player.Center, DustID.GoldFlame,
					velocity, 150, default, 2f + Main.rand.NextFloat() * 1f);
				dust.noGravity = true;
				dust.fadeIn = 1.2f;
			}

			// ===== 2. 蓝色时光水晶爆发 =====
			for (int i = 0; i < 30; i++)
			{
				Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
				Dust d = Dust.NewDustPerfect(player.Center + speed * 20f,
					DustID.BlueCrystalShard, speed * 8f, Scale: 1.8f);
				d.noGravity = true;
			}

			// ===== 3. 彩色烟花爆炸 =====
			for (int i = 0; i < 40; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(3f, 10f);
				Vector2 velocity = new Vector2(
					(float)Math.Cos(angle) * speed,
					(float)Math.Sin(angle) * speed);

				Color color = Color.Lerp(
					new Color(255, 215, 0),
					new Color(100, 200, 255),
					Main.rand.NextFloat());

				Dust spark = Dust.NewDustPerfect(player.Center, DustID.FireworksRGB,
					velocity, 100, color, 1.5f + Main.rand.NextFloat() * 0.8f);
				spark.noGravity = true;
				spark.fadeIn = 0.8f;
			}

			// ===== 4. 时光涟漪（扩散光圈） =====
			for (int ring = 0; ring < 3; ring++)
			{
				int count = 12 + ring * 8;
				float baseRadius = 20f + ring * 20f;
				for (int i = 0; i < count; i++)
				{
					float a = MathHelper.TwoPi * i / count;
					float r = baseRadius + Main.rand.NextFloat(-5f, 5f);
					Vector2 dustPos = player.Center + new Vector2(
						(float)Math.Cos(a) * r,
						(float)Math.Sin(a) * r);

					int dustType = (ring % 2 == 0) ? DustID.GoldFlame : DustID.MagicMirror;
					Dust ringDust = Dust.NewDustPerfect(dustPos, dustType,
						Vector2.Zero, 100, default, 1.2f - ring * 0.2f);
					ringDust.noGravity = true;
					ringDust.velocity = (player.Center - dustPos) * 0.2f;
				}
			}

			// ===== 5. 屏幕闪光效果（通过大量白色粒子模拟） =====
			for (int i = 0; i < 20; i++)
			{
				Vector2 flashPos = player.Center + new Vector2(
					Main.rand.NextFloat(-60f, 60f),
					Main.rand.NextFloat(-60f, 60f));
				Dust flash = Dust.NewDustPerfect(flashPos, DustID.WhiteTorch,
					Vector2.Zero, 200, default, 1.5f + Main.rand.NextFloat() * 1f);
				flash.noGravity = true;
				flash.fadeIn = 1.5f;
			}

			Text.ShowTextGreen(player, "春秋蝉·回溯完成！");
		}

		/// <summary>
		/// 清除玩家身上的有害Debuff
		/// </summary>
		private void ClearHarmfulBuffs(Player player)
		{
			int[] buffsToKeep = new int[]
			{
				BuffID.Ironskin,
				BuffID.Regeneration,
				BuffID.Swiftness,
				BuffID.Endurance,
				BuffID.Lifeforce,
				BuffID.Wrath,
				BuffID.Rage,
				BuffID.Inferno,
				BuffID.Shine,
				BuffID.NightOwl,
				BuffID.Hunter,
				BuffID.WellFed,
				BuffID.WellFed2,
				BuffID.WellFed3,
				BuffID.SugarRush,
			};

			for (int i = 0; i < player.buffType.Length; i++)
			{
				if (player.buffType[i] > 0 && player.buffTime[i] > 0)
				{
					bool shouldKeep = false;
					foreach (int keepId in buffsToKeep)
					{
						if (player.buffType[i] == keepId)
						{
							shouldKeep = true;
							break;
						}
					}

					if (!shouldKeep && !Main.debuff[player.buffType[i]])
					{
						shouldKeep = true;
					}

					if (!shouldKeep)
					{
						player.buffType[i] = 0;
						player.buffTime[i] = 0;
					}
				}
			}
		}

		/// <summary>
		/// 播放回溯启动视觉特效 — 时光倒流启动的华丽序幕
		/// </summary>
		private void PlayRebirthEffect(Player player)
		{
			// ===== 1. 金色时光粒子大爆发 =====
			for (int i = 0; i < 60; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(3f, 12f);
				Vector2 velocity = new Vector2(
					(float)Math.Cos(angle) * speed,
					(float)Math.Sin(angle) * speed);

				Dust dust = Dust.NewDustPerfect(player.Center, DustID.GoldFlame,
					velocity, 150, default, 2f + Main.rand.NextFloat() * 1f);
				dust.noGravity = true;
				dust.fadeIn = 1.2f;
			}

			// ===== 2. 蓝色时光水晶漩涡 =====
			for (int i = 0; i < 40; i++)
			{
				float angle = MathHelper.TwoPi * i / 40f;
				float radius = Main.rand.NextFloat(25f, 55f);
				Vector2 dustPos = player.Center + new Vector2(
					(float)Math.Cos(angle) * radius,
					(float)Math.Sin(angle) * radius);

				Dust dust = Dust.NewDustPerfect(dustPos, DustID.BlueCrystalShard,
					Vector2.Zero, 100, default, 1.3f + Main.rand.NextFloat() * 0.5f);
				dust.noGravity = true;
				dust.velocity = (player.Center - dustPos) * 0.15f;
			}

			// ===== 3. 彩色时间火花 =====
			for (int i = 0; i < 30; i++)
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				float speed = Main.rand.NextFloat(2f, 8f);
				Vector2 velocity = new Vector2(
					(float)Math.Cos(angle) * speed,
					(float)Math.Sin(angle) * speed);

				Color color = Color.Lerp(
					new Color(255, 215, 0),
					new Color(100, 200, 255),
					Main.rand.NextFloat());

				Dust spark = Dust.NewDustPerfect(player.Center, DustID.FireworksRGB,
					velocity, 100, color, 1.2f + Main.rand.NextFloat() * 0.6f);
				spark.noGravity = true;
				spark.fadeIn = 0.6f;
			}

			// ===== 4. 白色闪光（模拟时间停止的瞬间） =====
			for (int i = 0; i < 15; i++)
			{
				Vector2 flashPos = player.Center + new Vector2(
					Main.rand.NextFloat(-40f, 40f),
					Main.rand.NextFloat(-40f, 40f));
				Dust flash = Dust.NewDustPerfect(flashPos, DustID.WhiteTorch,
					Vector2.Zero, 200, default, 1.8f + Main.rand.NextFloat() * 0.8f);
				flash.noGravity = true;
				flash.fadeIn = 1.5f;
			}
		}

		// ===== 每帧更新逻辑 =====
		public override void UpdateInventory(Player player)
		{
			base.UpdateInventory(player);

			// 时光能量缓慢恢复
			if (rebirthEnergy < MAX_REBIRTH_ENERGY)
			{
				rebirthEnergy = Math.Min(MAX_REBIRTH_ENERGY, rebirthEnergy + ENERGY_RECOVERY_RATE);
			}

			// 更新回溯滑行状态
			if (isRebirthSliding)
			{
				UpdateRebirthSlide(player);
			}

			// 春秋蝉在背包中且已炼化时，持续记录玩家位置
			if (hasBeenControlled && !isRebirthSliding && player.whoAmI == Main.myPlayer)
			{
				positionRecordTimer++;
				if (positionRecordTimer >= RECORD_INTERVAL)
				{
					positionRecordTimer = 0;

					// 记录当前位置（取整到方块坐标，减少抖动）
					Vector2 recordedPos = new Vector2(
						(float)Math.Floor(player.position.X / 16f) * 16f,
						(float)Math.Floor(player.position.Y / 16f) * 16f);

					positionHistory.Add(recordedPos);

					// 超出最大数量时移除最旧的记录
					if (positionHistory.Count > MAX_HISTORY_COUNT)
					{
						positionHistory.RemoveAt(0);
					}
				}
			}
		}

		// ===== 手持时的效果 =====
		protected override int moddustType => DustID.GoldFlame;

		// ===== 数据持久化 =====
		public override void SaveData(TagCompound tag)
		{
			base.SaveData(tag);

			tag["rebirthEnergy"] = rebirthEnergy;

			// 保存位置历史（最多保存最近100条，避免存档过大）
			var posList = new List<TagCompound>();
			int saveCount = Math.Min(positionHistory.Count, 100);
			int startIdx = positionHistory.Count - saveCount;
			for (int i = startIdx; i < positionHistory.Count; i++)
			{
				posList.Add(new TagCompound {
					["x"] = positionHistory[i].X,
					["y"] = positionHistory[i].Y
				});
			}
			tag["positionHistory"] = posList;
		}

		public override void LoadData(TagCompound tag)
		{
			base.LoadData(tag);

			rebirthEnergy = tag.GetFloat("rebirthEnergy");

			positionHistory.Clear();
			if (tag.ContainsKey("positionHistory"))
			{
				var posList = tag.GetList<TagCompound>("positionHistory");
				foreach (var entry in posList)
				{
					positionHistory.Add(new Vector2(
						entry.GetFloat("x"),
						entry.GetFloat("y")));
				}
			}
		}
	}
}
