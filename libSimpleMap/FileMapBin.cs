using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using libGis;

namespace libSimpleMap
{
    class BinFileMapAccess : ISpMapAccess
    {
        bool compression = true;
        MiddleType middleType;
        IBinDataAccess dal;
        public bool IsConnected { get; set; }

        public BinFileMapAccess(MiddleType middleType)
        {
            this.middleType = middleType;
            IsConnected = false;
        }

        public int ConnectMapData(string connectStr)
        {

            switch (middleType)
            {
                //case MiddleType.FileSystem:
                //    this.connectStr = connectStr + @"\";
                //    dal = new FileAccessLib();
                //    break;

                case MiddleType.SQLite:
                    dal = new SQLiteAccess();
                    break;

                case MiddleType.Postgres:
                    dal = new PostgresAccess();
                    break;

                default:
                    break;
            }

            int ret = dal.Connect(connectStr);
            if (ret == 0)
                IsConnected = true;

            return ret;
        }

        public int DisconnectMapData()
        {
            return 0;
        }


        //public CmnTile CreateTile(uint tileId)
        //{
        //    return new SpTile(tileId);
        //}

        public MapLink[] GetRoadLink(uint tileId, ushort maxRoadType = 0xFFFF)
        {
            List<MapLink> tmpLinkList = new List<MapLink>();

            byte[] tileBuf = dal.GetLinkData(tileId);
            if (tileBuf == null)
                return tmpLinkList.ToArray();


            if (compression)
            {
                Zlib zlib = new Zlib();
                tileBuf = zlib.Decompress(tileBuf);

            }

            byte[] tmpBuf = new byte[1024];
            TileXY baseTileXY = new TileXY(tileId);
            //using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (MemoryStream ms = new MemoryStream(tileBuf))
            {
                //fs.Read(tileBuf, 0, (int)fs.Length);
                //fs.CopyTo(ms, (int)fs.Length);


                //データ数
                ms.Read(tmpBuf, 0, 2);
                ushort numLink = BitConverter.ToUInt16(tmpBuf, 0);
                for (ushort i = 0; i < numLink; i++)
                {
                    MapLink tmpLink;
                    if (false)
                    {
                        ms.Read(tmpBuf, 0, 8);
                        tmpLink = new MapLink(tmpBuf);
                        tmpLink.edgeNodeTileId[0] = tileId;
                        tmpLink.edgeNodeTileId[1] = GisTileCode.S_CalcTileId((ushort)(baseTileXY.x + tmpLink.endNodeTileOffset.offsetX), (ushort)(baseTileXY.y + tmpLink.endNodeTileOffset.offsetY));
                    }
                    else
                    {
                        tmpLink = new MapLink();
                        tmpLink.index = i;


                        ms.Read(tmpBuf, 0, 2);
                        tmpLink.edgeNodeIndex[0] = BitConverter.ToUInt16(tmpBuf, 0);

                        ms.Read(tmpBuf, 0, 1);
                        sbyte offsetX = (sbyte)tmpBuf[0];
                        tmpLink.endNodeTileOffset.offsetX = offsetX;

                        tmpLink.edgeNodeTileId[0] = tileId;

                        ms.Read(tmpBuf, 0, 1);
                        sbyte offsetY = (sbyte)tmpBuf[0];
                        tmpLink.edgeNodeTileId[1] = GisTileCode.S_CalcTileId((ushort)(baseTileXY.x + offsetX), (ushort)(baseTileXY.y + offsetY));
                        tmpLink.endNodeTileOffset.offsetY = offsetY;

                        ms.Read(tmpBuf, 0, 2);
                        tmpLink.edgeNodeIndex[1] = BitConverter.ToUInt16(tmpBuf, 0);

                        ms.Read(tmpBuf, 0, 2);
                        tmpLink.linkCost = BitConverter.ToUInt16(tmpBuf, 0);

                        ms.Read(tmpBuf, 0, 2);
                        tmpLink.linkLength = BitConverter.ToUInt16(tmpBuf, 0);

                        ms.Read(tmpBuf, 0, 1);
                        tmpLink.roadType = tmpBuf[0];

                        ms.Read(tmpBuf, 0, 1);
                        tmpLink.fOneWay = (sbyte)tmpBuf[0];


                    }

                    if (tmpLink.roadType > maxRoadType)
                        break;

                    tmpLinkList.Add(tmpLink);

                    //test
                    //byte[] testByte = tmpLink.Serialize();
                    //MapLink testAfte = new MapLink(testByte);
                }

            }


            return tmpLinkList.ToArray();
        }


