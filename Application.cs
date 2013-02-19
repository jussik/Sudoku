using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Euler96
{
	/// <summary>
	/// The main Sudoku solver application.
	/// </summary>
	public class Application
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0) {
				Console.WriteLine("Usage: sudoku <filename.txt>");
				return;
			}
			
			// parse file
			string filename = args[0];
			Dictionary<string, List<string>> grids = new Dictionary<string, List<string>>();
			List<string> current = null;
			foreach (string lineFull in File.ReadLines(filename)) {
				string line = lineFull.Trim();
				if (line.Length == 0)
					continue;
				
				char fc = line[0];
				if (fc >= 48 && fc <= 57) {
					// is number
					if (current == null)
						throw new Exception("No grid name defined");
					current.Add(line);
				} else {
					current = new List<string>();
					grids.Add(line, current);
				}
			}
			
			// run games
			List<Task> tasks = new List<Task>();
			object consoleLock = new object();

			int count = 0;
			int solved = 0;
			int tripleSum = 0;

			foreach (KeyValuePair<string, List<string>> pair in grids) {
				string gridString = string.Join("", pair.Value);
				Sudoku game = new Sudoku(pair.Key, gridString);

				// Start new threads if deemed useful
				Task task = Task.Factory.StartNew(() => {
					game.Solve();

					count++;
					if(game.Solved) {
						solved++;
						tripleSum += game.GetTriple();
					}

					lock(consoleLock) {
						Console.WriteLine(game.Name);
						Console.WriteLine(game.Grid);
						Console.WriteLine();
					}
				});

				tasks.Add(task);
			}
			
			Task.WaitAll(tasks.ToArray());

			Console.WriteLine("Solved {0}/{1}", solved, count);
			Console.WriteLine("Triple sum: {0}", tripleSum);
		}
	}
}

