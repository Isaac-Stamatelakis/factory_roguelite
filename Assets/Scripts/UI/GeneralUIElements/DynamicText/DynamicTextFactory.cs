using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UI {
    public static class DynamicTextColorFactory
    {
        public static List<Color> getRainbow() {
            List<Color> rainbowColors = new List<Color>();
            rainbowColors.Add(new Color(1f, 0f, 0f)); // Red
            rainbowColors.Add(new Color(1f, 0.5f, 0f)); // Orange
            rainbowColors.Add(new Color(1f, 1f, 0f)); // Yellow
            rainbowColors.Add(new Color(0f, 1f, 0f)); // Green
            rainbowColors.Add(new Color(0f, 0f, 1f)); // Blue
            rainbowColors.Add(new Color(0.29f, 0f, 0.51f)); // Indigo
            rainbowColors.Add(new Color(0.93f, 0.51f, 0.93f)); // Violet
            return rainbowColors;
        }
        public static List<Color> GetSunsetColors()
        {
            List<Color> sunsetColors = new List<Color>();
            sunsetColors.Add(new Color(1f, 0.2f, 0f)); // Deep Orange
            sunsetColors.Add(new Color(1f, 0.55f, 0f)); // Orange
            sunsetColors.Add(new Color(1f, 0.76f, 0.62f)); // Peach
            sunsetColors.Add(new Color(1f, 0.94f, 0.73f)); // Light Peach
            sunsetColors.Add(new Color(0.94f, 0.89f, 0.55f)); // Sand
            return sunsetColors;
        }

        public static List<Color> GetEarthTones()
        {
            List<Color> earthToneColors = new List<Color>();
            earthToneColors.Add(new Color(0.6f, 0.4f, 0.2f)); // Brown
            earthToneColors.Add(new Color(0.39f, 0.29f, 0.13f)); // Dark Brown
            earthToneColors.Add(new Color(0.87f, 0.72f, 0.53f)); // Tan
            earthToneColors.Add(new Color(0.29f, 0.47f, 0.29f)); // Olive Green
            earthToneColors.Add(new Color(0.4f, 0.34f, 0.28f)); // Taupe
            return earthToneColors;
        }

        public static List<Color> GetPastelColors()
        {
            List<Color> pastelColors = new List<Color>();
            pastelColors.Add(new Color(1f, 0.8f, 0.8f)); // Light Pink
            pastelColors.Add(new Color(0.8f, 1f, 0.8f)); // Light Green
            pastelColors.Add(new Color(0.8f, 0.8f, 1f)); // Light Blue
            pastelColors.Add(new Color(1f, 0.92f, 0.8f)); // Light Peach
            pastelColors.Add(new Color(0.8f, 1f, 1f)); // Light Cyan
            return pastelColors;
        }

        public static List<Color> GetRandomColors(int count)
        {
            List<Color> randomColors = new List<Color>();
            while (count > 0) {
                randomColors.Add(new Color(Random.value, Random.value, Random.value));
                count --;
            }
            return randomColors;
        }
    }

    public static class DynamicTextPositionFactory {
        public static List<VerticalAlignmentOptions> getWave() {
            return new List<VerticalAlignmentOptions>{
                VerticalAlignmentOptions.Top,
                VerticalAlignmentOptions.Middle,
                VerticalAlignmentOptions.Bottom,
                VerticalAlignmentOptions.Middle
            };
        }
        public static List<VerticalAlignmentOptions> GetDiagonalWave()
        {
            return new List<VerticalAlignmentOptions>{
                VerticalAlignmentOptions.Top,
                VerticalAlignmentOptions.Middle,
                VerticalAlignmentOptions.Bottom
            };
        }
    }
}

