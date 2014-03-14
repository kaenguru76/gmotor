//#define CHECK 

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace GomokuEngine
{

    class VCT
    {
        bool[][] vctArray;
        bool[] actualBoard;
        int[] vctLine;
        Player vctPlayer;
        int vctLength;
        bool vctActive;
        bool locked;

        public VCT(int boardSize)
        {
            //compute number of squares of board
            int numberOfSquares = boardSize * boardSize;
            int numberOfPlies = numberOfSquares;

            //initialize threatArray
            vctArray = new bool[numberOfPlies][];
            for (int ply = 0; ply < numberOfPlies; ply++)
            {
                vctArray[ply] = new bool[numberOfSquares];

                for (int square = 0; square < numberOfSquares; square++)
                {
                    vctArray[ply][square] = false;
                }
            }

            vctPlayer = Player.None;
            vctLength = 0;

            actualBoard = vctArray[vctLength];
            for (int i = 0; i < actualBoard.Length; i++)
            {
                actualBoard[i] = true;
            }

            vctLine = new int[numberOfPlies];
            vctActive = false;
            locked = true;
        }

        public bool CreatesVct(int square)
        {
            if (actualBoard[square] == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void MakeMove(int square, Player player)
        {
            if (player == vctPlayer)
            {
                if (CreatesVct(square))
                {
                    vctLine[vctLength++] = square;
                    actualBoard = vctArray[vctLength];
#if CHECK
                    for (int i = 0; i < actualBoard.Length; i++)
                    {
                        Debug.Assert(actualBoard[i] == false, "Not allowed VCT found! " + vctLength);
                    }
#endif
                    locked = false;
                }
            }
        }

        public void UndoMove(int square)
        {
            if (vctLength == 0)
            {
                vctPlayer = Player.None;
                vctActive = false;
            }

            if (vctLength > 0 && vctLine[vctLength-1] == square)
            {
#if CHECK
                for (int i = 0; i < actualBoard.Length; i++)
                {
                    Debug.Assert(actualBoard[i] == false, "Not allowed VCT found! " + vctLength);
                }
#endif
                vctLength--;
                actualBoard = vctArray[vctLength];
                locked = true;
            }
        }

        public int GainSquare
        {
            get
            {
                if (vctLength > 0)
                {
                    return vctLine[vctLength - 1];
                }
                else
                {
                    return -1;
                }
            }
        }

        public void AddVct(int square)
        {
            if (vctLength > 0 && locked == false)
            {
                actualBoard[square] = true;
            }
        }

        public void RemoveVct(int square)
        {
            if (vctLength > 0 && locked == false)
            {
                actualBoard[square] = false;
            }
        }

        public bool Locked
        {
            set
            {
                locked = value;
            }

            get
            {
                return locked;
            }
        }

        public Player VctPlayer
        {
            set
            {
                if (value == Player.None)
                {
                    //stop VCT
                    if (vctLength == 0)
                    {
                        vctPlayer = value;
                        vctActive = false;
                    }
                }
                else
                {
                    //start VCT
                    if (vctLength == 0)
                    {
                        vctPlayer = value;
                        vctActive = true;
                    }
                }
            }

            get
            {
                return vctPlayer;
            }
        }

        public bool VctActive
        {
            get
            {
                Debug.Assert(vctActive == (vctPlayer == Player.None) ? false : true);

                return vctActive; 
            }
        }

        public int VctLength
        {
            get
            {
                return vctLength;
            }
        }

        public bool VctDepth0
        {
            get
            {
            	if (vctLength == 0 && vctActive) 
            		return true;
            	else
                	return false;
            }
        }

  #if CHECK
        void CheckVCT()
        {
            if (vctLength == 0)
            {
                //for debugging purposes only!
                for (int i = 0; i < actualBoard.Length; i++)
                {
                    Debug.Assert(vctArray[1][i] == false, "Not allowed VCT found! " + vctLength);
                }
            }
        }
#endif

    }
}
