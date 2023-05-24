using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using g;

namespace tools {
	static public class strings {
		//static public string Insert(string source, string toAdd, int i) {
		//	return source.Substring(0, i) + toAdd + source.Substring(i, source.Length-i);
		//}
		static public string mult(char c, int amt) {
			if (amt <= 0) {
				return c.ToString();
			}
			string str = "";
			for (int i = 0; i < amt; i++) {
				str += c;
			}
			return str;
		}
		static public string join(string combiner, List<string> listToCombine) {
			string Output = "";
			for (int i = 0; i < listToCombine.Count; i++) {
				Output += listToCombine[i];
				if (i != listToCombine.Count-1) {
					Output += combiner;
				}
			}
			return Output;
		}
		static public bool softBool(string text) {
			List<string> options = new List<string> { "true", "on", "yes" };
			foreach (string val in options) {
				if (text.ToLower().Equals(val)) {
					return true;
				}
			}
			return false;
		}
		static public List<string> Split(string text, char splitter) {
			List<string> result = new() { "" };
			Stack<char> quoteScope = new();
			int resultPlace = 0;

			for (int i = 0; i < text.Length; i++) {
				char singleChar = text[i];
				if (singleChar == '"' || singleChar == '\'') {
					if (quoteScope.Count == 0) {
						quoteScope.Push(singleChar);
					} else if (quoteScope.Peek() == singleChar) {
						quoteScope.Pop();
					} else {
						quoteScope.Push(singleChar);
					}
				} else if (singleChar == splitter && quoteScope.Count == 0) {
					resultPlace++;
					result.Add("");
				} else {
					result[resultPlace] = result[resultPlace] + singleChar;
				}
			}
			return result;
		}
	}
}