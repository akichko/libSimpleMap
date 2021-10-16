using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Akichko.libGis;

namespace libSimpleMap
{
    class SpTextMapAccess : SpMapAccess
    {
        string mapPath;
        //bool isConnencted = false;
        public override bool IsConnected { get; set; }

        public SpTextMapAccess()
        {
            IsConnected = false;
        }

        public override int ConnectMap(string connectStr)
        {
            if (Directory.Exists(connectStr))
            {
                this.mapPath = connectStr + "\\";
                IsConnected = true;
                return 0;
            }
            else
            {
                Console.WriteLine("Map File not exists!");
                return -1;
            }
        }


        public override int DisconnectMap()
        {
            return 0;
        }

        //public Tile GetTile(int tileId)
        //{
        //    //非同期読み込みもいずれ対応したい
        //    //更新を考慮するか？？

        //    List<MapLink> tmpLink = GetRoadLink(tileId);
        //    List<MapNode> tmpNode = GetRoadNode(tileId);
        //    List<MapLink> tmpLinkGeometry = GetRoadGeometry(tileId);


        //    Tile tmpTile = new Tile(tileId);
        //    tmpTile.tile.link = tmpLink;
        //    tmpTile.tile.MergeGeometry(tmpLinkGeometry);
        //    tmpTile.tile.node = tmpNode;
        //    tmpTile.tile.tileId = tileId;

        //    tmpTile.tile.link.ForEach(x => x.road = tmpTile.road);
        //    tmpTile.tile.node.ForEach(x => x.road = tmpTile.road);

        //    tmpTile.tile.hasLink = true;
        //    tmpTile.tile.hasNode = true;
        //    tmpTile.tile.hasGeometry = true;


        //    return tmpTile;
        //}



        //public CmnTile CreateTile(uint tileId)
        //{
        //    return new SpTile(tileId);
        //}

        public MapLink[] GetRoadLink(uint tileId, ushort maxRoadType = 0xFFFF)
        {
            List<MapLinkFull> tmpLinkList = new List<MapLinkFull>();

            string fileName = string.Format($"{mapPath}LINK\\{tileId}_LINK.txt");
            if (!File.Exists(fileName))
                return tmpLinkList.ToArray();

            using (var sr = new StreamReader(mapPath + "LINK\\" + tileId + "_LINK.txt"))
            {
                ushort numLink = 0;
                String fbuf;

                while ((fbuf = sr.ReadLine()) != null)
                {
                    MapLinkFull tmpLink = new MapLinkFull();
                    tmpLink.index = numLink++;

                    string[] csv_column = fbuf.Split('\t');


                    //CSVformat
                    int csvIndex = 0;
                    string sLinkdId = csv_column[csvIndex++]; //極論、なくても可
                    string sStartNodeId = csv_column[csvIndex++];   //削除可能？
                    string sStartNodeIndex = csv_column[csvIndex++];
                    string sEndNodeId = csv_column[csvIndex++];     //削除可能？
                    string sEndNodeTileId = csv_column[csvIndex++]; //
                    string sEndNodeIndex = csv_column[csvIndex++];
                    //string sLinkCost = csv_column[csvIndex++];
                    //string sLength = csv_column[csvIndex++];
                    string sRoadType = csv_column[csvIndex++];
                    string sOneWay = csv_column[csvIndex++];

                    if (sEndNodeTileId == "") sEndNodeTileId = "0";
                    if (sEndNodeIndex == "") sEndNodeIndex = "0";


                    tmpLink.LinkId = UInt64.Parse(sLinkdId);
                    tmpLink.edgeNodeId[0] = Int64.Parse(sStartNodeId);
                    tmpLink.edgeNodeId[1] = Int64.Parse(sEndNodeId);
                    tmpLink.edgeNodeTileId[0] = tileId;
                    tmpLink.edgeNodeTileId[1] = UInt32.Parse(sEndNodeTileId);
                    if (tmpLink.edgeNodeTileId[1] == 0)
                        tmpLink.edgeNodeTileId[1] = tileId;
                    tmpLink.edgeNodeIndex[0] = UInt16.Parse(sStartNodeIndex);
                    tmpLink.edgeNodeIndex[1] = UInt16.Parse(sEndNodeIndex);
                    //tmpLink.linkCost = short.Parse(sLinkCost);
                    tmpLink.roadType = byte.Parse(sRoadType);
                    tmpLink.fOneWay = sbyte.Parse(sOneWay);
                    //link[i].numLanes = byte.Parse(csv_column[10]);

                    tmpLinkList.Add(tmpLink);
                }

            }
            return tmpLinkList.ToArray();
        }


