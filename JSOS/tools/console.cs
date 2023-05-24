using System;
using System.Collections.Generic;
using g;

namespace tools {
	static public class console {
		static public void InvertColors() {
			ConsoleColor fg = Console.ForegroundColor;
			ConsoleColor bg = Console.BackgroundColor;
			Console.ForegroundColor = bg;
			Console.BackgroundColor = fg;
		}
		static private int ErasePrint(int xPos, int xOffset, int lengthToErase, string toPrint) {
			Console.SetCursorPosition(xPos, Console.CursorTop);
			Console.Write(new String(' ', lengthToErase + 1));
			Console.SetCursorPosition(xPos, Console.CursorTop);
			Console.Write(toPrint);

			xOffset = tools.numbers.bound(xOffset, 0, toPrint.Length);

			Console.SetCursorPosition(xPos + xOffset, Console.CursorTop);
			return xOffset;
		}
		static public string ReadLine() {
			int xPos = Console.CursorLeft;
			int xOffset = 0;
			int placeInHistory = globals.commandHistory.Count;
			globals.commandHistory.Add("");
			string CumulativeString = "";
			ConsoleKeyInfo CurrentKey = Console.ReadKey();
			int ToErase = 0;
			while (CurrentKey.Key != ConsoleKey.Enter) {
				if (CurrentKey.Key == ConsoleKey.Backspace) {
					if (CumulativeString.Length > 0 && xOffset > 0) {
						xOffset--;
						CumulativeString = CumulativeString.Remove(xOffset, 1);
					}
				} else if (CurrentKey.Key == ConsoleKey.Delete) {
					if (CumulativeString.Length > xOffset) {
						CumulativeString = CumulativeString.Remove(xOffset, 1);
					}
				} else if (CurrentKey.Key == ConsoleKey.UpArrow) {
					if (placeInHistory > 0) {
						ToErase = CumulativeString.Length;
						placeInHistory -= 1;
						if (xOffset == 0 || xOffset == globals.commandHistory[placeInHistory + 1].Length) {
							xOffset = globals.commandHistory[placeInHistory].Length;
						}
						CumulativeString = globals.commandHistory[placeInHistory];
					}
				} else if (CurrentKey.Key == ConsoleKey.DownArrow) {
					if (placeInHistory < globals.commandHistory.Count - 1) {
						ToErase = CumulativeString.Length;
						placeInHistory += 1;
						if (xOffset == 0 || xOffset == globals.commandHistory[placeInHistory - 1].Length) {
							xOffset = globals.commandHistory[placeInHistory].Length;
						}
						CumulativeString = globals.commandHistory[placeInHistory];
					}
				} else if (CurrentKey.Key == ConsoleKey.LeftArrow) {
					xOffset--;
				} else if (CurrentKey.Key == ConsoleKey.RightArrow) {
					xOffset++;
				} else if (CurrentKey.Key == ConsoleKey.Home) {
					xOffset = 0;
				} else if (CurrentKey.Key == ConsoleKey.End) {
					xOffset = CumulativeString.Length;
				} else {
					CumulativeString = CumulativeString.Insert(xOffset, CurrentKey.KeyChar.ToString());
					xOffset++;
				}
				xOffset = ErasePrint(xPos, xOffset, ToErase, CumulativeString);
				ToErase = CumulativeString.Length;
				CurrentKey = Console.ReadKey();
			}
			Console.WriteLine();
			globals.commandHistory[globals.commandHistory.Count - 1] = CumulativeString;
			return CumulativeString;
		}
		static public ConsoleColor GetColor(string colorName) {
			colorName = colorName.ToLower();
			if (colorName == "black") {
				return ConsoleColor.Black;
			} else if (colorName == "white") {
				return ConsoleColor.White;
			} else if (colorName == "blue") {
				return ConsoleColor.Blue;
			} else if (colorName == "darkblue") {
				return ConsoleColor.DarkBlue;
			} else if (colorName == "cyan") {
				return ConsoleColor.Cyan;
			} else if (colorName == "darkcyan") {
				return ConsoleColor.DarkCyan;
			} else if (colorName == "gray") {
				return ConsoleColor.Gray;
			} else if (colorName == "darkgray") {
				return ConsoleColor.DarkGray;
			} else if (colorName == "green") {
				return ConsoleColor.Green;
			} else if (colorName == "darkgreen") {
				return ConsoleColor.DarkGreen;
			} else if (colorName == "magenta") {
				return ConsoleColor.Magenta;
			} else if (colorName == "darkmagenta") {
				return ConsoleColor.DarkMagenta;
			} else if (colorName == "red") {
				return ConsoleColor.Red;
			} else if (colorName == "darkred") {
				return ConsoleColor.DarkRed;
			} else if (colorName == "yellow") {
				return ConsoleColor.Yellow;
			} else if (colorName == "darkyellow") {
				return ConsoleColor.DarkYellow;
			} else {
				return ConsoleColor.Black;
			}
		}
	}
}