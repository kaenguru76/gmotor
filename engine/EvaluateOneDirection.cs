using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GomokuEngine 
{
	public enum Player { None, BlackPlayer, WhitePlayer }

	public enum Direction {left2right, leftdown2rightup, down2up,rightdown2leftup };

    public enum OneDirectionEvaluation : byte
    {
        overline,
        o4,
        c4,
        o3,
        s3,
        c3,
        o2p,//o2plus
        o2,
        c2,
        o1p,//o1plus
        o1,
        c1,
        o0,
        c0,
        nothing,
        unknown,
    };

    public class OneDirectionData
    {
        public int hash;
        public OneDirectionEvaluation evaluationBlack;
        public OneDirectionEvaluation evaluationWhite;
    }

    /***********************************************************************
     *  This class gets evaluation for one direction of a square
     ***********************************************************************/
    class EvaluateOneDirection
	{
        byte[] evaluationTable;
        OneDirectionData[][] oneDirectionData;
        OneDirectionEvaluation[][] pattern5black;//[pattern][shift]
        OneDirectionEvaluation[][] pattern5white;//[pattern][shift]

        public EvaluateOneDirection(int boardSize)
        {
            //compute number of squares of board
            int numberOfSquares = boardSize * boardSize;

            oneDirectionData = new OneDirectionData[numberOfSquares][];
            for (int square = 0; square < numberOfSquares; square++)
            {
                oneDirectionData[square] = new OneDirectionData[4];
                for (int direction = 0; direction < 4; direction++)
                {
                    oneDirectionData[square][direction] = new OneDirectionData();

                    //oneDirectionData[square][direction].square = square;
                    //oneDirectionData[square][direction].direction = (Direction)direction;
                    oneDirectionData[square][direction].hash = 0xFFFFF;
                    oneDirectionData[square][direction].evaluationBlack = OneDirectionEvaluation.nothing;
                    oneDirectionData[square][direction].evaluationWhite = OneDirectionEvaluation.nothing;
                }
            }

            InitializePattern5();

            //allocate table with lineQuality for 4^10 square combinations
            evaluationTable = new byte[0x100000];

            /* initialize all elements of table */
            for (int pattern = 0; pattern <= evaluationTable.GetUpperBound(0); pattern++)
            {
                evaluationTable[pattern] = GetCategory(pattern);
            }
        }
        
        #region initialization of Categories

        byte GetCategory(int pattern)
        {
            OneDirectionEvaluation blackEvaluation = OneDirectionEvaluation.unknown;
            OneDirectionEvaluation whiteEvaluation = OneDirectionEvaluation.unknown;

            OneDirectionEvaluation tmpBlack;
            OneDirectionEvaluation tmpWhite;

            int pattern5;
            int shift;

            //go through all fives
            for (shift = 0; shift < 6; shift++)
            {
                pattern5 = pattern & 0x3FF;

                tmpBlack = pattern5black[pattern5][shift];
                tmpWhite = pattern5white[pattern5][shift];

                if (tmpBlack < blackEvaluation) blackEvaluation = tmpBlack;
                if (tmpWhite < whiteEvaluation) whiteEvaluation = tmpWhite;

                //shift pattern by one stone
                pattern >>= 2;
            }

            return (byte)((byte)blackEvaluation +((byte)whiteEvaluation << 4));
        }

        void InitializePattern5()
        {
            //initialize shift 0 to 5
            pattern5black = new OneDirectionEvaluation[0x400][];
            pattern5white = new OneDirectionEvaluation[0x400][];

            //initialize all shifts
            for (int pattern = 0; pattern < 0x400; pattern++)
            {
                pattern5black[pattern] = new OneDirectionEvaluation[6];
                pattern5white[pattern] = new OneDirectionEvaluation[6];
            }

            Player[] blackStones = new Player[5];
            Player[] whiteStones = new Player[5];
            Player[] blackStonesReversed = new Player[5];
            Player[] whiteStonesReversed = new Player[5];

            //go through all combinations of pattern5
            for (int pattern = 0; pattern < 0x400; pattern++)
            {
                //disassemble to stones
                for (int stone = 0; stone < 5; stone++)
                {
                    switch ((pattern >> (2 * stone)) & 3)
                    {
                        case 0://nothing
                            blackStones[stone] = Player.None;
                            whiteStones[stone] = Player.None;
                            break;

                        case 1://black stone
                            blackStones[stone] = Player.BlackPlayer;
                            whiteStones[stone] = Player.WhitePlayer;
                            break;

                        case 2://white stone
                            blackStones[stone] = Player.WhitePlayer;
                            whiteStones[stone] = Player.BlackPlayer;
                            break;

                        case 3://wall
                            blackStones[stone] = Player.WhitePlayer;
                            whiteStones[stone] = Player.WhitePlayer;
                            break;
                    }
                }



                pattern5black[pattern][0] = GetShift0(blackStones);
                pattern5white[pattern][0] = GetShift0(whiteStones);

                pattern5black[pattern][1] = GetShift1(blackStones);
                pattern5white[pattern][1] = GetShift1(whiteStones);

                pattern5black[pattern][2] = GetShift2(blackStones);
                pattern5white[pattern][2] = GetShift2(whiteStones);

                for (int stone = 0; stone < 5; stone++)
                {
                    blackStonesReversed[stone] = blackStones[4-stone];
                    whiteStonesReversed[stone] = whiteStones[4-stone];
                }

                pattern5black[pattern][3] = GetShift2(blackStonesReversed);
                pattern5white[pattern][3] = GetShift2(whiteStonesReversed);

                pattern5black[pattern][4] = GetShift1(blackStonesReversed);
                pattern5white[pattern][4] = GetShift1(whiteStonesReversed);

                pattern5black[pattern][5] = GetShift0(blackStonesReversed);
                pattern5white[pattern][5] = GetShift0(whiteStonesReversed);
            }
        }

        OneDirectionEvaluation GetShift0(Player[] stones)
        {

            int nbBlack = 0;
            int nbWhite = 0;

            //count numbers of stones
            for (int index = 1; index <= 4; index++)
            {
                switch (stones[index])
                {
                    case Player.BlackPlayer:
                        nbBlack++;
                        break;

                    case Player.WhitePlayer:
                        nbWhite++;
                        break;
                }
            }

            if (nbWhite > 0) return OneDirectionEvaluation.nothing;

            switch (nbBlack)
            {
                case 4:
                    if (stones[0] == Player.None) return OneDirectionEvaluation.o4;
                    if (stones[0] == Player.BlackPlayer) return OneDirectionEvaluation.overline;
                    if (stones[0] == Player.WhitePlayer) return OneDirectionEvaluation.c4;
                    break;

                case 3:
                    if (stones[0] == Player.None) return OneDirectionEvaluation.s3;
                    if (stones[0] == Player.WhitePlayer) return OneDirectionEvaluation.c3;
                    break;

                case 2:
                    return OneDirectionEvaluation.c2;
                    /*if (stones[0] == Player.None) return OneDirectionEvaluation.c2;
                    if (stones[0] == Player.WhitePlayer) return OneDirectionEvaluation.c2;
                    break;*/

                case 1:
                    return OneDirectionEvaluation.c1;
                    /*if (stones[0] == Player.None) return OneDirectionEvaluation.c1;
                    if (stones[0] == Player.WhitePlayer) return OneDirectionEvaluation.c1;
                    break;*/

                case 0:
                    return OneDirectionEvaluation.c0;
                    /*if (stones[0] == Player.None) return OneDirectionEvaluation.c0;
                    if (stones[0] == Player.WhitePlayer) return OneDirectionEvaluation.c0;
                    break;*/
            }
            return OneDirectionEvaluation.nothing;
        }

        OneDirectionEvaluation GetShift1(Player[] stones)
        {

            int nbBlack = 0;
            int nbWhite = 0;

            //count numbers of stones
            for (int index = 1; index <= 3; index++)
            {
                switch (stones[index])
                {
                    case Player.BlackPlayer:
                        nbBlack++;
                        break;

                    case Player.WhitePlayer:
                        nbWhite++;
                        break;
                }
            }

            if (nbWhite > 0) return OneDirectionEvaluation.nothing;

            if (stones[0] == Player.WhitePlayer && stones[4] == Player.WhitePlayer) return OneDirectionEvaluation.nothing;

            switch (nbBlack)
            {
                case 3:
                    if (stones[0] == Player.BlackPlayer && stones[4] == Player.BlackPlayer) return OneDirectionEvaluation.overline;
                    if (stones[0] == Player.None && stones[4] == Player.None) return OneDirectionEvaluation.o3;
                    if (stones[0] == Player.BlackPlayer) return OneDirectionEvaluation.c4;
                    if (stones[4] == Player.BlackPlayer) return OneDirectionEvaluation.c4;
                    if (stones[0] == Player.WhitePlayer && stones[4] == Player.None) return OneDirectionEvaluation.c3;
                    if (stones[0] == Player.None && stones[4] == Player.WhitePlayer) return OneDirectionEvaluation.c3;
                    break;

                case 2:
                    if (stones[0] == Player.None && stones[4] == Player.None && stones[1] == Player.None) return OneDirectionEvaluation.o2p;
                    if (stones[0] == Player.None && stones[4] == Player.None) return OneDirectionEvaluation.o2;
                    if (stones[0] == Player.BlackPlayer) return OneDirectionEvaluation.c3;
                    if (stones[4] == Player.BlackPlayer) return OneDirectionEvaluation.c3;
                    if (stones[0] == Player.WhitePlayer && stones[4] == Player.None) return OneDirectionEvaluation.c2;
                    if (stones[0] == Player.None && stones[4] == Player.WhitePlayer) return OneDirectionEvaluation.c2;
                    break;

                case 1:
                    if (stones[0] == Player.None && stones[4] == Player.None && stones[3] == Player.BlackPlayer) return OneDirectionEvaluation.o1p;
                    if (stones[0] == Player.None && stones[4] == Player.None) return OneDirectionEvaluation.o1; 
                    if (stones[0] == Player.BlackPlayer) return OneDirectionEvaluation.c2;
                    if (stones[4] == Player.BlackPlayer) return OneDirectionEvaluation.c2;
                    if (stones[0] == Player.WhitePlayer && stones[4] == Player.None) return OneDirectionEvaluation.c1;
                    if (stones[0] == Player.None && stones[4] == Player.WhitePlayer) return OneDirectionEvaluation.c1;
                    break;

                case 0:
                    if (stones[0] == Player.None && stones[4] == Player.None) return OneDirectionEvaluation.o0;
                    if (stones[0] == Player.BlackPlayer) return OneDirectionEvaluation.c1;
                    if (stones[4] == Player.BlackPlayer) return OneDirectionEvaluation.c1;
                    if (stones[0] == Player.WhitePlayer && stones[4] == Player.None) return OneDirectionEvaluation.c0;
                    if (stones[0] == Player.None && stones[4] == Player.WhitePlayer) return OneDirectionEvaluation.c0;
                    break;
            }

            return OneDirectionEvaluation.nothing;
        }

        OneDirectionEvaluation GetShift2(Player[] stones)
        {
            int nbBlack = 0;
            int nbWhite = 0;

            //count numbers of stones
            for (int index = 1; index <= 3; index++)
            {
                switch (stones[index])
                {
                    case Player.BlackPlayer:
                        nbBlack++;
                        break;

                    case Player.WhitePlayer:
                        nbWhite++;
                        break;
                }
            }

            if (nbWhite > 0) return OneDirectionEvaluation.nothing;

            if (stones[0] == Player.WhitePlayer && stones[4] == Player.WhitePlayer) return OneDirectionEvaluation.nothing;

            switch (nbBlack)
            {
                case 3:
                    if (stones[0] == Player.BlackPlayer && stones[4] == Player.BlackPlayer) return OneDirectionEvaluation.overline;
                    if (stones[0] == Player.None && stones[4] == Player.None) return OneDirectionEvaluation.o3;
                    if (stones[0] == Player.BlackPlayer) return OneDirectionEvaluation.c4;
                    if (stones[4] == Player.BlackPlayer) return OneDirectionEvaluation.c4;
                    if (stones[0] == Player.WhitePlayer) return OneDirectionEvaluation.c3;
                    if (stones[4] == Player.WhitePlayer) return OneDirectionEvaluation.c3;
                    break;

                case 2:
                    if (stones[0] == Player.None && stones[4] == Player.None && stones[1] == Player.BlackPlayer && stones[2] == Player.BlackPlayer) return OneDirectionEvaluation.o2p;
                    if (stones[0] == Player.None && stones[4] == Player.None && stones[2] == Player.BlackPlayer && stones[3] == Player.BlackPlayer) return OneDirectionEvaluation.o2p;
                    if (stones[0] == Player.None && stones[4] == Player.None && stones[2] == Player.None) return OneDirectionEvaluation.o2;
                    if (stones[0] == Player.BlackPlayer) return OneDirectionEvaluation.c3;
                    if (stones[4] == Player.BlackPlayer) return OneDirectionEvaluation.c3;
                    if (stones[0] == Player.WhitePlayer && stones[4] == Player.None) return OneDirectionEvaluation.c2;
                    if (stones[0] == Player.None && stones[4] == Player.WhitePlayer) return OneDirectionEvaluation.c2;
                    break;

                case 1:
                    if (stones[0] == Player.None && stones[4] == Player.None && stones[2] == Player.BlackPlayer) return OneDirectionEvaluation.o1p;
                    if (stones[0] == Player.None && stones[4] == Player.None && stones[3] == Player.BlackPlayer) return OneDirectionEvaluation.o1p;
                    if (stones[0] == Player.None && stones[4] == Player.None && stones[1] == Player.BlackPlayer) return OneDirectionEvaluation.o1;

                    if (stones[0] == Player.BlackPlayer) return OneDirectionEvaluation.c2;
                    if (stones[4] == Player.BlackPlayer) return OneDirectionEvaluation.c2;
                    if (stones[0] == Player.WhitePlayer || stones[4] == Player.None) return OneDirectionEvaluation.c1;
                    if (stones[0] == Player.None || stones[4] == Player.WhitePlayer) return OneDirectionEvaluation.c1;
                    break;

                case 0:
                    if (stones[0] == Player.None && stones[4] == Player.None) return OneDirectionEvaluation.o0;
                    if (stones[0] == Player.BlackPlayer) return OneDirectionEvaluation.c1;
                    if (stones[4] == Player.BlackPlayer) return OneDirectionEvaluation.c1;
                    if (stones[0] == Player.WhitePlayer && stones[4] == Player.None) return OneDirectionEvaluation.c0;
                    if (stones[0] == Player.None && stones[4] == Player.WhitePlayer) return OneDirectionEvaluation.c0;
                    break;
            }

            return OneDirectionEvaluation.nothing;
        }

        #endregion

        //main evaluation function
        public OneDirectionData Modify(int square, Direction direction, int distance, Player symbol, Player playerOnMove)
        {
            //access data
            OneDirectionData actualData = oneDirectionData[square][(int)direction];
            
            //modify pattern
            switch (symbol)
            {
                case Player.None:
                    actualData.hash &= ~(3 << (distance << 1));
                    break;

                case Player.BlackPlayer:
                    actualData.hash |= (1 << (distance << 1));
                    break;

                case Player.WhitePlayer:
                    actualData.hash |= (2 << (distance << 1));
                    break;
            }

            /* get category, for both players */
            byte evaluation = evaluationTable[actualData.hash];

            //actualData.oldEvaluationBlack = actualData.evaluationBlack;
            actualData.evaluationBlack = (OneDirectionEvaluation)(evaluation & 0x0F);

            //actualData.oldEvaluationWhite = actualData.evaluationWhite;
            actualData.evaluationWhite = (OneDirectionEvaluation)(evaluation >> 4);

            return actualData;
        }


		public OneDirectionData[] GetDirectionData(int square)
		{
            OneDirectionData[] copy = new OneDirectionData[4];
            for (int direction = 0; direction < 4; direction++)
            {
                copy[direction] = oneDirectionData[square][direction];
            }
            return copy;
		}
	}
}
