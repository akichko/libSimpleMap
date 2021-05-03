using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libGis;

namespace libSimpleMap
{

    public class SpMapMgr : CmnMapMgr //, IMapAccess
    {
        MapDataType mapDataType;
        //new IMapAccess mal;
        //public bool IsConnected { get; set; } = false;
        //bool isConnected;

        public SpMapMgr(MapDataType mapDataType) : base(new GisTileCode())
        {
            
            this.mapDataType = mapDataType;

            switch (mapDataType)
            {
                case MapDataType.TextFile:
                    mal = new FileMapAccess();
                    break;

                //case MapDataType.BinaryFile:
                //    mal = new BinFileMapAccess(MiddleType.FileSystem);
                //    break;

                case MapDataType.SQLite:
                    mal = new SpMapBinMapAccess(MiddleType.SQLite);
                    break;

                case MapDataType.Postgres:
                    mal = new SpMapBinMapAccess(MiddleType.Postgres);
                    break;

                case MapDataType.MapManager:
                    throw new NotImplementedException();

                default:
                    break;
            }
        }


        //public int LoadTile(uint tileId, MapContentType reqType = MapContentType.All, byte maxRoadType = 255)
        //{
        //    if (!mal.IsConnected) return -1;

        //    CmnTile tmpTile;
        //    SpTile tmpRoad;
        //    bool isNew = false;

        //    if (tileDic.ContainsKey(tileId))
        //    {
        //        tmpTile = tileDic[tileId];
        //    }
        //    else
        //    {
        //        tmpTile = new SpTile(tileId);
        //        isNew = true;
        //    }
        //    tmpRoad = tmpTile.tile;


        //    if ((reqType & MapContentType.Link) == MapContentType.Link)
        //    {
        //        if (tmpRoad.loadedLinkLevel < maxRoadType)
        //        {
        //            tmpRoad.link = mal.GetRoadLink(tileId, maxRoadType);
        //            //tmpRoad.link.ForEach(x => x.tile = tmpRoad);
        //            //tmpRoad.hasLink = true;
        //            tmpRoad.loadedLinkLevel = maxRoadType;
        //        }
        //    }
        //    if ((reqType & MapContentType.Node) == MapContentType.Node)
        //    {
        //        if (tmpRoad.loadedNodeLevel < maxRoadType)
        //        {
        //            tmpRoad.node = mal.GetRoadNode(tileId, maxRoadType);
        //            tmpRoad.node.ForEach(x => x.tile = tmpRoad);
        //            //tmpRoad.hasNode = true;
        //            tmpRoad.loadedNodeLevel = maxRoadType;
        //        }
        //    }
        //    if ((reqType & MapContentType.LinkGeometry) == MapContentType.LinkGeometry)
        //    {
        //        if (tmpRoad.loadedGeometryLevel < maxRoadType)// && tmpRoad.loadedLinkLevel >= maxRoadType)
        //        {
        //            MapLink[] tmpShapeLink = mal.GetRoadGeometry(tileId);
        //            tmpRoad.MergeGeometry(tmpShapeLink, true);
        //            // tmpRoad.hasGeometry = true;
        //            tmpRoad.loadedGeometryLevel = maxRoadType;
        //        }
        //    }
        //    if ((reqType & MapContentType.LinkAttribute) == MapContentType.LinkAttribute)
        //    {
        //        if (tmpRoad.loadedAttributeLevel < maxRoadType)// && tmpRoad.loadedLinkLevel >= maxRoadType)
        //        {
        //            List<MapLink> tmpAttrLink = mal.GetRoadAttribute(tileId);
        //            if (tmpAttrLink != null)
        //            {
        //                tmpRoad.MergeAttribute(tmpAttrLink, true);
        //                tmpRoad.loadedAttributeLevel = maxRoadType;

        //            }
        //        }
        //    }

        //    if (isNew)
        //        tileDic.Add(tileId, tmpTile);

        //    return 0;
        //}


        //public List<uint> GetMapTileIdList()
        //{
        //    if (!mal.IsConnected) return null;
        //    return mal.GetMapTileIdList();
        //}



