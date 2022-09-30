/*============================================================================
MIT License

Copyright (c) 2021 akichko

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
============================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akichko.libGis;

namespace libSimpleMap
{
    public class SpRouteMgr : CmnRouteMgr
    {
        public SpRouteMgr(SpMapMgr mapMgr, DykstraSetting setting) : base(mapMgr, setting)
        {
            //this.setting = setting;
        }

        //地図仕様に応じてオーバーライド
        protected override byte CalcMaxUsableRoadType(uint targetTileId)
        {
            if (targetTileId == orgHdl.TileId || targetTileId == dstHdl.TileId)
                return 9;

            double DistFromStartTile = mapMgr.tileApi.CalcTileDistance(targetTileId, orgHdl.TileId);
            double DistFromDestTile = mapMgr.tileApi.CalcTileDistance(targetTileId, dstHdl.TileId);

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


        public override RoutingMapType RoutingMapType
        {
            get
            {
                RoutingMapType ret = new RoutingMapType();

                ret.roadNwObjType = (uint)(SpMapContentType.Link | SpMapContentType.Node);

                ret.roadNwObjTypeList = new List<uint>{
                    (uint)SpMapContentType.Link,
                    (uint)SpMapContentType.Node
                };

                //ret.roadNwObjReqType = new ReqType[]{
                //    new ReqType((uint)(SpMapContentType.Link)),
                //    new ReqType((uint)(SpMapContentType.Node))
                //    };
                ret.roadNwObjFilter = new CmnObjFilter();
                ret.roadNwObjFilter
                    .AddRule((uint)(SpMapContentType.Link), null)
                    .AddRule((uint)(SpMapContentType.Node), null);
                ret.roadGeometryObjType = (uint)SpMapContentType.LinkGeometry;
                ret.linkObjType = (uint)SpMapContentType.Link;
                ret.nextLinkRefType = (int)SpMapRefType.NextLink;
                ret.backLinkRefType = (int)SpMapRefType.BackLink;

                return ret;
            }
        }

    }
}
