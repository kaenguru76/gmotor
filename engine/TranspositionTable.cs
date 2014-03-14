using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuEngine
{
    public enum TTEvaluationType 
    { 
        Exact,
        UpperBound, 
        LowerBound,
    }

    class TranspositionTableItem
    {
        public ulong key;
        public int value;
        public TTEvaluationType type;
        public int depthLeft;
        public int examinedMoves;
        public int bestMove;

        public TranspositionTableItem()
        {
            key = 0;
            value = 0;
            type = TTEvaluationType.Exact;
            depthLeft = 0;
            examinedMoves = 0;
            bestMove = -1;
        }

        public override string ToString()
        {
            return key.ToString() + "," + value.ToString();
        }
    }
    /*
    class TranspositionTableVctItem
    {
        public ulong key;
        public VctStatus value;
        public int depth;
        public int examinedMoves;
        public int bestMove;

        public TranspositionTableVctItem()
        {
            key = 0;
            value = VctStatus.Disproven;
            depth = 0;
            examinedMoves = 0;
            bestMove = -1;
        }
        
        public override string ToString()
        {
            return key.ToString() + "," + value.ToString();
        }
    }*/
    
    class TranspositionTable
    {
		TranspositionTableItem[] items;
        int tableSize;//size of table in MB
		int tableItems;//number of items in table
        int boardSize;

		ulong zobristKey;
		int successfulHits;
		//int successfulVctHits;
		int failureHits;
		//int failureVctHits;

		Random random;

		ulong[] hashBlackSquare;//black hash codes
		ulong[] hashWhiteSquare;//white hash codes
		ulong[] hashGainSquare;

		//dictionary remembers everythink, not like hash table        
        Dictionary<ulong,TranspositionTableItem> dictionary;
        bool useDictionary;
		//ulong trapKey=0;
		int gainSquare;

		public TranspositionTable(int boardSize)
        {
            this.boardSize = boardSize;
            tableSize = 10000000;

            random = new Random();

			hashBlackSquare = new ulong[boardSize * boardSize];
			hashWhiteSquare = new ulong[boardSize * boardSize];
			hashGainSquare = new ulong[boardSize * boardSize];
            
			for (int square = 0; square < boardSize * boardSize; square++)
			{
				hashBlackSquare[square] = rand64();
				hashWhiteSquare[square] = rand64();
				hashGainSquare[square] = rand64();
			}

          	dictionary = new Dictionary<ulong,TranspositionTableItem>();

          	useDictionary = false;
            ResetTables(useDictionary);
		}
		
        public TranspositionTableItem Lookup()
        {
            if (tableItems == 0) return null;

        	TranspositionTableItem item;

        	if (useDictionary)
        	{
        		if (dictionary.TryGetValue(zobristKey, out item))
        		{
        			successfulHits++;
                	return item;            			
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
                int index = (int)(zobristKey % (ulong)tableItems);
                item = items[index];
        	
                /* key must be the same */
                if (item.key == zobristKey)
                {
                    successfulHits++;
                    return item;
                }
                else
                {
                    failureHits++;
                    return null;
                }
        	}
        }

        public void Store(int value, TTEvaluationType type, int depthLeft, int examinedMoves, int bestMove)
        {
            if (tableItems != 0)
            {
	          	/* get access to item */
                int index = (int)(zobristKey % (ulong)tableItems);
                TranspositionTableItem tableItem = items[index];

                tableItem.key = zobristKey;
                tableItem.value = value;
                tableItem.type = type;
                tableItem.depthLeft = depthLeft;
                tableItem.examinedMoves = examinedMoves;
                tableItem.bestMove = bestMove;

                //store the same data also into dictionary
                if (useDictionary)
                {
                	//add key if it does not exist yet
                	if (!dictionary.ContainsKey(zobristKey))
                		dictionary.Add(zobristKey, tableItem);
                }
            }
            
        }

		public int TableSize
        {
            get{ return tableSize;}
            set{ tableSize = value;}
        }

		public void ResetTables(bool useDictionary)
        {
			gainSquare = -1;
			this.useDictionary = useDictionary;
			
			tableItems = tableSize / 30;
            
            items = new TranspositionTableItem[tableItems];

            for (int index = 0; index < tableItems; index++)
            {
                items[index] = new TranspositionTableItem();
            }

            //clear dictionary
            if (useDictionary)
            	dictionary.Clear();
        }
		
		public void ResetStatistics()
		{
            successfulHits = 0;
            //successfulVctHits = 0;
            failureHits = 0;
			//failureVctHits = 0;			
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

		public void MakeMove(int square, Player player, int gainSquare)
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
			
			//hash gainSquare
			if (gainSquare != this.gainSquare)
			{
				//unmap old
				if (this.gainSquare != -1) 
					zobristKey ^= hashGainSquare[this.gainSquare];
				//map new
				if (gainSquare != -1)
					zobristKey ^= hashGainSquare[gainSquare];
				//store gainSquare
				this.gainSquare = gainSquare;
			}
        }

        public void UndoMove(int square, Player player, int gainSquare)
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

            //hash gainSquare
			if (gainSquare != this.gainSquare)
			{
				//unmap old 
				if (this.gainSquare != -1)
					zobristKey ^= hashGainSquare[this.gainSquare];
				//map new
				if (gainSquare != -1)
					zobristKey ^= hashGainSquare[gainSquare];
				//store gainSquare
				this.gainSquare = gainSquare;
			}
        }

		public float SuccessfulHits
		{
			get
			{
				return (float)100*successfulHits/(successfulHits+failureHits);
			}
		}

//		public float SuccessfulVctHits
//		{
//			get
//			{
//				return (float)100*successfulVctHits/(successfulVctHits+failureVctHits);
//			}
//		}
	}


}
