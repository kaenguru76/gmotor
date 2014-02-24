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
        	List<int> principalVariation;
        	
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
                //int evaluation = ScoreConstants.loss;

                if (depth == 0) gameBoard.VctActive = true;

                int evaluation = AlphaBeta(depth, -int.MaxValue, int.MaxValue, out principalVariation);
 				if (TimeoutReached()) break;
 
                //generate moves
                if (depth == 0) gameBoard.VctActive = true;
                
                List<ABMove> possibleMoves = gameBoard.GeneratePossibleMoves();
                if (possibleMoves.Count == 0 && depth > 0 && gameBoard.GetPlayedMoves().Count > 0) break;

 
                //depth search finished->store results
                sInfo.depth = depth;
                sInfo.evaluation = evaluation;
                sInfo.possibleMoves = new List<ABMove>(possibleMoves);
                sInfo.vctActive = gameBoard.VctActive;
                
				//get principal variation
//                if (principalVariation.Count == 0 && possibleMoves.Count > 0)
//                {
//                	principalVariation.Add(possibleMoves[0].square);
//                }
                
                sInfo.principalVariation = new List<ABMove>();
                if (principalVariation != null)
                {
	            	foreach(int square in principalVariation)
	            	{
	            		ABMove move = new ABMove(square,gameBoard.GetPlayerOnMove(),gameBoard.GetBoardSize());
	        			sInfo.principalVariation.Add(move);
	            	}
                }
                
                if (sInfo.depth == 0)
                {
                    //end VCT
                    gameBoard.VctActive = false;
                }

                if (depth > 0 && gameBoard.GetPlayedMoves().Count == 0) break; //completely first move
                if ((depth > 0 && evaluation == EvaluationConstants.min) || evaluation == EvaluationConstants.max) break;
            } 
//L1:

            //end VCT
            gameBoard.VctActive = false;
            
            if (gameBoard.GetPlayerOnMove() == Player.WhitePlayer)
            {
            	//toggle evaluation for white on move - due to negamax
				sInfo.evaluation = -sInfo.evaluation;
				
				//and also for all possible moves
				//foreach(ABMove move in sInfo.possibleMoves)
				//{
					//toggle value
				//	move.value = -move.value;
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
				//}
            }

            //get winner if any
