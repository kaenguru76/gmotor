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
        //SearchInformation searchInfo;
        //List<ABMove> playedMoves;
        GameBoard gameBoard;
        TranspositionTable transpositionTable;
        //int examinedMoves;
        //int nbCutoffs;
        //int nbVCTCutoffs;
        SearchInformation sInfo;

		public Search(GameBoard gameBoard, TranspositionTable transpositionTable)
		{
            this.gameBoard = gameBoard;
            this.transpositionTable = transpositionTable;

            //searchInfo = new SearchInformation();
            maxThinkingTime = new TimeSpan(0, 0, 5);
            iterativeDeepening = true;
            maxSearchDepth = 30;
            //playedMoves = new List<ABMove>();
        }

        public void RootSearch()
        {
            startTime = DateTime.Now;

            //initialize counter
            timeoutCounter = 0;
            
            //initialize time measurement
			stopThinking = false;
			
			sInfo = new SearchInformation();

			//iterative search or fix search?
			int startingDepth = (iterativeDeepening) ? 0 : maxSearchDepth;

            //start iterative deepening search
            for (int depth = startingDepth; depth <= maxSearchDepth; depth++)
            {
                int evaluation = -int.MaxValue;
                ABMove bestMove = null;

                if (depth == 0)
                {
                    // start VCT
                    gameBoard.VctActive = true;
                }

                //generate moves
                List<ABMove> possibleMoves = gameBoard.GeneratePossibleMoves();
                if (possibleMoves.Count == 0 && depth > 0 && gameBoard.GetPlayedMoves().Count > 0) break;

                foreach (ABMove move in possibleMoves)
                {
                    int beta;
                    int alpha;

                    #region AB search
                    alpha = -int.MaxValue;
                    beta = int.MaxValue;

                    gameBoard.MakeABMove(move);
    
                    move.value = -AlphaBeta(depth-1, -beta, -alpha);

                    //get some data from TT
                    TranspositionTableItem ttItem = transpositionTable.Lookup(gameBoard.VctPlayer);
                    if (ttItem != null)
                    {
	                    move.valueType = ttItem.type;
    	                move.examinedMoves = ttItem.examinedMoves;
						move.depth = ttItem.depth;
                    }
                    
                    gameBoard.UndoABMove();

                    sInfo.examinedMoves++;

                    if (TimeoutReached())
                    {
                        if (bestMove == null) bestMove = move;
                        goto L1;
                    }
                    #endregion

                    if (move.value > evaluation || bestMove == null)
                    {
                        evaluation = move.value;
                        bestMove = move;

                        if (evaluation == int.MaxValue) break;
                    }
                }

                //depth search finished->store results
                sInfo.depth = depth;
                sInfo.evaluation = evaluation;
                sInfo.bestMove = bestMove;
                sInfo.possibleMoves = new List<ABMove>(possibleMoves);
                sInfo.vctActive = gameBoard.VctActive;

                if (sInfo.depth == 0)
                {
                    //end VCT
                    gameBoard.VctActive = false;
                }

                if (gameBoard.GetPlayedMoves().Count == 0 && bestMove != null) break;
                if ((sInfo.depth > 0 && evaluation == -int.MaxValue) || evaluation == int.MaxValue) break;
            } 
L1:

            //end VCT
            gameBoard.VctActive = false;
            
            if (gameBoard.GetPlayerOnMove() == Player.WhitePlayer)
            {
            	//toggle evaluation for white on move - due to negamax
				sInfo.evaluation = -sInfo.evaluation;
				
				//and also for all possible moves
				foreach(ABMove move in sInfo.possibleMoves)
				{
					//toggle value
					move.value = -move.value;
					//and toggle bounds
					/*switch (move.valueType)
					{
						case TTEvaluationType.LowerBound:
							move.valueType = TTEvaluationType.UpperBound;
							break;
						case TTEvaluationType.UpperBound:
							move.valueType = TTEvaluationType.LowerBound;
							break;
					}*/
				}
            }

            //get winner if any
        	switch (sInfo.evaluation)
        	{
            	case int.MaxValue:
            		sInfo.winner = Player.BlackPlayer;
            		break;
            	case -int.MaxValue:
            		sInfo.winner = Player.WhitePlayer;
            		break;
        	}
                        
            //raise event with search information
            ThinkingFinished(sInfo);
        }

        int AlphaBeta(int depth, int alpha, int beta)
        {
            //quiescence search
            if (depth <= 0)
            {
                // start VCT
                gameBoard.VctActive = true;
                int score = AlphaBetaVCT(0, alpha, beta);
                //stop VCT
                gameBoard.VctActive = false;
                
                if (score == -int.MaxValue) // VCT was not succesfull
                	return gameBoard.GetEvaluation();
				else
					return score;
            }

            //game finished
        	if (gameBoard.GetWinner() != Player.None)
            {
                return gameBoard.GetEvaluation();
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
            
            int bestMove = -1;
            int bestValue = -int.MaxValue;
            int examinedMoves = sInfo.examinedMoves;

            //do normal search
            List<int> moves = gameBoard.GeneratePossibleSquares();

            foreach (int move in moves)
            {
                gameBoard.MakeMove(move);

                int value = -AlphaBeta(depth - 1, -beta, -alpha);

                gameBoard.UndoMove();
                
                sInfo.examinedMoves++;

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
                            sInfo.nbCutoffs++;
                            break;
                        }
                    }
                }
            }

            if (bestValue < alpha)  // an upper bound value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.UpperBound, depth, bestMove, sInfo.examinedMoves - examinedMoves);
            else if (bestValue >= beta)  // lower bound value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.LowerBound, depth, bestMove, sInfo.examinedMoves - examinedMoves);
            else // a true minimax value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.Exact, depth, bestMove, sInfo.examinedMoves - examinedMoves);

            return bestValue;
        }

        int AlphaBetaVCT(int depth, int alpha, int beta)
        {
        	//max depth reached or game finished
        	if (depth == -17 || gameBoard.GetWinner() != Player.None)
            {
                return gameBoard.GetEvaluation();
            }

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

            //int examinedMoves = gameBoard.ExaminedMoves;

            int bestValue = -int.MaxValue;
            int bestMove = -1;
            int examinedMoves = sInfo.examinedMoves;
            
            List<int> moves = gameBoard.GeneratePossibleSquares();
            foreach (int move in moves)
            {
                gameBoard.MakeMove(move);

                int value = -AlphaBetaVCT(depth - 1, -beta, -alpha);

                gameBoard.UndoMove();

                sInfo.examinedMoves++;

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
                            sInfo.nbVCTCutoffs++;
                            break;
                        }
                    }
                }
            }

            if (bestValue < alpha)  // an upper bound value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.UpperBound, depth, bestMove, sInfo.examinedMoves - examinedMoves);
            else if (bestValue >= beta)  // lower bound value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.LowerBound, depth, bestMove, sInfo.examinedMoves - examinedMoves);
            else // a true minimax value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.Exact, depth, bestMove, sInfo.examinedMoves - examinedMoves);
            
            return bestValue;
        }

		bool TimeoutReached()
        {
	        //TimeSpan elapsedTime;
			//float TThits;

			if (!stopThinking)
            {
                if (timeoutCounter % 10000 == 9999)
                {
                    sInfo.elapsedTime = DateTime.Now - startTime;;
                    sInfo.TThits = transpositionTable.SuccessfulHits;
                    	
                    ThinkingProgress(sInfo);
                }
                timeoutCounter++;

                if (sInfo.elapsedTime > maxThinkingTime)
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
