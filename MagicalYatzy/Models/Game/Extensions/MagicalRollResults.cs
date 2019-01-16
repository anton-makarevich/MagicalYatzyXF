using System;

namespace Sanet.MagicalYatzy.Models.Game.Extensions
{
    public static class MagicalRollResults
    {
        public static int[] GetMagicResults(this Scores hands)
        {
            var rollResults = new int[5];
            var rand = new Random();
            int firstResult;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (hands)
            {
                case Scores.ThreeOfAKind:
                    rollResults[0] = rollResults[1] = rollResults[2] = rand.Next(1, 7);
                    rollResults[3]= rand.Next(1, 7);
                    rollResults[4]= rand.Next(1, 7);
                    break;
                case Scores.FourOfAKind:
                    rollResults[0] = rollResults[1] = rollResults[2] = rollResults[3] =rand.Next(1, 7);
                    rollResults[4] = rand.Next(1, 7);
                    break;
                case Scores.FullHouse:
                    rollResults[0] = rollResults[1] = rollResults[2] = rand.Next(1, 4);
                    rollResults[3] = rollResults[4] = rand.Next(4, 7);
                    break;
                case Scores.SmallStraight:
                    firstResult = rand.Next(1,4);
                    for (var i = 0; i < 4; i++)
                        rollResults[i] = firstResult + i;
                    rollResults[4] = rand.Next(1, 7);
                    break;
                case Scores.LargeStraight:
                    firstResult = rand.Next(1,3);
                    for (var i = 0; i < 5; i++)
                        rollResults[i] = firstResult + i;
                    break;
                case Scores.Kniffel:
                    firstResult = rand.Next(1, 7);
                    for (var i = 0; i < 5; i++)
                        rollResults[i] = firstResult;
                    break;
            }

            return rollResults;
        }
    }
}