        public MapNode[] GetRoadNode(uint tileId, ushort maxRoadType = 0xFFFF)
        {
            List<MapNode> tmpNodeList = new List<MapNode>();

            byte[] tileBuf = dal.GetNodeData(tileId);
            if (tileBuf == null)
                return tmpNodeList.ToArray();

            if (compression)
            {
                Zlib zlib = new Zlib();
                tileBuf = zlib.Decompress(tileBuf);

            }

            TileXY baseTileXY = new TileXY(tileId);
            byte[] tmpBuf = new byte[32];

            //using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (MemoryStream ms = new MemoryStream(tileBuf))
            {
                //fs.Read(tileBuf, 0, (int)fs.Length);
                //fs.CopyTo(ms, (int)fs.Length);

                //データ数
                ms.Read(tmpBuf, 0, 2);
                ushort numNode = BitConverter.ToUInt16(tmpBuf, 0);

                for (int i = 0; i < numNode; i++)
                {
                    MapNode tmpNode = new MapNode();

                    ms.Read(tmpBuf, 0, 2);
                    ushort numConnectLink = BitConverter.ToUInt16(tmpBuf, 0);
                    List<ConnectLink> tmpConnectLinkList = new List<ConnectLink>();

                    byte tmpMinRoadType = 0xff;
                    for (int j = 0; j < numConnectLink; j++)
                    {
                        ConnectLink tmpLink = new ConnectLink();

                        ms.Read(tmpBuf, 0, 1);
                        sbyte offsetX = (sbyte)tmpBuf[0];

                        ms.Read(tmpBuf, 0, 1);
                        sbyte offsetY = (sbyte)tmpBuf[0];

                        tmpLink.tileId = GisTileCode.S_CalcTileId((ushort)(baseTileXY.x + offsetX), (ushort)(baseTileXY.y + offsetY));

                        ms.Read(tmpBuf, 0, 2);
                        tmpLink.linkIndex = BitConverter.ToUInt16(tmpBuf, 0);

                        ms.Read(tmpBuf, 0, 1);
                        tmpLink.linkDirection = (byte)tmpBuf[0];

                        ms.Read(tmpBuf, 0, 1);
                        tmpLink.roadType = (byte)tmpBuf[0];
                        if (tmpLink.roadType < tmpMinRoadType)
                            tmpMinRoadType = tmpLink.roadType;

                        ms.Read(tmpBuf, 0, 1);
                        ms.Read(tmpBuf, 0, 1);

                        tmpConnectLinkList.Add(tmpLink);
                    }

                    tmpNode.connectLink = tmpConnectLinkList.ToArray();
                    

                    if (tmpMinRoadType > maxRoadType)
                    {
                        break;
                    }
                    tmpNodeList.Add(tmpNode);

                }
                return tmpNodeList.ToArray();
            }
        }


