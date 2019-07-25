using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.ActivityInsights
{
    internal static class RandomXorShift
    {
        private static Int32 s_state32;
        private static Int64 s_state64;

        static RandomXorShift()
        {
            s_state32 = Environment.TickCount;
            s_state64 = DateTimeOffset.Now.ToFileTime();

            Next32();
            Next64();
        }


        public static Int32 Next32()
        {
            unchecked
            {
                while (true)
                {
                    Int32 val = Volatile.Read(ref s_state32);
                    Int32 orig = val;

                    val ^= val << 13;
                    val ^= val >> 17;
                    val ^= val << 5;

                    Int32 prev = Interlocked.CompareExchange(ref s_state32, val, orig);
                    if (prev == orig)
                    {
                        return val;
                    }
                }
            }
        }

        public static Int64 Next64()
        {
            unchecked
            {
                while (true)
                {
                    Int64 val = Interlocked.Read(ref s_state64);
                    Int64 orig = val;

                    val ^= val << 13;
                    val ^= val >> 7;
                    val ^= val << 17;

                    Int64 prev = Interlocked.CompareExchange(ref s_state64, val, orig);
                    if (prev == orig)
                    {
                        return val;
                    }
                }
            }
        }
    }
}
