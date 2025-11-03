using System;
using System.Collections.Generic;

public interface IHandReadable
{
    IReadOnlyList<string> GetHandIds();
    event Action OnHandChanged;
}
