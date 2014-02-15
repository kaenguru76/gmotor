//#define CHECK 

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


            score = 0;

            #region initialize score table
		    scoreTable = new int[Enum.GetValues(typeof(BothPlayerEvaluation)).Length];
            
            for (int index1 = 0; index1 < scoreTable.Length; index1++)
            {
            	int opositeIndex = scoreTable.Length - index1 - 1;
            	
            	int score1 = opositeIndex*opositeIndex/**opositeIndex*/;

				//toggle score1 for defending moves            	
            	BothPlayerEvaluation eval = (BothPlayerEvaluation)index1;
            	string str1 = eval.ToString();
            	if (str1.Contains("defending"))
           	    {
					score1 = -score1;            			
            	}
            	scoreTable[index1] = score1; 
            }
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
                int nbMoves = AddMovesToList(eval, list1); 
                int score2 = scoreTable[(byte)eval];
            	score1 += nbMoves * score2;
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
