using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace GomokuEngine
{
	class Sorting
	{
        BothPlayerEvaluation[] evaluation;
        int[] scoreTable;
        int score;
        
        int[] firstItem;
        int[] previousItem;
        int[] nextItem;
        VCT threats;

        public Sorting(int boardSize, VCT threats)
		{
			//compute number of squares of board
			int numberOfSquares = boardSize * boardSize;

            evaluation = new BothPlayerEvaluation[numberOfSquares];
            for (int square = 0; square < numberOfSquares; square++)
            {
                evaluation[square] = BothPlayerEvaluation.unknown;
            }

            scoreTable = new int[Enum.GetValues(typeof(BothPlayerEvaluation)).Length];

            score = 0;

            #region initialize score table
            scoreTable[(byte)BothPlayerEvaluation.four_attacking] = 100;
            scoreTable[(byte)BothPlayerEvaluation.four_defending] = -121;
            scoreTable[(byte)BothPlayerEvaluation.o3_attacking] = 100;
            scoreTable[(byte)BothPlayerEvaluation.o3_defending] = -100;
            scoreTable[(byte)BothPlayerEvaluation.s3_attacking] = 70;
            scoreTable[(byte)BothPlayerEvaluation.s3_defending] = -57;
            scoreTable[(byte)BothPlayerEvaluation.c3xc3_attacking] = 80;
            scoreTable[(byte)BothPlayerEvaluation.c3xc3_defending] = -80;
            scoreTable[(byte)BothPlayerEvaluation.vcf_attacking] = 70;
            scoreTable[(byte)BothPlayerEvaluation.vcf_defending] = -52;
            scoreTable[(byte)BothPlayerEvaluation.c3_attacking] = 66;
            scoreTable[(byte)BothPlayerEvaluation.c3_defending] = -66;
            scoreTable[(byte)BothPlayerEvaluation.o2xo2_attacking] = 30;
            scoreTable[(byte)BothPlayerEvaluation.o2xo2_defending] = -30;
            scoreTable[(byte)BothPlayerEvaluation.vct_attacking] = 30;
            scoreTable[(byte)BothPlayerEvaluation.vct_defending] = -30;
            scoreTable[(byte)BothPlayerEvaluation.o2p_attacking] = 10;
            scoreTable[(byte)BothPlayerEvaluation.o2_attacking] = 10;
            scoreTable[(byte)BothPlayerEvaluation.o2p_defending] = -10;
            scoreTable[(byte)BothPlayerEvaluation.o2_defending] = -10;
            scoreTable[(byte)BothPlayerEvaluation.tripple1_attacking] = 5;
            scoreTable[(byte)BothPlayerEvaluation.tripple1_defending] = -5;
            scoreTable[(byte)BothPlayerEvaluation.double1_attacking] = 5;
            scoreTable[(byte)BothPlayerEvaluation.double1_defending] = -5;
            scoreTable[(byte)BothPlayerEvaluation.o1_both] = 5;
            scoreTable[(byte)BothPlayerEvaluation.o1p_attacking] = 5;
            scoreTable[(byte)BothPlayerEvaluation.o1_attacking] = 5;
            scoreTable[(byte)BothPlayerEvaluation.o1p_defending] = -5;
            scoreTable[(byte)BothPlayerEvaluation.o1_defending] = -5;
            scoreTable[(byte)BothPlayerEvaluation.rest] = 0;
            scoreTable[(byte)BothPlayerEvaluation.forbidden] = 0;
            scoreTable[(byte)BothPlayerEvaluation.occupied] = 0;
            scoreTable[(byte)BothPlayerEvaluation.unknown] = 0;
            #endregion

            #region initialize sorting

            previousItem = new int[numberOfSquares];
			nextItem = new int[numberOfSquares];

			for (int index = 0; index < numberOfSquares; index++)
			{
				previousItem[index] = index - 1;
				nextItem[index] = index + 1;
			}

			//fix last item
			nextItem[numberOfSquares - 1] = -1;

			//initialize first item
            firstItem = new int[Enum.GetValues(typeof(BothPlayerEvaluation)).Length];

            for (int index = 0; index < Enum.GetValues(typeof(BothPlayerEvaluation)).Length; index++)
			{
				firstItem[index] = -1;
			}
            firstItem[(int)BothPlayerEvaluation.unknown] = 0;

			#endregion

            this.threats = threats;
		}

        void SortSquare(int square, BothPlayerEvaluation oldEvaluation, BothPlayerEvaluation newEvaluation)
		{
            //remove square from old position
			int previous = previousItem[square];
			if (previous == -1)
			{
				//fix forward pointer
				int next = firstItem[(int)oldEvaluation] = nextItem[square];

				//fix backward pointer
				if (next != -1)
				{
					//this is the first item in the list
					previousItem[next] = -1;
				}
			}
			else
			{
				//fix forward pointer
				int next = nextItem[previous] = nextItem[square];

				//fix backward pointer
				if (next != -1)
				{
					previousItem[next] = previousItem[square];
				}
			}

			//place line to new position
			int first = firstItem[(int)newEvaluation];
            nextItem[square] = first;

			//is something in list?
			if (first != -1)
			{
				//fix backward pointer
				previousItem[first] = square;
			}

			//fix pointers of line
			previousItem[square] = -1;
			firstItem[(int)newEvaluation] = square;
        }

        public bool Modify(int square, BothPlayerEvaluation newEvaluation)
        {
            //get old evaluation
            BothPlayerEvaluation oldEvaluation = evaluation[square];

            if (newEvaluation == oldEvaluation) return false;

            SortSquare(square, oldEvaluation, newEvaluation);

            //modify evaluation
            score += scoreTable[(byte)newEvaluation] - scoreTable[(byte)oldEvaluation];

            evaluation[square] = newEvaluation;

#if CHECK
            int score1 = 0;
            List<int> list1 = new List<int>();
            for (BothPlayerEvaluation eval = BothPlayerEvaluation.four_attacking; eval < BothPlayerEvaluation.unknown; eval++)
            {
                score1 += sorting.AddMovesToList(eval, list1) * scoreEvaluation[(byte)eval];
            }
            System.Diagnostics.Debug.Assert(score == score1,"Score value is wrong!");
#endif
            return true;
        }


        /*	function adds moves to list */
        public int AddMovesToList(BothPlayerEvaluation evaluation, List<int> moves)
        {
            //get first square
            int square = firstItem[(int)evaluation];
            int addedMoves = 0;

            //loop through all squares
            while (square != -1)
            {
                moves.Add(square);
                addedMoves++;

                //get next square
                square = nextItem[square];
            }

            return addedMoves;
        }

        /*	function adds moves to list */
        public int AddVCTMovesToList(BothPlayerEvaluation evaluation, List<int> moves)
        {
            //get first square
            int square = firstItem[(int)evaluation];
            int addedMoves = 0;

            //loop through all squares
            while (square != -1)
            {
                if (threats.CreatesVct(square))
                {
                    moves.Add(square);
                    addedMoves++;
                }
                //get next square
                square = nextItem[square];
            }

            return addedMoves;
        }
        
        public bool IsWinner()
        {
            if (firstItem[(int)BothPlayerEvaluation.four_attacking] >= 0) return true;
            if (firstItem[(int)BothPlayerEvaluation.four_defending] >= 0) return false;
            if (firstItem[(int)BothPlayerEvaluation.o3_attacking] >= 0) return true;

            return false;
        }

        public bool Exists(BothPlayerEvaluation evaluation)
        {
            if (firstItem[(int)evaluation] >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Score
        {
            get
            {
                return score;
            }
        }

        public BothPlayerEvaluation GetEvaluation(int square)
        {
            return evaluation[square];
        }

        public bool IsWinningMove(int square)
        {
            if (evaluation[square] == BothPlayerEvaluation.four_attacking)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int[] GetScoreEvaluation()
        {
            return scoreTable;
        }

        public void SetScoreEvaluation(int[] scoreEvaluation)
        {
            for (int index = 0; index < scoreEvaluation.Length; index++)
            {
                this.scoreTable[index] = scoreEvaluation[index];
            }
        }

    }
}
