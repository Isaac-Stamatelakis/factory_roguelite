using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOutlineGenerator
{
    private bool padding;
    private Color[,] pixels;
    private Color outlineColor;

    public SpriteOutlineGenerator(bool padding, Color[,] pixels, Color outlineColor)
    {
        this.padding = padding;
        this.pixels = pixels;
        this.outlineColor = outlineColor;
    }

    public Color[,] generate() {
        Vector2Int size = new Vector2Int(pixels.GetLength(0),pixels.GetLength(1));
        Color[,] outlinePixels = new Color[size.x,size.y];
        BFS(Vector2Int.zero,outlinePixels,pixels);
        if (!padding) {
            return outlinePixels;
        }
        Color[,] paddedOutline = new Color[size.x+2,size.y+2];
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                paddedOutline[x+1,y+1]=outlinePixels[x,y];
            }
        }
        BFSBorder(paddedOutline,pixels);
        return paddedOutline;
    }

    private void BFSBorder(Color[,] paddedOutlinePixels, Color[,] pixels) {
        
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Vector2Int start = -Vector2Int.one;
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        visited.Add(start);
        queue.Enqueue(start);

        Vector2Int[] directions = {
            Vector2Int.down,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0) {
            Vector2Int current = queue.Dequeue();
            bool xEdge = current.x == -1 || current.x == pixels.GetLength(0);
            bool yEdge = current.y == -1 || current.y == pixels.GetLength(1);
            if (!xEdge && !yEdge) {
                continue;
            }
            bool inPaddedBounds = current.x >= -1 && current.x <= pixels.GetLength(0) && current.y >= -1 && current.y <= pixels.GetLength(1);
            if (!inPaddedBounds) {
                continue;
            }
            foreach (var direction in directions) {
                Vector2Int neighbor = current + direction;
                bool neighborInBounds = neighbor.x >= 0 && neighbor.y >= 0 && neighbor.x < pixels.GetLength(0) && neighbor.y < pixels.GetLength(1);
                if (neighborInBounds) {
                    bool neighborEmpty = pixels[neighbor.x,neighbor.y].a==0;
                    if (!neighborEmpty) {
                        paddedOutlinePixels[current.x+1,current.y+1] = outlineColor;
                    }
                    continue;
                }
                
                if (!visited.Contains(neighbor)) {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
    }
    private void BFS(Vector2Int start, Color[,] outlinePixels, Color[,] pixels)
    {
        bool[,] visited = new bool[pixels.GetLength(0),pixels.GetLength(1)];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        visited[start.x,start.y] = true;
        queue.Enqueue(start);
        Vector2Int[] directions = {
            Vector2Int.down,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0) {
            Vector2Int current = queue.Dequeue();
            bool empty = pixels[current.x,current.y].a == 0;

            foreach (var direction in directions) {
                Vector2Int neighbor = current + direction;
                if (
                    neighbor.x >= 0 && neighbor.x < pixels.GetLength(0) &&
                    neighbor.y >= 0 && neighbor.y < pixels.GetLength(1)
                ) {
                    bool neighborEmpty = pixels[neighbor.x,neighbor.y].a == 0;
                    bool writeOutline = neighborEmpty && !empty;
                    if (writeOutline) {
                        outlinePixels[neighbor.x,neighbor.y] = outlineColor;
                    }

                    if (!visited[neighbor.x,neighbor.y]) {
                        queue.Enqueue(neighbor);
                        visited[neighbor.x,neighbor.y] = true;
                    }
                }
            }
        }
    }

}
