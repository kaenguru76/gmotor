using System;
using System.Collections.Generic;
using System.Threading;

namespace GomokuEngine
{
	public static class EvaluationConstants
	{
		public const int max = 1000000;
		public const int min = -1000000;
	    
		static public string Score2Text(int evaluation)
		{
			string s1;
			switch (evaluation) {
				case EvaluationConstants.max:
					s1 = "Black wins";
					break;
				case EvaluationConstants.min:
					s1 = "White wins";
					break;
				default:
					s1 = evaluation.ToString();
					break;
			}	
			return s1;
		}
	}
	
	public class Engine
	{
		public delegate void GenericEvent();
		public delegate void NewGameEvent();
		public event NewGameEvent NewGameE;

		//public delegate void MovesChangedEvent(GameInformation gameInformation);
		//public event MovesChangedEvent MovesChangedE;

		public delegate void ThinkingFinishedEvent(SearchInformation info);
		public event ThinkingFinishedEvent ThinkingFinished;

		public delegate void ThinkingProgressEvent(SearchInformation info);
		public event ThinkingProgressEvent ThinkingProgress;

		GameBoard gameBoard;
		TranspositionTable transpositionTable;
		Search search;
		
		public GameInformation gameInformation;
		bool thinking;
        
		string _fileName;
		public event GenericEvent FileNameChanged;

		string _blackPlayerName;
		string _whitePlayerName;
		public event GenericEvent BlackPlayerNameChanged;
		public event GenericEvent WhitePlayerNameChanged;

		List<BoardSquare> gameMoveList;

		public Engine()
		{
		}
		/*
        public void NewGame(int boardSize)
        {
            //while (thinking) ;
            
            
            //initialize all
            transpositionTable = new TranspositionTable(boardSize);
            gameBoard = new GameBoard(boardSize,transpositionTable);
            search = new Search(gameBoard, transpositionTable);
            search.ThinkingFinished += new Search.ThinkingFinishedEvent(search_ThinkingFinished);
            search.ThinkingProgress += new Search.ThinkingProgressEvent(search_ThinkingProgress);

            //copy move list
            gameInformation = new GameInformation();
            gameInformation.gameMoveList = new List<BoardSquare>();
            //gameInformation.fileName = "New Game";
            //gameInformation.blackPlayerName = "Black Player";
            //gameInformation.whitePlayerName = "White Player";

            NewGameE();

            //MovesChanged();
        }*/

		public void NewGame(int boardSize, List<BoardSquare> moveList = null)
		{
			//while (thinking) ;

			//initialize all
			transpositionTable = new TranspositionTable(boardSize);
			gameBoard = new GameBoard(boardSize, transpositionTable);
			search = new Search(gameBoard, transpositionTable);
			search.ThinkingFinished += search_ThinkingFinished;
			search.ThinkingProgress += search_ThinkingProgress;

			//copy move list
			gameMoveList = (moveList == null) ? new List<BoardSquare>() : new List<BoardSquare>(moveList);

			//play all moves
			foreach (BoardSquare move in gameMoveList) {
				//BoardSquare move = gameInformation.gameMoveList[index];
				//MakeMove(move);
				gameBoard.MakeMove(move.Index);
			}
			
			BlackPlayerName = "Black Player";
			WhitePlayerName = "White Player";
			FileName = "New Game";

			NewGameE();

			//MovesChanged();
		}

		public void MakeMove(BoardSquare move)
		{
			//while (thinking);

			//get square
			//int square = conversions.RowAndColumn2Index(row, column);
            
			//if square is occupied, exit
			if (gameBoard.GetSymbol(move.Index) != Player.None)
				return;

			gameBoard.MakeMove(move.Index);

			//MovesChanged();
		}

		//        public void Redraw()
		//        {
		//            while (thinking) ;
		//
		//            MovesChanged();
		//        }

		//        public BoardSquare GetLastPlayedMove()
		//        {
		//            if (playedMoves.Count > 0)
		//                return playedMoves[playedMoves.Count - 1];
		//            else
		//                return null;
		//        }

		public void Redo()
		{
			while (thinking)
				;
            
			bool movesAreEqual = true;

			//check if moves are equal;
			for (int index = 0; index < gameBoard.playedSquares.Count && index < gameMoveList.Count; index++) {
				if (gameBoard.playedSquares[index] != gameMoveList[index].Index) {
					movesAreEqual = false;
					break;
				}
			}

			if (movesAreEqual && gameMoveList.Count > gameBoard.playedSquares.Count) {
				BoardSquare move = gameMoveList[gameBoard.playedSquares.Count];
				//make move
				MakeMove(move);
			}

			//MovesChanged();
		}

		public void RedoAll()
		{
			while (thinking)
				;
            
			bool movesAreEqual = true;

			//check if moves are equal;
			for (int index = 0; index < gameBoard.playedSquares.Count && index < gameMoveList.Count; index++) {
				if (gameBoard.playedSquares[index] != gameMoveList[index].Index) {
					movesAreEqual = false;
					break;
				}
			}

			while (movesAreEqual && gameMoveList.Count > gameBoard.playedSquares.Count) {
				BoardSquare move = gameMoveList[gameBoard.playedSquares.Count];
				//make move
				MakeMove(move);
			}

			//MovesChanged();
		}

		public void Undo()
		{
			while (thinking)
				;
            
			//undo one move
			if (gameBoard.playedSquares.Count > 0) {
//                ABMove move = gameBoard.GetPlayedMoves()[gameBoard.GetPlayedMoves().Count - 1];
				gameBoard.UndoMove();

				//MovesChanged();
			}
		}

