using System;
using System.Collections.Generic;
using System.Linq;

namespace Euler96
{
	public class Cell
	{
		public int Value { get; set; }
		public HashSet<int> Possibilities { get; set; }
		
		public int Location { get; set; }
		public Grid Grid { get; set; }

		public Row Row { get; set; }
		public Column Column { get; set; }
		public Box Box { get; set; }

		private Cell[] adjacents;
		public IEnumerable<Cell> AdjacentCells {
			get {
				if(adjacents == null) {
					adjacents = Row.Cells
						.Union(Column.Cells)
						.Union(Box.Cells)
						.Where(c => c != this)
						.ToArray();
				}
				return adjacents;
			}
		}

		public Cell()
		{
			Possibilities = new HashSet<int>();
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}
}

