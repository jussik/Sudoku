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
		/// Initializes a new <see cref="Sudoku.Grid"/> and prepares its Rows, Columns, Boxes and Cells.
		/// </summary>
		public Grid()
		{
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
		}

		/// <summary>
		/// Load the <see cref="Grid"/> with the contents of the string.
		/// </summary>
		public void Load(string gridString)
		{
			if(gridString.Length < 81)
				throw new Exception("Grid string not of length 81");

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

		public void WritePossibilitiesChart()
		{
			Console.WriteLine();
			Console.WriteLine(new string('-', 37));
			for (int r=0; r<9; r++) {
				Row row = Rows[r];
				for(int cr=0;cr<3;cr++) {
					Console.Write("|");
					for(int c=0;c<9;c++) {
						Cell cell = row.Cells[c];
						if(cell.Value > 0) {
							if(cr == 1) {
								Console.Write(">{0}<", cell.Value);
							} else {
								Console.Write("   ");
							}
						} else {
							for(int i=0;i<3;i++) {
								var val = (cr)*3+i+1;
								Console.Write(cell.Possibilities.Contains(val)
								              ? val.ToString()
								              : " ");
							}
						}
						Console.Write("|");
					}
					Console.WriteLine();
				}
				Console.WriteLine(new String('-', 37));
			}
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

		/// <summary>
		/// Saves the current grid into a <see cref="GridState"/>.
		/// </summary>
		public GridState SaveState()
		{
			return SaveState(null);
		}
		/// <summary>
		/// Saves the current grid into a <see cref="GridState"/> using a particular description.
		/// </summary>
		public GridState SaveState(string description)
		{
			GridState state = new GridState
			{
				Description = description,
				Cells = new CellState[81]
			};

			for (var i=0; i<81; i++) {
				state.Cells[i] = Cells[i].SaveState();
			}

			return state;
		}

		/// <summary>
		/// Loads the grid from an existing <see cref="GridState"/>.
		/// </summary>
		public void LoadState(GridState state) 
		{
			for(var i=0;i<81;i++) {
				Cells[i].LoadState(state.Cells[i]);
			}

		}
	}
}

