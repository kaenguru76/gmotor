using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace GomokuEngine
{
	public class ABMove : BoardSquare
    {
        //public int square;
        readonly Player _player;
        public BothPlayerEvaluation moveType;
        public int value;
        public TTEvaluationType valueType;
        public int examinedMoves;
        public int depthLeft;
        public Player vctPlayer;
        public TimeSpan time;

       // int boardSize;
/*
        public ABMove()
        {
        }*/

		public ABMove(int boardSize, int index, Player player) : base(boardSize, index)
		{
//			_boardSize = boardSize;
//			_index = index;
			_player = player;
		}
		/*
        public ABMove(int square, Player player, int boardSize, TimeSpan time)
        {
            this.square = square;
            this.player = player;
            this.boardSize = boardSize;
            this.time = time;
        }

        public ABMove(int square, Player player, int boardSize)
        {
            this.square = square;
            this.player = player;
            this.boardSize = boardSize;
        }*/

//        public override string ToString()
//        {
//            Conversions conversions = new Conversions(boardSize);
//            return conversions.Complete(square);
//        }

        public string[] ToStringArray(int index)
        {
            string s1 = EvaluationConstants.Score2Text(value);

            string s2;
            switch (valueType)
            {
                case TTEvaluationType.LowerBound:
                    s2 = ">= " + s1;
                    break;
                case TTEvaluationType.UpperBound:
                    s2 = "<= " + s1;
                    break;
                default:
                    s2 = s1;
                    break;
            }


            string[] str = { Convert.ToString(index), this.ToString() + " (" + Index.ToString() + ")", time.TotalSeconds.ToString(),
                               moveType.ToString(), s2, vctPlayer.ToString(), examinedMoves.ToString(), depthLeft.ToString(),
                               };
            return str;
        }

//        public int Row
//        {
//            get
//            {
//                Conversions conversions = new Conversions(boardSize);
//                return conversions.Index2Row(square);
//            }
//        }
//
//        public int Column
//        {
//            get
//            {
//                Conversions conversions = new Conversions(boardSize);
//                return conversions.Index2Column(square);
//            }
//        }
    }

}
