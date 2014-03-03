using System;
using System.Collections.Generic;
using System.Text;



namespace GomokuEngine
{
    public enum FourDirectionsEvaluation : byte
    {
        overline,
        four,
    
        o3,
        c3xc3,
        s3,
        c3xo2,
        c3xc2,
        c3xo1,
        c3,
        
        o2xo2,
        o2xc2,
        o2xo1,
        o2p,
        o2,
        
        tripple1,
        double1,
        o1p,
        o1,
        c1,
        
        o0,
        c0,
        
        nothing,
        forbidden,
        occupied,
        unknown,
    };

    public class FourDirectionsData
    {
        public uint hash;
        public FourDirectionsEvaluation evaluation;
        //public bool modified;
    }

    /***********************************************************************
     *  This class gets square evaluation for one player
     ***********************************************************************/
    class EvaluateFourDirections
    {
        FourDirectionsEvaluation[] lookupTable;
        FourDirectionsData[] onePlayerData;

        public EvaluateFourDirections(int boardSize)
        {
            //compute number of squares of board
            int numberOfSquares = boardSize * boardSize;

            //initialize onePlayerData
            onePlayerData = new FourDirectionsData[numberOfSquares];
            uint nothing = (uint)OneDirectionEvaluation.valueless;
            nothing = nothing | nothing << 4 | nothing << 8 | nothing << 12;
            for (int square = 0; square < numberOfSquares; square++)
            {
                onePlayerData[square] = new FourDirectionsData();
                onePlayerData[square].hash = (UInt16)nothing;
            }

            lookupTable = new FourDirectionsEvaluation[0x10000];

            /* initialize squareLevelArray */
            for (byte lc3 = 0; lc3 < Enum.GetValues(typeof(OneDirectionEvaluation)).Length; lc3++)
            {
                for (byte lc2 = 0; lc2 < Enum.GetValues(typeof(OneDirectionEvaluation)).Length; lc2++)
                {
                    for (byte lc1 = 0; lc1 < Enum.GetValues(typeof(OneDirectionEvaluation)).Length; lc1++)
                    {
                        for (byte lc0 = 0; lc0 < Enum.GetValues(typeof(OneDirectionEvaluation)).Length; lc0++)
                        {
                            int index = lc0 | lc1 << 4 | lc2 << 8 | lc3 << 12;
                            lookupTable[index] = GetPlayerEvaluation((OneDirectionEvaluation)lc0, (OneDirectionEvaluation)lc1,
                                (OneDirectionEvaluation)lc2, (OneDirectionEvaluation)lc3);
                        }
                    }
                }
            }
        }

