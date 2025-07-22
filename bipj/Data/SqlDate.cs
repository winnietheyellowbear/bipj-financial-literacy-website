using System;
using System.Data.SqlTypes;

namespace bipj.Data
{
    internal static class SqlDate
    {
        static readonly DateTime Min = SqlDateTime.MinValue.Value;
        static readonly DateTime Max = SqlDateTime.MaxValue.Value;

        internal static void Clamp(ref DateTime? from, ref DateTime? to)
        {
            if (from.HasValue && from.Value < Min) from = Min;
            if (to.HasValue && to.Value > Max) to = Max;
        }
    }
}