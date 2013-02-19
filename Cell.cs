using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
	/// <summary>
	/// A single cell in a Sudoku <see cref="Puzzle"/>.
	/// </summary>
	public class Cell
	{
		private int val;
		/// <summary>
		/// The numerical value of the <see cref="Cell"/>.
		/// </summary>
		public int Value {
			get {
				return val;
			}
			set {
				val = value;
				foreach(Cell adj in AdjacentCells) {
					adj.Possibilities.Remove(val);
				}
			}
		}
		/// <summary>
		/// The set of all remaining possible values.
		/// </summary>
		public HashSet<int> Possibilities { get; set; }

		/// <summary>
		/// The location of the <see cref="Cell"/> in the <see cref="Grid"/> as an integer between 0 and 80.
		/// </summary>
		public int Location { get; set; }
		/// <summary>
		/// The parent <see cref="Grid"/> of the <see cref="Cell"/>.
		/// </summary>
		public Grid Grid { get; set; }

		/// <summary>
		/// The <see cref="Row"/> where the <see cref="Cell"/> resides.
		/// </summary>
		public Row Row { get; set; }
		/// <summary>
		/// The <see cref="Column"/> where the <see cref="Cell"/> resides.
		/// </summary>
		public Column Column { get; set; }
		/// <summary>
		/// The <see cref="Box"/> where the <see cref="Cell"/> resides.
		/// </summary>
		public Box Box { get; set; }

		private Cell[] adjacents;
		/// <summary>
		/// All of the <see cref="Cell"/>s in the same <see cref="Row"/>, <see cref="Column"/> or <see cref="Box"/> as the current.
		/// </summary>
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

		/// <summary>
		/// Initializes a new <see cref="Cell"/>.
		/// </summary>
		public Cell()
		{
			Possibilities = new HashSet<int>();
		}

		/// <summary>
		/// Returns the value of the current <see cref="Cell"/> as a <see cref="String"/>.
		/// </summary>
		public override string ToString()
		{
			return Value.ToString();
		}
	}
}

