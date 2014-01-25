using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GomokuEngine
{
    public class TuningInfo
    {
        public int[] scoreEvaluation;

        public TuningInfo(int[] scoreEvaluation)
        {
            this.scoreEvaluation = new int[scoreEvaluation.Length];
            for (int index1 = 0; index1 < scoreEvaluation.Length; index1++)
            {
                this.scoreEvaluation[index1] = scoreEvaluation[index1];
            }
        }

        /* copy constructor */
        public TuningInfo(TuningInfo original)
        {
            scoreEvaluation = new int[original.scoreEvaluation.Length];
            for (int index1 = 0; index1 < original.scoreEvaluation.Length; index1++)
            {
                scoreEvaluation[index1] = original.scoreEvaluation[index1];
            }
        }
    }

}