        FourDirectionsEvaluation GetPlayerEvaluation(OneDirectionEvaluation lc0, OneDirectionEvaluation lc1, 
            OneDirectionEvaluation lc2, OneDirectionEvaluation lc3)
        {
            byte[] nbLevels = new byte[Enum.GetValues(typeof(OneDirectionEvaluation)).Length];

            nbLevels[(byte)lc0]++;
            nbLevels[(byte)lc1]++;
            nbLevels[(byte)lc2]++;
            nbLevels[(byte)lc3]++;

            if (nbLevels[(byte)OneDirectionEvaluation.overline] > 0) return FourDirectionsEvaluation.overline;
            if (nbLevels[(byte)OneDirectionEvaluation.o4] > 0) return FourDirectionsEvaluation.four;
            if (nbLevels[(byte)OneDirectionEvaluation.c4] > 0) return FourDirectionsEvaluation.four;

            if (nbLevels[(byte)OneDirectionEvaluation.o3] > 0) return FourDirectionsEvaluation.o3;
            if (nbLevels[(byte)OneDirectionEvaluation.c3] + nbLevels[(byte)OneDirectionEvaluation.s3] > 1) return FourDirectionsEvaluation.c3xc3;
            if (nbLevels[(byte)OneDirectionEvaluation.s3] > 0) return FourDirectionsEvaluation.s3;
            if (nbLevels[(byte)OneDirectionEvaluation.c3] > 0 && nbLevels[(byte)OneDirectionEvaluation.o2p] + nbLevels[(byte)OneDirectionEvaluation.o2] > 0) return FourDirectionsEvaluation.c3xo2;
            if (nbLevels[(byte)OneDirectionEvaluation.c3] > 0 && nbLevels[(byte)OneDirectionEvaluation.c2] > 0) return FourDirectionsEvaluation.c3xc2;
            if (nbLevels[(byte)OneDirectionEvaluation.c3] > 0 && nbLevels[(byte)OneDirectionEvaluation.o1p] + nbLevels[(byte)OneDirectionEvaluation.o1] > 0) return FourDirectionsEvaluation.c3xo1;
            if (nbLevels[(byte)OneDirectionEvaluation.c3] > 0) return FourDirectionsEvaluation.c3;

            if (nbLevels[(byte)OneDirectionEvaluation.o2p] + nbLevels[(byte)OneDirectionEvaluation.o2] > 1) return FourDirectionsEvaluation.o2xo2;
            if (nbLevels[(byte)OneDirectionEvaluation.o2p] + nbLevels[(byte)OneDirectionEvaluation.o2] > 0 && nbLevels[(byte)OneDirectionEvaluation.c2] > 0) return FourDirectionsEvaluation.o2xc2;
            if (nbLevels[(byte)OneDirectionEvaluation.o2p] + nbLevels[(byte)OneDirectionEvaluation.o2] > 0 && nbLevels[(byte)OneDirectionEvaluation.o1p] + nbLevels[(byte)OneDirectionEvaluation.o1] > 0) return FourDirectionsEvaluation.o2xo1;
            if (nbLevels[(byte)OneDirectionEvaluation.o2p] > 0) return FourDirectionsEvaluation.o2p;
            if (nbLevels[(byte)OneDirectionEvaluation.o2] > 0) return FourDirectionsEvaluation.o2;

            if (nbLevels[(byte)OneDirectionEvaluation.c2] + nbLevels[(byte)OneDirectionEvaluation.o1p] + nbLevels[(byte)OneDirectionEvaluation.o1] > 2) return FourDirectionsEvaluation.tripple1;
            if (nbLevels[(byte)OneDirectionEvaluation.c2] + nbLevels[(byte)OneDirectionEvaluation.o1p] + nbLevels[(byte)OneDirectionEvaluation.o1] > 1) return FourDirectionsEvaluation.double1;
            if (nbLevels[(byte)OneDirectionEvaluation.c2] + nbLevels[(byte)OneDirectionEvaluation.o1p] > 0) return FourDirectionsEvaluation.o1p;
            if (nbLevels[(byte)OneDirectionEvaluation.c2] + nbLevels[(byte)OneDirectionEvaluation.o1p] + nbLevels[(byte)OneDirectionEvaluation.o1] > 0) return FourDirectionsEvaluation.o1;

            if (nbLevels[(byte)OneDirectionEvaluation.c1] > 0) return FourDirectionsEvaluation.c1;

            if (nbLevels[(byte)OneDirectionEvaluation.o0] == 4) return FourDirectionsEvaluation.o0;
            if (nbLevels[(byte)OneDirectionEvaluation.o0] > 0) return FourDirectionsEvaluation.c0;
            if (nbLevels[(byte)OneDirectionEvaluation.c0] > 0) return FourDirectionsEvaluation.c0;

            return FourDirectionsEvaluation.nothing;
        }

        //return black evaluation
        public FourDirectionsData Modify(int square, Direction direction, OneDirectionEvaluation directionEvaluation, out bool changed)
        {
            FourDirectionsData actualData = onePlayerData[square];
            
            int shift = (int)direction << 2;
            uint mask = (uint)(0x000F << shift);

            //uint hash = actualData.hash; //copy of hash  

            /* clear bits */
            actualData.hash &= ~mask;
            /* set bits */
            actualData.hash |= (uint)directionEvaluation << shift;
			FourDirectionsEvaluation evaluation = lookupTable[actualData.hash];

            if (evaluation != actualData.evaluation) 
            {
            	actualData.evaluation = evaluation;
            	changed = true;
            }
            else
            {
            	changed = false;
            }
           	return actualData;
        }

        public FourDirectionsEvaluation GetEvaluation(int square)
        {
            return onePlayerData[square].evaluation;
        }
    }
}
