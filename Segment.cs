using System;
using System.Collections.Generic;

namespace Euler96
{
	public abstract class Segment
	{
		public int Id { get; private set; }
		public Grid Grid { get; private set; }
		
		public Cell[] cells;
		public IEnumerable<Cell> Cells {
			get {
				if(cells == null) {
					cells = new Cell[9];
					InitCells();
				}
				return cells;
			}
		}

		public Segment(int id, Grid grid) {
			Id = id;
			Grid = grid;
		}

		protected abstract void InitCells();

		public override string ToString()
		{
			return string.Join<Cell>(" ", cells);
		}
	}

	public class Row : Segment
	{
		public Row(int id, Grid grid) : base(id, grid) { }

		protected override void InitCells()
		{
			int start = Id * 9;
			for (int i=0; i<9; i++) {
				cells[i] = Grid.Cells[start + i];
			}
		}
	}
	
	public class Column : Segment
	{
		public Column(int id, Grid grid) : base(id, grid) { }

		protected override void InitCells()
		{
			for (int i=0; i<9; i++) {
				cells[i] = Grid.Cells[Id + 9 * i];
			}
		}
	}
	
	public class Box : Segment
	{
		public Box(int id, Grid grid) : base(id, grid) { }

		protected override void InitCells()
		{
			int start = 3 * ((Id % 3) + (Id / 3) * 9);
			for (int i=0; i<9; i++) {
				cells[i] = Grid.Cells[start + (i % 3) + (i / 3) * 9];
			}
		}
	}
}