//        	switch (sInfo.evaluation)
//        	{
//            	case ScoreConstants.max:
//            		sInfo.winner = Player.BlackPlayer;
//            		break;
//            	case ScoreConstants.min:
//            		sInfo.winner = Player.WhitePlayer;
//            		break;
//        	}
                        
            //raise event with search information
            ThinkingFinished(sInfo);
        }

        int AlphaBeta(int depth, int alpha, int beta, out List<int> principalVariation)
        {        	
        	principalVariation = null;//new List<int>();
            int bestValue = -int.MaxValue;
            int examinedMoves = sInfo.examinedMoves;

            //quiescence search
            if (depth == 0)
            {
            	if (beta == int.MaxValue)
            	{
	                // start VCT
    	            gameBoard.VctActive = true;
    	            bestValue = AlphaBetaVCT(0, EvaluationConstants.max-1, beta, out principalVariation);
    	            //stop VCT
    	            gameBoard.VctActive = false;
                
    	            if (bestValue == EvaluationConstants.max) return bestValue;// VCT was succesfull
            	}
            	//principalVariation = new List<int>();
            	bestValue = gameBoard.GetEvaluation();
				goto L1;
            }

            //game finished
        	if (gameBoard.GameFinished)
            {
        		//principalVariation = new List<int>();
				bestValue = gameBoard.GetEvaluation();
        		goto L1; 
            }

            //look into TT if this position was not evaluated already before          
            TranspositionTableItem ttItem = transpositionTable.Lookup(gameBoard.VctPlayer);
            if (ttItem != null)
            {
                if (ttItem.depth == depth || ttItem.value == EvaluationConstants.max)
                {
                    switch (ttItem.type)
                    {
                        case TTEvaluationType.Exact:
                    		return ttItem.value; 

                        case TTEvaluationType.LowerBound:
                            if (ttItem.value >= beta) return ttItem.value;; 
                            if (ttItem.value > alpha) alpha = ttItem.value;
                            break;

                        case TTEvaluationType.UpperBound:
                            if (ttItem.value <= alpha) return ttItem.value;; 
                            if (ttItem.value < beta) beta = ttItem.value;
                            break;
                    }
                }
            }
            
			List<int> principalVariationTmp;
	
            //do normal search
            List<int> moves = gameBoard.GeneratePossibleSquares();

            foreach (int move in moves)
            {
                gameBoard.MakeMove(move);

                int value = -AlphaBeta(depth - 1, -beta, -alpha, out principalVariationTmp);

                gameBoard.UndoMove();
                
                sInfo.examinedMoves++;

                if (TimeoutReached()) return bestValue;
 				
                if (value > bestValue)
                {
                    bestValue = value;
                    if (value > alpha)
                    {
	                    if (value >= beta)
                        {
                            sInfo.nbCutoffs++;
                            break;
                        }
                        alpha = value;
                       	principalVariation = (principalVariationTmp == null) ? new List<int>():new List<int>(principalVariationTmp);
	                    principalVariation.Insert(0,move);
	                    if (value == EvaluationConstants.max) break;
                    }
                }
            }

           L1:
            if (bestValue <= alpha && principalVariation == null)  // an upper bound value
            	transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.UpperBound, depth, sInfo.examinedMoves - examinedMoves);
            else if (bestValue >= beta)  // lower bound value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.LowerBound, depth, sInfo.examinedMoves - examinedMoves);
            else // a true minimax value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.Exact, depth, sInfo.examinedMoves - examinedMoves);
            
            return bestValue;
        }

        int AlphaBetaVCT(int depth, int alpha, int beta, out List<int> principalVariation)
        {
        	principalVariation = null; //new List<int>();
            int bestValue = -int.MaxValue;
            int examinedMoves = sInfo.examinedMoves;

			//max depth reached or game finished
        	if (depth == -17 || gameBoard.GameFinished)
            {
                bestValue = gameBoard.GetEvaluation();
                //principalVariation = new List<int>();
                goto L1;
            }

        	//look into TT if this position was not evaluated already before
            TranspositionTableItem ttItem = transpositionTable.Lookup(gameBoard.VctPlayer);
            if (ttItem != null)
            {
                if (ttItem.depth == depth || ttItem.value == EvaluationConstants.max)
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

			List<int> principalVariationTmp;
            
            List<int> moves = gameBoard.GeneratePossibleSquares();
            foreach (int move in moves)
            {
                gameBoard.MakeMove(move);

                int value = -AlphaBetaVCT(depth - 1, -beta, -alpha, out principalVariationTmp);

                gameBoard.UndoMove();

                sInfo.examinedMoves++;

                if (TimeoutReached()) return bestValue;

                if (value > bestValue)
                {
                    bestValue = value;

                    if (value > alpha)
                    {
                        if (value >= beta)
                        {
                            sInfo.nbVCTCutoffs++;
                            break;
                        }
                        alpha = value;
                       	principalVariation = (principalVariationTmp == null) ? new List<int>():new List<int>(principalVariationTmp);
	                    principalVariation.Insert(0,move);
	                    if (value == EvaluationConstants.max) break;
                    }
                }
            }
            
           L1:
            if (bestValue <= alpha && principalVariation == null)  // an upper bound value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.UpperBound, depth, sInfo.examinedMoves - examinedMoves);
            else if (bestValue >= beta)  // lower bound value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.LowerBound, depth, sInfo.examinedMoves - examinedMoves);
            else // a true minimax value
                transpositionTable.Store(bestValue, gameBoard.VctPlayer, TTEvaluationType.Exact, depth, sInfo.examinedMoves - examinedMoves);

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
