using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using libGis;
using System.Drawing;


namespace libSimpleMap
{

    public class SpTile : CmnTile //, ITile
    {
        public MapLink[] link; //並びは道路種別
        public MapNode[] node;
        public MapLink[] linkGeometry;
        public MapLink[] linkAttr;


        //public byte loadedRoadType = 0;
        //public byte loadedLinkLevel = 0;
        //public byte loadedNodeLevel = 0;
        //public byte loadedGeometryLevel = 0;
        //public byte loadedAttributeLevel = 0;



        public uint x { get; private set; }
        public uint y { get; private set; }
        //public SpTile tile { get; protected set; }

        public SpTile() { }
        public SpTile(uint tileId)
        {
            tileInfo = new GisTileCode(tileId);


            objDic = new Dictionary<UInt16, CmnObjGroup>();
            //objDic.Add((UInt16)SpMapContentType.Link, new SpObjGroup());
            //objDic.Add((UInt16)SpMapContentType.Node, new SpObjGroup());



        }

        override public CmnTile CreateTile(uint tileId)
        {
            return new SpTile(tileId);
        }

        public MapNode GetMapNode(Int64 nodeId)
        {
            foreach (MapNode mapNode in node)
            {
                if (mapNode.nodeId == nodeId)
                    return mapNode;
            }
            return null;
        }


        public LinkHandle GetMapLink(int linkIndex)
        {
            if (linkIndex >= link.Length)
                return null;

            return new LinkHandle(this, link[linkIndex]);
            //return link[linkIndex];
        }

        public NodeHandle GetMapNode(int targetNodeIndex)
        {
            if (targetNodeIndex >= node.Length)
                return null;

            return new NodeHandle(this, node[targetNodeIndex]);
        }



        public override int UpdateObjGroup(UInt16 objType, CmnObjGroup objGroup) /* abstract ?? */
        {
            //上書き
            objDic[objType] = objGroup;

            switch ((SpMapContentType)objType)
            {
                case SpMapContentType.Link:
                    link = (MapLink[])objDic[objType].objArray;

                    break;

                case SpMapContentType.Node:
                    node = (MapNode[])objDic[objType].objArray;
                    objGroup.isDrawable = false;
                    objGroup.isGeoSearchable = false;
                    break;


                case SpMapContentType.LinkGeometry:
                    linkGeometry = (MapLink[])objDic[objType].objArray;
                    MergeGeometry(linkGeometry, true);
                    objGroup.isDrawable = false;
                    objGroup.isGeoSearchable = false;
                    break;

                case SpMapContentType.LinkAttribute:
                    linkAttr = (MapLink[])objDic[objType].objArray;
                    MergeAttribute(linkGeometry, true);
                    objGroup.isDrawable = false;
                    objGroup.isGeoSearchable = false;
                    break;

                default:
                    return 0;
            }

            return 0;
        }


        public bool MergeGeometry(MapLink[] shapeLinkList, bool ignoreError)
        {
            if (!ignoreError)
            {
                if (link.Length != shapeLinkList.Length)
                {
                    Console.WriteLine("link num not match");
                    return false;
                }
            }
            for (int i = 0; i < link.Length; i++)
            {
                if (link[i].linkId != shapeLinkList[i].linkId)
                {
                    return false;
                }
            }
            for (int i = 0; i < link.Length; i++)
            {
                link[i].geometry = shapeLinkList[i].geometry;
            }
            return true;
        }


        public bool MergeAttribute(MapLink[] attrLinkList, bool ignoreError)
        {
            if (!ignoreError)
            {
                if (link.Length != attrLinkList.Length)
                {
                    Console.WriteLine("link num not match");
                    return false;
                }
            }
            for (int i = 0; i < link.Length; i++)
            {
                if (link[i].linkId != attrLinkList[i].linkId)
                {
                    return false;
                }
            }
            for (int i = 0; i < link.Length; i++)
            {
                link[i].attribute = attrLinkList[i].attribute;
            }
            return true;
        }



        //public void PrintGeometry()
        //{
        //    foreach (var mapLink in link)
        //    {
        //        mapLink.PrintGeometry(1);
        //        Console.WriteLine("");
        //    }

