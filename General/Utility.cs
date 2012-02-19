using System.Collections.Generic;

namespace XF
{
    public static class Utility
    {
        /////////////////////////////////////////////////////////////////////////////////////

        #region General helpers
        /// <summary>Receives a list of evenly typed parameters, and RETURNS one of them AT RANDOM</summary>        
        public static T select_one<T>(params T[] plist)
        {
            if (plist.Length == 0) return default(T);
            return plist[Random.range(0, plist.Length - 1)];
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Array extensions
        public static bool ArrayContains<T>(this T[] array, T obj) where T : class
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == obj) return true;
            }
            return false;
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region IList extensions for shuffling, removing a random object, etc.
        public static void Shuffle<T>(this IList<T> list)
        {
            var rng = new Components.MersenneTwister();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list.Count == 0) return default(T);
            return list[Random.range(0, list.Count - 1)];
        }
        public static void RemoveRandom<T>(this IList<T> list)
        {
            if (list.Count == 0) return;
            list.RemoveAt(Random.range(0, list.Count - 1));
        }
        public static T HatPick<T>(this IList<T> list)
        {
            if (list.Count == 0) return default(T);
            var id = Random.range(0, list.Count - 1);
            T item = list[id];
            list.RemoveAt(id);
            return item;
        }
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Generic minimum and maximum
        public static T min<T>(params T[] param_list) where T : System.IComparable
        {
            if (param_list.Length == 0) return default(T);
            T current_minimum = param_list[0];
            for (int i = 1; i < param_list.Length; i++)
                if (param_list[i].CompareTo(current_minimum) < 0) current_minimum = param_list[i];

            return current_minimum;
        }

        public static T max<T>(params T[] param_list) where T : System.IComparable
        {
            if (param_list.Length == 0) return default(T);
            T current_maximum = param_list[0];
            for (int i = 1; i < param_list.Length; i++)
                if (param_list[i].CompareTo(current_maximum) > 0) current_maximum = param_list[i];

            return current_maximum;
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region min and max for any set of float parameters

        public static float min(params float[] param_list)
        {
            var min = float.MaxValue;
            for (int i = 0; i < param_list.Length; i++) if (param_list[i] < min) min = param_list[i];
            return min;
        }

        public static float max(params float[] param_list)
        {
            var max = float.MinValue;
            for (int i = 0; i < param_list.Length; i++) if (param_list[i] > max) max = param_list[i];
            return max;
        } 
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

        #region Container class to store a value type by reference
        // suggested usage assuming int x is defined : Ref<int> rsi = new Ref<int>(()=>x,v=>{x=v;});
        public class Ref<T> where T : struct
        {
            private System.Func<T>      getter;
            private System.Action<T>    setter;
            public Ref(System.Func<T> getter, System.Action<T> setter)
            {
                this.getter = getter;
                this.setter = setter;
            }
            public T value
            {
                get { return getter(); }
                set { setter(value);  }
            }
        }        

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////

    }
}
