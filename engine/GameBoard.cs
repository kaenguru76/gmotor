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
        List<int> playedSquares;
        List<ABMove> playedMoves;

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
            playedMoves = new List<ABMove>();

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

        public void MakeABMove(ABMove move)
        {
            playedMoves.Add(move);

            //VctActive = (move.vctPlayer == Player.None) ? false:true;
            MakeMove(move.square);
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
            transpositionTable.MakeMove(square, playerOnMove);

            //toggle playerOnMove
            playerOnMove = (playerOnMove == Player.BlackPlayer) ? Player.WhitePlayer : Player.BlackPlayer;

        }

        public void UndoABMove()
        {
            UndoMove();
            playedMoves.RemoveAt(playedMoves.Count - 1);
        }

        public void UndoMove()
        {
            int square = playedSquares[playedSquares.Count - 1];

            if (symbol[square] == Player.None) return;

            //toggle playerOnMove
            playerOnMove = (playerOnMove == Player.BlackPlayer) ? Player.WhitePlayer : Player.BlackPlayer;

            if (vct.LastVctMove == square) vct.Locked = false;
            EvaluateConnectedSquares(square, Player.None);

            vct.UndoMove(square);

            //update zobrist key
            transpositionTable.UndoMove(square, playerOnMove);

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

                FourDirectionsData fourDirectionsBlack = evaluateFourDirectionsBlack.Modify(connectedSquare.square, connectedSquare.direction, oneDirectionData.evaluationBlack);

                FourDirectionsData fourDirectionsWhite = evaluateFourDirectionsWhite.Modify(connectedSquare.square, connectedSquare.direction, oneDirectionData.evaluationWhite);

                BothData bothData = evaluateBoth.Modify(connectedSquare.square, fourDirectionsBlack.evaluation, fourDirectionsWhite.evaluation);

                bool changedBlack = sortingBlack.Modify(connectedSquare.square, bothData.evaluationBlackOnMove);
                bool changedWhite = sortingWhite.Modify(connectedSquare.square, bothData.evaluationWhiteOnMove);


                if (changedBlack || changedWhite)
                {
                    if (placedSymbol == Player.None)
                    {
                        vct.RemoveVct(connectedSquare.square);
                    }
                    else
                    {
                        vct.AddVct(connectedSquare.square);
                    }
                }     
            }
        }

        public List<ABMove> GeneratePossibleMoves(Player vctPlayer)
        {
            List<ABMove> movesC = new List<ABMove>();

            List<int> possibleSquares = GeneratePossibleSquares(vctPlayer);
            
            foreach (int square in possibleSquares)
            {
            	ABMove move = new ABMove(square,playerOnMove,boardSize);
            	movesC.Add(move);
                BothPlayerEvaluation bothPlayerEvaluation = (playerOnMove == Player.BlackPlayer)? sortingBlack.GetEvaluation(square):
                    sortingWhite.GetEvaluation(square);
            	move.moveType = bothPlayerEvaluation;
            	move.vctPlayer = vct.VctPlayer;

            	//get information from transposition table
                transpositionTable.MakeMove(square, playerOnMove);
                
                TranspositionTableItem ttItem = transpositionTable.Lookup();
                if (ttItem != null)
                {
                	move.valueType = ttItem.type;
                	move.examinedMoves = ttItem.examinedMoves;
                	move.depth = ttItem.depth;
                	if (playerOnMove == Player.WhitePlayer)
                	{
                		//toggle evaluation for white
                		move.value = ttItem.value;
                	}
                	else
                	{
                		//correct evaluation is with "-" 
                		move.value = -ttItem.value;      

						//toggle bounds
						if (move.valueType == TTEvaluationType.UpperBound) move.valueType = TTEvaluationType.LowerBound;
						if (move.valueType == TTEvaluationType.LowerBound) move.valueType = TTEvaluationType.UpperBound;
                	}
                }
                
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
                }

                transpositionTable.UndoMove(square, playerOnMove);
            }
            return movesC;
        }

        public List<int> GeneratePossibleSquares(Player vctPlayer)
        {
            if (gameFinished == false)
            {
                if (playerOnMove == Player.BlackPlayer)
                {
                    if (vctPlayer == Player.BlackPlayer)
                    {
                        return moveGenerator.GeneratePossibleVctMoves(sortingBlack);
                    }
                    else
                    {
                        return moveGenerator.GeneratePossibleMoves(sortingBlack);
                    }
                }
                else
                {
                    if (vctPlayer == Player.WhitePlayer)
                    {
                        return moveGenerator.GeneratePossibleVctMoves(sortingWhite);
                    }
                    else
                    {
                        return moveGenerator.GeneratePossibleMoves(sortingWhite);
                    }
                }
            }
            else
            {
                //game already finished->return no moves
                return new List<int>();
            }
        }

        public int GetBoardSize()
        {
            return boardSize;
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

        public List<ABMove> GetPlayedMoves()
        {
            return playedMoves;
        }

        public ABMove GetLastPlayedMove()
        {
            if (playedMoves.Count > 0)
                return playedMoves[playedMoves.Count - 1];
            else
                return null;
        }

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

//        public void GetEvaluationDetail(out int blackScore, out int whiteScore)
//        {
//            blackScore = sortingBlack.Score;
//            whiteScore = sortingWhite.Score;
//        }

        //public int ExaminedMoves
        //{
        //    get
        //    {
        //        return examinedMoves;
        //    }
        //}

        //public void NewSearch()
        //{
        //    examinedMoves = 0;
        //}

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

        //public bool TerminalNode(ref int value)
        //{
        //    Player evaluation = Winner();

        //    if (evaluation == Player.BlackPlayer)
        //    {
        //        if (playerOnMove == Player.BlackPlayer)
        //        {
        //            value = int.MaxValue;
        //        }
        //        else
        //        {
        //            value = -int.MaxValue;
        //        }
        //        return true;
        //    }

        //    if (evaluation == Player.WhitePlayer)
        //    {
        //        if (playerOnMove == Player.WhitePlayer)
        //        {
        //            value = int.MaxValue;
        //        }
        //        else
        //        {
        //            value = -int.MaxValue;
        //        }
        //        return true;
        //    }
        //    return false;
        //}

        public int VctLength
        {
            get
            {
                return vct.VctLength;
            }
        }

    }
}
