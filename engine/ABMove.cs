using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace GomokuEngine
{
    public class ABMove
    {
        public int square;
        public Player player;
        public BothPlayerEvaluation moveType;
        public int value;
        public TTEvaluationType valueType;
        public int examinedMoves;
        public int depth;
        public Player vctPlayer;
        public TimeSpan time;
        public VctStatus vctBlack;
        public VctStatus vctWhite;
        public int vctBlackDepth;
        public int vctWhiteDepth;
        public int examinedMovesVctBlack;
        public int examinedMovesVctWhite;

        int boardSize;

        public ABMove()
        {
        }

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
        }

        public override string ToString()
        {
            Conversions conversions = new Conversions(boardSize);
            return conversions.Complete(square);
        }

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


            string[] str = { Convert.ToString(index), this.ToString() + " (" + square.ToString() + ")", time.TotalSeconds.ToString(),
                               moveType.ToString(), s2, vctPlayer.ToString(), examinedMoves.ToString(), depth.ToString(),
                               vctBlack.ToString(), vctWhite.ToString(), vctBlackDepth.ToString(), vctWhiteDepth.ToString(),
                               examinedMovesVctBlack.ToString(), examinedMovesVctWhite.ToString()};
            return str;
        }

        public int Row
        {
            get
            {
                Conversions conversions = new Conversions(boardSize);
                return conversions.Index2Row(square);
            }
        }

        public int Column
        {
            get
            {
                Conversions conversions = new Conversions(boardSize);
                return conversions.Index2Column(square);
            }
        }
    }

}
