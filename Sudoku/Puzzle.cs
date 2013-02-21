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
		/// Gets a value indicating whether this <see cref="Puzzle"/> has changed from its original state.
		/// </summary>
		public bool Changed { get; private set; }

		/// <summary>
		/// Initializes a new Sudoku <see cref="Puzzle"/> instance from a string.
		/// </summary>
		public Puzzle(string name, string gridString)
		{
			Name = name;
			GridString = gridString;

			Grid = new Grid();
			Grid.Load(GridString);
		}
		
		/// <summary>
		/// Initializes a new Sudoku <see cref="Puzzle"/> instance from a grid.
		/// </summary>
		public Puzzle(string name, Grid grid)
		{
			Name = name;
			Grid = grid;
		}
		
		public void Solve()
		{
			Solve("Initial");
		}
		/// <summary>
		/// Solve the <see cref="Puzzle"/>.
		/// </summary>
		public void Solve(string initialStateDescription)
		{
			if (SaveStateHistory) {
				State = new StateHistory();
				State.Add(Grid.SaveState(initialStateDescription));
			}

			InitLog2Lookup();

			while (!Solved) {
				// repeat all solvers until solved or no change happens in any solver
				bool changedThisIteration = IterateSolvers(FindSingles,
				                              FindHiddenSingles,
				                              FindLockedCandidates1,
				                              FindLockedCandidates2,
				                              FindNakedPairs,
				                              Guess);

				// keep note of if we have ever changed
				Changed = Changed || changedThisIteration;

				// no change in any solver means we're stuck
				if(!changedThisIteration)
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

			if(Solved)
				return true; // This use case happens if we guess

			if(SaveStateHistory)
				State.Add(Grid.SaveState(func.Method.Name));

			// check for solved
			if (change && Grid.Cells.All(c => c.Value > 0)) {
				Solved = true;
				return true;
			}

			// continue using rest of actions, mark changed as true if rest changed
			if(!change && funcs.Count() > 1)
				change = IterateSolvers(funcs.Skip(1)) || change;

			return change;
		}

		// We'll be needing a log2 lookup table for a couple of solvers
		// returns the log2 of the index if the result is an integer, -1 if not
		// max log2 is 2**LOG2_LOOKUP_SIZE-1
		private const int LOG2_LOOKUP_SIZE = 3;
		private static int[] log2Lookup = new int[1 << LOG2_LOOKUP_SIZE];
		private static void InitLog2Lookup()
		{
			if (log2Lookup[0] == 0) {
				for(int i=0;i<(1<<LOG2_LOOKUP_SIZE);i++) {
					log2Lookup[i] = -1;
				}
				for(int i=0;i<LOG2_LOOKUP_SIZE;i++) {
					log2Lookup[1 << i] = i;
				}
			}
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
				for(int i=0;i<10;i++) {
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
				for (int i=1; i<=9; i++) {
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
		
		// if a segment has a specific possiblity, it is flagged under the index of that possibility value.
		// row 0 is 0b001, row 1 is 0b010, row 2 is 0b100.
		// e.g. if both rows 0 and 2 have 8 as a possiblity, then lockedResultCounts[8] == 0b101
		private int[] lockedResultCounts = new int[10];

		// cellIndexSelector takes the index of the segment (row or column) and the index of the cell in that segment
		//   and expects the index of that cell with respect to the box it resides in
		// segmentSelector takes a box and the index of a segment in that box and expects an object reference to that box
		private bool FindLockedCandidates1Inner(Func<int, int, int> cellIndexSelector, Func<Box, int, Segment> segmentSelector)
		{
			bool changed = false;
			foreach (Box box in Grid.Boxes) {
				for (int i=0; i<10; i++) {
					lockedResultCounts[i] = 0;
				}
			
				// loop each row/column in box
				for (int seg=0; seg<3; seg++) {
					for (int ic=0; ic<3; ic++) {
						Cell cell = box.Cells[cellIndexSelector(seg, ic)];
						if (cell.Value == 0) {
							foreach(int p in cell.Possibilities) {
								// flag this row/col as having this possibility
								lockedResultCounts[p] |= 1 << seg;
							}
						}
					}
				}
			
				// check for unique rows/columns
				for (int val=1; val<=9; val++) {
					int uniqueRow = log2Lookup[lockedResultCounts[val]];
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

		/// <summary>
		/// Reduces possibilities for cells in cases where a single box in a row or column contains all the occurances of a possibility.
		/// </summary>
		private bool FindLockedCandidates2()
		{
			bool changed = FindLockedCandidates2Inner(Grid.Rows, (cell, row) => cell.Row == row);
			return FindLockedCandidates2Inner(Grid.Columns, (cell, col) => cell.Column == col) || changed;
		}
		// segComparer takes a cell and a segment and expects a true if that cell is in that segment
		private bool FindLockedCandidates2Inner(IEnumerable<Segment> segments, Func<Cell,Segment,bool> segComparer)
		{
			bool changed = false;
			foreach (Segment seg in segments) {
				// clear results
				for (int i=0; i<10; i++) {
					lockedResultCounts[i] = 0;
				}

				// check which box each possibility falls in
				for(int ib=0;ib<3;ib++) {
					for(int ic=0;ic<3;ic++) {
						Cell cell = seg.Cells[ic + 3 * ib];
						if(cell.Value == 0) {
							foreach(int p in cell.Possibilities)
								lockedResultCounts[p] |= 1 << ib;
						}
					}
				}
				// if possibilities fall only in one box, remove that possibilitiy from the other cells in the box
				for(int val=0;val<9;val++) {
					int uniqueBox = log2Lookup[lockedResultCounts[val]];
					if(uniqueBox > -1) {
						Box box = seg.Cells[uniqueBox * 3].Box;
						foreach(Cell cell in box.Cells) {
							if (segComparer(cell, seg) || cell.Value > 0)
								continue;

							changed = cell.Possibilities.Remove(val) || changed;
						}
					}
				}
			}
			return changed;
		}

		/// <summary>
		/// Finds situations where two cells in a segment have the same (and only) 2 possibilities.
		/// </summary>
		private bool FindNakedPairs()
		{
			bool changed = FindNakedPairsInner(Grid.Rows);
			changed = FindNakedPairsInner(Grid.Columns) || changed;
			return FindNakedPairsInner(Grid.Boxes) || changed;
		}
		// stores a pair of possibilities for all cells that have exactly 2 possibilities
		private int[][] nakedPairBuffer = new int[9][];
		private bool FindNakedPairsInner(IEnumerable<Segment> segments)
		{
			bool changed = false;
			
			foreach (Segment r in segments) {
				// reset pair buffer
				for(int i=0;i<9;i++) {
					nakedPairBuffer[i] = null;
				}
				
				for(int i=0;i<9;i++) {
					Cell cell = r.Cells[i];
					if(cell.Value == 0 && cell.Possibilities.Count == 2) {
						// possibilities should (!) always be in order, but it depends on implementation
						// we'll sort them just in case
						nakedPairBuffer[i] = cell.Possibilities.OrderBy(n => n).ToArray();
					}
				}
				
				for(int i=0;i<8;i++) {
					if(nakedPairBuffer[i] != null) {
						int[] pair1 = nakedPairBuffer[i];
						for(int j=i+1;j<9;j++) {
							int[] pair2 = nakedPairBuffer[j];
							if(pair2 != null && pair1[0] == pair2[0] && pair1[1] == pair2[1]) {
								for(int c=0;c<9;c++) {
									if(c == i || c == j)
										continue;
									
									Cell cell = r.Cells[c];
									if(cell.Value > 0)
										continue;
									
									changed = cell.Possibilities.Remove(pair1[0]) || changed;
									changed = cell.Possibilities.Remove(pair1[1]) || changed;
								}
							}
						}
					}
				}
			}
			
			return changed;
		}

		private bool Guess()
		{
			// If we haven't changed the grid at all up to this point, give up
			// this should prevent infinite recursion
			if(!Changed)
				return false;

			int leastPossibilities = 10;
			int bestCandidate = -1;

			for (int ci=0; ci<81; ci++) {
				Cell cell = Grid.Cells[ci];
				if (cell.Value == 0 && cell.Possibilities.Count < leastPossibilities) {
					leastPossibilities = cell.Possibilities.Count;
					bestCandidate = ci;
				}
			}

			if (bestCandidate > -1) {
				foreach(int p in Grid.Cells[bestCandidate].Possibilities) {
					Grid newGrid = new Grid();
					newGrid.LoadState(Grid.SaveState());

					newGrid.Cells[bestCandidate].Value = p;

					Puzzle puzzle = new Puzzle(Name, newGrid);
					puzzle.SaveStateHistory = SaveStateHistory;

					puzzle.Solve("Guess");
					
					if(puzzle.Solved) {
						Grid.LoadState(newGrid.SaveState());
						State.Concat(puzzle.State);
						Solved = true;
						return true;
					}
				}
			}

			return false;
		}
	}
}
