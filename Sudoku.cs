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

			// simple checking of rows, columns and boxes
			while (!Solved && Iterate()) { }
		}

		public bool Iterate()
		{
			bool changed = false;
			bool passSolved = true;
			foreach(Cell cell in Grid.Cells) {
				if (cell.Value == 0) {
					foreach(Cell adj in cell.AdjacentCells) {
						if(adj.Value > 0)
							cell.Possibilities.Remove(adj.Value);
					}
					
					if(cell.Possibilities.Count == 1) {
						cell.Value = cell.Possibilities.First();
						changed = true;
					} else {
						passSolved = false;
					}
				}
			}
			Solved = passSolved;
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