        //}

        public int WriteMap(string fileName, bool append)
        {
            StreamWriter sw = new StreamWriter(fileName, append);
            if (sw == null)
                return -1;
            foreach (MapLink mapLink in link)
            {
                mapLink.WriteGeometry(1, sw);
                sw.WriteLine("");
            }
            sw.Close();
            return 0;

        }


        public LinkHandle SearchNearestMapLink(LatLon latlon, byte maxRoadType = 0xFF)
        {

            MapLink tmpLink = link
                .Where(x => x.roadType <= maxRoadType)
                .OrderBy(x => x.GetDistance(latlon))
                .FirstOrDefault();

            return new LinkHandle(this, tmpLink);

        }


        public LinkDistance SearchNearestMapLink2(LatLon latlon, byte maxRoadType = 0xFF)
        {

            MapLink tmpLink = link
                .Where(x => x.roadType <= maxRoadType)
                .OrderBy(x => x.GetDistance(latlon))
                .FirstOrDefault();


            return tmpLink.GetDistance2(latlon);

        }


        public int CalcLinkCost()
        {
            Array.ForEach(link, x => x.CalcCost());
            //link.ForEach(x => x.CalcCost());

            return 0;
        }



        public static List<UInt16> GetMapContentTypeList()
        {
            return ((uint[])Enum.GetValues(typeof(SpMapContentType)))
                .Select(x => (UInt16)x)
                .Where(x => (UInt16)x != 0xFFFF)
                .ToList();
        }
    }

    
    public class SpObjGroup : CmnObjGroup
    {
        public override UInt16 Type { get; }

        public SpObjGroup(UInt16 type)
        {
            Type = type;
        }
    }

    public class MapLink : CmnObj
    {
        // public SpTile tile; //消して、参照構造体で扱う？
        public UInt64 linkId; //リンク属性の方に移動
        public short index;
        public Int64[] edgeNodeId; //必ず２つ。[0]:始点側 [1]:終点側 //容量削減可能
        public short[] edgeNodeIndex; //必ず２つ。[0]:始点側 [1]:終点側
        public uint[] edgeNodeTileId; //オフセット位置。終点のみ

        public TileOffset2 endNodeTileOffset;

        public byte roadType; //4bit
        public short linkLength;
        public short linkCost;
        public sbyte fOneWay; //表示用にここにも

        public LatLon[] geometry { get; set; }
        public LinkAttribute attribute { get; set; }

        public override UInt64 Id { get { return linkId; } }
        public override UInt16 Type { get { return (UInt16)SpMapContentType.Link; } }
        public override UInt16 SubType { get { return (UInt16)roadType; } }
        public override LatLon[] Geometry { get { return geometry; }  }

        //public Int64 linkId
        //{
        //    get{ return 0; }
        //    set{; }
        //}

        //public Int64[] edgeNodeId
        //{
        //    get { return null; }
        //    set {; }
        //}

        public TileOffset2 GetEdgeNodeTileOffset(sbyte direction)
        {
            if (direction == 0)
                return new TileOffset2(0);
            else if (direction == 1)
                return endNodeTileOffset;
            else
                return new TileOffset2(0);
        }

        //public Int64 upLinkId;
        //public Int64[] downLinkId;


        public MapLink()
        {
            edgeNodeId = new Int64[2];
            edgeNodeIndex = new short[2];
            edgeNodeTileId = new uint[2];
        }

        public void PrintGeometry(int direction)
        {
            if (direction == 0)
            {
                for (int i = geometry.Length - 1; i >= 0; i--)
                {
                    Console.WriteLine($"{geometry[i].lon}\t{geometry[i].lat}");
                }

            }
            else
            {
                for (int i = 0; i < geometry.Length; i++)
                {
                    Console.WriteLine($"{geometry[i].lon}\t{geometry[i].lat}");
                }
            }
        }

        public int WriteGeometry(int direction, StreamWriter sw)
        {
            if (direction == 0)
            {
                for (int i = geometry.Length - 1; i >= 0; i--)
                {
                    sw.WriteLine($"{geometry[i].lon}\t{geometry[i].lat}");
                }
            }
            else
            {
                for (int i = 0; i < geometry.Length; i++)
                {
                    sw.WriteLine($"{geometry[i].lon}\t{geometry[i].lat}");
                }
            }
            return 0;
        }



