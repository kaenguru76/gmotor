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
        public List<BoardSquare> gameMoveList;
        public List<BoardSquare> playedMoves;
        public List<ABMove> possibleMoves;
        public BoardSquare nextMove;
		public BoardSquare GainSquare;
        
        public GameInformation()
        {
        }
    }
}
