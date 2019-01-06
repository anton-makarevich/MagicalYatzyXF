namespace Sanet.MagicalYatzy.Models.Game.DieResultExtensions
{
    public static class AiHelpers
    {
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
    }
}