        public MapLink[] GetRoadGeometry(uint tileId, ushort maxRoadType = 0xFFFF)
        {
            List<MapLink> tmpLinkShapeList = new List<MapLink>();

            byte[] tileBuf = dal.GetGeometryData(tileId);
            if (tileBuf == null)
                return tmpLinkShapeList.ToArray();


            if (compression)
            {
                Zlib zlib = new Zlib();
                tileBuf = zlib.Decompress(tileBuf);

            }

            TileXY baseTileXY = new TileXY(tileId);
            byte[] tmpBuf = new byte[32];

            //using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (MemoryStream ms = new MemoryStream(tileBuf))
            {
                //データ数
                ms.Read(tmpBuf, 0, 2);
                ushort numLink = BitConverter.ToUInt16(tmpBuf, 0);

                for (int i = 0; i < numLink; i++)
                {
                    MapLink tmpLink = new MapLink();
                    LatLon sLatLon = new LatLon();
                    List<LatLon> tmpGeometry = new List<LatLon>();

                    ms.Read(tmpBuf, 0, 4);
                    int tmpLat = BitConverter.ToInt32(tmpBuf, 0);
                    sLatLon.lat = (double)tmpLat / 1000000.0;

                    ms.Read(tmpBuf, 0, 4);
                    int tmpLon = BitConverter.ToInt32(tmpBuf, 0);
                    sLatLon.lon = (double)tmpLon / 1000000.0;

                    tmpGeometry.Add(sLatLon);

                    ms.Read(tmpBuf, 0, 2);
                    ushort numGeometry = BitConverter.ToUInt16(tmpBuf, 0);

                    for (int j = 0; j < numGeometry; j++)
                    {
                        LatLon tmpLatLon = new LatLon();

                        ms.Read(tmpBuf, 0, 2);
                        short tmpLatDiff = BitConverter.ToInt16(tmpBuf, 0);
                        tmpLat = tmpLat + BitConverter.ToInt16(tmpBuf, 0);
                        tmpLatLon.lat = tmpLat / 1000000.0;

                        ms.Read(tmpBuf, 0, 2);
                        short tmpLonDiff = BitConverter.ToInt16(tmpBuf, 0);
                        tmpLon = tmpLon + BitConverter.ToInt16(tmpBuf, 0);
                        tmpLatLon.lon = tmpLon / 1000000.0;

                        tmpGeometry.Add(tmpLatLon);
                    }
                    tmpLink.geometry = tmpGeometry.ToArray();
                    tmpLinkShapeList.Add(tmpLink);
                }

                //tmpLinkShapeList.ForEach(x =>
                //{
                //    x.geometry = LatLon.DouglasPeuker(x.geometry, 3.0, 2000.0);
                //});

                return tmpLinkShapeList.ToArray();
            }
        }

        public MapLink[] GetRoadAttribute(uint tileId, ushort maxRoadType = 0xFFFF)
        {
            //List<MapLink> tmpLinkAttributeList = new List<MapLink>();

            byte[] tileBuf = dal.GetAttributeData(tileId);
            if (tileBuf == null)
                return null;
            
            if (compression)
            {
                Zlib zlib = new Zlib();
                tileBuf = zlib.Decompress(tileBuf);
            }

            byte[] tmpBuf = new byte[32];
            byte[] tmpStrBuf;

            using (MemoryStream ms = new MemoryStream(tileBuf))
            {
                //データ数
                ms.Read(tmpBuf, 0, 2);
                ushort numLink = BitConverter.ToUInt16(tmpBuf, 0);

                MapLink[] attrArray = new MapLink[numLink];

                for (int i = 0; i < numLink; i++)
                {
                    //MapLink tmpLink = new MapLink();
                    MapLink tmpAttr = new MapLink();
                    tmpAttr.attribute = new LinkAttribute();

                    ms.Read(tmpBuf, 0, 8);
                    tmpAttr.attribute.linkId = BitConverter.ToUInt64(tmpBuf, 0);
                    //tmpAttr.linkId = tmpAttr.attribute.linkId;
                    
                    ms.Read(tmpBuf, 0, 8);
                    tmpAttr.attribute.wayId = BitConverter.ToUInt64(tmpBuf, 0);


                    ms.Read(tmpBuf, 0, 2);
                    ushort numTag = BitConverter.ToUInt16(tmpBuf, 0);


                    for (int j = 0; j < numTag; j++)
                    {
                        LatLon tmpLatLon = new LatLon();

                        ms.Read(tmpBuf, 0, 2);
                        ushort tagNameSize = BitConverter.ToUInt16(tmpBuf, 0);

                        tmpStrBuf = new byte[tagNameSize];
                        ms.Read(tmpStrBuf, 0, tagNameSize);
                        string tagName = System.Text.Encoding.UTF8.GetString(tmpStrBuf);

                        ms.Read(tmpBuf, 0, 2);
                        ushort tagValSize = BitConverter.ToUInt16(tmpBuf, 0);

                        tmpStrBuf = new byte[tagValSize];
                        ms.Read(tmpStrBuf, 0, tagValSize);
                        string tagVal = System.Text.Encoding.UTF8.GetString(tmpStrBuf);

                        System.Text.Encoding.UTF8.GetString(tmpStrBuf);

                        tmpAttr.attribute.tagInfo.Add(new TagInfo(tagName, tagVal));

                    }

                    attrArray[i] = tmpAttr;
                }
                return attrArray;
            }
        }



