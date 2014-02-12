using System;
using System.Collections.Generic;
using System.Threading;

namespace GomokuEngine
{
    public class Engine
	{
		public delegate void NewGameEvent();
        public event NewGameEvent NewGameE;

        public delegate void MovesChangedEvent(GameInformation gameInformation);
        public event MovesChangedEvent MovesChangedE;

        public delegate void ThinkingFinishedEvent(SearchInformation info);
        public event ThinkingFinishedEvent ThinkingFinished;

        public delegate void ThinkingProgressEvent(SearchInformation info);
        public event ThinkingProgressEvent ThinkingProgress;

        GameBoard gameBoard;
        TranspositionTable transpositionTable;
        Search search;
		
        public GameInformation gameInformation;
        bool thinking;

        public Engine()
		{
            gameInformation = new GameInformation();
            
            gameInformation.fileName = "New Game";
            gameInformation.blackPlayerName = "Black Player";
            gameInformation.whitePlayerName = "White Player";
            thinking = false;
        }

        public void NewGame(int boardSize)
        {
            while (thinking) ;
            
            //initialize all
            transpositionTable = new TranspositionTable(boardSize);
            gameBoard = new GameBoard(boardSize,transpositionTable);
            search = new Search(gameBoard, transpositionTable);
            search.ThinkingFinished += new Search.ThinkingFinishedEvent(search_ThinkingFinished);
            search.ThinkingProgress += new Search.ThinkingProgressEvent(search_ThinkingProgress);

            //copy move list
            gameInformation.gameMoveList = new List<ABMove>();
            gameInformation.blackPlayerName = "Black Player";
            gameInformation.whitePlayerName = "White Player";

        	NewGameE();

            MovesChanged();
        }

        public void LoadGame(int boardSize, List<ABMove> moveList)
        {
            while (thinking) ;

            //initialize all
            transpositionTable = new TranspositionTable(boardSize);
            gameBoard = new GameBoard(boardSize, transpositionTable);
            search = new Search(gameBoard, transpositionTable);
            search.ThinkingFinished += new Search.ThinkingFinishedEvent(search_ThinkingFinished);
            search.ThinkingProgress += new Search.ThinkingProgressEvent(search_ThinkingProgress);

            //copy move list
            gameInformation.gameMoveList = new List<ABMove>(moveList);

            //play all moves
            for (int index = 0; index < gameInformation.gameMoveList.Count; index++)
            {
                ABMove move = gameInformation.gameMoveList[index];
                gameBoard.MakeABMove(move);
            }

            NewGameE();

            MovesChanged();
        }

        public void MakeMove(ABMove move)
        {
            while (thinking);

			//if square is occupied, exit
			if (gameBoard.GetSymbol(move.square) !=  Player.None) return;

			//make move
			gameBoard.MakeABMove(move);

            MovesChanged();
        }

        public void Redraw()
        {
            while (thinking) ;

            MovesChanged();
        }

		public void Redo()
		{
            while (thinking) ;
            
            bool movesAreEqual = true;

			//check if moves are equal;
            for (int index = 0; index < gameBoard.GetPlayedMoves().Count && index < gameInformation.gameMoveList.Count; index++)
			{
                if (gameBoard.GetPlayedMoves()[index].square != gameInformation.gameMoveList[index].square)
				{
					movesAreEqual = false;
					break;
				}
			}

            if (movesAreEqual && gameInformation.gameMoveList.Count > gameBoard.GetPlayedMoves().Count)
			{
                ABMove move = gameInformation.gameMoveList[gameBoard.GetPlayedMoves().Count];
				//make move
                gameBoard.MakeABMove(move);
			}

            MovesChanged();
		}

		public void RedoAll()
		{
            while (thinking) ;
            
            bool movesAreEqual = true;

			//check if moves are equal;
            for (int index = 0; index < gameBoard.GetPlayedMoves().Count && index < gameInformation.gameMoveList.Count; index++)
			{
                if (gameBoard.GetPlayedMoves()[index].square != gameInformation.gameMoveList[index].square)
				{
					movesAreEqual = false;
					break;
				}
			}

            while (movesAreEqual && gameInformation.gameMoveList.Count > gameBoard.GetPlayedMoves().Count)
			{
                ABMove move = gameInformation.gameMoveList[gameBoard.GetPlayedMoves().Count];
				//make move
                gameBoard.MakeABMove(move);
			}

            MovesChanged();
        }

		public void Undo()
		{
            while (thinking) ;
            
            //undo one move
            if (gameBoard.GetPlayedMoves().Count > 0)
			{
                ABMove move = gameBoard.GetPlayedMoves()[gameBoard.GetPlayedMoves().Count - 1];
                gameBoard.UndoABMove();

                MovesChanged();
            }
		}

		public void UndoAll()
		{
            while (thinking) ;
            
            //undo all moves
            while (gameBoard.GetPlayedMoves().Count > 0)
			{
                ABMove move = gameBoard.GetPlayedMoves()[gameBoard.GetPlayedMoves().Count - 1];
                gameBoard.UndoABMove();
			}

            MovesChanged();
        }

