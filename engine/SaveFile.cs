using System;
using System.Collections.Generic;
using System.IO;


namespace GomokuEngine
{
	public class SaveFile
	{
		//		public FileParameters fileParameters;
		//		string fileName;

		public SaveFile(string fileName, int boardSize, string blackPlayerName, string whitePlayerName, List<BoardSquare> moveList)
		{
//			this.fileName = fileName;

//			fileParameters.BoardSize = boardSize;
//			fileParameters.BlackPlayerName = blackPlayerName;
//			fileParameters.WhitePlayerName = whitePlayerName;
//			fileParameters.MoveList = moveList;

			string extension = Path.GetExtension(fileName);
            
			switch (extension) {
				case ".psq":
					SavePsqFile(fileName, boardSize, blackPlayerName, whitePlayerName, moveList);
					break;
			}
		}

		void SavePsqFile(string fileName, int boardSize, string blackPlayerName, string whitePlayerName, List<BoardSquare> moveList)
		{
			//open stream
			var file = new StreamWriter(fileName);
			//write to stream
			//first line
			file.WriteLine("Piskvorky {0}x{0}, 6:6, 0", boardSize);
            
			foreach (BoardSquare move in moveList) {
				file.WriteLine("{0},{1},{2}", move.Index / boardSize + 1, move.Index % boardSize + 1, 0);
			}

			file.WriteLine("pbrain-{0}.exe", blackPlayerName);
			file.WriteLine("pbrain-{0}.exe", whitePlayerName);
			file.WriteLine("{0}", 0);

			file.Close();
		}

	}
}