		public void UndoAll()
		{
			while (thinking)
				;
            
			//undo all moves
			while (gameBoard.playedSquares.Count > 0) {
				//ABMove move = gameBoard.GetPlayedMoves()[gameBoard.GetPlayedMoves().Count - 1];
				gameBoard.UndoMove();
			}

			//MovesChanged();
		}

		public int BoardSize {
			get {
				//exit if no game
				if (search == null)
					return 0;
				return gameBoard.BoardSize; 
			}
		}

		public TimeSpan MaxThinkingTime {
			get {
				//exit if no game
				if (search == null)
					return new TimeSpan(0, 0, 0);
				return search.MaxThinkingTime; 
			}
			set {
				//exit if no game
				if (search == null)
					return;
				search.MaxThinkingTime = value; 
			}
		}

		public bool IterativeDeepening {
			get {
				//exit if no game
				if (search == null)
					return false;
				return search.IterativeDeepening;
			}
			set {
				//exit if no game
				if (search == null)
					return;
				search.IterativeDeepening = value;
			}
		}

		public int MaxSearchDepth {
			get {
				//exit if no game
				if (search == null)
					return 0;
				return search.MaxSearchDepth;
			}
			set {
				//exit if no game
				if (search == null)
					return;
				search.MaxSearchDepth = value;
			}
		}

		public int TranspositionTableSize {
			get {
				//exit if no game
				if (search == null)
					return 0;
				return transpositionTable.TableSize; 
			}

			set {
				//exit if no game
				if (search == null)
					return;
				transpositionTable.TableSize = value; 
			}
		}

		public void StartThinking()
		{
			while (thinking)
				;

			//exit if no game
			if (search == null)
				throw new NotImplementedException("Engine not yet initialized!");

			thinking = true;
			//create delegate
			ThreadStart delegate1 = new ThreadStart(search.RootSearch);
			//create thread
			Thread thread1 = new Thread(delegate1);
			thread1.Name = "Search Thread";

			//start thread
			thread1.Start();

		}

		//        void MovesChanged()
		//        {
		//            gameInformation.playedMoves = gameBoard.GetPlayedMoves();
		//
		//            //determine next move
		//            gameInformation.nextMove = null;
		//            if (gameInformation.gameMoveList.Count > gameInformation.playedMoves.Count)
		//            {
		//                int index1;
		//                for (index1 = 0; index1 < gameInformation.playedMoves.Count; index1++)
		//                {
		//                    if (gameInformation.playedMoves[index1].square != gameInformation.gameMoveList[index1].square) break;
		//                }
		//
		//                if (index1 == gameInformation.playedMoves.Count)
		//                {
		//                    gameInformation.nextMove = gameInformation.gameMoveList[index1];
		//                }
		//            }
		//
		//            gameInformation.possibleMoves = gameBoard.GeneratePossibleMoves(gameBoard.VctPlayer, gameBoard.VctDepth0);
		//
		//            gameInformation.Evaluation = (gameBoard.PlayerOnMove == Player.BlackPlayer) ? gameBoard.GetEvaluation():-gameBoard.GetEvaluation();
		//            gameInformation.GainSquare = new ABMove(gameBoard.GainSquare,gameBoard.PlayerOnMove,gameBoard.BoardSize);
		//            MovesChangedE(gameInformation);
		//        }

		public void GetSquareInfo(string notification, out SquareInfo squareInfo)
		{
			var square = new BoardSquare(gameBoard.BoardSize, notification);

			gameBoard.GetSquareInfo(square.Index, out squareInfo);
		}
		
		//returns player who has few symbols
		public Player WhoIsOnMove {
			get {
				return gameBoard.PlayerOnMove;
			}
		}

		public string Version {
			get {
				string version = this.GetType().Assembly.GetName().Version.ToString();
				return version;
			}
		}

		public void ResetTtTable(bool useDictionary)
		{
			while (thinking)
				;
			if (search == null)
				return;
			transpositionTable.ResetTables(useDictionary);
		}

		public Player GetSymbol(int row, int column)
		{
			var square = new BoardSquare(gameBoard.BoardSize, row, column);
			return gameBoard.GetSymbol(square.Index);
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
//            while (thinking) ;
//            
//            //store number of moves
//            var playedMoves = new List<int> (gameBoard.playedSquares);
//
//            //undo all moves
//            for (int i = 0; i<playedMoves.Count; i++)
//            {
//                gameBoard.UndoMove();
//            }
//
//
//            gameBoard.SetTuningInfo(info);
//
//            //redo all moves
//            for (int i = 0; i < playedMoves.Count; i++)
//            {
//                gameBoard.MakeMove(playedMoves[i]);
//            }
//            
//            ResetTtTable(false);

		}

		public bool VctActive {
			set {
				gameBoard.VctActive = value;
				//MovesChanged();
			}

			get {
				return gameBoard.VctActive;
			}
		}

		public bool Thinking {
			get {
				return thinking;
			}
		}
        
		public string FileName {
			get {
				return _fileName;
			}
			set {
				_fileName = value;
				FileNameChanged();
			}
		}
        
		public string BlackPlayerName {
			get {
				return _blackPlayerName;
			}
			set {
				_blackPlayerName = value;
				BlackPlayerNameChanged();
			}
		}
        
		public string WhitePlayerName {
			get {
				return _whitePlayerName;
			}
			set {
				_whitePlayerName = value;
				WhitePlayerNameChanged();
			}
		}

		public List<BoardSquare> PlayedMoves {
			get {
				var list1 = new List<BoardSquare>();
        		
				foreach (int element in gameBoard.playedSquares) {
					var bs = new BoardSquare(gameBoard.BoardSize, element);
					list1.Add(bs);
				}
				return list1;
			}
        		
		}
	}
}
