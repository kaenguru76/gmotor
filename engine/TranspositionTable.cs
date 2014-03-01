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

    public enum TT_VCT_Status 
    { 
        Proven,
        Disproven, 
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
            value = 0;
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
    
    class TranspositionTableVCTItem
    {
        public ulong key;
        public TT_VCT_Status value;
        public int depth;
        public int examinedMoves;
        public int bestMove;

        public TranspositionTableVCTItem()
        {
            key = 0;
            value = TT_VCT_Status.Disproven;
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
		TranspositionTableVCTItem[] itemsVctBlack;
		TranspositionTableVCTItem[] itemsVctWhite;
        int tableSize;//size of table in MB
		int tableItems;//number of items in table
        int boardSize;

		ulong zobristKey;
		int successfulHits;
		int successfulVCTHits;
		int failureHits;
		int failureVctHits;

		Random random;

		ulong[] hashBlackSquare;//black hash codes
		ulong[] hashWhiteSquare;//white hash codes

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

          	dictionary = new Dictionary<ulong,TranspositionTableItem>();

          	useDictionary = false;
            ResetTable(useDictionary);
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

        public TranspositionTableVCTItem LookupVctBlack()
        {
            if (tableItems == 0) return null;

            /* get access to item */
            int index = (int)(zobristKey % (ulong)tableItems);
            TranspositionTableVCTItem item = itemsVctBlack[index];
    	
            /* key must be the same */
            if (item.key == zobristKey)
            {
                successfulVCTHits++;
                return item;
            }
            else
            {
                failureVctHits++;
                return null;
            }
        }

        public TranspositionTableVCTItem LookupVctWhite()
        {
            if (tableItems == 0) return null;

            /* get access to item */
            int index = (int)(zobristKey % (ulong)tableItems);
            TranspositionTableVCTItem item = itemsVctWhite[index];
    	
            /* key must be the same */
            if (item.key == zobristKey)
            {
                successfulVCTHits++;
                return item;
            }
            else
            {
                failureVctHits++;
                return null;
            }
        }
                
        public void Store(int value, TTEvaluationType type, int depth, int examinedMoves, int bestMove)
        {
            if (tableItems != 0)
            {
	          	/* get access to item */
                int index = (int)(zobristKey % (ulong)tableItems);
                TranspositionTableItem tableItem = items[index];

                tableItem.key = zobristKey;
                tableItem.value = value;
                tableItem.type = type;
                tableItem.depth = depth;
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

		public void StoreVctBlack(TT_VCT_Status value, int depth, int examinedMoves, int bestMove)
        {
            if (tableItems == 0) return;
            
          	/* get access to item */
            int index = (int)(zobristKey % (ulong)tableItems);
            TranspositionTableVCTItem item = itemsVctBlack[index];

            item.key = zobristKey;
            item.value = value;
            item.depth = depth;
            item.examinedMoves = examinedMoves;
            tableItem.bestMove = bestMove;
        }

		public void StoreVctWhite(TT_VCT_Status value, int depth, int examinedMoves, int bestMove)
        {
            if (tableItems == 0) return;
            
          	/* get access to item */
            int index = (int)(zobristKey % (ulong)tableItems);
            TranspositionTableVCTItem item = itemsVctWhite[index];

            item.key = zobristKey;
            item.value = value;
            item.depth = depth;
            item.examinedMoves = examinedMoves;
            tableItem.bestMove = bestMove;
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
			failureVctHits = 0;
            
            items = new TranspositionTableItem[tableItems];
            itemsVctBlack = new TranspositionTableVCTItem[tableItems];
            itemsVctWhite = new TranspositionTableVCTItem[tableItems];

            for (int index = 0; index < tableItems; index++)
            {
                items[index] = new TranspositionTableItem();
                itemsVctBlack[index] = new TranspositionTableVCTItem();
                itemsVctWhite[index] = new TranspositionTableVCTItem();
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
				return (float)100*successfulVCTHits/(successfulVCTHits+failureVctHits);
			}
		}
	}


}
