using System;
using System.Collections.Generic;
using static tools.shell;

namespace tools {
	static public class lists {
		static public int IndiceMagnitude(int indice) { // How long a list needs to be to contain the indice, obeying negative wrapparounds per python
			if (indice < 0) {
				return Math.Abs(indice);
			}
			return indice + 1;
		}
		static public List<string> Combine(List<string> one, List<string> two) {
			List<string> ret = new List<string>();
			foreach (string element in one) {
				ret.Add(element);
			}
			foreach (string element in two) {
				ret.Add(element);
			}
			return ret;
		}
		static public string ToString(List<string> stringList) {
			return String.Join(", ", stringList.ToArray());
		}
		static public int IndexOf(List<string> stringList, string Find) {
			int at = -1;
			for (int i = 0; i < stringList.Count; i++) {
				if (stringList[i] == Find) {
					at = i;
					break;
				}
			}
			return at;
		}
		static public int IndexOf(List<simpleArg> argList, string Find) {
			int at = -1;
			for (int i = 0; i < argList.Count; i++) {
				if (argList[i].contents == Find) {
					at = i;
					break;
				}
			}
			return at;
		}
		internal static bool contains(List<string> stringList, string Find) {
			for (int i = 0; i < stringList.Count; i++) {
				if (stringList[i] == Find) {
					return true;
				}
			}
			return false;
		}
	}
}