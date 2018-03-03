using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Flags]
	public enum Status
	{
        Failure = 1 << 31,
        Success = 1 << 30,
        InProgress = 1 << 29,
        WrongMagic = 1 << 0,
        WrongVersion = 1 << 1,
        OutOfMemory = 1 << 2,
        InvalidParam = 1 << 3,
        BufferTooSmall = 1 << 4,
        OutOfNodes = 1 << 5,
        PartialResult = 1 << 6,
        DetailMask = 0x0ffffff
	}
}