        public MapNode[] GetRoadNode(uint tileId, ushort maxRoadType = 0xFFFF)
        {
            List<MapNode> tmpNodeList = new List<MapNode>();

            string fileName = string.Format($"{mapPath}NODE\\{tileId}_NODE.txt");
            if (!File.Exists(fileName))
                return tmpNodeList.ToArray();

            using (StreamReader sr = new StreamReader(mapPath + "NODE\\" + tileId + "_NODE.txt"))
            {

                MapNode tmpNode = null;
                String fbuf;

                short index = 0;
                UInt64 preNodeId = 0;
                List<ConnectLink> tmpConnectLinkList = null;

                //データ数読み込み
                while ((fbuf = sr.ReadLine()) != null)
                {
                    string[] csv_column = fbuf.Split('\t');

                    //CSVformat
                    int csvIndex = 0;
                    string sNodeId = csv_column[csvIndex++];
                    string sConnLinkId = csv_column[csvIndex++];
                    string sConnLinkTileId = csv_column[csvIndex++];
                    string sConnLinkIndex = csv_column[csvIndex++];
                    string sConnLinkDirection = csv_column[csvIndex++];
                    string sConnLinkRoadType = csv_column[csvIndex++];

                    UInt64 tmpNodeId = UInt64.Parse(sNodeId);

                    if (tmpNodeId != preNodeId)
                    {
                        if (tmpNode != null)
                            tmpNode.connectLink = tmpConnectLinkList.ToArray();
                        tmpNode = new MapNode();
                        tmpNode.index = index;
                        tmpNode.nodeId = tmpNodeId;
                        tmpConnectLinkList = new List<ConnectLink>();
                        preNodeId = tmpNodeId;
                        tmpNodeList.Add(tmpNode);
                        index++;
                    }
                    ConnectLink tmpConnectLink = new ConnectLink();
                    tmpConnectLink.tileId = uint.Parse(sConnLinkTileId);
                    if (tmpConnectLink.tileId == 0)
                        tmpConnectLink.tileId = tileId;
                    tmpConnectLink.linkId = Int64.Parse(sConnLinkId);
                    tmpConnectLink.linkIndex = ushort.Parse(sConnLinkIndex);
                    tmpConnectLink.linkDirection = (DirectionCode)byte.Parse(sConnLinkDirection);
                    tmpConnectLink.roadType = byte.Parse(sConnLinkRoadType);

                    tmpConnectLinkList.Add(tmpConnectLink);
                }
                tmpNode.connectLink = tmpConnectLinkList.ToArray();
            }
            return tmpNodeList.ToArray();
        }


        public MapLink[] GetRoadGeometry(uint tileId, ushort maxRoadType = 0xFFFF)
        {
            List<MapLinkFull> tmpLinkShapeList = new List<MapLinkFull>();

            string fileName = string.Format($"{mapPath}LINKGEOMETRY\\{tileId}_LINKGEOMETRY.txt");
            if (!File.Exists(fileName))
                return tmpLinkShapeList.ToArray();

            using (var sr = new StreamReader(mapPath + "LINKGEOMETRY\\" + tileId + "_LINKGEOMETRY.txt"))
            {
                String fbuf;

                int index = 0;
                UInt64 preLinkId = UInt64.MaxValue;

                MapLinkFull tmpLinkShape = new MapLinkFull();
                List<LatLon> tmpGeometry = new List<LatLon>();

                //データ数読み込み
                while ((fbuf = sr.ReadLine()) != null)
                {
                    string[] csv_column = fbuf.Split('\t');

                    //CSVformat
                    string sLinkId = csv_column[0];
                    string sLon = csv_column[1];
                    string sLat = csv_column[2];

                    UInt64 tmpLinkId = UInt64.Parse(sLinkId);

                    if (tmpLinkId != preLinkId)
                    {
                        tmpLinkShape.geometry = tmpGeometry.ToArray();
                        tmpLinkShape = new MapLinkFull();

                        tmpGeometry = new List<LatLon>();
                        tmpLinkShape.LinkId = tmpLinkId;
                        preLinkId = tmpLinkId;
                        tmpLinkShapeList.Add(tmpLinkShape);

                        index++;
                    }
                    LatLon tmpLatLon = new LatLon(double.Parse(sLat), double.Parse(sLon));

                    tmpGeometry.Add(tmpLatLon);

                }
                tmpLinkShape.geometry = tmpGeometry.ToArray();
            }

            //形状点間引き（データ生成時がいいかも）
            //tmpLinkShapeList.ForEach(x =>
            //{
            //    x.geometry = LatLon.DouglasPeuker(x.geometry, 3.0, 3000.0);
            //});

            return tmpLinkShapeList.ToArray();
        }


        public MapLinkAttribute[] GetRoadAttribute2(uint tileId, ushort maxRoadType = 0xFFFF) { throw new NotImplementedException(); }

