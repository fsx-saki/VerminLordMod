using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerminLordMod.Content.Prefixes
{
	class ColeopteraPrefix:CrustaceaPrefix
	{
		public override int Power => base.Power / 3 * 2;
	}
}