        public LatLon[] GetGeometry(int direction)
        {
            if (direction == 0)
                return geometry.Reverse().ToArray();
            else
                return geometry;
        }

        public override double GetDistance(LatLon latlon)
        {
            return latlon.GetDistanceToPolyline(geometry);

        }


        public LinkDistance GetDistance2(LatLon latlon)
        {
            if (geometry.Count() <= 1)
                return null;

            double minDistance = Double.MaxValue;
            int index = -1;

            for (int i = 0; i < geometry.Count() - 1; i++)
            {
                double tmp = latlon.GetDistanceToLine(geometry[i], geometry[i + 1]);
                if (tmp < minDistance)
                {
                    minDistance = tmp;
                    index = i;
                }
            }

            double offset = 0;
            for (int i = 0; i < index; i++)
            {
                offset += geometry[i].GetDistanceTo(geometry[i + 1]);
            }
            LinkPos linkPos = new LinkPos(null, this, offset);

            return new LinkDistance(linkPos, minDistance);

        }


        public int CalcCost()
        {
            if (geometry.Length <= 1) return -1;

            double tmpLength = 0;

            for (int i = 1; i < geometry.Length; i++)
            {
                tmpLength += geometry[i].GetDistanceTo(geometry[i - 1]);
            }

            linkLength = (short)tmpLength;

            double weight = 1.0;
            if (roadType <= 2) weight = 0.5;
            else if (roadType <= 4) weight = 0.6;
            else if (roadType <= 6) weight = 0.7;

            linkCost = (short)(linkLength * weight);
            return 0;

        }


        public int show_geometry()
        {
            //    Console.WriteLine("◆LINK Id={0}", linkId);
            //    for (int i = 0; i < shape.numCoord; i++)
            //    {
            //        Console.WriteLine("  {0}:\tX:{1}\ty:{2}", i, shape.geometry[i].x, shape.geometry[i].y);

            //    }
            return 0;
        }

        public int show_geometry_for_plot()
        {
            //for (int i = 0; i < shape.numCoord; i++)
            //{
            //    Console.WriteLine("{0}\t{1}\t{2}\t0", i, shape.geometry[i].x, shape.geometry[i].y);

            //}
            Console.WriteLine("");
            Console.WriteLine("");

            //Console.Write("\n\n");

            return 0;
        }

        public int show_geometry_for_plot3D(int direction, int start_z, int end_z)
        {
            //Console.WriteLine("<{0}\t{1}\t{2}>", direction, start_z, end_z);

            //if (direction == 1)
            //{
            //    for (int i = 0; i < shape.numCoord; i++)
            //    {
            //        Console.WriteLine("{0}\t{1}\t{2}\t{3}", i, shape.geometry[i].x, shape.geometry[i].y, start_z + (end_z - start_z) * i / (double)(shape.numCoord - 1));
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < shape.numCoord; i++)
            //    {
            //        Console.WriteLine("{0}\t{1}\t{2}\t{3}", i, shape.geometry[shape.numCoord - 1 - i].x, shape.geometry[shape.numCoord - 1 - i].y, start_z + ((end_z - start_z) * i) / (double)(shape.numCoord - 1));
            //    }
            //}
            //Console.WriteLine("");
            //Console.WriteLine("");

            return 0;
        }


