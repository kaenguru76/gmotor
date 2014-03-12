using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace GomokuEngine
{
    class MoveGenerator
    {
        int boardSize;

        public MoveGenerator(int boardSize)
        {
            this.boardSize = boardSize;
        }

        public List<int> GeneratePossibleMoves(Sorting sorting, bool vctAttackingMoves)
        {
            List<int> moves = new List<int>();

            sorting.AddMovesToList(BothPlayerEvaluation.overline_defending, moves, false);
            if (sorting.AddMovesToList(BothPlayerEvaluation.four_attacking, moves, vctAttackingMoves) > 0) goto L1;
            
            if (sorting.AddMovesToList(BothPlayerEvaluation.four_defending_vct, moves, false) > 0) goto L1;
            
            //if (vctAttackingMoves && sorting.Exists(BothPlayerEvaluation.four_defending)) goto L1;
            
            if (sorting.AddMovesToList(BothPlayerEvaluation.four_defending, moves, false) > 0) goto L1;

            if (sorting.AddMovesToList(BothPlayerEvaluation.o3_attacking, moves, vctAttackingMoves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.c3xc3_attacking, moves, vctAttackingMoves) > 0) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.c3xo2_attacking, moves, vctAttackingMoves);
            sorting.AddMovesToList(BothPlayerEvaluation.c3xo1_attacking, moves, vctAttackingMoves);
            sorting.AddMovesToList(BothPlayerEvaluation.s3_attacking, moves, vctAttackingMoves);
            
            //if (vctAttackingMoves == false)
            //{
            	sorting.AddMovesToList(BothPlayerEvaluation.c3_attacking, moves, vctAttackingMoves);
            //}
            int o3_defending = sorting.AddMovesToList(BothPlayerEvaluation.o3_defending_vct, moves, false);
            if (o3_defending > 0 && vctAttackingMoves) goto L1;
            if (vctAttackingMoves && sorting.Exists(BothPlayerEvaluation.o3_defending)) goto L1;

            o3_defending += sorting.AddMovesToList(BothPlayerEvaluation.o3_defending, moves, false); 
            if (o3_defending == 1)
    	       	sorting.AddMovesToList(BothPlayerEvaluation.s3_defending, moves, false);
            if (o3_defending > 0) goto L1;

            if (vctAttackingMoves == false)
            {
    	        sorting.AddMovesToList(BothPlayerEvaluation.c3xc3_defending, moves, false);
    	        sorting.AddMovesToList(BothPlayerEvaluation.c3xo2_defending, moves, false);
			    sorting.AddMovesToList(BothPlayerEvaluation.c3xo1_defending, moves, false);
	            sorting.AddMovesToList(BothPlayerEvaluation.c3_defending, moves, false);
            }

            sorting.AddMovesToList(BothPlayerEvaluation.o2xo2_attacking, moves, vctAttackingMoves);
            sorting.AddMovesToList(BothPlayerEvaluation.o2xo1_attacking, moves, vctAttackingMoves);

            if (vctAttackingMoves) goto L1;

            sorting.AddMovesToList(BothPlayerEvaluation.o2xo2_defending, moves, false);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.o2xo1_defending, moves, false);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.double1_both, moves, vctAttackingMoves);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.o2p_attacking, moves, vctAttackingMoves);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.o2_attacking, moves, vctAttackingMoves);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.tripple1_attacking, moves, vctAttackingMoves);
            if (moves.Count > 10) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.double1_attacking, moves, vctAttackingMoves) > 0) goto L1;
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.o2p_defending, moves, false);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.o2_defending, moves, false);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.tripple1_defending, moves, false);
            if (moves.Count > 10) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.double1_defending, moves, false) > 0) goto L1;
            if (moves.Count > 10) goto L1;

            if (sorting.AddMovesToList(BothPlayerEvaluation.o1_both, moves, vctAttackingMoves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.o1p_attacking, moves, vctAttackingMoves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.o1_attacking, moves, vctAttackingMoves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.o1p_defending, moves, vctAttackingMoves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.o1_defending, moves, vctAttackingMoves) > 0) goto L1;

            //take random moves
            List<int> tmpList = new List<int>();
            sorting.AddMovesToList(BothPlayerEvaluation.rest, tmpList, vctAttackingMoves);

            //take only central moves
            List<int> tmpList2 = new List<int>();
            Conversions con = new Conversions(boardSize);
            foreach (int move in tmpList)
            {
                int row = con.Index2Row(move);
                int col = con.Index2Column(move);
                if (row >= 4 && row < boardSize - 4 && col >= 4 && col < boardSize - 4)
                {
                    tmpList2.Add(move);
                }
            }
            if (tmpList2.Count > 0)
            {
                Random r = new Random();
                int ri = r.Next(0, tmpList2.Count - 1);
                moves.Add(tmpList2[ri]);
                goto L1;
            }

            // no central moves
            if (tmpList.Count > 0)
            {
                Random r = new Random();
                int ri = r.Next(0, tmpList.Count - 1);
                moves.Add(tmpList[ri]);
                goto L1;
            }
L1:
            return moves;
        }


    }
}
