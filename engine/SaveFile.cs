using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
//using System.Xml.Serialization;

namespace GomokuEngine
{
	public struct FileParameters
	{
		public int BoardSize;
		public string BlackPlayerName;
		public string WhitePlayerName;
		public List<ABMove> MoveList;
	}

	public class SaveFile
	{
		public FileParameters fileParameters;
		string fileName;

        public SaveFile(string fileName, int boardSize, string blackPlayerName, string whitePlayerName, List<ABMove> moveList)
		{
			this.fileName = fileName;

			fileParameters.BoardSize = boardSize;
			fileParameters.BlackPlayerName = blackPlayerName;
			fileParameters.WhitePlayerName = whitePlayerName;
			fileParameters.MoveList = moveList;


            if (fileName.EndsWith(".psq"))
            {
                SavePsqFile(fileName);
            }
        }

		void SavePsqFile(string fileName)
		{
			//open stream
            StreamWriter file = new StreamWriter(fileName);
			//write to stream
            //first line
            file.WriteLine("Piskvorky {0}x{0}, 6:6, 0", fileParameters.BoardSize);
            
			foreach (ABMove move in fileParameters.MoveList)
			{
                file.WriteLine("{0},{1},{2}", move.square / fileParameters.BoardSize + 1, move.square % fileParameters.BoardSize + 1, 0);
			}

            file.WriteLine("pbrain-{0}.exe", fileParameters.BlackPlayerName);
            file.WriteLine("pbrain-{0}.exe", fileParameters.WhitePlayerName);
            file.WriteLine("{0}", 0);

            file.Close();
		}

    }
}
