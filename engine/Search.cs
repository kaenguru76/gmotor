using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace GomokuEngine
{
    class Search   
    {
        public delegate void ThinkingFinishedEvent(SearchInformation info);
        public event ThinkingFinishedEvent ThinkingFinished;

        public delegate void ThinkingProgressEvent(SearchInformation info);
        public event ThinkingProgressEvent ThinkingProgress;

        DateTime startTime;
        TimeSpan maxThinkingTime;
        int timeoutCounter;
        int maxSearchDepth;
        bool iterativeDeepening;
		bool stopThinking;
        SearchInformation searchInfo;
        List<ABMove> playedMoves;
        GameBoard gameBoard;
        TranspositionTable transpositionTable;

		public Search(GameBoard gameBoard, TranspositionTable transpositionTable)
		{
            this.gameBoard = gameBoard;
            this.transpositionTable = transpositionTable;

            searchInfo = new SearchInformation();
            maxThinkingTime = new TimeSpan(0, 0, 5);
            iterativeDeepening = true;
            maxSearchDepth = 30;
            playedMoves = new List<ABMove>();
        }

        public void RootSearch()
        {
            //gameBoard.NewSearch();

            //reset number of alpha-beta cutoffs
            searchInfo.nbCutoffs = 0;
            searchInfo.nbVCTCutoffs = 0;

            startTime = DateTime.Now;

            //initialize counter
            timeoutCounter = 0;
            
            //initialize time measurement
			stopThinking = false;

            searchInfo.examinedMoves = 0;
            //start time
			searchInfo.elapsedTime = new TimeSpan(0);

			searchInfo.winner = Player.None;
            searchInfo.bestMove = null;
            searchInfo.reachedDepth = 0;

            int startingDepth;
            //searchInfo.evaluation = -int.MaxValue;

            searchInfo.evaluation = gameBoard.GetEvaluation();

            if (iterativeDeepening)
            {
                startingDepth = 0;
            }
            else
            {
                startingDepth = maxSearchDepth;
            }

            for (int depth = startingDepth; depth <= maxSearchDepth; depth++)
            {
                int bestValue = -int.MaxValue;
                ABMove bestMove = null;

                if (depth == 0)
                {
                    // start VCT
                    gameBoard.VctActive = true;
                }

                //generate moves
                searchInfo.possibleMoves = gameBoard.GeneratePossibleMoves();

                foreach (ABMove move in searchInfo.possibleMoves)
                {
                    int beta;
                    int alpha;

                    #region MDT search
                    //int upperbound = int.MaxValue;
                    //int lowerbound = -int.MaxValue;
                    //do
                    //{
                    //    if (move.value == lowerbound)
                    //    {
                    //        beta = move.value + 1;
                    //    }
                    //    else
                    //    {
                    //        beta = move.value;
                    //    }
                    //    alpha = beta - 1;

                    //    gameBoard.MakeABMove(move);
                    //    if (depth == 0)
                    //    {
                    //        move.value = -AlphaBetaVCT(depth);
                    //    }
                    //    else
                    //    {
                    //        move.value = -AlphaBeta(depth, -beta, -alpha);
                    //    }
                    //    gameBoard.UndoABMove();

                    //    if (TimeoutReached())
                    //    {
                    //        if (searchInfo.bestMove == null) searchInfo.bestMove = move;
                    //        goto L1;
                    //    }

                    //    if (move.value < beta)
                    //    {
                    //        upperbound = move.value;
                    //    }
                    //    else
                    //    {
                    //        lowerbound = move.value;
                    //    }
                    //} while (lowerbound < upperbound);
                    #endregion

                    #region AB search
                    alpha = -int.MaxValue;
                    beta = int.MaxValue;

                    gameBoard.MakeABMove(move);
                    move.value = -AlphaBeta(depth, -beta, -alpha);
                    gameBoard.UndoABMove();

                    searchInfo.examinedMoves++;

                    if (TimeoutReached())
                    {
                        if (searchInfo.bestMove == null) searchInfo.bestMove = move;
                        goto L1;
                    }
                    #endregion

                    if (move.value > bestValue || bestMove == null)
                    {
                        bestValue = move.value;
                        bestMove = move;

                        if (bestValue == int.MaxValue) break;
                    }
                }

                if (searchInfo.possibleMoves.Count == 0) break;

                searchInfo.possibleMoves = gameBoard.GeneratePossibleMoves();//to get values from TT
                searchInfo.reachedDepth = depth;
                searchInfo.evaluation = bestValue;
                searchInfo.bestMove = bestMove;
                searchInfo.vctActive = gameBoard.VctActive;

                if (depth == 0)
                {
                    //end VCT
                    gameBoard.VctActive = false;
                }

                if ((depth > 0 && bestValue == -int.MaxValue) || bestValue == int.MaxValue) break;
            } 
L1:

            //end VCT
            gameBoard.VctActive = false;
 
            if (searchInfo.evaluation == int.MaxValue)
            {
                searchInfo.winner = (gameBoard.GetPlayerOnMove() == Player.BlackPlayer) ? Player.BlackPlayer : Player.WhitePlayer;
            }

            if (searchInfo.evaluation == -int.MaxValue)
            {
                searchInfo.winner = (gameBoard.GetPlayerOnMove() == Player.BlackPlayer) ? Player.WhitePlayer : Player.BlackPlayer;
            }

            //searchInfo.possibleMoves = gameBoard.GeneratePossibleMoves();
            //searchInfo.examinedMoves = gameBoard.ExaminedMoves;

            //raise event with copy of search information
            ThinkingFinished(new SearchInformation(searchInfo));
        }

        int AlphaBeta(int depth, int alpha, int beta)
        {
            //quiescence search
            if (depth == 0)
            {
                // start VCT
                gameBoard.VctActive = true;
                int score = AlphaBetaVCT(0, alpha, beta);
                //stop VCT
                gameBoard.VctActive = false;
                return score;
            }

            //Debug.Assert(gameBoard.VctPlayer == Player.None);

            //look into TT if this position was not evaluated already before          
            TranspositionTableItem ttItem = transpositionTable.Lookup(gameBoard.VctPlayer);
            if (ttItem != null)
            {
                if (ttItem.depth == depth || ttItem.value == int.MaxValue)
                {
                    switch (ttItem.type)
                    {
                        case TTEvaluationType.Exact:
                            return ttItem.value;

                        case TTEvaluationType.LowerBound:
                            if (ttItem.value >= beta) return ttItem.value;
                            if (ttItem.value > alpha) alpha = ttItem.value;
                            break;

                        case TTEvaluationType.UpperBound:
                            if (ttItem.value <= alpha) return ttItem.value;
                            if (ttItem.value < beta) beta = ttItem.value;
                            break;
                    }
                }
            }
            
            int examinedMoves = searchInfo.examinedMoves;

            int bestMove = -1;
            int bestValue = -int.MaxValue;
            List<int> moves;

            //#region VCT search
            //    bestScore = AlphaBetaVCT(1);

            //    if (beta == int.MaxValue)
            //    {
            //        // start VCT
            //        gameBoard.VctActive = true;

            //        moves = gameBoard.GeneratePossibleSquares();
            //        foreach (int move in moves)
            //        {
            //            gameBoard.MakeMove(move);

            //            int score = -AlphaBetaVCT(0);

            //            gameBoard.UndoMove();

            //            if (TimeoutReached()) return bestScore;

            //            if (score > bestScore)
            //            {
            //                bestScore = score;
            //                bestMove = move;

            //                if (bestScore >= beta)
            //                {
            //                    searchInfo.nbCutoffs++;
            //                    break;
            //                }
            //            }
            //        }

            //        //stop VCT
            //        gameBoard.VctActive = false;

            //        if (bestScore == int.MaxValue)
            //        {
            //            transpositionTable.Store(bestScore, gameBoard.VctPlayer, TTEvaluationType.LowerBound, depth, bestMove, gameBoard.ExaminedMoves - examinedMoves);
            //            return bestScore;
            //        }
            //    }
            //#endregion

            if (depth == 1)
            {
                bestValue = gameBoard.GetEvaluation();
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.Exact, depth, bestMove, searchInfo.examinedMoves - examinedMoves);
                return bestValue;
            }

            bestValue = -int.MaxValue;

            //do normal search
            moves = gameBoard.GeneratePossibleSquares();

            foreach (int move in moves)
            {
                gameBoard.MakeMove(move);

                int value = -AlphaBeta(depth - 1, -beta, -alpha);

                gameBoard.UndoMove();

                searchInfo.examinedMoves++;

                if (TimeoutReached()) return bestValue;

                if (value > bestValue)
                {
                    bestValue = value;
                    bestMove = move; //this is up to now best move

                    if (value > alpha)
                    {
                        alpha = value;
                        if (value >= beta)
                        {
                            searchInfo.nbCutoffs++;
                            break;
                        }
                    }
                }
            }

//L1:
            if (bestValue <= alpha)  // an upper bound value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.UpperBound, depth, bestMove, searchInfo.examinedMoves - examinedMoves);
            else if (bestValue >= beta)  // lower bound value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.LowerBound, depth, bestMove, searchInfo.examinedMoves - examinedMoves);
            else // a true minimax value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.Exact, depth, bestMove, searchInfo.examinedMoves - examinedMoves);

            return bestValue;
        }

        int AlphaBetaVCT(int depth, int alpha, int beta)
        {
            ////look into TT if this position was not evaluated already before          
            //TranspositionTableItem ttItem = transpositionTable.Lookup(gameBoard.VctPlayer);
            //if (ttItem != null)
            //{
            //    if (ttItem.depth == depth/* || ttItem.value == int.MaxValue*/)
            //    {
            //        return ttItem.value;
            //    }
            //}

            //int examinedMoves = gameBoard.ExaminedMoves;

            int bestValue = -int.MaxValue;
            int bestMove = -1;

            //Terminal node?
            if (gameBoard.GetWinner() != Player.None) return gameBoard.GetEvaluation();

            //maximal depth reached
            if (depth == -17) return gameBoard.GetEvaluation();

            List<int> moves = gameBoard.GeneratePossibleSquares();
            foreach (int move in moves)
            {
                gameBoard.MakeMove(move);

                int value = -AlphaBetaVCT(depth - 1, -beta, -alpha);

                gameBoard.UndoMove();

                searchInfo.examinedMoves++;

                if (TimeoutReached()) return bestValue;

                if (value > bestValue)
                {
                    bestValue = value;
                    bestMove = move;

                    if (value > alpha)
                    {
                        alpha = value;
                        if (value >= beta)
                        {
                            searchInfo.nbVCTCutoffs++;
                            break;
                        }
                    }
                }
            }

     //L1:
            //transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.Exact, depth, bestMove, gameBoard.ExaminedMoves - examinedMoves);
            return bestValue;
        }

		bool TimeoutReached()
        {
			if (!stopThinking)
            {
                if (timeoutCounter % 1000 == 999)
				{
					//refresh timeout reached
                    searchInfo.elapsedTime = DateTime.Now - startTime;
                    searchInfo.TThits = transpositionTable.SuccessfulHits;
                    //searchInfo.examinedMoves = gameBoard.ExaminedMoves;
				}

                if (timeoutCounter % 10000 == 9999)
                {
                    //raise event with copy of search information
                    ThinkingProgress(new SearchInformation(searchInfo));
                }
                timeoutCounter++;

                if (searchInfo.elapsedTime > maxThinkingTime)
				{
					stopThinking = true;
				}
				else
				{
					stopThinking = false;
				}
            }

			return stopThinking;
        }

		public void StopThinking()
		{
			stopThinking = true;
		}

        public TimeSpan MaxThinkingTime
        {
            get { return maxThinkingTime; }
            set { maxThinkingTime = value; }
        }

        public bool IterativeDeepening
        {
            get { return iterativeDeepening; }
            set { iterativeDeepening = value; }
        }

        public int MaxSearchDepth
        {
            get 
            { 
                return maxSearchDepth; 
            }
            set 
            { 
                maxSearchDepth = value;
            }
        }
    
    }
}
