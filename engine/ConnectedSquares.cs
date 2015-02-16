using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuEngine
{
    public class ConnectedSquare
    {
        public int square;
        public Direction direction;
        public int distance;

        public ConnectedSquare(int square, Direction direction, int distance)
        {
            this.square = square;
            this.direction = direction;
            this.distance = distance;
        }

//        public override string ToString()
//        {
//            //Conversions conversions = new Conversions(20);
//            //return conversions.Complete(square);
//        }
    }

    class ConnectedSquares
    {
        List <ConnectedSquare>[] connectedSquares;
        int boardSize;

        public ConnectedSquares(int boardSize)
        {
            this.boardSize = boardSize;
            int numberOfSquares = boardSize * boardSize;

            //influenced squares
            connectedSquares = new List<ConnectedSquare>[numberOfSquares];

            //initialize influenced squares
            for (int square = 0; square < numberOfSquares; square++)
            {
                connectedSquares[square] = new List<ConnectedSquare>();
            }

            #region initialize 

            //Conversions conversions = new Conversions(boardSize);

            for (int row = 0; row < boardSize; row++)
            {
                for (int column = 0; column < boardSize; column++)
                {
                	BoardSquare square = new BoardSquare(boardSize, row, column);
                    //int square = conversions.RowAndColumn2Index(row, column);

                    for (Direction direction = Direction.left2right; direction <= Direction.rightdown2leftup; direction++)
                    {
                        for (int distance = -5; distance <= 5; distance++)
                        {
                            if (distance == 0) continue;

                            int influencedRow = row;
                            int influencedColumn = column;

                            switch (direction)
                            {
                                case Direction.left2right:
                                    influencedColumn += distance;
                                    break;
                                case Direction.leftdown2rightup:
                                    influencedRow += distance;
                                    influencedColumn += distance;
                                    break;
                                case Direction.down2up:
                                    influencedRow += distance;
                                    break;
                                case Direction.rightdown2leftup:
                                    influencedRow += distance;
                                    influencedColumn -= distance;
                                    break;
                            }
                            //check if square is on board
                            if (influencedRow >= 0 && influencedRow < boardSize && influencedColumn >= 0 && influencedColumn < boardSize)
                            {
                            	BoardSquare influencedSquare = new BoardSquare(boardSize, influencedRow, influencedColumn);
                                //int influencedSquare = conversions.RowAndColumn2Index(influencedRow, influencedColumn);

                                //add to influenced squares
                                connectedSquares[square.Index].Add (new ConnectedSquare(influencedSquare.Index, direction, distance < 0 ? 4 - distance : 5 - distance));
                            }
                        }
                    }
                }
            }
            #endregion
        }

        public List<ConnectedSquare>  GetConnectedSquares(int square)
        {
            return connectedSquares[square];
        }
    }
}
