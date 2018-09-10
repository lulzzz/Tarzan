using SharpPcap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Ingest.Utils
{
    public static class PosixTimeval_
    {
        public static long ToUnixTimeMilliseconds(this PosixTimeval timeval)
        {
            return (long)((timeval.Seconds * 1000) + (timeval.MicroSeconds / 1000));
        }

        internal static PosixTimeval FromUnixTimeMilliseconds(long v)
        {
            return new PosixTimeval((ulong)(v / 1000), (ulong)((v % 1000) * 1000));
        }
    }
}
