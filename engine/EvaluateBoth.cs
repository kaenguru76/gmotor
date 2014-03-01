using System;

namespace GomokuEngine
{
    public enum BothPlayerEvaluation : byte
    {
    	overline_defending,
        four_attacking,
        four_defending,
        o3_attacking,
        o3_defending,
        s3_attacking,
        s3_defending,
        c3xc3_attacking,
        c3xc3_defending,
        c3xo2_attacking,
        c3xo2_defending,
        c3xo1_attacking,
        c3xo1_defending,
        c3_attacking,
        c3_defending,
        o2xo2_attacking,
        o2xo2_defending,
        vct_attacking,
        vct_defending,
        o2p_attacking,
        o2_attacking,
        o2p_defending,
        o2_defending,
        tripple1_attacking,
        tripple1_defending,
        double1_attacking,
        double1_defending,
        o1_both,
        o1p_attacking,
        o1_attacking,
        o1p_defending,
        o1_defending,
        rest,
        forbidden,
        occupied,
        unknown,
    };

    public class BothData
    {
        public int hash;
        public BothPlayerEvaluation evaluationBlackOnMove;
        public BothPlayerEvaluation evaluationWhiteOnMove;
    }

    class EvaluateBoth
    {
        BothPlayerEvaluation[] lookupTableBlack;
        BothPlayerEvaluation[] lookupTableWhite;
        int boardSize;
        BothData[] bothData;

        public EvaluateBoth(int boardSize)
        {
            this.boardSize = boardSize;

            //compute number of squares of board
            int numberOfSquares = boardSize * boardSize;

            bothData = new BothData[numberOfSquares];
            for (int square = 0; square < numberOfSquares; square++)
            {
                bothData[square] = new BothData();
                bothData[square].hash = -1;
            }


            InitializeEvaluationTable();
        }

        void InitializeEvaluationTable()
        {
            int nbPlayerEvaluations = Enum.GetValues(typeof(FourDirectionsEvaluation)).Length;

            lookupTableBlack = new BothPlayerEvaluation[0x400]; //2^5 * 2^5 = 32 * 32 = 1024 entries
            lookupTableWhite = new BothPlayerEvaluation[0x400]; //2^5 * 2^5 = 32 * 32 = 1024 entries

            /* initialize evaluationArray */
            for (int evaluationBlack = 0; evaluationBlack < nbPlayerEvaluations; evaluationBlack++)
            {
                for (int evaluationWhite = 0; evaluationWhite < nbPlayerEvaluations; evaluationWhite++)
                {
                    int index = evaluationBlack << 5 | evaluationWhite;
                    lookupTableBlack[index] = GetBothPlayerEvaluation((FourDirectionsEvaluation)evaluationBlack, (FourDirectionsEvaluation)evaluationWhite);
                    lookupTableWhite[index] = GetBothPlayerEvaluation((FourDirectionsEvaluation)evaluationWhite, (FourDirectionsEvaluation)evaluationBlack);
                }
            }

        }

         BothPlayerEvaluation GetBothPlayerEvaluation(FourDirectionsEvaluation attacker, FourDirectionsEvaluation defender)
        {
            if (attacker <= FourDirectionsEvaluation.overline) return BothPlayerEvaluation.forbidden;
            if (defender <= FourDirectionsEvaluation.overline) return BothPlayerEvaluation.overline_defending;

            if (attacker <= FourDirectionsEvaluation.four) return BothPlayerEvaluation.four_attacking;
            if (defender <= FourDirectionsEvaluation.four) return BothPlayerEvaluation.four_defending;

            if (attacker <= FourDirectionsEvaluation.o3) return BothPlayerEvaluation.o3_attacking;
            if (attacker <= FourDirectionsEvaluation.c3xc3) return BothPlayerEvaluation.c3xc3_attacking;
            if (attacker <= FourDirectionsEvaluation.c3xo2) return BothPlayerEvaluation.c3xo2_attacking;
            if (attacker <= FourDirectionsEvaluation.c3xo1) return BothPlayerEvaluation.c3xo1_attacking;
            if (attacker <= FourDirectionsEvaluation.s3) return BothPlayerEvaluation.s3_attacking;
            if (attacker <= FourDirectionsEvaluation.c3) return BothPlayerEvaluation.c3_attacking;

            if (defender <= FourDirectionsEvaluation.o3) return BothPlayerEvaluation.o3_defending;
            if (defender <= FourDirectionsEvaluation.c3xc3) return BothPlayerEvaluation.c3xc3_defending;
            if (defender <= FourDirectionsEvaluation.c3xo2) return BothPlayerEvaluation.c3xo2_defending;
            if (defender <= FourDirectionsEvaluation.s3) return BothPlayerEvaluation.s3_defending;

            if (attacker <= FourDirectionsEvaluation.o2xo2) return BothPlayerEvaluation.o2xo2_attacking;
            if (attacker <= FourDirectionsEvaluation.o2xo1) return BothPlayerEvaluation.vct_attacking;
            if (defender <= FourDirectionsEvaluation.c3xo1) return BothPlayerEvaluation.c3xo1_defending;
            if (attacker <= FourDirectionsEvaluation.o2p) return BothPlayerEvaluation.o2p_attacking;
            if (attacker <= FourDirectionsEvaluation.o2) return BothPlayerEvaluation.o2_attacking;

            if (defender <= FourDirectionsEvaluation.c3) return BothPlayerEvaluation.c3_defending;

            if (defender <= FourDirectionsEvaluation.o2xo2) return BothPlayerEvaluation.o2xo2_defending;
            if (defender <= FourDirectionsEvaluation.o2xo1) return BothPlayerEvaluation.vct_defending;
            if (defender <= FourDirectionsEvaluation.o2p) return BothPlayerEvaluation.o2p_defending;
            if (defender <= FourDirectionsEvaluation.o2) return BothPlayerEvaluation.o2_defending;

            if (attacker <= FourDirectionsEvaluation.tripple1) return BothPlayerEvaluation.tripple1_attacking;
            if (defender <= FourDirectionsEvaluation.tripple1) return BothPlayerEvaluation.tripple1_defending;

            if (attacker <= FourDirectionsEvaluation.double1) return BothPlayerEvaluation.double1_attacking;
            if (defender <= FourDirectionsEvaluation.double1) return BothPlayerEvaluation.double1_defending;

            if (attacker <= FourDirectionsEvaluation.o1 && defender <= FourDirectionsEvaluation.o1) return BothPlayerEvaluation.o1_both;
            if (attacker <= FourDirectionsEvaluation.o1p) return BothPlayerEvaluation.o1p_attacking;
            if (attacker <= FourDirectionsEvaluation.o1) return BothPlayerEvaluation.o1_attacking;
            if (defender <= FourDirectionsEvaluation.o1p) return BothPlayerEvaluation.o1p_defending;
            if (defender <= FourDirectionsEvaluation.o1) return BothPlayerEvaluation.o1_defending;

            return BothPlayerEvaluation.rest;
        }

        public BothData Modify(int square, FourDirectionsEvaluation evaluationBlack, FourDirectionsEvaluation evaluationWhite)
        {
            // get hash code
            int hash = (int)evaluationBlack << 5 | (int)evaluationWhite;

            //test if something changed
            BothData actualData = bothData[square];
            if (actualData.hash == hash) return actualData;
            actualData.hash = hash;

            actualData.evaluationBlackOnMove = lookupTableBlack[hash];
            actualData.evaluationWhiteOnMove = lookupTableWhite[hash];

            return actualData;
        }

    }
}
