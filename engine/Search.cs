//#define CHECK 

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace GomokuEngine
{
    public enum VctStatus 
    { 
        Disproven, 
        Proven,
    }

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
        GameBoard gameBoard;
        TranspositionTable transpositionTable;
        SearchInformation sInfo;

		public Search(GameBoard gameBoard, TranspositionTable transpositionTable)
		{
            this.gameBoard = gameBoard;
            this.transpositionTable = transpositionTable;

            maxThinkingTime = new TimeSpan(0, 0, 5);
            iterativeDeepening = true;
            maxSearchDepth = 30;
        }

        public void RootSearch()
        {
        	List<int> principalVariation;
        	
            startTime = DateTime.Now;

            transpositionTable.ResetStatistics();
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
                int evaluation = AlphaBeta(depth, -int.MaxValue, int.MaxValue, out principalVariation);
 				if (TimeoutReached()) break;
 
                //generate moves
                gameBoard.VctActive = (depth > 0) ? false:true;
               
                List<ABMove> possibleMoves = gameBoard.GeneratePossibleMoves(gameBoard.VctPlayer);
                if (possibleMoves.Count == 0 && depth > 0 && gameBoard.GetPlayedMoves().Count > 0) break;

 
                //depth search finished->store results
                sInfo.depth = depth;
                sInfo.evaluation = evaluation;
                sInfo.possibleMoves = new List<ABMove>(possibleMoves);
                sInfo.vctActive = gameBoard.VctActive;
                
				//if principal variation is empty, then the best move should be stored in TT
                if (principalVariation == null)
                {
                	if (gameBoard.PlayerOnMove == Player.BlackPlayer)
                	{
                		TranspositionTableVctItem ttItem = transpositionTable.LookupVctBlack();
	            		if (ttItem != null)
	            		{
	            			if (ttItem.value == VctStatus.Proven)
	            			{
            					principalVariation = new List<int>();
	                			principalVariation.Add(ttItem.bestMove);
	            			}
	            		}
                	}
                	else
                	{
                		TranspositionTableVctItem ttItem = transpositionTable.LookupVctWhite();
	            		if (ttItem != null)
	            		{
	            			if (ttItem.value == VctStatus.Proven)
	            			{
            					principalVariation = new List<int>();
	                			principalVariation.Add(ttItem.bestMove);	            				
	            			}
	            		}
                	}
                		
	                if (principalVariation == null)
	                {
           	 		   	TranspositionTableItem ttItem2 = transpositionTable.Lookup();
            			if (ttItem2 != null)
            			{
            				//illegal move -> research 
            				if (ttItem2.bestMove == -1) continue;
            			
           					principalVariation = new List<int>();
                			principalVariation.Add(ttItem2.bestMove);
           				}
	                }
            	}
                
                //if (principalVariation != null)
                //{
	                sInfo.principalVariation = new List<ABMove>();
	            	foreach(int square in principalVariation)
	            	{
	            		ABMove move = new ABMove(square,gameBoard.PlayerOnMove,gameBoard.GetBoardSize());
	        			sInfo.principalVariation.Add(move);
	            	}
                //}
                
                if (sInfo.depth == 0)
                {
                    //end VCT
                    gameBoard.VctActive = false;
                }

                if (gameBoard.GetPlayedMoves().Count == 0) break; //completely first move
                if ((depth > 0 && evaluation == EvaluationConstants.min) || evaluation == EvaluationConstants.max) break;
            } 
//L1:

            //end VCT
            gameBoard.VctActive = false;
            
            if (gameBoard.PlayerOnMove == Player.WhitePlayer)
            {
            	//toggle evaluation for white on move - due to negamax
				sInfo.evaluation = -sInfo.evaluation;
            }
                        
            sInfo.elapsedTime = DateTime.Now - startTime;
            sInfo.TtHits = transpositionTable.SuccessfulHits;
            sInfo.TtVctHits = transpositionTable.SuccessfulVctHits;
			
            //raise event with search information
            ThinkingFinished(sInfo);
        }

        int AlphaBeta(int depth, int alpha, int beta, out List<int> principalVariation)
        {        	
        	principalVariation = null;
            int bestValue = -int.MaxValue;
            int bestMove = -1;
            int examinedMoves = sInfo.examinedMoves;

            //if (beta >= EvaluationConstants.max) // then VCT makes sense
            //{
	            // start VCT
		        gameBoard.VctActive = true;
		        VctStatus vctStatus = VCTSearch(0, gameBoard.PlayerOnMove, out principalVariation);

				//stop VCT
		        gameBoard.VctActive = false;
    		
		        if (vctStatus == VctStatus.Proven)
		        {
		        	//depth = 0;
		        	bestValue = EvaluationConstants.max;
		        	goto L1;
		        	//return tmpValue;
		        }
            //}
		            
            if (depth == 0)
            {
    	        bestValue = gameBoard.GetEvaluation();
				goto L1;
				//return bestValue;
            }

            //game finished
        	if (gameBoard.GameFinished)
            {
				bestValue = gameBoard.GetEvaluation();
        		goto L1; 
//				return bestValue;
            }

            //look into TT if this position was not evaluated already before          
            TranspositionTableItem ttItem = transpositionTable.Lookup();
            if (ttItem != null)
            {
                if (ttItem.depth == depth/* || ttItem.value == EvaluationConstants.max*/)
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
            List<int> moves = gameBoard.GeneratePossibleSquares(Player.None);

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
                    bestMove = move;
                    
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
	                    if (value >= EvaluationConstants.max) break;
                    }
                }
            }

           L1:
            if (bestValue <= alpha && principalVariation == null)  // an upper bound value
            	transpositionTable.Store(bestValue, TTEvaluationType.UpperBound, depth, sInfo.examinedMoves - examinedMoves, bestMove);
            else if (bestValue >= beta)  // lower bound value
                transpositionTable.Store(bestValue, TTEvaluationType.LowerBound, depth, sInfo.examinedMoves - examinedMoves, bestMove);
            else // a true minimax value
                transpositionTable.Store(bestValue, TTEvaluationType.Exact, depth, sInfo.examinedMoves - examinedMoves, bestMove);