        public int BoardSize
        {
			get 
            {
                //exit if no game
                if (search == null) return 0;
                return gameBoard.GetBoardSize(); 
            }
        }

        public TimeSpan MaxThinkingTime
        {
            get 
            {
                //exit if no game
                if (search == null) return new TimeSpan(0,0,0);
                return search.MaxThinkingTime; 
            }
            set 
            {
                //exit if no game
                if (search == null) return;
                search.MaxThinkingTime = value; 
            }
        }

        public bool IterativeDeepening
        {
            get
            {
                //exit if no game
                if (search == null) return false;
                return search.IterativeDeepening;
            }
            set
            {
                //exit if no game
                if (search == null) return;
                search.IterativeDeepening = value;
            }
        }

        public int MaxSearchDepth
        {
            get
            {
                //exit if no game
                if (search == null) return 0;
                return search.MaxSearchDepth;
            }
            set
            {
                //exit if no game
                if (search == null) return;
                search.MaxSearchDepth = value;
            }
        }

        public int TranspositionTableSize
        {
            get 
            {
                //exit if no game
                if (search == null) return 0;
                return transpositionTable.TableSize; 
            }

            set 
            {
                //exit if no game
                if (search == null) return;
                transpositionTable.TableSize = value; 
            }
        }

        public void StartThinking()
        {
            while (thinking) ;

            //exit if no game
            if (search == null) throw new NotImplementedException("Engine not yet initialized!");

            thinking = true;
            //create delegate
            ThreadStart delegate1 = new ThreadStart(search.RootSearch);
            //create thread
            Thread thread1 = new Thread(delegate1);
            thread1.Name = "Search Thread";

            //start thread
            thread1.Start();

        }

        void MovesChanged()
        {
            gameInformation.playedMoves = gameBoard.GetPlayedMoves();

            //determine next move
            gameInformation.nextMove = null;
            if (gameInformation.gameMoveList.Count > gameInformation.playedMoves.Count)
            {
                int index1;
                for (index1 = 0; index1 < gameInformation.playedMoves.Count; index1++)
                {
                    if (gameInformation.playedMoves[index1].square != gameInformation.gameMoveList[index1].square) break;
                }

                if (index1 == gameInformation.playedMoves.Count)
                {
                    gameInformation.nextMove = gameInformation.gameMoveList[index1];
                }
            }

            gameInformation.possibleMoves = gameBoard.GeneratePossibleMoves();
            
            gameInformation.Evaluation = (gameBoard.GetPlayerOnMove() == Player.BlackPlayer) ? gameBoard.GetEvaluation():-gameBoard.GetEvaluation();
            //gameBoard.GetEvaluationDetail(out gameInformation.BlackScore, out gameInformation.WhiteScore);

            MovesChangedE(gameInformation);
        }

		public void GetSquareInfo(string notification, out SquareInfo squareInfo)
		{
            Conversions conversions = new Conversions(gameBoard.GetBoardSize());
			int square = conversions.Square(notification);

            gameBoard.GetSquareInfo(square, out squareInfo);
		}
		
		//returns player who has few symbols
        public Player WhoIsOnMove
        {
            get
            {
                return gameBoard.GetPlayerOnMove();
            }
        }

        public string Version
        {
            get
            {
                string version = this.GetType().Assembly.GetName().Version.ToString();
                return version;
            }
        }

        public void ResetTtTable(bool useDictionary)
        {
            while (thinking) ;
            if (search == null) return;
            transpositionTable.ResetTable(useDictionary);
        }

        public Player GetSymbol(int row, int column)
		{
            Conversions conversions = new Conversions(gameBoard.GetBoardSize());
            int square = conversions.RowAndColumn2Index(row, column);
            return gameBoard.GetSymbol(square);
		}

		public void StopThinking()
		{
			search.StopThinking();
		}

        void search_ThinkingProgress(SearchInformation info)
        {
            ThinkingProgress(info);
        }

        void search_ThinkingFinished(SearchInformation info)
        {
            thinking = false;
            ThinkingFinished(info);
        }

        public TuningInfo GetTuningInfo()
        {
            return gameBoard.GetTuningInfo();
        }

        public void SetTuningInfo(TuningInfo info)
        {
            while (thinking) ;
            
            //store number of moves
            List<ABMove> playedMoves = new List<ABMove> (gameBoard.GetPlayedMoves());

            //undo all moves
            for (int i = 0; i<playedMoves.Count; i++)
            {
                gameBoard.UndoABMove();
            }


            gameBoard.SetTuningInfo(info);

            //redo all moves
            for (int i = 0; i < playedMoves.Count; i++)
            {
                gameBoard.MakeABMove(playedMoves[i]);
            }
            
            ResetTtTable(false);

        }

        public bool VctActive
        {
            set
            {
                gameBoard.VctActive = value;
                MovesChanged();
            }

            get
            {
                return gameBoard.VctActive;
            }
        }

        public bool Thinking
        {
            get
            {
                return thinking;
            }
        }
    }
}
