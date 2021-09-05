using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libGis;

namespace libSimpleMap
{
    public class SpRouteMgr : CmnRouteMgr
    {
        public SpRouteMgr(SpMapMgr mapMgr) : base(mapMgr) { }

        //地図仕様に応じてオーバーライド
        protected override byte CalcMaxUsableRoadType(uint targetTileId)
        {
            if (targetTileId == orgHdl.TileId || targetTileId == dstHdl.TileId)
                return 9;

            float DistFromStartTile = (float)mapMgr.tileApi.CalcTileDistance(targetTileId, orgHdl.TileId);
            float DistFromDestTile = (float)mapMgr.tileApi.CalcTileDistance(targetTileId, dstHdl.TileId);

            double minDist = Math.Min(DistFromStartTile, DistFromDestTile);

            //double minDist = MapTool.CalcTileDistance(tileId, startTileId);
            //double distFromDest = MapTool.CalcTileDistance(tileId, destTileId);
            //minDist = Math.Min(minDist, distFromDest);

            if (minDist < 5000)
            {
                return 8;
            }
            else if (minDist < 8000)
            {
                return 6;
            }
            else if (minDist < 12000)
            {
                return 5;
            }
            else if (minDist < 15000)
            {
                return 4;
            }
            else if (minDist < 50000)
            {
                return 3;
            }
            else if (minDist < 100000)
            {
                return 2;
            }
            else //  >100km
            {
                return 1;
            }

        }


    }
}