#if CHECK            
            System.Diagnostics.Debug.Assert(gameBoard.VctPlayer == Player.None,"Wrong value of VctPlayer");
#endif            
            return bestValue;
        }

        VctStatus VCTSearch(int depth, Player vctToProve, out List<int> principalVariation)
        {
        	principalVariation = null;
            VctStatus status = VctStatus.Disproven;
            int examinedVtcMoves = sInfo.examinedVctMoves;
            int bestMove = -1;

            if (sInfo.deepestVctSearch > depth) sInfo.deepestVctSearch = depth;
            
			//max depth reached or game finished
			if (gameBoard.GameFinished)
        	{
              	if (vctToProve == gameBoard.PlayerOnMove)
              	{
               		status = VctStatus.Disproven;
                }
              	else
              	{
               		status = VctStatus.Proven;
                }
                goto L1;
                //return status;
            }
        	
			if (depth == -17)
        	{
                goto L1;
                //return status;
            }
        	
        	//look into TT if this position was not evaluated already before
           	if (vctToProve == Player.BlackPlayer)
           	{
	            TranspositionTableVctItem ttItem = transpositionTable.LookupVctBlack();
	            if (ttItem != null && ttItem.depth == depth) return ttItem.value;
           	}
           	else
           	{
	            TranspositionTableVctItem ttItem = transpositionTable.LookupVctWhite();
	            if (ttItem != null && ttItem.depth == depth) return ttItem.value;
           	}

			List<int> principalVariationTmp;
            
            List<int> moves = gameBoard.GeneratePossibleSquares(vctToProve);
            foreach (int move in moves)
            {
                gameBoard.MakeMove(move);

                VctStatus tmpStatus = VCTSearch(depth - 1, vctToProve, out principalVariationTmp);
                gameBoard.UndoMove();

                sInfo.examinedMoves++;
                sInfo.examinedVctMoves++;

                if (TimeoutReached()) return VctStatus.Disproven;

                if (gameBoard.PlayerOnMove == vctToProve)
                {
	                if (tmpStatus == VctStatus.Proven)
    	            {
	                	status = VctStatus.Proven;
	                	bestMove = move;
    	               	principalVariation = (principalVariationTmp == null) ? new List<int>():new List<int>(principalVariationTmp);
		                principalVariation.Insert(0,move);
    	                break;
    	            }
                }
	            else
                {
	                if (tmpStatus == VctStatus.Disproven)
    	            {
	                	status = VctStatus.Disproven;
	                	bestMove = move;
    	               	principalVariation = (principalVariationTmp == null) ? new List<int>():new List<int>(principalVariationTmp);
		                principalVariation.Insert(0,move);
    	                break;
    	            }
	                else
	                {
	                	status = VctStatus.Proven;
	                	if (bestMove == -1)
	                	{
	                		bestMove = move;
	    	               	principalVariation = (principalVariationTmp == null) ? new List<int>():new List<int>(principalVariationTmp);
			                principalVariation.Insert(0,move);
	                	}
	                }
                }	            	
            }
            
           L1:
            if (vctToProve == Player.BlackPlayer)
            	transpositionTable.StoreVctBlack(status, depth, sInfo.examinedVctMoves - examinedVtcMoves, bestMove);
            else
            	transpositionTable.StoreVctWhite(status, depth, sInfo.examinedVctMoves - examinedVtcMoves, bestMove);
			            	

            System.Diagnostics.Debug.Assert(gameBoard.VctPlayer != Player.None,"Wrong value of VctPlayer");
            
            return status;
        }

		bool TimeoutReached()
        {
			if (!stopThinking)
            {
                if (timeoutCounter % 10000 == 9999)
                {
                    sInfo.elapsedTime = DateTime.Now - startTime;
                    sInfo.TtHits = transpositionTable.SuccessfulHits;
                    sInfo.TtVctHits = transpositionTable.SuccessfulVctHits;
                    	
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
