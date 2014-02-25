using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuEngine
{
    public enum TTEvaluationType 
    { 
        //Unknown,
        Exact,
        UpperBound, 
        LowerBound,
    }

    class TranspositionTableItem
    {
        public ulong key;
        public int value;
        public TTEvaluationType type;
        public int depth;
        public int examinedMoves;
        public int bestMove;

        public TranspositionTableItem()
        {
            key = 0;
            value = int.MaxValue;
            type = TTEvaluationType.Exact;
            depth = 0;
            examinedMoves = 0;
            bestMove = -1;
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
		int successfulVCTHits;
		int failureHits;

		Random random;

		ulong[] hashBlackSquare;//black hash codes
		ulong[] hashWhiteSquare;//white hash codes
        ulong hashVctBlack;
        ulong hashVctWhite;

		//dictionary remembers everythink, not like hash table        
        Dictionary<ulong,TranspositionTableItem> dictionary;
        bool useDictionary;


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

          	dictionary = new Dictionary<ulong,TranspositionTableItem>();

          	useDictionary = false;
            ResetTable(useDictionary);
		}

		public ulong GetZobristKey(Player vctPlayer)
		{
            switch (vctPlayer)
            {
            	case Player.BlackPlayer:
                	return zobristKey ^ hashVctBlack;
                case Player.WhitePlayer:
                	return zobristKey ^ hashVctWhite;
                default:
                	return zobristKey;
        	}			
		}
		
        public TranspositionTableItem Lookup(Player vctPlayer)
        {
            if (tableItems == 0) return null;

            ulong tmpZobristKey = GetZobristKey(vctPlayer);
        	TranspositionTableItem tableItem;

        	if (useDictionary)
        	{
        		if (dictionary.TryGetValue(tmpZobristKey, out tableItem))
        		{
        			successfulHits++;
                    if (vctPlayer != Player.None) successfulVCTHits++;
                	return tableItem;            			
        		}
    			else
    			{
        			failureHits++;
        			return null;      				
    			}
        	}
        	else
        	{
                /* get access to item */
                int index = (int)(tmpZobristKey % (ulong)tableItems);
                tableItem = items[index];
        	
                /* key must be the same */
                if (tableItem.key == tmpZobristKey)
                {
                    successfulHits++;
                    if (vctPlayer != Player.None) successfulVCTHits++;
                    return tableItem;
                }
                else
                {
                    failureHits++;
                    return null;
                }
        	}
        }

		public void Store(int value, Player vctPlayer, TTEvaluationType type, int depth, int examinedMoves, int bestMove)
        {
            if (tableItems != 0)
            {
	          	ulong tmpZobristKey = GetZobristKey(vctPlayer);;

	          	/* get access to item */
                int index = (int)(tmpZobristKey % (ulong)tableItems);
                TranspositionTableItem tableItem = items[index];

                tableItem.key = tmpZobristKey;
                tableItem.value = value;
                tableItem.type = type;
                tableItem.depth = depth;
                tableItem.examinedMoves = examinedMoves;
                tableItem.bestMove = bestMove;

                //store the same data also into dictionary
                if (useDictionary)
                {
                	//add key if it does not exist yet
                	if (!dictionary.ContainsKey(tmpZobristKey))
                		dictionary.Add(tmpZobristKey, tableItem);
                }
            }
            
        }

        public int TableSize
        {
            get{ return tableSize;}
            set{ tableSize = value;}
        }

		public void ResetTable(bool useDictionary)
        {
			this.useDictionary = useDictionary;
			
			tableItems = tableSize / 30;

            successfulHits = 0;
            successfulVCTHits = 0;
            failureHits = 0;

            items = new TranspositionTableItem[tableItems];

            for (int index = 0; index < tableItems; index++)
            {
                items[index] = new TranspositionTableItem();
            }

            //clear dictionary
            if (useDictionary)
            	dictionary.Clear();
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

		public float SuccessfulVCTHits
		{
			get
			{
				return (float)100*successfulVCTHits/successfulHits;
			}
		}
	}


}
