using System;

namespace SearchEngine
{
    [Flags]
    public enum FieldFlags : byte
    {
        None = 0,
        Stored = 1,
        Analyzed = 2
    }
}