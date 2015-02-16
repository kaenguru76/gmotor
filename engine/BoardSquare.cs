/*
 * Created by SharpDevelop.
 * User: Vančurovi
 * Date: 15.2.2015
 * Time: 22:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace GomokuEngine
{
	/// <summary>
	/// Description of BoardSquare.
	/// </summary>
	public class BoardSquare
	{
		readonly int _boardSize;
		readonly int _index;
		
		public BoardSquare(int boardSize, int row, int column)
		{
			_boardSize = boardSize;
			_index = boardSize * row + column;
		}
		
		public BoardSquare(int boardSize, int index)
		{
			_boardSize = boardSize;
			_index = index;
		}
		
		public BoardSquare(int boardSize, string notification)
		{
			_boardSize = boardSize;

			for (int row = 0; row < boardSize; row++)
			{
				for (int column = 0; column < boardSize; column++)
				{
					if (ToString(row, column) == notification)
					{
						_index = boardSize * row + column;
						return;
					}
				}
			}
			throw new NotImplementedException();
		}

		public int Index
		{
			get {
				return _index;
			}
		}
		
		public int Row 
		{
			get {
				return _index / _boardSize;
			}
		}

		public int Column 
		{
			get {
				return _index % _boardSize;
			}
		}
		
		public string RowToString()
		{
			return RowToString(Row);
		}

		public string RowToString(int row)
		{
			string str1 = Convert.ToString(row + 1);
			if (row < 9) str1 = " " + str1;
			return (str1);
		}
		
		public string ColumnToString()
		{
			return ColumnToString(Column);
        }
		
		public string ColumnToString(int column)
		{
           	return (Convert.ToString("abcdefghijklmnopqrstuvwxyz"[column]));
        }

		public override string ToString()
		{
			return ColumnToString() + RowToString();
		}

		public string ToString(int row, int column)
		{
			return ColumnToString(column) + RowToString(row);
		}
	}
}
