using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerminLordMod.Content
{
	public class Randommer
	{/// <summary>
	/// 百分之多少概率成功
	/// </summary>
	/// <param name="per">百分率</param>
	/// <returns></returns>
		public static bool Roll(int per) {
			Random random = new Random();
			return random.Next(100) < per;
		}
	}
}
