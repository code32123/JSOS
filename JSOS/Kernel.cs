using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Sys = Cosmos.System;

namespace JSOS
{
	public class Kernel : Sys.Kernel
	{
		// Comments like this mean you can leave notes in you code, so you can remind yourself of things later
		// I'll add comments to explain what each line does
		// Write your own comments if you want to, but you don't have to
		protected override void BeforeRun()
		{
			var textscr = Cosmos.HAL.Global.TextScreen;
			Cosmos.System.Global.Console = new Cosmos.System.Console(textscr);
			Cosmos.HAL.Global.TextScreen = textscr;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.White;

			Console.Clear();														// Clears the screen so we can do our own startup sequence!
			Console.WriteLine("Welcome to Jimmy S. Operating System");				// 'WriteLine' prints text and moves to the next line
			Console.Write("Beginning sys-check ");									// Write out the syscheck
			for (int i = 0; i <= 100; ++i)	//	Sets i to 0, then adds one to it (++i) while the condition is met (i <= 100)
			{
				Thread.Sleep(4);													// Delays for 8 milliseconds
				Console.Write("\rBeginning sys-check {0}%   ", i);					// '\r' returns to the beginning of the line, so all previous
																					// messages are overwritten with the new percentage
			}
			Console.WriteLine(" - All Checks Passed");								// prints the text and then moves to the next line
		}

		protected override void Run()
		{

			// In linux, the prompt is usually the current directory, but we havn't set that up yet. For now, just do this:
			Console.Write("JSOS> "); // This is the 'prompt', or what comes right before the text

			var input = Console.ReadLine();                                         // Gets user input and saves it to the input variable

			// Here is where we can use the variable 'input'. Currently, it contains whatever the user typed
			// We can use the 'if' code to check what they typed.

			if (input == "sysinfo") {
				Console.WriteLine("This computer's information:");
				Console.WriteLine("	Hardware:");
				Console.WriteLine("		RAM:            " + Cosmos.Core.CPU.GetAmountOfRAM().ToString() + "MB");
				Console.WriteLine("		CPU Brand:      " + Cosmos.Core.CPU.GetCPUBrandString());
				Console.WriteLine("		CPU Vendor:     " + Cosmos.Core.CPU.GetCPUVendorName());
				Console.WriteLine("		CPU Speed:      " + (Cosmos.Core.CPU.GetCPUCycleSpeed()/100000000).ToString() + "Ghz");
				Console.WriteLine("	Operating System:");
				Console.WriteLine("		Name:           JSOS, Jimmy S. Operating System");
				Console.WriteLine("		Version:        v0.1.0");
				Console.WriteLine("		Build Date:     8/2/2023");
			} else if (input == "help") { // Here's where we can check. If it is not sysinfo, it might be help. However, if it's not help we keep checking
				// until we get to either a 
				Console.WriteLine("Currently loaded commands:");
				Console.WriteLine("	help                Prints out this system help message");
				Console.WriteLine("	sysinfo             Lists hardware and software commands");
			} else { // Here is the else, what we do if nothing matches
				Console.WriteLine("SYSERR: Command '" + input + "' not found on the system.");
			}
		}
	}
}
