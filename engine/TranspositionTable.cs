using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuEngine
{
    public enum TTEvaluationType 
    { 
        Unknown,
        Exact,
        UpperBound, 
        LowerBound,
    }

    class TranspositionTableItem
    {
        public ulong key;
        public int value;
        public TTEvaluationType type;
        //public Player vctPlayer;
        public int depth;
        public int bestMove;
        public int examinedMoves;

        public TranspositionTableItem()
        {
            key = 0;
            value = int.MaxValue;
            type = TTEvaluationType.Exact;
            depth = 0;
            bestMove = -1;
            examinedMoves = 0;
            //vctPlayer = Player.None;
        }

        public override string ToString()
        {
            return key.ToString() + "," + value.ToString();
        }
    }
    
    class TranspositionTable
    {
		TranspositionTableItem[] items;
        int tableSize;//size of table in MB
		int tableItems;//number of items in table
        int boardSize;

		ulong zobristKey;
		int successfulHits;
		int failureHits;

		Random random;

		ulong[] hashBlackSquare;//black hash codes
		ulong[] hashWhiteSquare;//white hash codes
        ulong hashVctBlack;
        ulong hashVctWhite;

		public TranspositionTable(int boardSize)
        {
            this.boardSize = boardSize;
            tableSize = 10000000;

            random = new Random();

			hashBlackSquare = new ulong[boardSize * boardSize];
			hashWhiteSquare = new ulong[boardSize * boardSize];
            
			for (int square = 0; square < boardSize * boardSize; square++)
			{
				hashBlackSquare[square] = rand64();
				hashWhiteSquare[square] = rand64();
			}

            hashVctBlack = rand64();
            hashVctWhite = rand64();

            ResetTable();
		}

        public TranspositionTableItem Lookup(Player vctPlayer)
        {
            if (tableItems != 0)
            {
                ulong tmpZobristKey;

                switch (vctPlayer)
                {
                    case Player.BlackPlayer:
                        tmpZobristKey = zobristKey ^ hashVctBlack;
                        break;
                    case Player.WhitePlayer:
                        tmpZobristKey = zobristKey ^ hashVctWhite;
                        break;
                    default:
                        tmpZobristKey = zobristKey;
                        break;
                }

                /* get access to item */
                int index = (int)(tmpZobristKey % (ulong)tableItems);
                TranspositionTableItem tableItem = items[index];

                /* key must be the same */
                if (tableItem.key == tmpZobristKey)
                {
                    successfulHits++;
                    return tableItem;
                }
                else
                {
                    failureHits++;
                    return null;
                }
            }
            return null;
        }

		public void Store(int value, Player vctPlayer, TTEvaluationType type, int depth, int bestMove, int examinedMoves)
        {
            if (tableItems != 0)
            {
                ulong tmpZobristKey;

                switch (vctPlayer)
                {
                    case Player.BlackPlayer:
                        tmpZobristKey = zobristKey ^ hashVctBlack;
                        break;
                    case Player.WhitePlayer:
                        tmpZobristKey = zobristKey ^ hashVctWhite;
                        break;
                    default:
                        tmpZobristKey = zobristKey;
                        break;
                }

                /* get access to item */
                int index = (int)(tmpZobristKey % (ulong)tableItems);
                TranspositionTableItem tableItem = items[index];

                tableItem.key = tmpZobristKey;
                tableItem.value = value;
                tableItem.type = type;
                //tableItem.vctPlayer = vctPlayer;
                tableItem.depth = depth;
                tableItem.bestMove = bestMove;
                tableItem.examinedMoves = examinedMoves;
            }
        }

        public int TableSize
        {
            get{ return tableSize;}
            set{ tableSize = value;}
        }

		public void ResetTable()
        {
			tableItems = tableSize / 30;

            successfulHits = 0;
            failureHits = 0;

            items = new TranspositionTableItem[tableItems];

            for (int index = 0; index < tableItems; index++)
            {
                items[index] = new TranspositionTableItem();
            }
        }

        /* returns 64-bit random number */
		ulong rand64()
		{
			ulong part1 = Convert.ToUInt64(random.Next(0x10000));
			ulong part2 = Convert.ToUInt64(random.Next(0x10000)) << 16;
			ulong part3 = Convert.ToUInt64(random.Next(0x10000)) << 32;
			ulong part4 = Convert.ToUInt64(random.Next(0x10000)) << 48;

			return part1 ^ part2 ^ part3 ^ part4;
		}

		public void MakeMove(int square, Player player)
		{
			//modify Zobrist key
			if (player == Player.BlackPlayer)
			{
				zobristKey ^= hashBlackSquare[square];
			}
			else
			{
				zobristKey ^= hashWhiteSquare[square];
			}
        }

        public void UndoMove(int square, Player player)
		{
            //modify Zobrist key
            if (player == Player.BlackPlayer)
            {
                zobristKey ^= hashBlackSquare[square];
            }
            else
            {
                zobristKey ^= hashWhiteSquare[square];
            }
        }

		public float SuccessfulHits
		{
			get
			{
				return (float)100*successfulHits/(successfulHits+failureHits);
			}
		}
	}


}
