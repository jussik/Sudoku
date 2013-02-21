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
		/// Retain a history of every action performed during a solve.
		/// </summary>
		public bool SaveStateHistory { get; set; }
		/// <summary>
		/// If SaveStateHistory is true, this will contain the list of all states.
		/// </summary>
		public StateHistory State { get; set; }

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

			if (SaveStateHistory) {
				State = new StateHistory();
				State.Add(Grid.SaveState("Initial"));
			}

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
		/// Run solvers until no further changes are registered or the <see cref="Puzzle"/> is solved.
		/// </summary>
		private bool IterateSolvers(params Func<bool>[] funcs)
		{
			return IterateSolvers(funcs.AsEnumerable());
		}
		/// <summary>
		/// Run solvers until no further changes are registered or the <see cref="Puzzle"/> is solved.
		/// </summary>
		private bool IterateSolvers(IEnumerable<Func<bool>> funcs)
		{
			bool change = false;
			// run head of actions
			Func<bool> func = funcs.First();
			change = func();

			if(SaveStateHistory)
				State.Add(Grid.SaveState(func.Method.Name));

			// check for solved
			if (change && Grid.Cells.All(c => c.Value > 0)) {
				Solved = true;
			}

			// continue using rest of actions, mark changed as true if rest changed
			if(!change && !Solved && funcs.Count() > 1)
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

		/// <summary>
		/// Reduces possibilities for cells in cases where a single row or column of a box contains all the occurances of a possibility.
		/// </summary>
		private bool FindLockedCandidates1()
		{
			// Find locked candidates in rows
			bool changed = FindLockedCandidates1Inner(
				(row, cell) => cell + 3 * row,
				(box, row) => box.Rows[row]);
			// Find locked candidates in columns
			return FindLockedCandidates1Inner(
				(col, cell) => col + 3 * cell,
				(box, col) => box.Columns[col]) || changed;
		}
		
		// if a row/column has a specific possiblity, it is flagged under the index of that possibility value.
		// row 0 is 0b001, row 1 is 0b010, row 2 is 0b100.
		// e.g. if both rows 0 and 2 have 8 as a possiblity, then locked1ResultCounts[8] == 0b101
		private int[] locked1ResultCounts = new int[10];
		// used to see if a value in locked1ResultCounts belongs to a unique row/column
		// uniqueRowLookup[1] == 0, [2] == 1, [4] == 2, otherwise -1
		private int[] locked1UniqueSegLookup = new int[] { -1, 0, 1, -1, 2, -1, -1, -1 };
		// cellIndexSelector takes the index of the segment (row or column) and the index of the cell in that segment
		//   and expects the index of that cell with respect to the box it resides in
		// segmentSelector takes a box and the index of a segment in that box and expects an object reference to that box
		private bool FindLockedCandidates1Inner(Func<int, int, int> cellIndexSelector, Func<Box, int, Segment> segmentSelector)
		{
			bool changed = false;
			foreach (Box box in Grid.Boxes) {
				// loop each row/column in box
				for (var i=0; i<10; i++) {
					locked1ResultCounts[i] = 0;
				}
			
				for (var seg=0; seg<3; seg++) {
					for (var cellPos=0; cellPos<3; cellPos++) {
						Cell cell = box.Cells[cellIndexSelector(seg, cellPos)];
						if (cell.Value == 0) {
							for (var val=1; val<=9; val++) {
								if (cell.Possibilities.Contains(val))
									// flag this row/col as having this possibility
									locked1ResultCounts[val] |= 1 << seg;
							}
						}
					}
				}
			
				// check for unique rows/columns
				for (var val=1; val<=9; val++) {
					int uniqueRow = locked1UniqueSegLookup[locked1ResultCounts[val]];
					if (uniqueRow > -1) {
						Segment seg = segmentSelector(box, uniqueRow);
						foreach (Cell cell in seg.Cells) {
							if (cell.Box == box || cell.Value > 0)
								continue;
						
							changed = cell.Possibilities.Remove(val) || changed;
						}
					}
				}
			}
			return changed;
		}
	}
}
