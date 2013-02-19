using System;
using System.Collections.Generic;
using System.Linq;

namespace Euler96
{
	public class Grid
	{
		public Cell[] Cells { get; private set; }
		public Row[] Rows { get; private set; }
		public Column[] Columns { get; private set; }
		public Box[] Boxes { get; private set; }

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
				int val = gridString[i] - 48;
				if(val < 0 || val > 9)
					throw new Exception("Unexpected character in input: " + gridString[i]);

				Cell c = new Cell {
					Grid = this,
					Location = i,

					Row = RowOf(i),
					Column = ColumnOf(i),
					Box = BoxOf(i),

					Value = val
				};

				if(val > 0)
					c.Possibilities.Add(val);
				else
					c.Possibilities.UnionWith(Enumerable.Range(1,9));

				Cells[i] = c;
			}
		}

		public override string ToString()
		{
			return string.Join<Row>("\n", Rows);
		}

		private Row RowOf(int i) {
			return Rows[i / 9];
		}

		private Column ColumnOf(int i) {
			return Columns[i % 9];
		}

		private Box BoxOf(int i) {
			return Boxes[(i % 9 ) / 3 + 3 * (i / 27)];
		}
	}
}

