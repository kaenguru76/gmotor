using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuEngine
{
    public class GameInformation
    {
        public int Evaluation;
        //public int BlackScore;
        //public int WhiteScore;
        public List<ABMove> gameMoveList;
        public string fileName;
        public string blackPlayerName;
        public string whitePlayerName;
        public List<ABMove> playedMoves;
        public List<ABMove> possibleMoves;
        public ABMove nextMove;
		public ABMove GainSquare;
        
        public GameInformation()
        {
        }
    }
}