        public List<uint> GetMapTileIdList()
        {
            //SQLite
            return dal.GetMapTileIdList();

            //Filesystem

            //List<uint> retList = new List<uint>();
            //string[] names = Directory.GetFiles(mapPath + "NODE", "*.txt");
            //foreach (string name in names)
            //{
            //    string tileName = name.Replace(mapPath + "NODE\\", "").Replace("_NODE.txt", "");
            //    retList.Add(uint.Parse(tileName));

            //}
            //return retList;
        }


        public int SaveTile(SpTile tile)
        {
            switch (middleType)
            {
                //case MiddleType.FileSystem:
                //    SaveRoadLink(tile);
                //    SaveRoadNode(tile);
                //    SaveRoadGeometry(tile);
                //    break;

                case MiddleType.SQLite:
                case MiddleType.Postgres:

                    byte[] linkBuf = MakeRoadLinkBin(tile);
                    byte[] nodeBuf = MakeRoadNodeBin(tile);
                    byte[] geometryBuf = MakeRoadGeometryBin(tile);
                    byte[] attributeBuf = MakeLinkAttributeBin(tile);

                    if (compression)
                    {
                        Zlib zlib = new Zlib();
                        linkBuf = zlib.Compress(linkBuf);
                        nodeBuf = zlib.Compress(nodeBuf);
                        geometryBuf = zlib.Compress(geometryBuf);
                        attributeBuf = zlib.Compress(attributeBuf);
                    }

                    dal.SaveAllData(tile.tileId, linkBuf, nodeBuf, geometryBuf, attributeBuf);

                    break;

                default:
                    break;
            }

            return 0;
        }


        public int SaveRoadLink(SpTile tile)
        {

            byte[] tileBuf = MakeRoadLinkBin(tile);

            if (compression)
            {
                Zlib zlib = new Zlib();
                tileBuf = zlib.Compress(tileBuf);
            }
            dal.SaveLinkData(tile.tileId, tileBuf, tileBuf.Length);

            return 0;
        }

        public byte[] MakeRoadLinkBin(SpTile tile)
        {
            byte[] tileBuf = new byte[1024 * 1024];
            int bufLength;
            byte[] tmpBuf;
            uint tileId = tile.tileId;
            SpTile tmpTile = tile;

            TileXY baseTileXY = new TileXY(tileId);


            using (MemoryStream ms = new MemoryStream(tileBuf))
            {
                //データ数
                tmpBuf = BitConverter.GetBytes((short)tmpTile.link.Length);
                ms.Write(tmpBuf, 0, 2);

                foreach (MapLink tmpLink in tmpTile.link)
                {

                    TileXY tileXY = new TileXY(tmpLink.edgeNodeTileId[1]);

                    if (false)
                    {

                        tmpLink.endNodeTileOffset.offsetX = (sbyte)(tileXY.x - baseTileXY.x);
                        tmpLink.endNodeTileOffset.offsetY = (sbyte)(tileXY.y - baseTileXY.y);

                        tmpBuf = tmpLink.Serialize();
                        ms.Write(tmpBuf, 0, 8);

                        //test
                        //MapLink testAfte = new MapLink(tmpBuf);


                    }
                    else
                    {

                        tmpBuf = BitConverter.GetBytes((short)tmpLink.edgeNodeIndex[0]);
                        ms.Write(tmpBuf, 0, 2);

                        //TileXY tileXY = new TileXY(tmpLink.GetEdgeNodeTileOffset(1).ToTileId(tile.tileId));

                        tmpBuf = BitConverter.GetBytes((sbyte)(tileXY.x - baseTileXY.x));
                        ms.Write(tmpBuf, 0, 1);

                        tmpBuf = BitConverter.GetBytes((sbyte)(tileXY.y - baseTileXY.y));
                        ms.Write(tmpBuf, 0, 1);

                        tmpBuf = BitConverter.GetBytes((short)tmpLink.edgeNodeIndex[1]);
                        ms.Write(tmpBuf, 0, 2);

                        tmpBuf = BitConverter.GetBytes((short)tmpLink.linkCost);
                        ms.Write(tmpBuf, 0, 2);

                        tmpBuf = BitConverter.GetBytes((short)tmpLink.linkLength);
                        ms.Write(tmpBuf, 0, 2);

                        tmpBuf = BitConverter.GetBytes((byte)tmpLink.roadType);
                        ms.Write(tmpBuf, 0, 1);

                        tmpBuf = BitConverter.GetBytes((sbyte)tmpLink.fOneWay);
                        ms.Write(tmpBuf, 0, 1);
                    }

                }

                bufLength = (int)ms.Position;

            }
            Array.Resize(ref tileBuf, bufLength);

            return tileBuf;
        }


