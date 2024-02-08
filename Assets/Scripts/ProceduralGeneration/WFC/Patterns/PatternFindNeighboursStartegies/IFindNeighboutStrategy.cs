using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollaps
{
    public interface IFindNeighboutStrategy
    {
        Dictionary<int, PatternNeighbours> FIndNeighbours(PatternDataResults patterndataResults);
    }
}