        override public CmnTile CreateTile(uint tileId)
        {
            return new SpTile(tileId);
        }




        //public override uint GetMapObjType(ECmnMapContentType cmnRefType)
        //{
        //    switch (cmnRefType)
        //    {
        //        case ECmnMapContentType.Link:
        //            return (int)SpMapContentType.Link;
        //        case ECmnMapContentType.Node:
        //            return (int)SpMapContentType.Node;
        //        case ECmnMapContentType.LinkGeometry:
        //            return (int)SpMapContentType.LinkGeometry;
        //        default:
        //            return 0;
        //    }
        //}

        //public override int GetMapRefType(ECmnMapRefType cmnRefType)
        //{
        //    switch (cmnRefType)
        //    {
        //        case ECmnMapRefType.NextLink:
        //            return (int)SpMapRefType.NextLink;
        //        case ECmnMapRefType.BackLink:
        //            return (int)SpMapRefType.BackLink;
        //        default:
        //            return 0;
        //    }
        //}

        /* 経路計算用 *****************************************************************/
        public override RoutingMapType RoutingMapType
        {
            get
            {
                RoutingMapType ret = new RoutingMapType();

                ret.roadNwObjType = (uint)(SpMapContentType.Link | SpMapContentType.Node);
                ret.roadGeometryObjType = (uint)SpMapContentType.LinkGeometry;
                ret.linkObjType = (uint)SpMapContentType.Link;
                ret.nextLinkRefType = (int)SpMapRefType.NextLink;
                ret.backLinkRefType = (int)SpMapRefType.BackLink;

                return ret;
            }
        }


        /*旧仕様 *************************************************************************************/


        //いずれ容量削減
        //public uint GetEdgeNodeTileId(SpTile tile, MapLink link, byte direction)
        //{
        //    return link.edgeNodeTileId[direction];
        //    //int nodeTileId = link.GetEdgeNodeTileOffset(direction).ToTileId(tile.tileId);
        //}

        ////いずれ容量削減
        //public uint GetConnectLinkTileId(SpTile tile, ConnectLink connectLink)
        //{
        //    return connectLink.tileId;

        //}

        //public NodeHandle GetEdgeNode(SpTile tile, MapLink link, byte direction, bool cacheOnly)
        //{
        //    ushort nodeIndex = link.edgeNodeIndex[direction]; //direction=1: 順方向

        //    uint nodeTileId = GetEdgeNodeTileId(tile, link, direction);

        //    NodeHandle tmpNodeHdl;

        //    if (tile.tileId == nodeTileId)
        //        tmpNodeHdl = tile.GetMapNode(nodeIndex);
        //    else
        //        tmpNodeHdl = SearchMapNode(nodeTileId, nodeIndex);

        //    if (tmpNodeHdl == null && cacheOnly)
        //    {
        //        return null;
        //    }
        //    else if (tmpNodeHdl == null && !cacheOnly)
        //    {
        //        return new NodeHandle(tile, null, nodeTileId, nodeIndex);
        //    }
        //    else
        //    {
        //        return tmpNodeHdl;
        //    }
        //}


        //public List<DLinkHandle> GetConnectLinks(NodeHandle nodeHdl, bool cacheOnly)
        //{
        //    List<DLinkHandle> retList = new List<DLinkHandle>();
        //    if (nodeHdl.mapNode == null)
        //        return retList;

        //    foreach (ConnectLink tmpConnectLink in nodeHdl.mapNode.connectLink)
        //    {
        //        uint connectTileId = GetConnectLinkTileId(nodeHdl.tile, tmpConnectLink);
        //        LinkHandle tmpLinkHdl = SearchMapLink(connectTileId, tmpConnectLink.linkIndex);

        //        if (tmpLinkHdl == null)
        //        {
        //            if (cacheOnly)
        //                continue;
        //            else
        //            {
        //                retList.Add(new DLinkHandle(nodeHdl.tile, null, 2, connectTileId, 0));
        //                continue;
        //            }
        //        }
        //        DLinkHandle tmpDLinkHdl = new DLinkHandle(tmpLinkHdl, tmpConnectLink.linkDirection);

