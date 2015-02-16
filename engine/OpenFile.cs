using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GomokuEngine
{
	public class OpenFile
	{
		//string fileName;
		//public FileParameters fileParameters;

        public OpenFile(string fileName, out int boardSize, out string blackPlayerName, out string whitePlayerName, 
                         out List<BoardSquare> moveList)
		{
			boardSize = 0;
			blackPlayerName = "";
			whitePlayerName = "";
			moveList = null;
			
			//this.fileName = fileName;
            string extension = Path.GetExtension(fileName);
            
			switch (extension) {
				case ".psq":
					OpenPsqFile(fileName, out boardSize, out blackPlayerName, out whitePlayerName, out moveList);
					break;
                
			}
		}

		void OpenPsqFile(string fileName, out int boardSize, out string blackPlayerName, out string whitePlayerName, 
                         out List<BoardSquare> moveList)
		{
			boardSize = 0;
			blackPlayerName = "";
			whitePlayerName = "";
			moveList = null;
			
			//open stream
			var file = new StreamReader(fileName);

            // decode first line
            string line = file.ReadLine();
            var r = new Regex(@"(\w+)\s+(\d+)x(\d+),\s*(\d+):(\d+),\s*(\d+)");
            Match m = r.Match(line);
            if (!m.Success) return;
            if (m.Groups[1].Value != "Piskvorky") return;
            if (m.Groups[2].Value != m.Groups[3].Value) return;
            boardSize = Convert.ToInt32(m.Groups[2].Value);
            
            //decode move list
            r = new Regex(@"(\d+),(\d+),(\d+)");
			moveList = new List<BoardSquare>();
			while ((line = file.ReadLine()) != null)
			{
				//scan line
				m = r.Match(line);
                if (!m.Success) break;

				//exctract information
				int row = Convert.ToInt32(m.Groups[1].Value)-1;
                int column = Convert.ToInt32(m.Groups[2].Value)-1;
                var timeSpan = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(m.Groups[3].Value));
				//create new move
				int square = row * boardSize + column;
				Player player = (moveList.Count % 2 == 1) ? Player.WhitePlayer : Player.BlackPlayer;
				moveList.Add(new BoardSquare(boardSize, square));
			}

            //decode player names
            r = new Regex(@"pbrain-(\w+)\.exe");
            m = r.Match(line);
            if (!m.Success) return;
            blackPlayerName = m.Groups[1].Value;
            line = file.ReadLine();
            m = r.Match(line);
            if (!m.Success) return;
            whitePlayerName = m.Groups[1].Value;

			file.Close();
		}
	}
}
