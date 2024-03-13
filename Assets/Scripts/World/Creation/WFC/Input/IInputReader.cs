using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollaps
{
    public interface IInputReader<T> 
    {
        IValue<T>[][] ReadInputToGrid();
    }
}

