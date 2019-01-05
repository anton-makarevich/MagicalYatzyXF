namespace Sanet.MagicalYatzy.Models.Game.DieResultExtensions
{
    public static class YatzyRules
    {
        //the score for the numeric 1-6 categories in Y
        public static int KniffelNumberScore(this DieResult result, int number)
        {
            var iTot = 0;

            foreach (int i in result.DiceResults)
            {
                if (i == number)
                {
                    iTot += number;
                }
            }
            return iTot;

        }

        public static int KniffelOfAKindScore(this DieResult result, int count)
        {
            int[] iOccur = new int[7];
            foreach (int res in result.DiceResults)
            {
                iOccur[res] += 1;
            }

            for (int i = 0; i <= 6; i++)
            {
                if (iOccur[i] >= count)
                {
                    return result.Total;
                }
            }
            return 0;
        }

        public static int KniffelFiveOfAKindScore(this DieResult result)
        {

            const int SCORE = 50;
            int[] iOccur = new int[7];
            
            foreach (int res in result.DiceResults)
            {
                iOccur[res] += 1;
            }

            for (int i = 0; i <= 6; i++)
            {
                if (iOccur[i] >= 5)
                {
                    return SCORE;
                }
            }
            return 0;
        }

        public static int KniffelChanceScore(this DieResult result)
        {
            return result.Total;
        }

        public static int XInRow(this DieResult result, ref int count)
        {
            
            int[] iOccur = new int[7];
            count = 3;
            foreach (int res in result.DiceResults)
            {
                iOccur[res] += 1;
            }
            for (int i = 1;i<5;i++)
                if (iOccur[i] >= 1 & iOccur[i+1] >= 1 & iOccur[i+2] >= 1)
                {
                    if (i < 4 && iOccur[i + 3] >= 1)
                        count = 4;
                    return i;

                }

            return 0;
        }

        public static int KniffelSmallStraightScore(this DieResult result/*bool ToFix, ref bool Fixed, int n = 3*/)
        {
            bool[] Fr = {
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };
            const int SCORE = 30;
            int[] iOccur = new int[7];
            int MinNum = 0;
            
            foreach (int res in result.DiceResults)
            {
                iOccur[res] += 1;
            }

            if (iOccur[1] >= 1 & iOccur[2] >= 1 & iOccur[3] >= 1 & iOccur[4] >= 1)
            {
                MinNum = 1;
            }

            if (iOccur[2] >= 1 & iOccur[3] >= 1 & iOccur[4] >= 1 & iOccur[5] >= 1)
            {
                MinNum = 2;
            }

            if (iOccur[3] >= 1 & iOccur[4] >= 1 & iOccur[5] >= 1 & iOccur[6] >= 1)
            {
                MinNum = 3;

            }
            if (!(MinNum == 0))
            {
                //if (ToFix)
                //{
                //    Fixed = true;
                //    for (i = MinNum; i <= MinNum + n; i++)
                //    {
                //        foreach (Die d_loopVariable in aDice)
                //        {
                //            d = d_loopVariable;

                //            if (d.Result == i & i < 7)
                //            {
                //                if (!Fr[i])
                //                {
                //                    d.Frozen = true;
                //                    Fr[i] = true;

                //                }
                //            }
                //        }
                //    }
                //}
                return SCORE;
            }
            return 0;
        }

        public static int KniffelLargeStraightScore(this DieResult result)
        {

            const int SCORE = 40;
            int[] iOccur = new int[7];

            foreach (int res in result.DiceResults)
            {
                iOccur[res] += 1;
            }

            if (iOccur[1] > 0 & iOccur[2] > 0 & iOccur[3] > 0 & iOccur[4] > 0 & iOccur[5] > 0)
            {
                return SCORE;
            }

            if (iOccur[2] > 0 & iOccur[3] > 0 & iOccur[4] > 0 & iOccur[5] > 0 & iOccur[6] > 0)
            {
                return SCORE;
            }
            return 0;
        }

        public static int KniffelFullHouseScore(this DieResult result)
        {

            const int SCORE = 25;
            int[] iOccur = new int[7];
            
            bool bPair = false;
            bool bTrip = false;


            foreach (int res in result.DiceResults)
            {
                iOccur[res] += 1;
            }

            for (int i = 0; i <= 6; i++)
            {
                if (iOccur[i] == 2)
                {
                    bPair = true;
                }
                else if (iOccur[i] == 3)
                {
                    bTrip = true;
                }
            }

            if (bPair & bTrip)
            {
                return SCORE;
            }
            return 0;
        }

        public static int NumPairs(this DieResult result)
        {

            int[] iOccur = new int[7];
            int bPair = 0;

            foreach (int res in result.DiceResults)
            {
                iOccur[res] += 1;
            }

            for (int i = 0; i <= 6; i++)
            {
                if (iOccur[i] > 1)
                {
                    bPair++;
                }
                //if (iOccur[i] > 3)
                //{
                //    bPair++;
                //}
                //if (iOccur[i] > 5)
                //{
                //    bPair++;
                //}
            }


            return bPair;
        }
    }
}