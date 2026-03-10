using System.Collections;
using System.Collections.Generic;

public static class Util
{
    public static void Shuffle<T>(IList<T> list, IRandomProvider random)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
