using System;
using System.Linq;

namespace Skuld.Bot.Extensions
{
    public static class NumberTools
    {
        public static bool CheckRecurring(this long value)
        {
            string temp = Convert.ToString(value);

            bool res = false;

            for (int x = 1; x < 10; x++)
            {
                bool local = temp.All(z => Convert.ToInt64(z) == x);

                if (local == true)
                {
                    res = true;
                    break;
                }
            }

            return res;
        }
    }
}