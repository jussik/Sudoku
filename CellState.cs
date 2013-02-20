using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
	/// <summary>
	/// Store a cell's value and possibilities
	/// </summary>
	public class CellState
	{
		/// <summary>
		/// The numerical value of the <see cref="CellState"/>.
		/// </summary>
		public int Value { get; set; }
		/// <summary>
		/// The set of all remaining possible values.
		/// </summary>
		public HashSet<int> Possibilities { get; set; }
		
		public string ToJson()
		{
			if(Value > 0)
				return Value.ToString();
			else
				return string.Format("[{0}]", string.Join(",", Possibilities));
		}
		
		public string ToJsonDiff(CellState previous, int index)
		{
			if (Value > 0) {
				if(Value == previous.Value)
					return null;
			} else if(Possibilities.SetEquals(previous.Possibilities)) {
					return null;
			}

			return string.Format("[{0},{1}]", index, ToJson());

		}
	}
}

