﻿using System;
using System.Threading;

//namespace Secs4Net
namespace Rorze.SECS.Driver
{
    internal sealed class SystemByteGenerator
    {
        private int _systemByte = new Random(Guid.NewGuid().GetHashCode()).Next();
        public int New() => Interlocked.Increment(ref _systemByte);
    }
}