        public override List<string[]> GetAttributeListItem()
        {
            List<string[]> listItem = new List<string[]>();

            string[] item;

            item = new string[] { "RoadType", $"{roadType}" };
            listItem.Add(item);

            //詳細計上表示
            if (true)
            {
                for (int i = 0; i < geometry.Length; i++)
                {
                    item = new string[] { $"geometry[{i}]", $"({geometry[i].lon:F7}, {geometry[i].lat:F7})" };
                    listItem.Add(item);
                }
            }
            //簡易表示
            else
            {
                item = new string[] { "geometry[S]", $"({geometry[0].lon:F7}, {geometry[0].lat:F7})" };
                listItem.Add(item);

                item = new string[] { "geometry[E]", $"({geometry[geometry.Length - 1].lon}, {geometry[geometry.Length - 1].lat})" };
                listItem.Add(item);
            }

            if (attribute != null)
            {
                item = new string[] { "Link ID", $"{attribute.linkId}" };
                listItem.Add(item);
                item = new string[] { "Way ID", $"{attribute.wayId}" };
                listItem.Add(item);
                for (int i = 0; i < attribute.tagInfo.Count; i++)
                {
                    item = new string[] { $"{attribute.tagInfo[i].tag}", $"{attribute.tagInfo[i].val}" };
                    listItem.Add(item);

                }

            }
            return listItem;

        }

    }



    public class MapLinkGeometry : CmnObj
    {
        public override UInt64 Id { get { return 0; } }
        public override UInt16 Type { get { return (UInt16)SpMapContentType.LinkGeometry; } }
        public override UInt16 SubType { get { return 0; } }
        public override LatLon[] Geometry { get { return null; } }
        public override double GetDistance(LatLon latlon) { return double.MaxValue; }
        public override List<string[]> GetAttributeListItem() { return null; }

    }

    public class MapLinkAttribute : CmnObj
    {
        public override UInt64 Id { get { return 0; } }
        public override UInt16 Type{ get { return (UInt16)SpMapContentType.LinkAttribute; } }
        public override UInt16 SubType { get { return 0; } }
        public override LatLon[] Geometry { get { return null; } }
        public override double GetDistance(LatLon latlon) { return double.MaxValue; }
        public override List<string[]> GetAttributeListItem() { return null; }
    }

    public class MapNode : CmnObj
    {
        //public SpTile tile; //検索時に記憶するようにする？
        public Int64 nodeId; //いずれ消す？
        public short index;
        public ConnectLink[] connectLink; //メッシュ内退出リンク

        //public bool f_upper_level; //いる？リンクにつける？
        //public bool f_zukakul; //いる？図郭外リンク数とどちらかでよい

        public MapNode()
        {
            //connectLink = new List<ConnectLink>();
        }


        public override UInt64 Id { get { return 0; } }
        public override UInt16 Type { get { return (UInt16)SpMapContentType.LinkAttribute; } }
        public override UInt16 SubType { get { return 0; } }
        public override LatLon[] Geometry { get { return null; } }
        public override double GetDistance(LatLon latlon) { return double.MaxValue; }
        public override List<string[]> GetAttributeListItem() { return null; }
    }

    public class ConnectLink
    {
        public uint tileId; //いずれオフセット位置にする？
        public Int64 linkId; //接続リンクId。不要となる予定
        public short linkIndex; //接続リンク。メッシュ内リンク番号。評価用
        public byte linkDirection; //接続リンクへ退出する方向がリンクの順逆方向かどうか。１：順方向。遷移禁止フラグ？いずれbool型
        public byte roadType;

        //public byte angle; //接続角度。評価用。４ビット？
        //public bool f_upper_level; //上位レベル有フラグ？道路種別によってフラグ設定する案もある
        //public bool f_reg; //接続リンクへ遷移する場合に規制あり
        //public bool f_cost; //接続リンクへ遷移する場合にコストあり
        //public bool f_multilink_start; //複合リンク規制またはコストの開始リンク
        //public bool f_multilink_mid; //複合リンク規制またはコストに関連するリンク
        //public List<Regulation> transReg; //接続リンクを最終リンクとする規制
        //public List<TransCost> transCost; //接続リンクを最終リンクとするコスト
    }

    public class LinkAttribute
    {
        public Int64 linkId;
        public Int64 wayId;
        //public byte link_type1;
        //public byte link_type2;
        //public byte link_type3;
        //public byte road_width;
        //public int road_name;
        //public int road_no;
        //public byte numLanes;
        //public bool tunnel;

        //tag
        public List<TagInfo> tagInfo;

        public LinkAttribute()
        {
            tagInfo = new List<TagInfo>();
        }
    }

    public class LinkHandle
    {
        public SpTile tile;
        public MapLink mapLink;

        public LinkHandle() { }

        public LinkHandle(SpTile tile, MapLink mapLink)
        {
            this.tile = tile;
            this.mapLink = mapLink;
        }

        public DLinkHandle ToDLinkHandle(byte direction)
        {
            return new DLinkHandle(tile, mapLink, direction);
        }

    }


    public class DLinkHandle
    {
        public SpTile tile;
        public MapLink mapLink;
        public byte direction;
        public uint tileId;

        public DLinkHandle() { }

        public DLinkHandle(SpTile tile, MapLink mapLink, byte direction)
        {
            this.tile = tile;
            this.mapLink = mapLink;
            this.direction = direction;
        }

        public DLinkHandle(SpTile tile, MapLink mapLink, byte direction, uint tileId, short linkIndex)
        {
            this.tile = tile;
            this.mapLink = mapLink;
            this.direction = direction;
        }

        public DLinkHandle(LinkHandle linkHdl, byte direction)
        {
            this.tile = linkHdl.tile;
            this.mapLink = linkHdl.mapLink;
            this.direction = direction;
        }

    }

    public class NodeHandle
    {
        public SpTile tile;
        public MapNode mapNode;
        public uint tileId;

        public NodeHandle() { }

        public NodeHandle(SpTile tile, MapNode mapNode)
        {
            this.tile = tile;
            this.mapNode = mapNode;
        }

        public NodeHandle(SpTile tile, MapNode mapNode, uint tileId, short nodeIndex)
        {
            this.tile = tile;
            this.mapNode = mapNode;
        }

    }


    public class LinkPos
    {
        public SpTile tile;
        public MapLink mapLink;
        public double offset;

        public LinkPos(SpTile tile, MapLink mapLink, double offset)
        {
            this.tile = tile;
            this.mapLink = mapLink;
            this.offset = offset;
        }

        public LinkHandle ToLinkHdl()
        {
            return new LinkHandle(tile, mapLink);
        }
    }

    public class LinkDistance
    {
        public LinkPos linkPos;
        public double distance;

        public LinkDistance(LinkPos linkPos, double distance)
        {
            this.linkPos = linkPos;
            this.distance = distance;
        }

    }


    public struct t_condition
    {
        int conditionId;
        int regulation1;
        int regulation2;
        int regulation3;
    }






    public class TileXY
    {
        public ushort x;
        public ushort y;

        public TileXY() { }

        public TileXY(uint tileId)
        {
            x = GisTileCode.GetX(tileId);
            y = GisTileCode.GetY(tileId);
        }

        public uint ToTileId()
        {
            return GisTileCode.SCalcTileId(x, y);
        }
    }


    public struct TileOffset2
    {
        public byte xy;

        public sbyte offsetX
        {
            get { return (sbyte)((sbyte)xy >> 4); }

            set { xy = (byte)((value << 4) | (xy & 0x0f)); }
        }

        public sbyte offsetY
        {
            get { return (sbyte)(((sbyte)((xy & 0x0f) << 4)) >> 4); }

            set { xy = (byte)(value & 0x0f | (xy & 0xf0)); }
        }

        public TileOffset2(byte xy)
        {
            this.xy = xy;
        }

    }

    public struct TileOffset
    {
        public sbyte offsetX;
        public sbyte offsetY;

        public TileOffset(uint tileId, uint baseTileId)
        {
            offsetX = (sbyte)(GisTileCode.GetX(tileId) - GisTileCode.GetX(baseTileId));
            offsetY = (sbyte)(GisTileCode.GetY(tileId) - GisTileCode.GetY(baseTileId));
        }

        public uint ToTileId(uint baseTileId)
        {
            return GisTileCode.SCalcTileId((ushort)(GisTileCode.GetX(baseTileId) + offsetX), (ushort)(GisTileCode.GetY(baseTileId) + offsetY));
        }
    }

    public enum MapDataType
    {
        TextFile,
        BinaryFile,
        SQLite,
        Postgres,
        MapManager
    }


    public enum MiddleType
    {
        FileSystem,
        SQLite,
        Postgres
    }

    public enum SpMapContentType
    {
        Link = 0x0001,
        Node = 0x0002,
        LinkGeometry = 0x0004,
        LinkAttribute = 0x0008,
        All = 0xffff

    }



}