        public int SaveRoadNode(SpTile tile)
        {
            byte[] tileBuf = MakeRoadNodeBin(tile);

            if (compression)
            {
                Zlib zlib = new Zlib();
                tileBuf = zlib.Compress(tileBuf);
            }

            dal.SaveNodeData(tile.tileId, tileBuf, tileBuf.Length);

            return 0;
        }

        public byte[] MakeRoadNodeBin(SpTile tile)
        {
            uint tileId = tile.tileId;
            SpTile tmpTile = tile;
            int bufLength;

            byte[] tileBuf = new byte[1024 * 1024];
            byte[] tmpBuf;
            TileXY baseTileXY = new TileXY(tileId);


            using (MemoryStream ms = new MemoryStream(tileBuf))
            {

                //データ数
                tmpBuf = BitConverter.GetBytes((short)tmpTile.node.Length);
                ms.Write(tmpBuf, 0, 2);

                foreach (MapNode tmpNode in tmpTile.node)
                {
                    tmpBuf = BitConverter.GetBytes((short)tmpNode.connectLink.Length);
                    ms.Write(tmpBuf, 0, 2);

                    foreach (ConnectLink link in tmpNode.connectLink)
                    {
                        TileXY tileXY = new TileXY(link.tileId);

                        tmpBuf = BitConverter.GetBytes((sbyte)(tileXY.x - baseTileXY.x));
                        ms.Write(tmpBuf, 0, 1);

                        tmpBuf = BitConverter.GetBytes((sbyte)(tileXY.y - baseTileXY.y));
                        ms.Write(tmpBuf, 0, 1);

                        tmpBuf = BitConverter.GetBytes((short)link.linkIndex);
                        ms.Write(tmpBuf, 0, 2);

                        tmpBuf = BitConverter.GetBytes((byte)link.linkDirection);
                        ms.Write(tmpBuf, 0, 1);

                        tmpBuf = BitConverter.GetBytes((byte)link.roadType);
                        //tmpBuf = BitConverter.GetBytes((byte)link.transReg.Count);
                        ms.Write(tmpBuf, 0, 1);

                        tmpBuf[0] = 0;
                        //tmpBuf = BitConverter.GetBytes((byte)link.transCost.Count);
                        ms.Write(tmpBuf, 0, 1);

                        //reserved
                        tmpBuf[0] = 0;
                        ms.Write(tmpBuf, 0, 1);
                    }
                }
                bufLength = (int)ms.Position;

            }

            Array.Resize(ref tileBuf, bufLength);

            return tileBuf;
        }


        public int SaveRoadGeometry(SpTile tile)
        {
            byte[] tileBuf = MakeRoadGeometryBin(tile);

            if (compression)
            {
                Zlib zlib = new Zlib();
                tileBuf = zlib.Compress(tileBuf);
            }
            dal.SaveGeometryData(tile.tileId, tileBuf, tileBuf.Length);

            return 0;
        }

