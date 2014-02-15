using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuEngine
{
    public class SearchInformation
    {
        public TimeSpan elapsedTime;
        public int examinedMoves;
        public List<ABMove> possibleMoves;
        public ABMove bestMove;
        public float TThits;
        public Player winner;
        public int evaluation;
        public int nbCutoffs;
        public int nbVCTCutoffs;
        //public int reachedDepth;
        public bool vctActive;
        public int depth;
        public List<int> principalVariation;

        public SearchInformation()
        {
        	nbCutoffs = 0;
            nbVCTCutoffs = 0;
            examinedMoves = 0;
			elapsedTime = new TimeSpan(0);
			winner = Player.None;
            bestMove = null;
            depth = 0;
        }

        //copy constructor
        public SearchInformation(SearchInformation searchInfo)
        {
            this.elapsedTime = searchInfo.elapsedTime;
            this.examinedMoves = searchInfo.examinedMoves;
            if (searchInfo.possibleMoves != null)
            {
                this.possibleMoves = new List<ABMove>(searchInfo.possibleMoves);
            }
            this.bestMove = searchInfo.bestMove;
            this.TThits = searchInfo.TThits;
            this.winner = searchInfo.winner;
            this.evaluation = searchInfo.evaluation;
            this.nbCutoffs = searchInfo.nbCutoffs;
            this.nbVCTCutoffs = searchInfo.nbVCTCutoffs;
            this.vctActive = searchInfo.vctActive;
            this.depth = searchInfo.depth;
        }

        public double MovesPerSecond
        {
            get
            {
                if (elapsedTime.TotalSeconds > 0)
                {
                    return examinedMoves / elapsedTime.TotalSeconds;
                }
                else
                {
                    return 0;
                }
            }
        }

    }

}
