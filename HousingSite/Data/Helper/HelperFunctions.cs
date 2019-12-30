using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSite.Data.Helper
{
    public static class HelperFunctions
    {
        public static int FindSmallestAboveMinimum(this List<int> list, int minimum)
        {
            if (list.Count < 1) { return 0; }

            int smallest = list[0];

            for(int i=0; i< list.Count; i++)
            {
                if(list[i] >= minimum)
                {
                    if (list[i] < smallest || smallest < minimum)
                    {
                        smallest = list[i];
                    }
                }
            }

            return smallest;
        }

        public static int FindBiggestBelowMaximum(this List<int> list, int maximum)
        {
            if (list.Count < 1) { return 0; }

            int biggest = list[0];

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] <= maximum)
                {
                    if(list[i] > biggest || biggest > maximum)
                    {
                        biggest = list[i];
                    }                    
                }
            }

            return biggest;
        }

        //public static T Average<T>(this List<T> list)
        //{
        //    if (!IsNumber<T>()) { return default; }
        //    if (list.Count == 0) { return default; }

        //    dynamic sum = list[0];

        //    if (list.Count > 1)
        //    {
        //        for (int element = 1; element < list.Count; element++)
        //        {
        //            sum += (dynamic)list[element];
        //        }
        //    }

        //    return (T)(sum / list.Count);
        //}
    }
}
