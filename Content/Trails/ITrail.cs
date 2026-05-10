using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VerminLordMod.Content.Trails
{
	/// <summary>
	/// 拖尾接口 - 所有拖尾类型必须实现此接口
	/// </summary>
	public interface ITrail
	{
		/// <summary>拖尾名称（调试用）</summary>
		string Name { get; }

		/// <summary>
		/// 拖尾需要的混合模式。
		/// TrailManager 会根据此属性自动切换 SpriteBatch 混合模式。
		/// null = 使用当前混合模式（不切换）。
		/// </summary>
		BlendState BlendMode { get; }

		/// <summary>每帧更新拖尾</summary>
		/// <param name="center">当前中心位置</param>
		/// <param name="velocity">当前速度</param>
		void Update(Vector2 center, Vector2 velocity);

		/// <summary>绘制拖尾</summary>
		void Draw(SpriteBatch sb);

		/// <summary>清空拖尾</summary>
		void Clear();

		/// <summary>拖尾是否还有内容</summary>
		bool HasContent { get; }
	}
}
