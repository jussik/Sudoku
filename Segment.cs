using System.Collections.Generic;

namespace Sudoku
{
	/// <summary>
	/// A segment of <see cref="Cell"/>s in a <see cref="Grid"/>.
	/// </summary>
	public abstract class Segment
	{
		/// <summary>
		/// Gets the sequence number of the segment in its <see cref="Grid"/>.
		/// </summary>
		public int Id { get; private set; }
		/// <summary>
		/// The parent <see cref="Grid"/> of the segment.
		/// </summary>
		public Grid Grid { get; private set; }
		
		protected Cell[] cells;
		/// <summary>
		/// All of the <see cref="Cell"/>s in the segment.
		/// </summary>
		public Cell[] Cells {
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

		/// <summary>
		/// Initialise the array of <see cref="Cell"/>s.
		/// </summary>
		protected abstract void InitCells();

		/// <summary>
		/// Returns a <see cref="String"/> that represents the current <see cref="Segment"/>.
		/// </summary>
		public override string ToString()
		{
			return string.Join<Cell>(" ", cells);
		}
	}

	/// <summary>
	/// A row of <see cref="Cell"/>s in a grid.
	/// </summary>
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
	
	/// <summary>
	/// A column of <see cref="Cell"/>s in a grid.
	/// </summary>
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
	
	/// <summary>
	/// A 3x3 box of <see cref="Cell"/>s in a grid.
	/// </summary>
	public class Box : Segment
	{
		private Row[] rows;
		/// <summary>
		/// Gets the <see cref="Row"/>s that intersect with this <see cref="Box"/>.
		/// </summary>
		public Row[] Rows {
			get {
				if(rows ==  null) {
					int start = (Id/3) * 3;
					rows = new Row[] {
						Grid.Rows[start],
						Grid.Rows[start + 1],
						Grid.Rows[start + 2]
					};
				}
				return rows;
			}
		}

		private Column[] columns;
		/// <summary>
		/// Gets the <see cref="Column"/>s that intersect with this <see cref="Box"/>.
		/// </summary>
		public Column[] Columns {
			get {
				if(columns ==  null) {
					int start = (Id%3) * 3;
					columns = new Column[] {
						Grid.Columns[start],
						Grid.Columns[start + 1],
						Grid.Columns[start + 2]
					};
				}
				return columns;
			}
		}

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