        public byte[] MakeRoadGeometryBin(SpTile tile)
        {
            uint tileId = tile.tileId;
            SpTile tmpTile = tile;

            int bufLength;

            byte[] tileBuf = new byte[2 * 1024 * 1024];
            byte[] tmpBuf;

            TileXY baseTileXY = new TileXY(tileId);

            using (MemoryStream ms = new MemoryStream(tileBuf))
            {


                //データ数
                tmpBuf = BitConverter.GetBytes((short)tmpTile.link.Length);
                ms.Write(tmpBuf, 0, 2);

                foreach (MapLink tmpLink in tmpTile.link)
                {

                    LatLon[] tmpGeometry = tmpLink.geometry;
                    //tmpGeometry = LatLon.DouglasPeuker(tmpLink.geometry, 3.0, 2000.0);

                    tmpBuf = BitConverter.GetBytes((int)(tmpGeometry[0].lat * 1000000));
                    ms.Write(tmpBuf, 0, 4);

                    tmpBuf = BitConverter.GetBytes((int)(tmpGeometry[0].lon * 1000000));
                    ms.Write(tmpBuf, 0, 4);

                    tmpBuf = BitConverter.GetBytes((short)(tmpGeometry.Length - 1));
                    ms.Write(tmpBuf, 0, 2);

                    for (int i = 1; i < tmpGeometry.Length; i++)
                    {

                        tmpBuf = BitConverter.GetBytes((short)(tmpGeometry[i].lat * 1000000) - (short)(tmpGeometry[i - 1].lat * 1000000));
                        ms.Write(tmpBuf, 0, 2);

                        tmpBuf = BitConverter.GetBytes((short)(tmpGeometry[i].lon * 1000000) - (short)(tmpGeometry[i - 1].lon * 1000000));
                        ms.Write(tmpBuf, 0, 2);

                    }

                }
                bufLength = (int)ms.Position;
            }

            Array.Resize(ref tileBuf, bufLength);
            //Console.WriteLine($"bufLength = {bufLength/1024/1024}");

            return tileBuf;
        }

        //public offsetLatLon ConvertGeometryOffset(LatLon[] geometry)
        //{
        //    for (int i = 1; i < geometry.Length; i++)
        //    {
        //        int offLatInt = (int)(geometry[i].lat * 1000000) - (int)(geometry[i - 1].lat * 1000000);
        //        int offLonInt = (int)(geometry[i].lon * 1000000) - (int)(geometry[i - 1].lon * 1000000);

        //        if(offLatInt > 32767)
        //        {

        //        }
        //    }

        //}

        //struct offsetLatLon
        //{
        //    short offsetLat;
        //    short offsetLon;
        //}

