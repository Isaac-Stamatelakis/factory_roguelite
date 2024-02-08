using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace WaveFunctionCollaps
{
    public class NeighbourStrategyFactory
    {
        Dictionary<string, Type> strategies;
        public NeighbourStrategyFactory()
        {
            LoadTypesIFindNeighboutStrategy();
        }

        private void LoadTypesIFindNeighboutStrategy()
        {
            strategies = new Dictionary<string, Type>();
            Type[] typesInThisAssembly = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var type in typesInThisAssembly)
            {
                if (type.GetInterface(typeof(IFindNeighboutStrategy).ToString()) != null)
                {
                    strategies.Add(type.Name.ToLower(), type);
                }
            }
        }

        public IFindNeighboutStrategy CreateInstance(string strategyName)
        {
            Type t = GetTypeToCreate(strategyName);
            if (t == null)
            {
                t = GetTypeToCreate("more");
            }
            return Activator.CreateInstance(t) as IFindNeighboutStrategy;

        }

        private Type GetTypeToCreate(string patternSizeName)
        {
            foreach (var possibleStrategy in strategies)
            {
                if (possibleStrategy.Key.Contains(patternSizeName))
                {
                    return strategies[possibleStrategy.Key];
                }
            }
            return null;
        }
    }

}
