using SharpPcap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Ingest
{
    public static class PosixTimeval_
    {
        public static long ToUnixTimeMilliseconds(this PosixTimeval timeval)
        {
            return (long)((timeval.Seconds * 1000) + (timeval.MicroSeconds / 1000));
        }
    }
}
