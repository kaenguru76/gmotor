#define CHECK 

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
                		TranspositionTableVCTItem ttItem = transpositionTable.LookupVctBlack();
	            		if (ttItem != null)
	            		{
	            			if (ttItem.value == TT_VCT_Status.Proven)
	            			{
            					principalVariation = new List<int>();
	                			principalVariation.Add(ttItem.bestMove);
								break;	                			
	            			}
	            		}
                	}
                	else
                	{
                		TranspositionTableVCTItem ttItem = transpositionTable.LookupVctWhite();
	            		if (ttItem != null)
	            		{
	            			if (ttItem.value == TT_VCT_Status.Proven)
	            			{
            					principalVariation = new List<int>();
	                			principalVariation.Add(ttItem.bestMove);	            				
	                			break;
	            			}
	            		}
                	}
                		
           	    	TranspositionTableItem ttItem2 = transpositionTable.Lookup();
            		if (ttItem2 != null)
            		{
           				principalVariation = new List<int>();
                		principalVariation.Add(ttItem2.bestMove);
                		break;
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
            sInfo.TThits = transpositionTable.SuccessfulHits;
            //sInfo.TTVCThits = transpositionTable.SuccessfulVCTHits;
			
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
		        TT_VCT_Status vctStatus = VCTSearch(0, gameBoard.PlayerOnMove, out principalVariation);
#if CHECK                
                /* the move should be storeds in TT */
                if (TimeoutReached() == false)
                {
	              	TranspositionTableItem ttItem1 = transpositionTable.Lookup();
	           		System.Diagnostics.Debug.Assert(ttItem1 != null);
	            	if (gameBoard.PlayerOnMove == Player.BlackPlayer)
	            	{
	            		System.Diagnostics.Debug.Assert(ttItem1.vctBlackdepth == 0);
		                if (vctStatus == TT_VCT_Status.Proven)
		                {
		            		System.Diagnostics.Debug.Assert(ttItem1.vctBlack == TT_VCT_Status.Proven); 
		            	}
		            	else
		            	{
		            		System.Diagnostics.Debug.Assert(ttItem1.vctBlack == TT_VCT_Status.Disproven); 
		            	}            		
	                }
	            	else
	            	{
	            		System.Diagnostics.Debug.Assert(ttItem1.vctWhitedepth == 0);
		                if (vctStatus == TT_VCT_Status.Proven)
		                {
		            		System.Diagnostics.Debug.Assert(ttItem1.vctWhite == TT_VCT_Status.Proven); 
		            	}
		            	else
		            	{
		            		System.Diagnostics.Debug.Assert(ttItem1.vctWhite == TT_VCT_Status.Disproven); 
		            	}            		
	                }  		
                }
#endif
				//stop VCT
		        gameBoard.VctActive = false;
    		
		        if (vctStatus == TT_VCT_Status.Proven)
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

        TT_VCT_Status VCTSearch(int depth, Player vctToProve, out List<int> principalVariation)
        {
        	principalVariation = null;
            TT_VCT_Status status = TT_VCT_Status.Disproven;
            int examinedMoves = sInfo.examinedMoves;
            int bestMove = -1;

            if (sInfo.deepestVctSearch > depth) sInfo.deepestVctSearch = depth;
            
			//max depth reached or game finished
			if (gameBoard.GameFinished)
        	{
              	if (vctToProve == gameBoard.PlayerOnMove)
              	{
               		status = TT_VCT_Status.Disproven;
                }
              	else
              	{
               		status = TT_VCT_Status.Proven;
                }
                goto L1;
                //return status;
            }
        	
			if (depth == -17)
        	{
              	//if (vctToProve == gameBoard.PlayerOnMove)
              	//{
               	//	status = TT_VCT_Status.Disproven;
                //}
              	//else
              	//{
               	//	status = TT_VCT_Status.Proven;
                //}
                goto L1;
                //return status;
            }
        	
        	//look into TT if this position was not evaluated already before
        	{
            TranspositionTableItem ttItem = transpositionTable.Lookup();
            if (ttItem != null)
            {
            	if (vctToProve == Player.BlackPlayer)
            	{
            		if (ttItem.vctBlack != TT_VCT_Status.Unknown && ttItem.vctBlackdepth == depth) return ttItem.vctBlack; 
            	}
            	else
            	{
            		if (ttItem.vctWhite != TT_VCT_Status.Unknown && ttItem.vctWhitedepth == depth) return ttItem.vctWhite; 
            	}
            }
        	}
        	
            //default initialization
            //if (gameBoard.PlayerOnMove == vctToProve)
            //{
            //	status = TT_VCT_Status.Disproven;
            //}
            //else
            //{
            //	status = TT_VCT_Status.Proven;
            //}            	

			List<int> principalVariationTmp;
            
            List<int> moves = gameBoard.GeneratePossibleSquares(vctToProve);
            foreach (int move in moves)
            {
                gameBoard.MakeMove(move);

                TT_VCT_Status tmpStatus = VCTSearch(depth - 1, vctToProve, out principalVariationTmp);
#if CHECK                
                /* the move should be storeds in TT */
                if (TimeoutReached() == false)
                {
	              	TranspositionTableItem ttItem = transpositionTable.Lookup();
	           		System.Diagnostics.Debug.Assert(ttItem != null);
	            	if (vctToProve == Player.BlackPlayer)
	            	{
	            		System.Diagnostics.Debug.Assert(ttItem.vctBlackdepth == depth-1);
		                if (tmpStatus == TT_VCT_Status.Proven)
		                {
		            		System.Diagnostics.Debug.Assert(ttItem.vctBlack == TT_VCT_Status.Proven); 
		            	}
		            	else
		            	{
		            		System.Diagnostics.Debug.Assert(ttItem.vctBlack == TT_VCT_Status.Disproven); 
		            	}            		
	                }
	            	else
	            	{
	            		System.Diagnostics.Debug.Assert(ttItem.vctWhitedepth == depth-1);
		                if (tmpStatus == TT_VCT_Status.Proven)
		                {
		            		System.Diagnostics.Debug.Assert(ttItem.vctWhite == TT_VCT_Status.Proven); 
		            	}
		            	else
		            	{
		            		System.Diagnostics.Debug.Assert(ttItem.vctWhite == TT_VCT_Status.Disproven); 
		            	}            		
	                }  		
                }
#endif
                gameBoard.UndoMove();

                sInfo.examinedMoves++;

                if (TimeoutReached()) return TT_VCT_Status.Disproven;

                if (gameBoard.PlayerOnMove == vctToProve)
                {
	                if (tmpStatus == TT_VCT_Status.Proven)
    	            {
	                	status = TT_VCT_Status.Proven;
	                	bestMove = move;
    	               	principalVariation = (principalVariationTmp == null) ? new List<int>():new List<int>(principalVariationTmp);
		                principalVariation.Insert(0,move);
    	                break;
    	            }
                }
	            else
                {
	                if (tmpStatus == TT_VCT_Status.Disproven)
    	            {
	                	status = TT_VCT_Status.Disproven;
	                	bestMove = move;
    	               	principalVariation = (principalVariationTmp == null) ? new List<int>():new List<int>(principalVariationTmp);
		                principalVariation.Insert(0,move);
    	                break;
    	            }
	                else
	                {
	                	status = TT_VCT_Status.Proven;
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
            transpositionTable.StoreVCT(status, vctToProve, depth, sInfo.examinedMoves - examinedMoves, bestMove);

#if CHECK            
            System.Diagnostics.Debug.Assert(gameBoard.VctPlayer != Player.None,"Wrong value of VctPlayer");
#endif 
            
            return status;
        }

		bool TimeoutReached()
        {
			if (!stopThinking)
            {
                if (timeoutCounter % 10000 == 9999)
                {
                    sInfo.elapsedTime = DateTime.Now - startTime;
                    sInfo.TThits = transpositionTable.SuccessfulHits;
                    //sInfo.TTVCThits = transpositionTable.SuccessfulVCTHits;
                    	
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
