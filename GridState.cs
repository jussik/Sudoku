using System.Linq;
using System.Web.Script.Serialization;

namespace Sudoku
{
	/// <summary>
	/// Store a a grid's state
	/// </summary>
	public class GridState
	{
		/// <summary>
		/// A description of the state.
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// All of the states of the cells in the grid.
		/// </summary>
		public CellState[] Cells { get; set; }

		public string ToJson()
		{
			return string.Format("[{0},{1}]", jsonString(Description), string.Join(",", Cells.Select(c => c.ToJson())));
		}
		
		public string ToJsonDiff(GridState previous)
		{
			return string.Format("[{0}{1}]", jsonString(Description),
                 string.Join("", Cells.Select((c,i) => c.ToJsonDiff(previous.Cells[i], i))
                     .Where(s => s != null)
                     .Select(s => "," + s))
            );
		}

		public bool HasChanged(GridState previous)
		{
			for (var i=0; i<81; i++) {
				if(Cells[i].Value != previous.Cells[i].Value || !Cells[i].Possibilities.SetEquals(previous.Cells[i].Possibilities))
					return true;
			}
			return false;
		}
		
		private string jsonString(string str) {
			return new JavaScriptSerializer().Serialize(str);
		}
	}
}

