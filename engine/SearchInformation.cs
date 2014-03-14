using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuEngine
{
    public class SearchInformation
    {
        public TimeSpan elapsedTime;
        public int examinedMoves;
        public int examinedVctMoves;
        public List<ABMove> possibleMoves;
        public float TtHits;
        //public float TtVctHits;
        public int evaluation;
        public int nbCutoffs;
        public bool vctActive;
        public int depth;
        public List<ABMove> principalVariation;
        public int deepestVctSearch;

        public SearchInformation()
        {
        	nbCutoffs = 0;
            examinedMoves = 0;
            examinedVctMoves = 0;
			elapsedTime = new TimeSpan(0);
            depth = 0;
            deepestVctSearch = 0;
        }

        //copy constructor
        public SearchInformation(SearchInformation searchInfo)
        {
            this.elapsedTime = searchInfo.elapsedTime;
            this.examinedMoves = searchInfo.examinedMoves;
            this.examinedVctMoves = searchInfo.examinedVctMoves;
            if (searchInfo.possibleMoves != null)
            {
                this.possibleMoves = new List<ABMove>(searchInfo.possibleMoves);
            }
            if (searchInfo.principalVariation != null)
            {
                this.principalVariation = new List<ABMove>(searchInfo.principalVariation);
            }
            this.TtHits = searchInfo.TtHits;
            //this.TtVctHits = searchInfo.TtVctHits;
            this.evaluation = searchInfo.evaluation;
            this.nbCutoffs = searchInfo.nbCutoffs;
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
	
		public string ExaminedMovesText
		{
			get
			{
				float vctShare = 0;
				if (examinedMoves > 0) vctShare = 100*examinedVctMoves / examinedMoves;				
					
				if (examinedMoves >= 2000)
            	{
                	return String.Format("{0}kN ({1:f0}%VCT)", examinedMoves / 1000, vctShare);
            	}
            	else
            	{
                	return String.Format("{0}N ({1:f0}%VCT)", examinedMoves, vctShare);
            	}
			}
		}
		
		public string MovesPerSecondText
		{
			get
			{
				return String.Format("{0:f1}kN/s", MovesPerSecond / 1000);
			}
		}
		
		public string CutoffsText
		{
			get
			{
				string str1;
				
				if (nbCutoffs > 2000)
	            {
	                str1 = String.Format("{0}k", nbCutoffs / 1000);
            	}
            	else
            	{
                	str1 = String.Format("{0}", nbCutoffs);
            	}

            	return str1;
			}
		}
	
    }

}