        public MapLink[] GetRoadAttribute(uint tileId, ushort maxRoadType = 0xFFFF)
        {
            List<MapLinkFull> tmpLinkAttrList = new List<MapLinkFull>();

            string fileName = string.Format($"{mapPath}LINKATTR\\{tileId}_LINKATTR.txt");
            if (!File.Exists(fileName))
                return tmpLinkAttrList.ToArray();

            using (var sr = new StreamReader(mapPath + "LINKATTR\\" + tileId + "_LINKATTR.txt"))
            {
                String fbuf;

                int index = 0;
                UInt64 preLinkId = 0xffffffffffffffff;

                MapLinkFull tmpLinkAttr = null;

                //データ数読み込み
                while ((fbuf = sr.ReadLine()) != null)
                {
                    string[] csv_column = fbuf.Split('\t');

                    //CSVformat
                    string sLinkIndex = csv_column[0];
                    string sLinkId = csv_column[1];
                    string sWayId = csv_column[2];
                    string sTagKey = csv_column[3];
                    string sTagValue = csv_column[4];

                    UInt64 tmpLinkId = UInt64.Parse(sLinkId);
                    UInt64 tmpWayId = UInt64.Parse(sWayId);

                    if (tmpLinkId != preLinkId)
                    {
                        tmpLinkAttr = new MapLinkFull();
                        tmpLinkAttr.attribute = new LinkAttribute();
                        tmpLinkAttr.attribute.linkId = UInt64.Parse(sLinkId);
                        tmpLinkAttr.attribute.wayId = UInt64.Parse(sWayId);
                        tmpLinkAttr.LinkId = UInt64.Parse(sLinkId);

                        preLinkId = tmpLinkId;
                        tmpLinkAttrList.Add(tmpLinkAttr);
                    }
                    TagInfo tmpTagInfo = new TagInfo(sTagKey, sTagValue);

                    tmpLinkAttr.attribute.tagInfo.Add(tmpTagInfo);

                }
            }
            return tmpLinkAttrList.ToArray();
        }

        public override int SaveTile(SpTile tile)
        {
            SaveRoadLink(tile);
            SaveRoadNode(tile);
            SaveRoadGeometry(tile);

            return 0;
        }


        public override int SaveRoadLink(SpTile tile)
        {
            throw new NotImplementedException();
        }

        public override int SaveRoadNode(SpTile tile)
        {
            throw new NotImplementedException();
        }

        public override int SaveRoadGeometry(SpTile tile)
        {
            throw new NotImplementedException();
        }

        public override List<uint> GetMapTileIdList()
        {
            List<uint> retList = new List<uint>();
            string[] names = Directory.GetFiles(mapPath + "NODE", "*.txt");
            foreach (string name in names)
            {
                string tileName = name.Replace(mapPath + "NODE\\", "").Replace("_NODE.txt", "");
                retList.Add(uint.Parse(tileName));

            }
            return retList;
        }


        public override List<UInt32> GetMapContentTypeList()
        {
            return SpTile.GetMapContentTypeList();
        }


        public override List<CmnObjGroup> LoadObjGroup(uint tileId, UInt32 type, UInt16 subType = 0xFFFF)
        {

            switch ((SpMapContentType)type)
            {
                case SpMapContentType.Link:

                    MapLink[] tmpMapLink = GetRoadLink(tileId, (byte)subType);
                    CmnObjGroup tmp = new CmnObjGroupArray(type, tmpMapLink, subType);
                    tmp.isDrawReverse = true;
                    return new List<CmnObjGroup> { tmp };

                case SpMapContentType.Node:

                    MapNode[] tmpMapNode = GetRoadNode(tileId, (byte)subType);
                    return new List<CmnObjGroup> { new CmnObjGroupArray(type, tmpMapNode, subType) };

                case SpMapContentType.LinkGeometry:

                    MapLinkFull[] tmpMapLinkGeometry = (MapLinkFull[])GetRoadGeometry(tileId, (byte)subType);
                    MapLinkGeometry[] tmpGeometry = tmpMapLinkGeometry.Select(x =>
                    {
                        MapLinkGeometry y = new MapLinkGeometry();
                        y.id = x.Id;
                        y.geometry = x.geometry;
                        return y;
                    }).ToArray();
                        
                    return new List<CmnObjGroup> { new CmnObjGroupArray(type, tmpGeometry, subType) };

                case SpMapContentType.LinkAttribute:

                    MapLinkFull[] tmpMapLinkAttr = (MapLinkFull[])GetRoadAttribute(tileId, (byte)subType);
                    MapLinkAttribute[] tmpAttribute = tmpMapLinkAttr.Select(x =>
                    {
                        MapLinkAttribute y = new MapLinkAttribute();
                        y.linkId = x.attribute.linkId;
                        y.wayId = x.attribute.wayId;
                        y.tagInfo = x.attribute.tagInfo;
                        return y;
                    }).ToArray();
                    return new List<CmnObjGroup> { new CmnObjGroupArray(type, tmpAttribute, subType) };
            }

            return null;
        }



        //public override List<CmnObjGroup> LoadObjGroupList(uint tileId, UInt32 type = 0xffffffff, ushort subType = 0xffff)
        //{
        //    return this.GetMapContentTypeList()
        //        .Where(x => (x & type) == x)
        //        .SelectMany(x => LoadObjGroup(tileId, x, subType))
        //        .ToList();
        //}

        public List<CmnObjGroup> LoadObjGroupList(uint tileId, uint type, Filter<ushort> filter)
        {
            throw new NotImplementedException();
        }

        //public override List<CmnObjGroup> LoadObjGroup(uint tileId, uint type, ushort subType)
        //{
        //    throw new NotImplementedException();
        //}
    }


}


