using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuEngine
{
    //this class is used to create text notation of a move
//	public class Conversions
//	{
//        int boardSize;
//        
//        public Conversions(int boardSize)
//        {
//            this.boardSize = boardSize;
//        }
//
//		//row notification
//		public string Row (int row)
//		{
//			string str1 = Convert.ToString(row + 1);
//			if (row < 9) str1 = " " + str1;
//			return (str1);
//		}
//
//		//column notification
//		public string Column (int column)
//		{
//            return (Convert.ToString("abcdefghijklmnopqrstuvwxyz"[column]));
//        }
//
//		//complete notification
//		public string Complete(int row, int column)
//		{
//			return (Column(column) +  Row(row));
//		}
//
//		public string Complete(int square)
//		{
//			if (square != -1)
//			{
//				int row = square / boardSize;
//				int column = square % boardSize;
//
//				return (Column(column) + Row(row));
//			}
//			else
//			{
//				return "null";
//			}
//		}
//		
//		public string Complete(List<ABMove> moveList)
//        {
//            string text = "";
//
//            for (int index = 0; index < moveList.Count; index++)
//            {
//                if (index == 0)
//                {
//                    text = text + moveList[index];
//                }
//                else
//                {
//                    text = text + ", " + moveList[index];
//                }
//            }
//            return text;
//        }
//
//		public int Square(string notification)
//		{
//			for (int row = 0; row < boardSize; row++)
//			{
//				for (int column = 0; column < boardSize; column++)
//				{
//					if (Complete(row, column) == notification)
//					{
//						return boardSize * row + column;
//					}
//				}
//			}
//			return -1;
//		}
//
//		public int Index2Row(int index)
//		{
//			return index / boardSize;
//		}
//
//		public int Index2Column(int index)
//		{
//			return index % boardSize;
//		}
//
//		public int RowAndColumn2Index(int row, int column)
//		{
//			return boardSize * row + column; 
//		}
//	}
}
