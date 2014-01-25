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
        public int reachedDepth;
        public bool vctActive;
        //public int examinedMoves;

        public SearchInformation()
        {
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
            //this.bestLine = new List<ABMove>();
            this.bestMove = searchInfo.bestMove;
            this.TThits = searchInfo.TThits;
            this.winner = searchInfo.winner;
            this.evaluation = searchInfo.evaluation;
            this.nbCutoffs = searchInfo.nbCutoffs;
            this.nbVCTCutoffs = searchInfo.nbVCTCutoffs;
            this.reachedDepth = searchInfo.reachedDepth;
            this.vctActive = searchInfo.vctActive;
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
