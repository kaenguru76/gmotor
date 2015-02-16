using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GomokuEngine
{
    public struct SquareInfo
    {
        public Player symbol;
        public OneDirectionData[] directionData;
        public FourDirectionsEvaluation blackPlayerEvaluation;
        public FourDirectionsEvaluation whitePlayerEvaluation;
        public BothPlayerEvaluation bothPlayerEvaluation;
        public bool vct;
    }

    class GameBoard
    {
        Player[] symbol;
        Player playerOnMove;

        EvaluateOneDirection evaluateOneDirection;
        EvaluateFourDirections evaluateFourDirectionsBlack;
        EvaluateFourDirections evaluateFourDirectionsWhite;
        ConnectedSquares influencedSquares;

        EvaluateBoth evaluateBoth;

        MoveGenerator moveGenerator;

        int boardSize;
        public List<int> playedSquares;

        bool gameFinished;
        int numberOfSquares;

        Sorting sortingBlack;
        Sorting sortingWhite;

        VCT vct;

        TranspositionTable transpositionTable;

        BothPlayerEvaluation[] backupEvaluationBlack;
        BothPlayerEvaluation[] backupEvaluationWhite;

        public GameBoard(int boardSize, TranspositionTable transpositionTable)
        {
            this.transpositionTable = transpositionTable;

            //store board size
            this.boardSize = boardSize;
            //compute number of squares of board
            numberOfSquares = boardSize * boardSize;

            influencedSquares = new ConnectedSquares(boardSize);

            this.vct = new VCT(boardSize);

            sortingBlack = new Sorting(boardSize, vct);
            sortingWhite = new Sorting(boardSize, vct);

            evaluateBoth = new EvaluateBoth(boardSize);

            evaluateFourDirectionsBlack = new EvaluateFourDirections(boardSize);
            evaluateFourDirectionsWhite = new EvaluateFourDirections(boardSize);

            evaluateOneDirection = new EvaluateOneDirection(boardSize);


            moveGenerator = new MoveGenerator(boardSize);

            playerOnMove = Player.BlackPlayer;
            //played moves
            playedSquares = new List<int>();
            //playedMoves = new List<ABMove>();

            // intialize symbols
            symbol = new Player[numberOfSquares];
            //by default, board is empty
            for (int square = 0; square < numberOfSquares; square++)
            {
                symbol[square] = Player.None;
            }

            //initialize board
            for (int square = 0; square < numberOfSquares; square++)
            {
                EvaluateConnectedSquares(square, Player.None);
            }

            gameFinished = false;

            backupEvaluationBlack = new BothPlayerEvaluation[numberOfSquares];
            for (int index = 0; index < numberOfSquares; index++)
            {
                backupEvaluationBlack[index] = BothPlayerEvaluation.unknown;
            }

            backupEvaluationWhite = new BothPlayerEvaluation[numberOfSquares];
            for (int index = 0; index < numberOfSquares; index++)
            {
                backupEvaluationWhite[index] = BothPlayerEvaluation.unknown;
            }

        }


        public void MakeMove(int square)
        {
        	if (gameFinished) return;

            //add square to list
            playedSquares.Add(square);

            //place symbol on board
            symbol[square] = playerOnMove;

            if ((playerOnMove == Player.BlackPlayer && sortingBlack.IsWinningMove(square) ||
                 (playerOnMove == Player.WhitePlayer && sortingWhite.IsWinningMove(square))))
            {
                gameFinished = true;
            }

            backupEvaluationBlack[square] = sortingBlack.GetEvaluation(square);
            sortingBlack.Modify(square, BothPlayerEvaluation.occupied);

            backupEvaluationWhite[square] = sortingWhite.GetEvaluation(square);
            sortingWhite.Modify(square, BothPlayerEvaluation.occupied);

            vct.MakeMove(square, playerOnMove);
            EvaluateConnectedSquares(square, playerOnMove);
            vct.Locked = true;

            //update zobrist key
            transpositionTable.MakeMove(square, playerOnMove, vct.GainSquare);

            //toggle playerOnMove
            playerOnMove = (playerOnMove == Player.BlackPlayer) ? Player.WhitePlayer : Player.BlackPlayer;

        }
//
//        public void UndoABMove()
//        {
//            UndoMove();
//            playedMoves.RemoveAt(playedMoves.Count - 1);
//        }

        public void UndoMove()
        {
            int square = playedSquares[playedSquares.Count - 1];

            if (symbol[square] == Player.None) return;

            //toggle playerOnMove
            playerOnMove = (playerOnMove == Player.BlackPlayer) ? Player.WhitePlayer : Player.BlackPlayer;

            if (vct.GainSquare == square) vct.Locked = false;
            EvaluateConnectedSquares(square, Player.None);

            vct.UndoMove(square);

            //update zobrist key
            transpositionTable.UndoMove(square, playerOnMove, vct.GainSquare);

            sortingBlack.Modify(square, backupEvaluationBlack[square]);
            sortingWhite.Modify(square, backupEvaluationWhite[square]);

            //remove symbol from board
            symbol[square] = Player.None;

            gameFinished = false;

            //remove last move from list
            playedSquares.RemoveAt(playedSquares.Count - 1);
        }

        void EvaluateConnectedSquares(int centralSquare, Player placedSymbol)
        {
            List<ConnectedSquare> connectedSquares = influencedSquares.GetConnectedSquares(centralSquare);

            /* go through all influenced squares */
            foreach (ConnectedSquare connectedSquare  in connectedSquares)
            {
                //do not evaluate occupied squares
                if (symbol[connectedSquare.square] != Player.None) continue;

                OneDirectionData oneDirectionData = evaluateOneDirection.Modify(connectedSquare.square, connectedSquare.direction, connectedSquare.distance, placedSymbol, playerOnMove);

                bool blackChanged;
                FourDirectionsData fourDirectionsBlack = evaluateFourDirectionsBlack.Modify(connectedSquare.square, connectedSquare.direction, oneDirectionData.evaluationBlack, out blackChanged);

                bool whiteChanged;
                FourDirectionsData fourDirectionsWhite = evaluateFourDirectionsWhite.Modify(connectedSquare.square, connectedSquare.direction, oneDirectionData.evaluationWhite, out whiteChanged);

                BothData bothData = evaluateBoth.Modify(connectedSquare.square, fourDirectionsBlack.evaluation, fourDirectionsWhite.evaluation);

                sortingBlack.Modify(connectedSquare.square, bothData.evaluationBlackOnMove);
                sortingWhite.Modify(connectedSquare.square, bothData.evaluationWhiteOnMove);

                switch (placedSymbol)
                {
                	case Player.BlackPlayer:
                		if (blackChanged /*&& fourDirectionsBlack.evaluation <= FourDirectionsEvaluation.o2xo1*/) vct.AddVct(connectedSquare.square);
						break;
						
                	case Player.WhitePlayer:
                		if (whiteChanged /*&& fourDirectionsWhite.evaluation <= FourDirectionsEvaluation.o2xo1*/) vct.AddVct(connectedSquare.square);
						break;

                	case Player.None:
                		vct.RemoveVct(connectedSquare.square);
						break;
                }
            }
        }

        public List<ABMove> GeneratePossibleMoves(Player vctPlayer, bool vctDepth0)
        {
            List<ABMove> movesC = new List<ABMove>();

            List<int> possibleSquares = GeneratePossibleSquares(vctPlayer, vctDepth0);
            
            foreach (int square in possibleSquares)
            {
            	ABMove move = new ABMove(boardSize, square, playerOnMove);
            	movesC.Add(move);
                BothPlayerEvaluation bothPlayerEvaluation = (playerOnMove == Player.BlackPlayer)? sortingBlack.GetEvaluation(square):
                    sortingWhite.GetEvaluation(square);
            	move.moveType = bothPlayerEvaluation;
            	move.vctPlayer = vct.VctPlayer;

            	//get information from transposition table
            	MakeMove(square);
                //transpositionTable.MakeMove(square, playerOnMove, vct.GainSquare);
                
                TranspositionTableItem ttItem = transpositionTable.Lookup();
                if (ttItem != null)
                {
                	move.valueType = ttItem.type;
                	move.examinedMoves = ttItem.examinedMoves;
                	move.depthLeft = ttItem.depthLeft;
               		move.value = ttItem.value;
                	switch (vct.VctPlayer)
                	{
               		case Player.None:
	                	if (playerOnMove == Player.WhitePlayer)
	                	{
	                		//toggle evaluation for white 
	                		move.value = -move.value;      
							//toggle also bounds
							if (move.valueType == TTEvaluationType.UpperBound) move.valueType = TTEvaluationType.LowerBound;
							if (move.valueType == TTEvaluationType.LowerBound) move.valueType = TTEvaluationType.UpperBound;
	                	}
	                	break;
	                
	                case Player.WhitePlayer:
	                	//toggle evaluation for white 
	                	move.value = -move.value;    
						break;	                		
                	}
                }
                /*
                TranspositionTableVctItem ttItemVctBlack = transpositionTable.LookupVctBlack();
                if (ttItemVctBlack != null)
                {
                	move.vctBlack = ttItemVctBlack.value;
                	move.vctBlackDepth = ttItemVctBlack.depth;
                	move.examinedMovesVctBlack = ttItemVctBlack.examinedMoves;
                }
                
                TranspositionTableVctItem ttItemVctWhite = transpositionTable.LookupVctWhite();
                if (ttItemVctWhite != null)
                {
                	move.vctWhite = ttItemVctWhite.value;
                	move.vctWhiteDepth = ttItemVctWhite.depth;
                	move.examinedMovesVctWhite = ttItemVctWhite.examinedMoves;
                }*/

                //transpositionTable.UndoMove(square, playerOnMove, vct.GainSquare);
                UndoMove();
            }
            return movesC;
        }

        public List<int> GeneratePossibleSquares(Player vctPlayer, bool vctDepth0)
        {
            if (gameFinished == false)
            {
                if (playerOnMove == Player.BlackPlayer)
                {
                	if (vctPlayer == Player.BlackPlayer)
                        return moveGenerator.GeneratePossibleMoves(sortingBlack, true, vctDepth0);
                        
                	if (vctPlayer == Player.WhitePlayer)
                       	return moveGenerator.GeneratePossibleMoves(sortingBlack, false, vctDepth0);

                	return moveGenerator.GeneratePossibleMoves(sortingBlack, false, vctDepth0);
                }
                else
                {
                    if (vctPlayer == Player.WhitePlayer)
                        return moveGenerator.GeneratePossibleMoves(sortingWhite, true, vctDepth0);

                    if (vctPlayer == Player.BlackPlayer)
                        return moveGenerator.GeneratePossibleMoves(sortingWhite, false, vctDepth0);

                    return moveGenerator.GeneratePossibleMoves(sortingWhite, false, vctDepth0);
                }
            }
            else
            {
                //game already finished->return no moves
                return new List<int>();
            }
        }

        public int BoardSize
        {
        	get
        	{
            	return boardSize;
        	}
        }

        public Player GetSymbol(int square)
        {
            return symbol[square];
        }

        public Player PlayerOnMove
        {
        	get
        	{
            	return playerOnMove;
        	}
        }

//        public List<ABMove> GetPlayedMoves()
//        {
//            return playedMoves;
//        }



        public bool GameFinished
        {
        	get
        	{
        		return gameFinished;
        	}
        }

        public void GetSquareInfo(int square, out SquareInfo squareInfo)
        {
            squareInfo.symbol = this.symbol[square];
            squareInfo.directionData = evaluateOneDirection.GetDirectionData(square);
            squareInfo.blackPlayerEvaluation = evaluateFourDirectionsBlack.GetEvaluation(square);
            squareInfo.whitePlayerEvaluation = evaluateFourDirectionsWhite.GetEvaluation(square);
            squareInfo.bothPlayerEvaluation = (playerOnMove == Player.BlackPlayer) ? sortingBlack.GetEvaluation(square) : sortingWhite.GetEvaluation(square);
            squareInfo.vct = vct.CreatesVct(square);
        }

        public TuningInfo GetTuningInfo()
        {
            TuningInfo info = new TuningInfo(sortingBlack.GetScoreEvaluation());

            return info;
        }

        public void SetTuningInfo(TuningInfo info)
        {
            sortingBlack.SetScoreEvaluation(info.scoreEvaluation);
            sortingWhite.SetScoreEvaluation(info.scoreEvaluation);
        }        

        public int GetEvaluation()
        {
            //since negamax is used, both players are maximizing their scores
            if (gameFinished)
            {
            	//return minimal value because the player which is actually 
            	//on move has lost
            	return EvaluationConstants.min;
            }
            else
            {
            	if (playerOnMove == Player.BlackPlayer)
                {
                	return sortingBlack.Score; //black tries to maximize
                }
                else
                {
            		return sortingWhite.Score; //white tries to maximize
                }
            }
        }

        public bool VctActive
        {
            set
            {
                if (value)
                {
                    vct.VctPlayer = playerOnMove;
                }
                else
                {
                    vct.VctPlayer = Player.None;
                }
            }
            get
            {
                return vct.VctActive;
            }
        }

        public Player VctPlayer
        {
            get
            {
                return vct.VctPlayer;
            }
        }

        public int VctLength
        {
            get
            {
                return vct.VctLength;
            }
        }
        
        public bool VctDepth0
        {
            get
            {
                return vct.VctDepth0;
            }
        }

        public int GainSquare
        {
            get
            {
                return vct.GainSquare;
            }
        }    }
}
