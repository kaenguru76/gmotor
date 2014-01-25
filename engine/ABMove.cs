using System;
using System.Collections.Generic;
using System.Linq;
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

        int boardSize;

        public ABMove()
        {
        }

        public ABMove(int square, Player player, int boardSize, BothPlayerEvaluation moveType, 
            int value, TTEvaluationType valueType, int examinedMoves, int depth, Player vctPlayer, TimeSpan time)
        {
            this.square = square;
            this.player = player;
            this.boardSize = boardSize;
            this.moveType = moveType;
            this.value = value;
            this.valueType = valueType;
            this.examinedMoves = examinedMoves;
            this.depth = depth;
            this.vctPlayer = vctPlayer;
            this.time = time;
        }

        public ABMove(int square, Player player, int boardSize, TimeSpan time)
        {
            this.square = square;
            this.player = player;
            this.boardSize = boardSize;
            this.moveType = BothPlayerEvaluation.unknown;
            this.value = 0;
            this.valueType = TTEvaluationType.Unknown;
            this.depth = -1;
            this.time = time;
        }

        public override string ToString()
        {
            Conversions conversions = new Conversions(boardSize);
            return conversions.Complete(square);
        }

        public string[] ToStringArray(int index)
        {
            string s1;
            switch (value)
            {
                case int.MaxValue:
                    s1 = "INF";
                    break;
                case -int.MaxValue:
                    s1 = "-INF";
                    break;
                default:
                    s1 = value.ToString();
                    break;
            }

            string s2;
            switch (valueType)
            {
                case TTEvaluationType.Exact:
                    s2 = "= " + s1;
                    break;
                case TTEvaluationType.LowerBound:
                    s2 = "<= " + s1;
                    break;
                case TTEvaluationType.UpperBound:
                    s2 = ">= " + s1;
                    break;
                default:
                    s2 = "? " + s1;
                    break;
            }


            string[] str = { Convert.ToString(index), this.ToString() + " (" + square.ToString() + ")", time.TotalSeconds.ToString(),
                               moveType.ToString(), s2, vctPlayer.ToString(), examinedMoves.ToString(), depth.ToString() };
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