        //        retList.Add(tmpDLinkHdl);

        //    }
        //    return retList;
        //}


        //public List<DLinkHandle> GetConnectLinkInfo(SpTile tile, MapLink link, byte direction, bool cacheOnly)
        //{
        //    NodeHandle tmpNodeHdl = GetEdgeNode(tile, link, direction, cacheOnly);

        //    if (cacheOnly && tmpNodeHdl == null)
        //        return new List<DLinkHandle>();

        //    return GetConnectLinks(tmpNodeHdl, cacheOnly);

        //}


        //public List<DLinkHandle> GetConnectLinks(SpTile tile, MapLink link, byte direction, bool cacheOnly, bool checkOneWay)
        //{
        //    //未読み込みメッシュのリンクは返却しない
        //    List<DLinkHandle> retList = new List<DLinkHandle>();

        //    int nodeIndex = link.edgeNodeIndex[direction]; //direction=1: 順方向

        //    //text版が未対応のため合わせる
        //    uint nodeTileId = link.edgeNodeTileId[direction];
        //    //int nodeTileId = link.GetEdgeNodeTileOffset(direction).ToTileId(tile.tileId);

        //    NodeHandle tmpNode;

        //    if (tile.tileId == nodeTileId)
        //        tmpNode = tile.GetMapNode(nodeIndex);
        //    else
        //        tmpNode = SearchMapNode(nodeTileId, nodeIndex);

        //    if (tmpNode == null)
        //    {
        //        if (cacheOnly)
        //            return retList;
        //        else
        //        {
        //            DLinkHandle tmpDlinkHdl = new DLinkHandle();
        //            tmpDlinkHdl.tileId = nodeTileId;

        //        }

        //    }


        //    foreach (ConnectLink tmpConnectLink in tmpNode.mapNode.connectLink)
        //    {
        //        SpTile tmpRoad = null;

        //        if (tile.tileId == tmpConnectLink.tileId)
        //        {
        //            tmpRoad = tile;
        //        }
        //        else if (tmpNode.tile.tileId == tmpConnectLink.tileId)
        //        {
        //            tmpRoad = tmpNode.tile;
        //        }
        //        else
        //        {
        //            SpTile tmpTile = (SpTile)SearchTile(tmpConnectLink.tileId);
        //            if (tmpTile != null)
        //                tmpRoad = tmpTile;
        //        }

        //        if (tmpRoad == null)
        //            return retList;

        //        DLinkHandle tmpLinkRef = new DLinkHandle();
        //        LinkHandle tmpHdl = tmpRoad.GetMapLink(tmpConnectLink.linkIndex);

        //        if (tmpHdl == null)
        //            continue;

        //        tmpLinkRef.tile = tmpHdl.tile;
        //        tmpLinkRef.mapLink = tmpHdl.mapLink;
        //        tmpLinkRef.direction = tmpConnectLink.linkDirection;

        //        //onewayチェック
        //        if ((tmpLinkRef.direction == 1 && tmpLinkRef.mapLink.fOneWay != -1)
        //            || (tmpLinkRef.direction == 0 && tmpLinkRef.mapLink.fOneWay != 1)
        //            || !checkOneWay)
        //        {
        //            retList.Add(tmpLinkRef);
        //        }
        //    }

        //    return retList;
        //}

        //public List<DLinkHandle> GetConnectLinks(LinkHandle linkRef, byte direction, bool cacheOnly, bool checkOneWay)
        //{
        //    return GetConnectLinks(linkRef.tile, linkRef.mapLink, direction, cacheOnly, checkOneWay);
        //}

        //public LinkHandle SearchMapLink(uint targetTileId, int targetLinkIndex)
        //{
        //    SpTile tmpTile = (SpTile)SearchTile(targetTileId);
        //    if (tmpTile == null)
        //        return null;

        //    return tmpTile.GetMapLink(targetLinkIndex);

        //}

        //public LinkHandle SearchMapLink(TileObjIndex tileLinkIndex)
        //{
        //    return SearchMapLink(tileLinkIndex.tileId, tileLinkIndex.index);
        //}

