using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class CastHelper
    {
        public static T Cast<T>(this object obj) => (T)obj;

        public static T TryCast<T>(this object obj, T defaultValue)
        {
            if (obj is T v)
                return v;
            return defaultValue;
        }

        public static T TryCast<T>(this object obj) => TryCast(obj, default(T));
    }
}
