using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.MultiBlock
{
    public class MultiBlockTileAggregate : TileEntityObject
    {
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MultiBlockTileAggregateInstance(this, tilePosition, tileItem, chunk);
        }
    }


    public interface IMultiBlockTileAggregate
    {
        public IMultiBlockTileEntity GetAggregator();
        public void SetAggregator(IMultiBlockTileEntity aggregator);
    }

    public class MultiBlockTileAggregateInstance : TileEntityInstance<MultiBlockTileAggregate>, IMultiBlockTileAggregate
    {
        private IMultiBlockTileEntity parentEntity;
        public MultiBlockTileAggregateInstance(MultiBlockTileAggregate tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public IMultiBlockTileEntity GetAggregator()
        {
            return parentEntity;
        }

        public void SetAggregator(IMultiBlockTileEntity aggregator)
        {
            parentEntity = aggregator;
        }
    }
}
