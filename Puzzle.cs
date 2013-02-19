using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
	// NOTE: Algorithms based on: http://angusj.com/sudoku/hints.php

	/// <summary>
	/// A single Sudoku <see cref="Puzzle"/>.
	/// </summary>
	public class Puzzle
	{
		/// <summary>
		/// The current name of the <see cref="Puzzle"/>.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// The grid contents as a string.
		/// </summary>
		public string GridString { get; private set; }

		/// <summary>
		/// The Grid instance of the <see cref="Puzzle"/>.
		/// </summary>
		public Grid Grid { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this <see cref="Puzzle"/> is solved.
		/// </summary>
		public bool Solved { get; private set; }

		/// <summary>
		/// Initializes a new Sudoku <see cref="Puzzle"/> instance.
		/// </summary>
		public Puzzle(string name, string gridString)
		{
			Name = name;
			GridString = gridString;
		}

		/// <summary>
		/// Solve the <see cref="Puzzle"/>.
		/// </summary>
		public void Solve()
		{
			Grid = new Grid();
			Grid.Load(GridString);

			while (!Solved) {
				// repeat all solvers until solved or no change happens in any solver
				if(!IterateSolvers(FindSingles, FindHiddenSingles, FindLockedCandidates1))
					break;
			}
		}
		
		/// <summary>
		/// Get the integer representing the first 3 numbers of the <see cref="Puzzle"/>.
		/// Used in Project Euler Problem 96.
		/// </summary>
		public int GetTriple()
		{
			return Grid.Cells[0].Value * 100
				+ Grid.Cells[1].Value * 10
					+ Grid.Cells[2].Value;
		}

		/// <summary>
		/// Repeat solvers until no further changes are registered or the <see cref="Puzzle"/> is solved.
		/// </summary>
		private bool IterateSolvers(params Func<bool>[] funcs)
		{
			return IterateSolvers(funcs.AsEnumerable());
		}
		/// <summary>
		/// Repeat solvers until no further changes are registered or the <see cref="Puzzle"/> is solved.
		/// </summary>
		private bool IterateSolvers(IEnumerable<Func<bool>> funcs)
		{
			bool change = false;
			// run head of actions until it has no more effect
			Func<bool> func = funcs.First();
			while (func()) {
				change = true;
			}

			// check for solved
			if (change && Grid.Cells.All(c => c.Value > 0)) {
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
		
		private int[] hiddenSinglesResultCounts = new int[10];
		private Cell[] hiddenSinglesResultTargets = new Cell[10];
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
					hiddenSinglesResultCounts[i] = 0;
				}

				// count how many times a possibility appears for this row/col/box
				foreach (Cell c in seg.Cells) {
					if(c.Value == 0) {
						foreach (int p in c.Possibilities) {
							hiddenSinglesResultCounts[p]++;
							hiddenSinglesResultTargets[p] = c;
						}
					}
				}

				// if we find a possibility with a single owner cell, apply that value to the cell
				for (var i=1; i<=9; i++) {
					if(hiddenSinglesResultCounts[i] == 1) {
						hiddenSinglesResultTargets[i].Value = i;
						changed = true;
					}
				}
			}
			return changed;
		}

		// if a row has a specific possiblity, it is flagged under the index of that possibility value.
		// row 0 is 0b001, row 1 is 0b010, row 2 is 0b100.
		// e.g. if both rows 0 and 2 have 8 as a possiblity, then locked1ResultCounts[8] == 0b101
		private int[] locked1ResultCounts = new int[10];
		// used to see if a value in locked1ResultCounts belongs to a unique row
		// uniqueRowLookup[1] == 0, [2] == 1, [4] == 2, otherwise -1
		private int[] locked1UniqueRowLookup = new int[] { -1, 0, 1, -1, 2, -1, -1, -1 };
		/// <summary>
		/// Reduces possibilities for cells in cases where a single row or column of a box contains all the occurances of a possibility.
		/// </summary>
		private bool FindLockedCandidates1()
		{
			foreach (Box box in Grid.Boxes) {
				for(var i=0;i<10;i++) {
					locked1ResultCounts[i] = 0;
				}
				// loop each row in box
				for(var r=0;r<3;r++) {
					for(var c=0;c<3;c++) {
						Cell cell = box.Cells[c + 3 * r];
						for(var i=1;i<=9;i++) {
							if(cell.Possibilities.Contains(i))
								locked1ResultCounts[i] |= 1 << r; // flag this row as having this possibility
						}
					}
					for(var i=1;i<=9;i++) {
						int uniqueRow = locked1UniqueRowLookup[locked1ResultCounts[i]];
						if(uniqueRow > -1) {
							//Console.WriteLine("Unique row {0} for value {1}", uniqueRow, i);
							Row row = box.Rows[r];
							foreach(Cell cell in row.Cells) {
								if(cell.Box == box || cell.Value > 0)
									continue;

								// TODO: figure out why this doesn't work
								/*bool changed = cell.Possibilities.Remove(i);
								if(changed)
									Console.WriteLine("Removed possiblity {0} from cell {1}", i, cell.Location);*/
							}
						}
					}
				}
			}
			return false;
		}
	}
}
