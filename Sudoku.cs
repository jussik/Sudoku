using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Euler96
{
	public class Sudoku
	{
		public string Name { get; private set; }
		public string GridString { get; private set; }

		public Grid Grid { get; private set; }
		public bool Solved { get; private set; }

		public Sudoku(string name, string gridString)
		{
			Name = name;
			GridString = gridString;
		}

		public void Run()
		{
			Grid = new Grid();
			Grid.Load(GridString);

			while (!Solved) {
				// repeat all solvers until solved or no change happens in any solver
				if(!IterateSolvers(FindSingles, DeepCheck))
					break;
			}
		}

		bool IterateSolvers(params Func<bool>[] actions)
		{
			return IterateSolvers(actions.AsEnumerable());
		}
		bool IterateSolvers(IEnumerable<Func<bool>> actions)
		{
			bool change = false;
			// run head of actions until it has no more effect
			Func<bool> func = actions.First();
			Console.WriteLine("Attempting {0}", func.Method.Name);
			while (func()) {
				Console.WriteLine("Success");
				change = true;
			}

			// check for solved
			if (Grid.Cells.All(c => c.Value > 0)) {
				Console.WriteLine("Solved!");
				Solved = true;
			}

			// continue using rest of actions, mark changed as true if rest changed
			if(!Solved && actions.Count() > 1)
				change = IterateSolvers(actions.Skip(1)) || change;

			return change;
		}

		public bool FindSingles()
		{
			bool changed = false;
			foreach(Cell cell in Grid.Cells) {
				if (cell.Value == 0) {
					// Remove adjacent cells' values from possibilities
					foreach(Cell adj in cell.AdjacentCells) {
						if(adj.Value > 0)
							cell.Possibilities.Remove(adj.Value);
					}

					// If only one possibility remains, use that
					if(cell.Possibilities.Count == 1) {
						cell.Value = cell.Possibilities.First();
						changed = true;
					}
				}
			}
			return changed;
		}
		
		private int[] deepCheckResultCounts = new int[10];
		private Cell[] deepCheckResultTargets = new Cell[10];
		public bool DeepCheck()
		{
			return DeepCheckInner(Grid.Rows) || DeepCheckInner(Grid.Columns) || DeepCheckInner(Grid.Boxes);
		}

		public bool DeepCheckInner(IEnumerable<Segment> segments)
		{
			bool changed = false;
			foreach (Segment seg in segments) {
				// reset counts for every row/col/box
				for(var i=0;i<10;i++) {
					deepCheckResultCounts[i] = 0;
				}

				// count how many times a possibility appears for this row/col/box
				foreach (Cell c in seg.Cells) {
					if(c.Value == 0) {
						foreach (int p in c.Possibilities) {
							deepCheckResultCounts[p]++;
							deepCheckResultTargets[p] = c;
						}
					}
				}

				// if we find a possibility with a single owner cell, apply that value to the cell
				for (var i=1; i<=9; i++) {
					if(deepCheckResultCounts[i] == 1) {
						deepCheckResultTargets[i].Value = i;
						changed = true;
					}
				}
			}
			return changed;
		}

		public int GetTriple()
		{
			return Grid.Cells[0].Value * 100
				+ Grid.Cells[1].Value * 10
				+ Grid.Cells[2].Value;
		}
	}
}
