using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
	public class StateHistory
	{
		public List<GridState> States { get; private set; }

		public StateHistory()
		{
			States = new List<GridState>();
		}

		public void Add(GridState state)
		{
			States.Add(state);
		}

		public void Concat(StateHistory history)
		{
			States.AddRange(history.States);
		}

		public string ToJson()
		{
			return string.Format("[{0},{1}]",
				States.First().ToJson(),
				string.Join(",", States.Skip(1).Select((gs, i) => gs.ToJsonDiff(States[i]))));
		}
	}
}