        //public MapLink SearchMapLink(TileObjId tileLinkId)
        //{
        //   SpTile tmpTile = SearchTile(tileLinkId.tileId);
        //    if (tmpTile == null)
        //        return null;
        //    foreach(MapLink mapLink in tmpTile.tile.link)
        //    {
        //        if (mapLink.linkId == tileLinkId.id)
        //            return mapLink;
        //    }
        //    return null;
        //}

        //public LinkHandle SearchMapLink(LatLon latlon, byte maxRoadType = 255)
        //{
        //    if (!mal.IsConnected)
        //        return null;

        //    uint tileId = CalcTileId(latlon);
        //    if (tileId < 0)
        //        return null;
        //    List<CmnTile> tileList = SearchTiles(tileId, 1, 1);
        //    if (tileList.Count == 0)
        //        return null;

        //    LinkHandle nearestLinkHdl = tileList
        //        .Select(x => ((SpTile)x).SearchNearestMapLink(latlon, maxRoadType))
        //        .OrderBy(x => x.mapLink.GetDistance(latlon))
        //        .FirstOrDefault();

        //    return nearestLinkHdl;


        //    //return tile.tile.SearchNearestMapLink(latlon, maxRoadType);
        //}

        //public LinkPos SearchMapLink2(LatLon latlon)
        //{
        //    if (!mal.IsConnected)
        //        return null;

        //    uint tileId = CalcTileId(latlon);
        //    if (tileId < 0)
        //        return null;
        //    List<CmnTile> tileList = SearchTiles(tileId, 1, 1);
        //    if (tileList.Count == 0)
        //        return null;


        //    var nearestLinkPos = tileList
        //        .Select(x => ((SpTile)x).SearchNearestMapLink2(latlon))
        //        .OrderBy(x => x.distance)
        //        .FirstOrDefault();

        //    return nearestLinkPos.linkPos;
        //    //return tile.tile.SearchNearestMapLink2(latlon).linkPos;
        //}

        //public NodeHandle SearchMapNode(uint targetTileId, int targetNodeIndex)
        //{
        //    SpTile tmpTile = (SpTile)SearchTile(targetTileId);
        //    if (tmpTile == null)
        //        return null;

        //    return tmpTile.GetMapNode(targetNodeIndex);

        //}

        //public NodeHandle SearchMapNode(TileObjIndex tileNodeIndex)
        //{
        //    return SearchMapNode(tileNodeIndex.tileId, tileNodeIndex.index);
        //}

        //public NodeHandle SearchMapNode(TileObjId tileNodeId)
        //{
        //    SpTile tmpTile = (SpTile)SearchTile(tileNodeId.tileId);
        //    if (tmpTile == null)
        //        return null;

        //    foreach (MapNode mapNode in tmpTile.node)
        //    {
        //        if (mapNode.nodeId == tileNodeId.id)
        //            return new NodeHandle(tmpTile, mapNode);
        //    }
        //    return null;

        //}






        //public async Task<uint> LoadTileList(List<uint> tileIdList)
        //{
        //    return 0;
        //}


        //public void PrintMap(int tileId)
        //{
        //    if (tileDic.ContainsKey(tileId))
        //        tileDic[tileId].tile.PrintGeometry();
        //}


        //new public List<CmnObjHdlRef> SearchRefObject(CmnObjHandle cmnObjHdl)
        //{
        //    switch ((SpMapContentType)cmnObjHdl.obj.Type)
        //    {
        //        case SpMapContentType.Link:

        //            //接続リンク
        //            List<DLinkHandle> connectLinkS = GetConnectLinks((SpTile)cmnObjHdl.tile, (MapLink)cmnObjHdl.obj, 0, true, false);
        //            var retS = connectLinkS.Select(x => new CmnObjHdlRef((ushort)SpMapRefType.BackLink, (CmnTile)x.tile, (CmnObj)x.mapLink)).ToList();

