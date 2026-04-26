using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;

namespace VerminLordMod.Content.Trails
{
	/// <summary>
	/// 拖尾管理器 - 管理多个 ITrail 实例的统一容器
	/// 可用于弹幕（ModProjectile）和玩家（ModPlayer）
	/// 
	/// 使用示例（弹幕）：
	///   private TrailManager trailManager = new TrailManager();
	///   public override void AI() {
	///       trailManager.Update(Projectile.Center, Projectile.velocity);
	///   }
	///   public override bool PreDraw(ref Color lightColor) {
	///       trailManager.Draw(Main.spriteBatch);
	///       return true;
	///   }
	/// 
	/// 使用示例（玩家）：
	///   private TrailManager trailManager = new TrailManager();
	///   public override void PostUpdate() {
	///       trailManager.Update(Player.Center, Player.velocity);
	///   }
	///   // 在 PlayerEffectDrawSystem 中调用 trailManager.Draw(sb)
	/// </summary>
	public class TrailManager
	{
		private List<ITrail> trails = new List<ITrail>();
		private List<ITrail> pendingAdd = new List<ITrail>();
		private List<ITrail> pendingRemove = new List<ITrail>();

		/// <summary>当前管理的拖尾数量</summary>
		public int Count => trails.Count;

		/// <summary>是否有任何拖尾还有内容</summary>
		public bool HasContent
		{
			get
			{
				foreach (var t in trails)
					if (t.HasContent) return true;
				return false;
			}
		}

		/// <summary>
		/// 添加一个拖尾
		/// </summary>
		public void Add(ITrail trail)
		{
			if (!trails.Contains(trail) && !pendingAdd.Contains(trail))
				pendingAdd.Add(trail);
		}

		/// <summary>
		/// 移除一个拖尾
		/// </summary>
		public void Remove(ITrail trail)
		{
			if (!pendingRemove.Contains(trail))
				pendingRemove.Add(trail);
		}

		/// <summary>
		/// 获取指定类型的拖尾（如 trailManager.Get<GhostTrail>()）
		/// </summary>
		public T Get<T>() where T : class, ITrail
		{
			foreach (var t in trails)
				if (t is T result) return result;
			foreach (var t in pendingAdd)
				if (t is T result) return result;
			return null;
		}

		/// <summary>
		/// 检查是否包含指定类型的拖尾
		/// </summary>
		public bool Has<T>() where T : class, ITrail
		{
			return Get<T>() != null;
		}

		/// <summary>
		/// 清空所有拖尾
		/// </summary>
		public void Clear()
		{
			foreach (var t in trails)
				t.Clear();
			trails.Clear();
			pendingAdd.Clear();
			pendingRemove.Clear();
		}

		/// <summary>
		/// 每帧更新所有拖尾
		/// </summary>
		/// <param name="center">当前中心位置</param>
		/// <param name="velocity">当前速度</param>
		public void Update(Vector2 center, Vector2 velocity)
		{
			// 处理待添加/移除
			ProcessPending();

			foreach (var t in trails)
			{
				t.Update(center, velocity);
			}
		}

		/// <summary>
		/// 绘制所有拖尾
		/// </summary>
		public void Draw(SpriteBatch sb)
		{
			foreach (var t in trails)
			{
				t.Draw(sb);
			}
		}

		private void ProcessPending()
		{
			if (pendingAdd.Count > 0)
			{
				trails.AddRange(pendingAdd);
				pendingAdd.Clear();
			}
			if (pendingRemove.Count > 0)
			{
				foreach (var t in pendingRemove)
				{
					t.Clear();
					trails.Remove(t);
				}
				pendingRemove.Clear();
			}
		}
	}
}
