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

        public List<int> GeneratePossibleMoves(Sorting sorting)
        {
            List<int> moves = new List<int>();

            sorting.AddMovesToList(BothPlayerEvaluation.overline_defending, moves);
            if (sorting.AddMovesToList(BothPlayerEvaluation.four_attacking, moves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.four_defending, moves) > 0) goto L1;

            if (sorting.AddMovesToList(BothPlayerEvaluation.o3_attacking, moves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.c3xc3_attacking, moves) > 0) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.c3xo2_attacking, moves);
            sorting.AddMovesToList(BothPlayerEvaluation.c3xo1_attacking, moves);
            sorting.AddMovesToList(BothPlayerEvaluation.s3_attacking, moves);
            sorting.AddMovesToList(BothPlayerEvaluation.c3_attacking, moves);

            int o3_defending = sorting.AddMovesToList(BothPlayerEvaluation.o3_defending, moves);
            sorting.AddMovesToList(BothPlayerEvaluation.c3xc3_defending, moves);
            sorting.AddMovesToList(BothPlayerEvaluation.c3xo2_defending, moves);
            if (o3_defending > 1) return moves;

            sorting.AddMovesToList(BothPlayerEvaluation.s3_defending, moves);
            if (o3_defending > 0) return moves;
            sorting.AddMovesToList(BothPlayerEvaluation.c3xo1_defending, moves);

            sorting.AddMovesToList(BothPlayerEvaluation.o2xo2_attacking, moves);
            sorting.AddMovesToList(BothPlayerEvaluation.vct_attacking, moves);

            sorting.AddMovesToList(BothPlayerEvaluation.o2xo2_defending, moves);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.c3_defending, moves);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.o2p_attacking, moves);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.tripple1_attacking, moves);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.vct_defending, moves);
            //if (moves.Count > 10) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.double1_attacking, moves) > 0) goto L1;
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.o2_attacking, moves);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.o2p_defending, moves);
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.tripple1_defending, moves);
            if (moves.Count > 10) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.double1_defending, moves) > 0) goto L1;
            if (moves.Count > 10) goto L1;
            sorting.AddMovesToList(BothPlayerEvaluation.o2_defending, moves);
            if (moves.Count > 10) goto L1;

            if (sorting.AddMovesToList(BothPlayerEvaluation.o1_both, moves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.o1p_attacking, moves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.o1_attacking, moves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.o1p_defending, moves) > 0) goto L1;
            if (sorting.AddMovesToList(BothPlayerEvaluation.o1_defending, moves) > 0) goto L1;

            //take random moves
            List<int> tmpList = new List<int>();
            sorting.AddMovesToList(BothPlayerEvaluation.rest, tmpList);

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

        public List<int> GeneratePossibleVctMoves(Sorting sorting)
        {
            List<int> moves = new List<int>();

            sorting.AddMovesToList(BothPlayerEvaluation.overline_defending, moves);
            if (sorting.AddMovesToList(BothPlayerEvaluation.four_attacking, moves) > 0) return moves;
            if (sorting.AddMovesToList(BothPlayerEvaluation.four_defending, moves) > 0) return moves;

            if (sorting.AddVCTMovesToList(BothPlayerEvaluation.o3_attacking, moves) > 0) return moves;
            if (sorting.AddVCTMovesToList(BothPlayerEvaluation.c3xc3_attacking, moves) > 0) return moves;
            sorting.AddVCTMovesToList(BothPlayerEvaluation.c3xo2_attacking, moves);
            sorting.AddVCTMovesToList(BothPlayerEvaluation.c3xo1_attacking, moves);
            sorting.AddVCTMovesToList(BothPlayerEvaluation.s3_attacking, moves);
            sorting.AddVCTMovesToList(BothPlayerEvaluation.c3_attacking, moves);

            //if (sorting.Exists(BothPlayerEvaluation.o3_defending)) return moves;
            //if (sorting.Exists(BothPlayerEvaluation.c3xc3_defending)) return moves;
            //if (sorting.Exists(BothPlayerEvaluation.s3_defending)) return moves;
            
            int o3_defending = sorting.AddVCTMovesToList(BothPlayerEvaluation.o3_defending, moves);
            sorting.AddVCTMovesToList(BothPlayerEvaluation.c3xc3_defending, moves);
            sorting.AddVCTMovesToList(BothPlayerEvaluation.c3xo2_defending, moves);
            if (o3_defending > 1) return moves;
            
            sorting.AddVCTMovesToList(BothPlayerEvaluation.s3_defending, moves);
            if (o3_defending > 0) return moves;
            //sorting.AddVCTMovesToList(BothPlayerEvaluation.vcf_defending, moves);

            sorting.AddVCTMovesToList(BothPlayerEvaluation.o2xo2_attacking, moves);
            sorting.AddVCTMovesToList(BothPlayerEvaluation.vct_attacking, moves);
            sorting.AddVCTMovesToList(BothPlayerEvaluation.o2p_attacking, moves);
            sorting.AddVCTMovesToList(BothPlayerEvaluation.o2_attacking, moves);

            return moves;
        }

    }
}
