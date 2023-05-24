using System;
using System.Collections.Generic;
using g;

namespace tools {
	static public class numbers {
		static public int bound(int val, int min, int max) {
			return Math.Max(min, Math.Min(max, val));
		}
	}
}