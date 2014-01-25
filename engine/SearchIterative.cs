using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    class SearchIterative
    {
        int actualDepth;
        Search search;

        public SearchIterative(int depth, List<Square> moves, int boardSize)
        {
            // iterative deepening loop 
            for (actualDepth = 0; actualDepth <= depth; actualDepth++)
            {
                search = new Search(actualDepth, moves, boardSize);
            }
        }
    }
}