        //            List<DLinkHandle> connectLinkE = GetConnectLinks((SpTile)cmnObjHdl.tile, (MapLink)cmnObjHdl.obj, 1, true, false);
        //            var retE = connectLinkE.Select(x => new CmnObjHdlRef((ushort)SpMapRefType.NextLink, (CmnTile)x.tile, (CmnObj)x.mapLink));

        //            retS.AddRange(retE);

        //            return retS;

        //        case SpMapContentType.Node:

        //            break;

        //    }
        //    return null;

        //}


        //public int WriteMap(uint tileId, string filename, bool append)
        //{
        //    if (tileDic.ContainsKey(tileId))
        //        return ((SpTile)tileDic[tileId]).WriteMap(filename, append);
        //    else
        //        return -1;
        //}


        //public void WriteMap(List<uint> tileIdList, string filename)
        //{
        //    foreach (uint id in tileIdList.Take(1))
        //    {
        //        WriteMap(id, filename, false);
        //    }
        //    foreach (uint id in tileIdList.Skip(1))
        //    {
        //        WriteMap(id, filename, true);
        //    }
        //}

        public int SaveTile(SpTile tile)
        {
            return ((ISpMapAccess)mal).SaveTile(tile);
        }


        /* MAL-API ****************************************************/

        //public int ConnectMapData(string mapPath)
        //{
        //    if (mal.IsConnected)
        //    {
        //        IsConnected = true;
        //        return 0;
        //    }
        //    return -1;
        //}
        ////public bool GetConnectStatus()
        ////{
        ////    return isConnected;
        ////}

        //public int DisconnectMapData()
        //{
        //    IsConnected = false;
        //    return 0;
        //}

        //public MapLink[] GetRoadLink(uint tileId, byte maxRoadType)
        //{
        //    LoadTile(tileId, (UInt16)SpMapContentType.Link, maxRoadType);

        //    SpTile tile = (SpTile)SearchTile(tileId);
        //    return tile.link;
        //}
        //public MapNode[] GetRoadNode(uint tileId, byte maxRoadType)
        //{
        //    LoadTile(tileId, (UInt16)SpMapContentType.Node, maxRoadType);

        //    SpTile tile = (SpTile)SearchTile(tileId);
        //    return tile.node;

        //}
        //public MapLink[] GetRoadGeometry(uint tileId, byte maxRoadType)
        //{
        //   // LoadTile(tileId, MapContentType.Link | MapContentType.LinkGeometry, maxRoadType);

        //    SpTile tile = (SpTile)SearchTile(tileId);
        //    return tile.link;
        //}
        //public MapLink[] GetRoadAttribute(uint tileId, byte maxRoadType)
        //{
        //    //LoadTile(tileId, MapContentType.Link | MapContentType.LinkAttribute, maxRoadType);

        //    SpTile tile = (SpTile)SearchTile(tileId);
        //    return tile.link;
        //}


        //public List<uint> GetMapTileIdList()
        //{
        //    return GetMapTileList();
        //}

        //public int SaveTile(SpTile tile) { return 0; }
        //public int SaveRoadLink(SpTile tile) { throw new NotImplementedException(); }
        //public int SaveRoadNode(SpTile tile) { throw new NotImplementedException(); }
        //public int SaveRoadGeometry(SpTile tile) { throw new NotImplementedException(); }

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


    public interface ISpMapAccess : ICmnMapAccess
    {
        //MapLink[] GetRoadLink(uint tileId, ushort maxRoadType = 0xFFFF);
        //MapNode[] GetRoadNode(uint tileId, ushort maxRoadType = 0xFFFF);
        //MapLink[] GetRoadGeometry(uint tileId, ushort maxRoadType = 0xFFFF);
        //MapLink[] GetRoadAttribute(uint tileId, ushort maxRoadType = 0xFFFF);
        //MapLinkAttribute[] GetRoadAttribute2(uint tileId, ushort maxRoadType = 0xFFFF);

        //List<uint> GetMapTileIdList();
        //int CalcTileDistance(int tileIdA, int tileIdB);

        int SaveTile(SpTile tile);
        int SaveRoadLink(SpTile tile);
        int SaveRoadNode(SpTile tile);
        int SaveRoadGeometry(SpTile tile);
    }



}
