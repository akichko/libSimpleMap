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

    public class SpMapMgr : CmnMapMgr //, IMapAccess
    {
        protected MapDataType mapDataType;

        public SpMapMgr(MapDataType mapDataType) : base(new GisTileCode())
        {            
            this.mapDataType = mapDataType;

            mapAccess = mapDataType switch
            {
                MapDataType.TextFile => new SpTextMapAccess(),
                //MapDataType.BinaryFile => new BinFileMapAccess(MiddleType.FileSystem),
                MapDataType.SQLite => new SpMapBinMapAccess(MiddleType.SQLite),
                MapDataType.Postgres => new SpMapBinMapAccess(MiddleType.Postgres),
                MapDataType.MapManager => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }


        override public CmnTile CreateTile(uint tileId)
        {
            return new SpTile(tileId);
        }


        /* 経路計算用 *****************************************************************/

        public override CmnRouteMgr CreateRouteMgr()
        {
            return new SpRouteMgr(this);
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



        public int SaveTile(SpTile tile)
        {
            return ((SpMapAccess)mapAccess).SaveTile(tile);
        }



        public override string[] GetMapContentTypeNames()
        {
            var ret = ((SpMapContentType[])Enum.GetValues(typeof(SpMapContentType)))
                .Select(x => Enum.GetName(typeof(SpMapContentType), x)).ToArray();
            return ret;
        }

        public override uint GetMapContentTypeValue(string objTypeName)
        {
            object result;
            bool ret = Enum.TryParse(typeof(SpMapContentType), objTypeName, false, out result);
            return ret ? (uint)result : 0;
            //(uint)Enum.Parse(typeof(SpMapContentType), objTypeName);
        }
    }


    public struct TagInfo
    {
        public string tag;
        public string val;

        public TagInfo(string tag, string val)
        {
            this.tag = tag;
            this.val = val;
        }
    }


    public abstract class SpMapAccess : ICmnMapAccess
    {
        static List<UInt32> mapContentTypeList;

        public SpMapAccess()
        {
            mapContentTypeList = ((uint[])Enum.GetValues(typeof(SpMapContentType)))
                .Where(x => x != 0xFFFF && x != (UInt32)SpMapContentType.Tile)
                .ToList();
        }

        public abstract bool IsConnected { get; set; }

        public abstract int SaveTile(SpTile tile);
        public abstract int SaveRoadLink(SpTile tile);
        public abstract int SaveRoadNode(SpTile tile);
        public abstract int SaveRoadGeometry(SpTile tile);

        //ICmnMapAccess I/F
        public abstract int ConnectMap(string connectStr);
        public abstract int DisconnectMap();
        public abstract List<uint> GetMapTileIdList();
        public virtual List<uint> GetMapContentTypeList() => mapContentTypeList;

        //public abstract List<CmnObjGroup> LoadObjGroupList(uint tileId, uint type = uint.MaxValue, ushort subType = ushort.MaxValue);
        public abstract IEnumerable<CmnObjGroup> LoadObjGroup(uint tileId, uint type, ushort subType = ushort.MaxValue);


        public virtual async Task<IEnumerable<CmnObjGroup>> LoadObjGroupAsync(uint tileId, UInt32 type, UInt16 subType = 0xFFFF)
        {
            Task<IEnumerable<CmnObjGroup>> taskRet = Task.Run(() => LoadObjGroup(tileId, type, subType));
            IEnumerable<CmnObjGroup> ret = await taskRet.ConfigureAwait(false);
            return ret;
        }


        protected virtual CmnObj CreateObj(SpMapContentType type)
        {
            switch (type)
            {
                case SpMapContentType.Link: return new MapLink();
                case SpMapContentType.Node: return new MapNode();
                case SpMapContentType.LinkGeometry: return new MapLinkGeometry();
                case SpMapContentType.LinkAttribute: return new MapLinkAttribute();
                default: throw new ArgumentException();
            }

        }

        public TimeStampRange GetTimeStampRange()
        {
            throw new NotImplementedException();
        }
    }



}
