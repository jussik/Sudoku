using System;
using System.Collections.Generic;
using System.Linq;

namespace Euler96
{
	/// <summary>
	/// A single Sudoku puzzle.
	/// </summary>
	public class Sudoku
	{
		/// <summary>
		/// The current name of the puzzle.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// The grid contents as a string.
		/// </summary>
		public string GridString { get; private set; }

		/// <summary>
		/// The Grid instance of the puzzle.
		/// </summary>
		public Grid Grid { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this puzzle is solved.
		/// </summary>
		public bool Solved { get; private set; }

		/// <summary>
		/// Initializes a new <see cref="Sudoku"/> puzzle instance.
		/// </summary>
		public Sudoku(string name, string gridString)
		{
			Name = name;
			GridString = gridString;
		}

		/// <summary>
		/// Solve the puzzle.
		/// </summary>
		public void Solve()
		{
			Grid = new Grid();
			Grid.Load(GridString);

			while (!Solved) {
				// repeat all solvers until solved or no change happens in any solver
				if(!IterateSolvers(FindSingles, FindHiddenSingles))
					break;
			}
		}
		
		/// <summary>
		/// Get the integer representing the first 3 numbers of the puzzle.
		/// </summary>
		public int GetTriple()
		{
			return Grid.Cells[0].Value * 100
				+ Grid.Cells[1].Value * 10
					+ Grid.Cells[2].Value;
		}

		/// <summary>
		/// Repeat solvers until no further changes are registered or the puzzle is solved.
		/// </summary>
		private bool IterateSolvers(params Func<bool>[] funcs)
		{
			return IterateSolvers(funcs.AsEnumerable());
		}
		/// <summary>
		/// Repeat solvers until no further changes are registered or the puzzle is solved.
		/// </summary>
		private bool IterateSolvers(IEnumerable<Func<bool>> funcs)
		{
			bool change = false;
			// run head of actions until it has no more effect
			Func<bool> func = funcs.First();
			//Console.WriteLine("Attempting {0}", func.Method.Name);
			while (func()) {
				//Console.WriteLine("Success");
				change = true;
			}

			// check for solved
			if (Grid.Cells.All(c => c.Value > 0)) {
				//Console.WriteLine("Solved!");
				Solved = true;
			}

			// continue using rest of actions, mark changed as true if rest changed
			if(!Solved && funcs.Count() > 1)
				change = IterateSolvers(funcs.Skip(1)) || change;

			return change;
		}

		/// <summary>
		/// Solve for cells where they only have a single possible value.
		/// </summary>
		private bool FindSingles()
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
		/// <summary>
		/// Solve for cells where their adjacents have no mutual possible values.
		/// </summary>
		private bool FindHiddenSingles()
		{
			return FindHiddenSinglesInner(Grid.Rows) || FindHiddenSinglesInner(Grid.Columns) || FindHiddenSinglesInner(Grid.Boxes);
		}
		private bool FindHiddenSinglesInner(IEnumerable<Segment> segments)
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
	}
}
