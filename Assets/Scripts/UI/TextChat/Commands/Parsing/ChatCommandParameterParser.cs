using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


namespace UI.Chat {
    public class ChatParseException : Exception
    {
        public ChatParseException(string message) : base(message)
        {
        }
    }
    public static class ChatCommandParameterParser
    {
        public static readonly Dictionary<string, Color> PresetColors = new Dictionary<string, Color> {
            { "black", Color.black },
            { "white", Color.white },
            { "red", Color.red },
            { "green", Color.green },
            { "blue", Color.blue },
            { "yellow", Color.yellow },
            { "cyan", Color.cyan },
            { "magenta", Color.magenta },
            { "gray", Color.gray },
            { "lightgray", new Color(0.75f, 0.75f, 0.75f) }, // Light Gray
            { "darkgray", new Color(0.25f, 0.25f, 0.25f) }, // Dark Gray
            { "orange", new Color(1.0f, 0.647f, 0.0f) }, // Orange
            { "purple", new Color(0.5f, 0.0f, 0.5f) }, // Purple
            { "pink", new Color(1.0f, 0.75f, 0.8f) }, // Pink
            { "brown", new Color(0.65f, 0.16f, 0.16f) } // Brown
        };

        public static readonly HashSet<string> TrueValues = new HashSet<string>{"true","1","on",};
        public static readonly HashSet<string> FalseValues = new HashSet<string>{"false","0","off"};
        
        public static int ParseInt(string[] parameters, int index, string parameterName, int? baseValue, int lowerBound = int.MinValue, int upperBound = int.MaxValue) {
            
            string parameter = parseParameter(parameters,index,parameterName);
            try {
                if (parameter.First() == '~')
                {
                    if (baseValue == null)
                    {
                        throw new ChatParseException($"Error parsing command parameter at {index}: '~' is not valid in this context");
                    }
                    int addition = System.Convert.ToInt32(parameter.Substring(1, parameter.Length - 1));
                    return (int) baseValue + addition;
                } 
                int value = System.Convert.ToInt32(parameter);
                if (value < lowerBound) {
                    throw new ChatParseException($"Error parsing command parameter at {index}: {parameter} exceeds max value of {upperBound}");
                }
                if (value > upperBound) {
                    throw new ChatParseException($"Error parsing command parameter at {index}: {parameter} is less than min value of {lowerBound}");
                }
                return value;
            } catch (FormatException) {
                throw new ChatParseException($"Error parsing command parameter at {index}: value is not an integer");
            }
        }
        
        public static float ParseFloat(string[] parameters, int index, string parameterName, float? baseValue = null, float lowerBound = float.MinValue, float upperBound = float.MaxValue) {
            
            string parameter = parseParameter(parameters,index,parameterName);
            try {
                if (parameter.First() == '~')
                {
                    if (baseValue == null)
                    {
                        throw new ChatParseException($"Error parsing command parameter at {index}: '~' is not valid in this context");
                    }

                    if (parameter.Length == 1) return (float)baseValue;
                    
                    float addition = System.Convert.ToSingle(parameter.Substring(1, parameter.Length - 1));
                    return (float) baseValue + addition;
                } 
                float value = System.Convert.ToSingle(parameter);
                if (value < lowerBound) {
                    throw new ChatParseException($"Error parsing command parameter at {index}: {parameter} exceeds max value of {upperBound}");
                }
                if (value > upperBound) {
                    throw new ChatParseException($"Error parsing command parameter at {index}: {parameter} is less than min value of {lowerBound}");
                }
                return value;
            } catch (FormatException) {
                throw new ChatParseException($"Error parsing command parameter at {index}: value is not an float");
            }
        }

        public static bool parseBool(string[] parameters, int index, string parameterName) {
            string parameter = parseParameter(parameters,index,parameterName).ToLower();
            if (TrueValues.Contains(parameter)) {
                return true;
            }
            if (FalseValues.Contains(parameter)) {
                return false;
            }
            throw new ChatParseException($"Error parsing command: value '{parameter}' at {index} is not a bool");
        }

        public static Color parseColor(string[] parameters, int index) {
            string parameter = parseParameter(parameters,index,"color");
            parameter = parameter.Replace(" ","");
            if (PresetColors.ContainsKey(parameter)) {
                return PresetColors[parameter];
            }
            string[] split = parameter.Split(",");
            if (!parameter.StartsWith('(') || !parameter.EndsWith(')') || split.Length != 3) {
                throw new ChatParseException($"Error parsing command: value '{parameter}' at index {index} is not formatted as (r,g,b)");
            }
            parameter = parameter.Trim('(', ')');
            string[] rgb = parameter.Split(",");
            int[] colors = new int[3];
            List<string> names = new List<string> {"r","g","b"};
            for (int i = 0; i < 3; i++) {
                colors[i] = ParseInt(rgb,i,names[i],null,lowerBound:0,upperBound:255);
            }
            return new Color(colors[0],colors[1],colors[2],1);
        }

        public static string parseParameter(string[] parameters, int index, string parameterName) {
            if (index >= parameters.Length) {
                throw new ChatParseException($"Error parsing command: {parameterName} at index '{index}' not provided");
            }
            return parameters[index];
        }
    }
}

