using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using WorldModule;
using System;
using ChunkModule.IO;

namespace ChunkModule.ClosedChunkSystemModule {
    public static class InactiveClosedChunkFactory 
    {
        public static SoftLoadedClosedChunkSystem importFromFolder(Vector2Int positionInSystem,int dim) {
            SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem = new SoftLoadedClosedChunkSystem(new List<SoftLoadedConduitTileChunk>{});
            HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
            dfsFormSystem(positionInSystem,dim,softLoadedClosedChunkSystem,seen);
            if (softLoadedClosedChunkSystem.UnloadedChunks.Count == 0) {
                return null;
            }
            return softLoadedClosedChunkSystem;
        }

        private static void dfsFormSystem(Vector2Int position, int dim, SoftLoadedClosedChunkSystem softLoadedClosedChunkSystem, HashSet<Vector2Int> seen) {
            if (seen.Contains(position)) {
                return;
            }
            seen.Add(position);
            string data = getFileData(position,dim);
            if (data == null) {
                return;
            }
            
            softLoadedClosedChunkSystem.addChunk(ChunkIO.fromData(data,position,dim));

            Vector2Int left = position+Vector2Int.left;
            dfsFormSystem(left,dim,softLoadedClosedChunkSystem,seen);

            Vector2Int right = position+Vector2Int.right;
            dfsFormSystem(right,dim,softLoadedClosedChunkSystem,seen);
            
            Vector2Int up = position+Vector2Int.up;
            dfsFormSystem(up,dim,softLoadedClosedChunkSystem,seen);

            Vector2Int down = position+Vector2Int.down;
            dfsFormSystem(down,dim,softLoadedClosedChunkSystem,seen);
        
        }

        private static string getFileData(Vector2Int position, int dim) {
            string folderPath = WorldCreation.getDimPath(Global.WorldName,dim);
            string filePath = folderPath + "\\" + ChunkIO.getName(position);
            if (!File.Exists(filePath)) {
                return null;
            }
            return File.ReadAllText(filePath);
        }
    }

}
