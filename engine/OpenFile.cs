using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
//using System.Xml.Serialization;


namespace GomokuEngine
{
	public class OpenFile
	{
		string fileName;
		public FileParameters fileParameters;

        public OpenFile(string fileName)
		{
            this.fileName = fileName;

            if (fileName.EndsWith(".psq"))
            {
                OpenPsqFile(fileName);
            }
		}

		void OpenPsqFile(string fileName)
		{
			//open stream
			StreamReader file = new StreamReader(fileName);

            // decode first line
            string line = file.ReadLine();
            Regex r = new Regex(@"(\w+)\s+(\d+)x(\d+),\s*(\d+):(\d+),\s*(\d+)");
            Match m = r.Match(line);
            if (!m.Success) return;
            if (m.Groups[1].Value != "Piskvorky") return;
            if (m.Groups[2].Value != m.Groups[3].Value) return;
            fileParameters.BoardSize = Convert.ToInt32(m.Groups[2].Value);
            
            //decode move list
            r = new Regex(@"(\d+),(\d+),(\d+)");
			fileParameters.MoveList = new List<ABMove>();
			while ((line = file.ReadLine()) != null)
			{
				//scan line
				m = r.Match(line);
                if (!m.Success) break;

				//exctract information
				int row = Convert.ToInt32(m.Groups[1].Value)-1;
                int column = Convert.ToInt32(m.Groups[2].Value)-1;
                TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(m.Groups[3].Value));
				//create new move
				int square = row * fileParameters.BoardSize + column;
				Player player = (fileParameters.MoveList.Count % 2 == 1) ? Player.WhitePlayer : Player.BlackPlayer;
				fileParameters.MoveList.Add(new ABMove(square, player, fileParameters.BoardSize,timeSpan));
			}

            //decode player names
            r = new Regex(@"pbrain-(\w+)\.exe");
            m = r.Match(line);
            if (!m.Success) return;
            fileParameters.BlackPlayerName = m.Groups[1].Value;
            line = file.ReadLine();
            m = r.Match(line);
            if (!m.Success) return;
            fileParameters.WhitePlayerName = m.Groups[1].Value;

			file.Close();
		}
	}
}
