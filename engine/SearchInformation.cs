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
        public float TThits;
        public float TTVCThits;
        //public Player winner;
        public int evaluation;
        public int nbCutoffs;
        public int nbVCTCutoffs;
        //public int reachedDepth;
        public bool vctActive;
        public int depth;
        public List<ABMove> principalVariation;
        public int deepestVctSearch;

        public SearchInformation()
        {
        	nbCutoffs = 0;
            nbVCTCutoffs = 0;
            examinedMoves = 0;
			elapsedTime = new TimeSpan(0);
			//winner = Player.None;
            depth = 0;
            deepestVctSearch = 0;
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
            if (searchInfo.principalVariation != null)
            {
                this.principalVariation = new List<ABMove>(searchInfo.principalVariation);
            }
            this.TThits = searchInfo.TThits;
            this.TTVCThits = searchInfo.TTVCThits;
            this.evaluation = searchInfo.evaluation;
            this.nbCutoffs = searchInfo.nbCutoffs;
            this.nbVCTCutoffs = searchInfo.nbVCTCutoffs;
            this.vctActive = searchInfo.vctActive;
            this.depth = searchInfo.depth;
            this.deepestVctSearch = searchInfo.deepestVctSearch;
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
        
        public string PrincipalVariationText
        {
        	get
        	{
		    	string pv = "";
		    	if (principalVariation != null)
		    	{
		        	foreach (ABMove move in this.principalVariation)
		        	{
		        		if (pv == "")
		        		{
		        			pv = move.ToString();
		        		}
		        		else
		        		{
		        			pv = pv + "/" + move;
		        		}
		        	}
		    	}
		        return pv;
        	}
        }

	public string EvaluationText
	{
		get
		{
            return EvaluationConstants.Score2Text(evaluation);
		}
	}
    }

}
