using System;
using System.Linq;

namespace Sudoku
{
	public class Grid
	{
		/// <summary>
		/// All of the <see cref="Cell"/>s in the <see cref="Grid"/>.
		/// </summary>
		public Cell[] Cells { get; private set; }
		/// <summary>
		/// All of the <see cref="Row"/>s in the <see cref="Grid"/>.
		/// </summary>
		public Row[] Rows { get; private set; }
		/// <summary>
		/// All of the <see cref="Column"/>s in the <see cref="Grid"/>.
		/// </summary>
		public Column[] Columns { get; private set; }
		/// <summary>
		/// All of the <see cref="Box"/>es in the <see cref="Grid"/>.
		/// </summary>
		public Box[] Boxes { get; private set; }

		/// <summary>
		/// Load the <see cref="Grid"/> with the contents of the string.
		/// </summary>
		public void Load(string gridString)
		{
			if(gridString.Length < 81)
				throw new Exception("Grid string not of length 81");
			
			Rows = new Row[9];
			Columns = new Column[9];
			Boxes = new Box[9];
			for (var i=0; i<9; i++) {
				Rows[i] = new Row(i, this);
				Columns[i] = new Column(i, this);
				Boxes[i] = new Box(i, this);
			}

			Cells = new Cell[81];
			for (var i=0; i<81; i++) {
				Cell c = new Cell {
					Grid = this,
					Location = i,
					
					Row = RowOf(i),
					Column = ColumnOf(i),
					Box = BoxOf(i)
				};

				c.Possibilities.UnionWith(Enumerable.Range(1,9));
				
				Cells[i] = c;
			}

			// Needs to be in two passes as settings values requires all adjacents to exist
			for (var i=0; i<81; i++) {
				int val = gridString[i] - 48;
				if(val < 0 || val > 9)
					throw new Exception("Unexpected character in input: " + gridString[i]);

				Cells[i].Value = val;
			}
		}

		/// <summary>
		/// Returns a <see cref="String"/> that represents the current <see cref="Grid"/>.
		/// </summary>
		public override string ToString()
		{
			return string.Join<Row>("\n", Rows);
		}

		/// <summary>
		/// Returns the <see cref="Row"/> at a specific location index.
		/// </summary>
		public Row RowOf(int i) {
			return Rows[i / 9];
		}
		
		/// <summary>
		/// Returns the <see cref="Column"/> at a specific location index.
		/// </summary>
		public Column ColumnOf(int i) {
			return Columns[i % 9];
		}
		
		/// <summary>
		/// Returns the <see cref="Box"/> at a specific location index.
		/// </summary>
		public Box BoxOf(int i) {
			return Boxes[(i % 9 ) / 3 + 3 * (i / 27)];
		}
	}
}

