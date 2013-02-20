using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Sudoku
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

			string filename = args[0];
			if (!File.Exists(filename)) {
				Console.WriteLine("File not found: {0}", filename);
				return;
			}
			
			// parse file
			List<Tuple<string, List<string>>> grids = new List<Tuple<string, List<string>>>();
			List<string> current = null;

			foreach (string lineFull in File.ReadLines(filename)) {
				string line = lineFull.Trim();
				if (line.Length == 0)
					continue;
				
				char fc = line[0];
				// Assume any line that starts with a number is a puzzle row
				if (fc >= 48 && fc <= 57) {
					// is number
					if (current == null)
						throw new Exception("No grid name defined");
					current.Add(line);
				} else {
					current = new List<string>();
					grids.Add(new Tuple<string, List<string>>(line, current));
				}
			}
			
			// solve puzzles
			List<Task> tasks = new List<Task>();
			List<Puzzle> puzzles = new List<Puzzle>();

			foreach (Tuple<string, List<string>> pair in grids) {
				string gridString = string.Join("", pair.Item2);
				Puzzle puzzle = new Puzzle(pair.Item1, gridString);
				puzzle.SaveStateHistory = true;
				puzzles.Add(puzzle);

				// Start new threads if deemed useful
				Task task = Task.Factory.StartNew(puzzle.Solve);

				tasks.Add(task);
			}
			
			Task.WaitAll(tasks.ToArray());

			// output results
			int count = 0;
			int solved = 0;
			int tripleSum = 0;
			int totalChecks = 0;
			int totalChanges = 0;

			foreach (Puzzle puzzle in puzzles) {
				count++;
				if (puzzle.Solved) {
					solved++;
					tripleSum += puzzle.GetTriple();
				}

				Console.WriteLine(puzzle.Name);
				Console.WriteLine(puzzle.Grid);
				int checks = puzzle.State.States.Count;
				int changes = puzzle.State.States.Where((gs, i) => i > 0 && gs.HasChanged(puzzle.State.States[i-1])).Count();
				totalChecks += checks;
				totalChanges += changes;
				Console.WriteLine("Changes/checks: {0}/{1}", changes, checks);
				Console.WriteLine("JSON history:");
				Console.WriteLine(puzzle.State.ToJson());
				Console.WriteLine();
			}

			Console.WriteLine("Puzzles solved {0}/{1}", solved, count);
			Console.WriteLine("Total changes/checks: {0}/{1}", totalChanges, totalChecks);
			Console.WriteLine("Triple sum: {0}", tripleSum);
		}
	}
}