        public byte[] MakeLinkAttributeBin(SpTile tile)
        {
            uint tileId = tile.tileId;
            SpTile tmpTile = tile;

            int bufLength;

            byte[] tileBuf = new byte[8 * 1024 * 1024];
            byte[] tmpBuf;
            byte[] tmpStrBuf;

            using (MemoryStream ms = new MemoryStream(tileBuf))
            {
                //データ数
                tmpBuf = BitConverter.GetBytes((short)tmpTile.link.Length);
                ms.Write(tmpBuf, 0, 2);

                foreach (MapLink tmpLinkAttr in tmpTile.linkAttr)
                {
                    tmpBuf = BitConverter.GetBytes((ulong)tmpLinkAttr.linkId);
                    ms.Write(tmpBuf, 0, 8);

                    tmpBuf = BitConverter.GetBytes((ulong)tmpLinkAttr.attribute.wayId);
                    ms.Write(tmpBuf, 0, 8);

                    int numTag = tmpLinkAttr.attribute.tagInfo.Count;
                    tmpBuf = BitConverter.GetBytes((short)numTag);
                    ms.Write(tmpBuf, 0, 2);

                    for (int i = 0; i < numTag; i++)
                    {
                        tmpStrBuf = System.Text.Encoding.UTF8.GetBytes(tmpLinkAttr.attribute.tagInfo[i].tag);
                        tmpBuf = BitConverter.GetBytes((ushort)tmpStrBuf.Length);
                        ms.Write(tmpBuf, 0, 2);
                        ms.Write(tmpStrBuf, 0, tmpStrBuf.Length);

                        tmpStrBuf = System.Text.Encoding.UTF8.GetBytes(tmpLinkAttr.attribute.tagInfo[i].val);
                        if(tmpStrBuf.Length > 0xffff)
                        {
                            throw new NotImplementedException();
                        }
                        tmpBuf = BitConverter.GetBytes((ushort)tmpStrBuf.Length);
                        ms.Write(tmpBuf, 0, 2);
                        ms.Write(tmpStrBuf, 0, tmpStrBuf.Length);
                        if (tmpStrBuf.Length > 0xffff)
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
                bufLength = (int)ms.Position;
            }

            Array.Resize(ref tileBuf, bufLength);

            return tileBuf;
        }




        public List<UInt32> GetMapContentTypeList()
        {
            return SpTile.GetMapContentTypeList();

            //return ((uint[])Enum.GetValues(typeof(SpMapContentType)))
            //    .Select(x => (UInt16)x)
            //    .Where(x => (UInt16)x != 0xFFFF)
            //    .ToList();
        }

        CmnTile LoadTile(uint tileId, UInt32 reqType = 0xFFFFFFFF, UInt16 reqMaxSubType = 0xFFFF)
        {
            List<UInt32> objTypeList = GetMapContentTypeList();
            SpTile tmpTile = new SpTile(tileId);

            foreach (UInt32 objType in objTypeList)
            {
                if ((reqType & objType) == objType)
                {
                    //読み込み
                    tmpTile.UpdateObjGroup(LoadObjGroup(tileId, objType, reqMaxSubType));
                }
            }

            return tmpTile;

        }

        public CmnObjGroup LoadObjGroup(uint tileId, UInt32 type, UInt16 subType = 0xFFFF)
        {
           
            switch((SpMapContentType)type)
            {
                case SpMapContentType.Link:

                    MapLink[] tmpMapLink = GetRoadLink(tileId, (byte)subType);
                    SpObjGroup tmp = new SpObjGroup(type, tmpMapLink, subType);
                    tmp.isDrawReverse = true;
                    return tmp;

                case SpMapContentType.Node:

                    MapNode[] tmpMapNode = GetRoadNode(tileId, (byte)subType);
                    return new SpObjGroup(type, tmpMapNode, subType);

                case SpMapContentType.LinkGeometry:

                    MapLink[] tmpMapLinkGeometry = GetRoadGeometry(tileId, (byte)subType);
                    return new SpObjGroup(type, tmpMapLinkGeometry, subType);

                case SpMapContentType.LinkAttribute:

                    MapLink[] tmpMapLinkAttr = GetRoadAttribute(tileId, (byte)subType);
                    return new SpObjGroup(type, tmpMapLinkAttr, subType);
            }

            return null;
        }

        public List<CmnObjGroup> LoadObjGroupList(uint tileId, UInt32 type = 0xffffffff, ushort subType = ushort.MaxValue)
        {
            return this.GetMapContentTypeList()
                .Where(x => (x & type) == x)
                .Select(x => LoadObjGroup(tileId, x, subType))
                .ToList();
        }
    }


    public abstract class IBinDataAccess
    {
        public abstract int Connect(string mapPath);
        public abstract int Disconnect();
        public abstract List<uint> GetMapTileIdList();
        public abstract byte[] GetRawData(uint tileId, SpMapContentType contentType);
        public abstract byte[] GetLinkData(uint tileId);
        public abstract byte[] GetNodeData(uint tileId);
        public abstract byte[] GetGeometryData(uint tileId);
        public abstract byte[] GetAttributeData(uint tileId);

        public abstract int SaveLinkData(uint tileId, byte[] tileBuf, int size);
        public abstract int SaveNodeData(uint tileId, byte[] tileBuf, int size);
        public abstract int SaveGeometryData(uint tileId, byte[] tileBuf, int size);

        public abstract int SaveAllData(uint tileId, byte[] linkBuf, byte[] nodeBuf, byte[] geometryBuf, byte[] attributeBuf);
    }
}
