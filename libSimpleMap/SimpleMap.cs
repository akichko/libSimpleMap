using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using libGis;
using System.Drawing;


namespace libSimpleMap
{
    //public abstract class SpObj : CmnObj
    //{
    //    //public virtual ICmnObjHandle ToICmnObjHandle(CmnTile tile)
    //    //{
    //    //    return new SpObjHandle(tile, this);
    //    //}
    //}

    //共通ハンドル型
    //public abstract class SpObjHandle : ICmnObjHandle
    //{
    //    public SpObjHandle() { }
    //    public SpObjHandle(CmnTile tile, CmnObj obj) : base(tile, obj) { }
    //}

    public class SpTile : CmnTile //, ITile
    {
        public MapLink[] link; //並びは道路種別
        public MapNode[] node;
        public MapLinkFull[] linkGeometry;
        public MapLinkFull[] linkAttr;

        public MapLinkGeometry[] geometry;
        public MapLinkAttribute[] attribute;

        override public UInt32 Type { get { return 0; } }

        public SpTile() { }
        public SpTile(uint tileId)
        {
            tileInfo = new GisTileCode(tileId);


            //objDic = new Dictionary<UInt16, CmnObjGroup>();
            //objDic.Add((UInt16)SpMapContentType.Link, new SpObjGroup());
            //objDic.Add((UInt16)SpMapContentType.Node, new SpObjGroup());



        }

        override public CmnTile CreateTile(uint tileId)
        {
            return new SpTile(tileId);
        }


        //public MapNode GetMapNode(UInt64 nodeId)
        //{
        //    foreach (MapNode mapNode in node)
        //    {
        //        if (mapNode.nodeId == nodeId)
        //            return mapNode;
        //    }
        //    return null;
        //}


        //public LinkHandle GetMapLink(int linkIndex)
        //{
        //    if (linkIndex >= link.Length)
        //        return null;

        //    return new LinkHandle(this, link[linkIndex]);
        //    //return link[linkIndex];
        //}

        //public NodeHandle GetMapNode(int targetNodeIndex)
        //{
        //    if (targetNodeIndex >= node.Length)
        //        return null;

        //    return new NodeHandle(this, node[targetNodeIndex]);
        //}



        public override int UpdateObjGroup(CmnObjGroup objGroup) /* abstract ?? */
        {
            UInt32 objType = objGroup.Type;

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

                    //リンクがないものは読み込まない
                    int loadObjNum = objGroup.objArray.Length;
                    int linkNum = link?.Length ?? 0;

                    if (loadObjNum > linkNum)
                    {
                        loadObjNum = linkNum;
                    }

                    //linkGeometry = (MapLink[])objDic[objType].objArray;
                    geometry = (MapLinkGeometry[])objDic[objType].objArray;

                    //geometry = geometry.Take(loadObjNum).ToArray();
                    Array.Resize(ref geometry, loadObjNum);
                    MergeGeometry2(geometry, true);
                    objGroup.isDrawable = false;
                    objGroup.isGeoSearchable = false;
                    break;

                case SpMapContentType.LinkAttribute:
                    //linkAttr = (MapLink[])objDic[objType].objArray;
                    attribute = (MapLinkAttribute[])objDic[objType].objArray;
                    //MergeAttribute(linkAttr, true);
                    objGroup.isDrawable = false;
                    objGroup.isGeoSearchable = false;
                    break;

                default:
                    return 0;
            }

            return 0;
        }

        public override int UpdateObjGroupList(List<CmnObjGroup> objGroupList)
        {
            objGroupList.ForEach(x => this.UpdateObjGroup(x));

            return 0;
        }


        public bool MergeGeometry2(MapLinkGeometry[] shapeLinkList, bool ignoreError)
        {
            if (!ignoreError)
            {
                if (link.Length != shapeLinkList.Length)
                {
                    Console.WriteLine("link num not match");
                    return false;
                }
            }
            //for (int i = 0; i < link.Length; i++)
            //{
            //    if (link[i].linkId != shapeLinkList[i].linkId)
            //    {
            //        return false;
            //    }
            //}
            for (int i = 0; i < link.Length; i++)
            {
                link[i].geometry = shapeLinkList[i].geometry;
            }
            return true;
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
                if (link[i].LinkId != shapeLinkList[i].LinkId)
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


        //public bool MergeAttribute(MapLink[] attrLinkList, bool ignoreError)
        //{
        //    if (!ignoreError)
        //    {
        //        if (link.Length != attrLinkList.Length)
        //        {
        //            Console.WriteLine("link num not match");
        //            return false;
        //        }
        //    }
        //    for (int i = 0; i < link.Length; i++)
        //    {
        //        if (link[i].linkId != attrLinkList[i].linkId)
        //        {
        //            return false;
        //        }
        //    }
        //    for (int i = 0; i < link.Length; i++)
        //    {
        //        link[i].attribute = attrLinkList[i].attribute;
        //    }
        //    return true;
        //}



        //public void PrintGeometry()
        //{
        //    foreach (var mapLink in link)
        //    {
        //        mapLink.PrintGeometry(1);
        //        Console.WriteLine("");
        //    }

        //}

        //public int WriteMap(string fileName, bool append)
        //{
        //    StreamWriter sw = new StreamWriter(fileName, append);
        //    if (sw == null)
        //        return -1;
        //    foreach (MapLink mapLink in link)
        //    {
        //        mapLink.WriteGeometry(1, sw);
        //        sw.WriteLine("");
        //    }
        //    sw.Close();
        //    return 0;

        //}


        //public LinkHandle SearchNearestMapLink(LatLon latlon, byte maxRoadType = 0xFF)
        //{

        //    MapLink tmpLink = link
        //        .Where(x => x.roadType <= maxRoadType)
        //        .OrderBy(x => x.GetDistance(latlon))
        //        .FirstOrDefault();

        //    return new LinkHandle(this, tmpLink);

        //}


        //public LinkDistance SearchNearestMapLink2(LatLon latlon, byte maxRoadType = 0xFF)
        //{

        //    MapLink tmpLink = link
        //        .Where(x => x.roadType <= maxRoadType)
        //        .OrderBy(x => x.GetDistance(latlon))
        //        .FirstOrDefault();


        //    return tmpLink.GetDistance2(latlon);

        //}


        public int CalcLinkCost()
        {
            Array.ForEach(link, x => {
                x.linkLength = (ushort)LatLon.CalcLength(x.Geometry);
            });
            //Array.ForEach(link, x => x.CalcCost());
            //link.ForEach(x => x.CalcCost());

            return 0;
        }



        public static List<UInt32> GetMapContentTypeList()
        {
            return ((uint[])Enum.GetValues(typeof(SpMapContentType)))
                .Select(x => (UInt32)x)
                .Where(x => x != 0xFFFF && x != (UInt32)SpMapContentType.Tile)
                .ToList();
        }


        //public override ICmnObjHandle ToICmnObjHandle(CmnTile tile)
        //{
        //    return new CmnObjHandle(tile, this);
        //}
    }


    //public class SpObjGroup : CmnObjGroup
    //{
    //    //public override UInt32 Type { get; }

    //    public SpObjGroup(UInt32 type) : base(type) { }

    //    public SpObjGroup(UInt32 type, CmnObj[] objArray, UInt16 loadedSubType) : base(type, objArray, loadedSubType) { }
    //    //{
    //    //    this.loadedSubType = loadedSubType;
    //    //    this.objArray = objArray;
    //    //}
    //}


    public class SpLinkHandle : CmnObjHandle
    {

        public SpLinkHandle(CmnTile tile, MapLink obj, byte direction = 0xff) : base(tile, obj, direction) { }

        public override LatLon[] Geometry => ((SpTile)tile).geometry?[obj.Index].Geometry;


        public override List<AttrItemInfo> GetAttributeListItem()
        {
            List<AttrItemInfo> listItem = new List<AttrItemInfo>();
            AttrItemInfo item;

            MapLink link = (MapLink)obj;
            //LinkAttribute attribute = ((SpTile)tile).linkAttr[obj.Index].attribute;
            MapLinkAttribute attribute = ((SpTile)tile).attribute[obj.Index];

            //基本属性

            listItem.Add(new AttrItemInfo(new string[] { "Id", $"{ObjId}" }, new AttrTag(0, new CmnSearchKey((int)SpMapContentType.Link).AddObjHandle(tile, this.obj), null))
                );
            listItem.Add(new AttrItemInfo(new string[] { "TileId", $"{tile.tileId}" }));
            listItem.Add(new AttrItemInfo(new string[] { "Index", $"{Index}" }));
            listItem.Add(new AttrItemInfo(new string[] { "RoadType", $"{link.roadType}" }));
            listItem.Add(new AttrItemInfo(new string[] { "Oneway", $"{Oneway}" }));
            listItem.Add(new AttrItemInfo(new string[] { "linkLength", $"{link.linkLength}" }));
            listItem.Add(new AttrItemInfo(new string[] { "Cost", $"{Cost}" }));

            //List<CmnObjHdlRef> nextLinkList = GetObjRefHdlList((int)SpMapRefType.NextLink);
            //nextLinkList.ForEach(x =>
            //{

            //    listItem.Add(new AttrItemInfo(new string[] { $"nextLink[]", $"nodeIndex={x.nextRef.key.objIndex}" }, new AttrTag((int)SpMapRefType.NextLink, x.nextRef.key, null)));
            //});

            //形状詳細表示
            if (true)
            {
                for (int i = 0; i < Geometry.Length; i++)
                {
                    //listItem.Add(new AttrItemInfo(new string[] { $"geometry[{i}]", $"({geometry[i].ToString()})" }, geometry[i]));
                    listItem.Add(new AttrItemInfo(new string[] { $"geometry[{i}]", $"({Geometry[i].ToString()})" }, new AttrTag((int)SpMapRefType.LatLon, null, Geometry[i])));
                }
            }
            //簡易表示
            else
            {
                listItem.Add(new AttrItemInfo(new string[] { $"geometry[S]", $"({Geometry[0].ToString()})" }, new AttrTag((int)SpMapRefType.LatLon, null, Geometry[0])));
                listItem.Add(new AttrItemInfo(new string[] { $"geometry[E]", $"({Geometry[Geometry.Length - 1].ToString()})" }, new AttrTag((int)SpMapRefType.LatLon, null, Geometry[Geometry.Length - 1])));
                //item = new AttrItemInfo(new string[] { "geometry[S]", $"({geometry[0].ToString()})" }, geometry[0]);
                //listItem.Add(item);

                //item = new AttrItemInfo(new string[] { "geometry[E]", $"({geometry[geometry.Length - 1].ToString()})"}, geometry[geometry.Length - 1]);
                //listItem.Add(item);
            }

            if (attribute != null)
            {
                item = new AttrItemInfo(new string[] { "Link ID", $"{attribute.linkId}" });
                listItem.Add(item);
                item = new AttrItemInfo(new string[] { "Way ID", $"{attribute.wayId}" });
                listItem.Add(item);
                for (int i = 0; i < attribute.tagInfo.Count; i++)
                {
                    item = new AttrItemInfo(new string[] { $"{attribute.tagInfo[i].tag}", $"{attribute.tagInfo[i].val}" });
                    listItem.Add(item);

                }

            }

            return listItem;

        }

    }

    public class MapLinkFull : MapLink
    {
        public override UInt64 LinkId { get; set; } //リンク属性の方に移動
        public Int64[] edgeNodeId; //必ず２つ。[0]:始点側 [1]:終点側 //容量削減可能
        public uint[] edgeNodeTileId; //オフセット位置。終点のみ
        public LinkAttribute attribute;

        public MapLinkFull()
        {
            edgeNodeId = new Int64[2];
            edgeNodeTileId = new uint[2];
        }


        public override uint EdgeNodeTileId(CmnTile tile, int index)
        {
            if (index == 0)
                return edgeNodeTileId[0];
            else if (index == 1)
                return edgeNodeTileId[1];
            else
                throw new ArgumentException();
        }
        public override LinkAttribute Attribute { get { return attribute; } set { attribute = value; } }

    }

    public class MapLink : CmnObj
    {
        public ushort index; //コスト計算管理情報など、外部テーブルを用意した場合の参照用
        public ushort[] edgeNodeIndex; //必ず２つ。[0]:始点側 [1]:終点側
        public TileOffset2 endNodeTileOffset;

      // public uint[] edgeNodeTileId; //オフセット位置。終点のみ

        public byte roadType; //4bit
        public ushort linkLength;
        public ushort linkCost;
        public sbyte fOneWay; //表示用にここにも

        public virtual LatLon[] geometry { get; set; }

        //public MapLinkAttribute linkAttr;

        public override UInt64 Id => LinkId;
        public override UInt32 Type => (UInt32)SpMapContentType.Link;
        public override UInt16 SubType => (UInt16)roadType;
        public override LatLon[] Geometry => geometry;
        public override double Length => linkLength;


        public override int Cost {
            get {
                double tmpCost;
                switch (roadType)
                {
                    case 1:
                        tmpCost = Length * 0.5;
                        break;
                    case 2:
                        tmpCost = Length * 0.6;
                        break;
                    case 3:
                        tmpCost = Length * 0.7;
                        break;
                    case 4:
                        tmpCost = Length * 0.8;
                        break;
                    case 5:
                        tmpCost = Length * 0.9;
                        break;
                    default:
                        tmpCost = Length;
                        break;
                }
                return (int)tmpCost;
            }
        }

        public override byte Oneway {
            get {
                if (fOneWay == 1)
                    return 1;
                else if (fOneWay == -1)
                    return 0;
                else
                    return 0xff;
            }
        }

        public virtual UInt64 LinkId { get { return 0; } set { return; } } //リンク属性の方に移動
        public virtual LinkAttribute Attribute { get { return null; } set { return; } }


        public virtual uint EdgeNodeTileId(CmnTile tile, int index) 
        {
            if (index == 0)
                return tile.tileInfo.TileId;
            else if (index == 1)
                return tile.tileInfo.CalcOffsetTileId(endNodeTileOffset.offsetX, endNodeTileOffset.offsetY);
            else
                throw new ArgumentException();
        }


        public MapLink()
        {
            edgeNodeIndex = new ushort[2];
            //edgeNodeTileId = new uint[2];
        }

        public MapLink(byte[] serialData)
        {
            PackData64 packData = new PackData64(serialData);

            //edgeNodeId = new Int64[2];
            edgeNodeIndex = new ushort[2];
            //edgeNodeTileId = new uint[2];

            edgeNodeIndex[0] = (ushort)packData.GetUInt(0, 16);
            edgeNodeIndex[1] = (ushort)packData.GetUInt(16, 16);
            endNodeTileOffset.offsetX = (sbyte)packData.GetInt(32, 2);
            endNodeTileOffset.offsetY = (sbyte)packData.GetInt(34, 2);
            linkLength = (ushort)packData.GetUInt(36, 15);
            roadType = (byte)packData.GetUInt(51, 4);
            fOneWay = (sbyte)packData.GetInt(55, 2);
            linkCost = (ushort)packData.GetUInt(57, 5);

        }


        public byte[] Serialize()
        {
            PackData64 packData = new PackData64();

            packData.SetUInt(0, 16, edgeNodeIndex[0]);
            packData.SetUInt(16, 16, edgeNodeIndex[1]);
            packData.SetInt(32, 2, endNodeTileOffset.offsetX);
            packData.SetInt(34, 2, endNodeTileOffset.offsetY);
            packData.SetUInt(36, 15, linkLength);
            packData.SetUInt(51, 4, roadType);
            packData.SetInt(55, 2, fOneWay);
            packData.SetUInt(57, 5, linkCost);

            return BitConverter.GetBytes(packData.rawData);
        }

        //public int WriteGeometry(int direction, StreamWriter sw)
        //{
        //    if (direction == 0)
        //    {
        //        for (int i = geometry.Length - 1; i >= 0; i--)
        //        {
        //            sw.WriteLine($"{geometry[i].lon}\t{geometry[i].lat}");
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < geometry.Length; i++)
        //        {
        //            sw.WriteLine($"{geometry[i].lon}\t{geometry[i].lat}");
        //        }
        //    }
        //    return 0;
        //}



        //public LatLon[] GetGeometry(int direction)
        //{
        //    if (direction == 0)
        //        return geometry.Reverse().ToArray();
        //    else
        //        return geometry;
        //}


        //public LinkDistance GetDistance2(LatLon latlon)
        //{
        //    if (geometry.Count() <= 1)
        //        return null;

        //    double minDistance = Double.MaxValue;
        //    int index = -1;

        //    for (int i = 0; i < geometry.Count() - 1; i++)
        //    {
        //        double tmp = latlon.GetDistanceToLine(geometry[i], geometry[i + 1]);
        //        if (tmp < minDistance)
        //        {
        //            minDistance = tmp;
        //            index = i;
        //        }
        //    }

        //    double offset = 0;
        //    for (int i = 0; i < index; i++)
        //    {
        //        offset += geometry[i].GetDistanceTo(geometry[i + 1]);
        //    }
        //    LinkPos linkPos = new LinkPos(null, this, offset);

        //    return new LinkDistance(linkPos, minDistance);

        //}


        //public int CalcCost()
        //{
        //    if (geometry.Length <= 1) return -1;

        //    double tmpLength = 0;

        //    for (int i = 1; i < geometry.Length; i++)
        //    {
        //        tmpLength += geometry[i].GetDistanceTo(geometry[i - 1]);
        //    }

        //    linkLength = (ushort)tmpLength;

        //    double weight = 1.0;
        //    if (roadType <= 2) weight = 0.5;
        //    else if (roadType <= 4) weight = 0.6;
        //    else if (roadType <= 6) weight = 0.7;

        //    linkCost = (ushort)(linkLength * weight);
        //    return 0;

        //}


        public override List<CmnObjRef> GetObjAllRefList(CmnTile tile, byte direction)
        {
            List<CmnObjRef> ret = new List<CmnObjRef>();

            //始点ノード側接続リンク
            CmnObjRef nodeRef = new CmnObjRef((int)SpMapRefType.BackLink, (UInt32)SpMapContentType.Node, false);
            nodeRef.key.tile = tile;
            nodeRef.key.objIndex = edgeNodeIndex[0];
            ret.Add(nodeRef);

            //始点ノード側接続リンク
            nodeRef = new CmnObjRef((int)SpMapRefType.NextLink,  (UInt32)SpMapContentType.Node, false);
            nodeRef.key.tileId = EdgeNodeTileId(tile, 1);
            //nodeRef.key.tileId = edgeNodeTileId[1];
            nodeRef.key.objIndex = edgeNodeIndex[1];
            ret.Add(nodeRef);

            return ret;
        }

        //public override List<AttrItemInfo> GetAttributeListItem(CmnTile tile)
        //{
        //    List<AttrItemInfo> listItem = new List<AttrItemInfo>();
        //    AttrItemInfo item;

        //    //基本属性

        //    listItem.Add(new AttrItemInfo(
        //        new string[] { "Id", $"{Id}" },
        //        new AttrTag(0, new CmnSearchKey((int)SpMapContentType.Link).AddObjHandle(tile, this), null))
        //        );
        //    listItem.Add(new AttrItemInfo(new string[] { "TileId", $"{tile.tileId}" }));
        //    listItem.Add(new AttrItemInfo(new string[] { "Index", $"{Index}" }));
        //    listItem.Add(new AttrItemInfo(new string[] { "RoadType", $"{roadType}" }));
        //    listItem.Add(new AttrItemInfo(new string[] { "Oneway", $"{Oneway}" }));
        //    listItem.Add(new AttrItemInfo(new string[] { "linkLength", $"{linkLength}" }));
        //    listItem.Add(new AttrItemInfo(new string[] { "Cost", $"{Cost}" }));

        //    List<CmnObjHdlRef> nextLinkList = GetObjRefHdlList((int)SpMapRefType.NextLink, tile);
        //    nextLinkList.ForEach(x =>
        //    {
        //        listItem.Add(new AttrItemInfo(new string[] { $"nextLink[]", $"" }, new AttrTag((int)SpMapRefType.NextLink, x.nextRef.key, null)));
        //    });

        //    //形状詳細表示
        //    if (true)
        //    {                
        //        for (int i = 0; i < geometry.Length; i++)
        //        {
        //            //listItem.Add(new AttrItemInfo(new string[] { $"geometry[{i}]", $"({geometry[i].ToString()})" }, geometry[i]));
        //            listItem.Add(new AttrItemInfo(new string[] { $"geometry[{i}]", $"({geometry[i].ToString()})" }, new AttrTag((int)SpMapRefType.LatLon, null, geometry[i])));
        //        }
        //    }
        //    //簡易表示
        //    else
        //    {
        //        listItem.Add(new AttrItemInfo(new string[] { $"geometry[S]", $"({geometry[0].ToString()})" }, new AttrTag((int)SpMapRefType.LatLon, null, geometry[0])));
        //        listItem.Add(new AttrItemInfo(new string[] { $"geometry[E]", $"({geometry[geometry.Length - 1].ToString()})" }, new AttrTag((int)SpMapRefType.LatLon, null, geometry[geometry.Length - 1])));
        //        //item = new AttrItemInfo(new string[] { "geometry[S]", $"({geometry[0].ToString()})" }, geometry[0]);
        //        //listItem.Add(item);

        //        //item = new AttrItemInfo(new string[] { "geometry[E]", $"({geometry[geometry.Length - 1].ToString()})"}, geometry[geometry.Length - 1]);
        //        //listItem.Add(item);
        //    }

        //    if (attribute != null)
        //    {
        //        item = new AttrItemInfo(new string[] { "Link ID", $"{attribute.linkId}" });
        //        listItem.Add(item);
        //        item = new AttrItemInfo(new string[] { "Way ID", $"{attribute.wayId}" });
        //        listItem.Add(item);
        //        for (int i = 0; i < attribute.tagInfo.Count; i++)
        //        {
        //            item = new AttrItemInfo(new string[] { $"{attribute.tagInfo[i].tag}", $"{attribute.tagInfo[i].val}" });
        //            listItem.Add(item);

        //        }

        //    }

        //    return listItem;

        //}




        override public List<CmnObjHdlRef> GetObjRefHdlList(int refType, CmnTile tile, byte direction = 0xff)
        {
            List<CmnObjHdlRef> ret = new List<CmnObjHdlRef>();            

            CmnObjHdlRef refNode;

            if (direction == 0xff)
                direction = 1;

            switch ((SpMapRefType)refType)
            {
                case SpMapRefType.NextLink:

                    refNode = new CmnObjHdlRef(null, refType, (UInt32)SpMapContentType.Node);
                    refNode.nextRef.key.tileId = EdgeNodeTileId(tile, direction);
                    //refNode.nextRef.key.tileId = edgeNodeTileId[direction];
                    refNode.nextRef.key.objIndex = edgeNodeIndex[direction];
                    refNode.nextRef.final = false;

                    ret.Add(refNode);
                    return ret;

                case SpMapRefType.BackLink:

                    refNode = new CmnObjHdlRef(null, refType, (UInt32)SpMapContentType.Node);
                    refNode.nextRef.key.tileId = EdgeNodeTileId(tile, 1 - direction);
                    //refNode.nextRef.key.tileId = edgeNodeTileId[1 - direction];
                    refNode.nextRef.key.objIndex = edgeNodeIndex[1 - direction];
                    refNode.nextRef.final = false;

                    ret.Add(refNode);
                    return ret;
                default:
                    break;
            }
            return ret;
        }


        //デバッグ・その他

        //public int show_geometry()
        //{
        //    //    Console.WriteLine("◆LINK Id={0}", linkId);
        //    //    for (int i = 0; i < shape.numCoord; i++)
        //    //    {
        //    //        Console.WriteLine("  {0}:\tX:{1}\ty:{2}", i, shape.geometry[i].x, shape.geometry[i].y);

        //    //    }
        //    return 0;
        //}

        //public int show_geometry_for_plot()
        //{
        //    //for (int i = 0; i < shape.numCoord; i++)
        //    //{
        //    //    Console.WriteLine("{0}\t{1}\t{2}\t0", i, shape.geometry[i].x, shape.geometry[i].y);

        //    //}
        //    Console.WriteLine("");
        //    Console.WriteLine("");

        //    //Console.Write("\n\n");

        //    return 0;
        //}

        //public int show_geometry_for_plot3D(int direction, int start_z, int end_z)
        //{
        //    //Console.WriteLine("<{0}\t{1}\t{2}>", direction, start_z, end_z);

        //    //if (direction == 1)
        //    //{
        //    //    for (int i = 0; i < shape.numCoord; i++)
        //    //    {
        //    //        Console.WriteLine("{0}\t{1}\t{2}\t{3}", i, shape.geometry[i].x, shape.geometry[i].y, start_z + (end_z - start_z) * i / (double)(shape.numCoord - 1));
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    for (int i = 0; i < shape.numCoord; i++)
        //    //    {
        //    //        Console.WriteLine("{0}\t{1}\t{2}\t{3}", i, shape.geometry[shape.numCoord - 1 - i].x, shape.geometry[shape.numCoord - 1 - i].y, start_z + ((end_z - start_z) * i) / (double)(shape.numCoord - 1));
        //    //    }
        //    //}
        //    //Console.WriteLine("");
        //    //Console.WriteLine("");

        //    return 0;
        //}

        public override CmnObjHandle ToCmnObjHandle(CmnTile tile, byte direction = 0xff)
        {
            return new SpLinkHandle(tile, this, direction);
        }


    }



    public class MapLinkGeometry : CmnObj
    {
        public UInt64 id;
        public LatLon[] geometry { get; set; }

        public override UInt64 Id => id;
        public override UInt32 Type { get { return (UInt16)SpMapContentType.LinkGeometry; } }

        public override LatLon[] Geometry => geometry;

        override public List<CmnObjHdlRef> GetObjRefHdlList(int refType, CmnTile thisTile, byte direction = 0xff)
        {
            List<CmnObjHdlRef> ret = new List<CmnObjHdlRef>();

            CmnObjHdlRef refNode;

            switch ((SpMapRefType)refType)
            {
                case SpMapRefType.RelatedLink:

                    refNode = new CmnObjHdlRef(null, refType, (UInt32)SpMapContentType.Link);
                    refNode.nextRef.key.tile = thisTile;
                    refNode.nextRef.key.objIndex = Index;

                    ret.Add(refNode);
                    return ret;

                default:
                    break;
            }
            return ret;
        }


    }

    public class MapLinkAttribute : CmnObj
    {
        public override UInt64 Id => linkId;
        public override UInt32 Type => (UInt16)SpMapContentType.LinkAttribute;

        public UInt64 linkId;
        public UInt64 wayId;
        public List<TagInfo> tagInfo;

        //public byte link_type1;
        //public byte link_type2;
        //public byte link_type3;
        //public byte road_width;
        //public int road_name;
        //public int road_no;
        //public byte numLanes;
        //public bool tunnel;

        public MapLinkAttribute()
        {
            tagInfo = new List<TagInfo>();
        }
    }

    public class MapNode : CmnObj
    {
        //public SpTile tile; //検索時に記憶するようにする？
        public UInt64 nodeId; //いずれ消す？
        public short index;
        public ConnectLink[] connectLink; //メッシュ内退出リンク

        //public bool f_upper_level; //いる？リンクにつける？
        //public bool f_zukakul; //いる？図郭外リンク数とどちらかでよい

        public MapNode()
        {
            //connectLink = new List<ConnectLink>();
        }


        public override UInt64 Id => nodeId;
        
        public override UInt32 Type { get { return (UInt32)SpMapContentType.LinkAttribute; } }

        public override List<CmnObjRef> GetObjAllRefList(CmnTile tile, byte direction = 1)
        {
            List<CmnObjRef> ret = new List<CmnObjRef>();

            foreach (var connLink in connectLink) {

                CmnObjRef linkRef = new CmnObjRef((int)SpMapRefType.RelatedLink, (UInt32)SpMapContentType.Link);
                linkRef.key.tileId = connLink.tileId;
                linkRef.key.objIndex = connLink.linkIndex;

                ret.Add(linkRef);
            }

            return ret;
        }



        override public List<CmnObjHdlRef> GetObjRefHdlList(int refType, CmnTile tile, byte direction = 1)
        {
            List<CmnObjHdlRef> ret = new List<CmnObjHdlRef>();

            switch ((SpMapRefType)refType)
            {
                case SpMapRefType.NextLink:
                    foreach (var x in connectLink)
                    {
                        CmnObjHdlRef refLink = new CmnObjHdlRef(null, refType, (UInt32)SpMapContentType.Link);
                        refLink.nextRef.key.tileId = x.tileId;
                        refLink.nextRef.key.objIndex = x.linkIndex;
                        refLink.nextRef.key.objDirection = x.linkDirection;

                        ret.Add(refLink);
                    }
                    return ret;

                case SpMapRefType.BackLink:

                    foreach (var x in connectLink)
                    {
                        CmnObjHdlRef refLink = new CmnObjHdlRef(null, refType, (UInt32)SpMapContentType.Link);
                        refLink.nextRef.key.tileId = x.tileId;
                        refLink.nextRef.key.objIndex = x.linkIndex;
                        refLink.nextRef.key.objDirection = (byte)(1 - x.linkDirection);

                        ret.Add(refLink);
                    }
                    return ret;
            }
            return ret;
        }
    }

    public class ConnectLink
    {
        public uint tileId; //いずれオフセット位置にする？
        public Int64 linkId; //接続リンクId。不要となる予定
        public ushort linkIndex; //接続リンク。メッシュ内リンク番号。評価用
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

        public ConnectLink() { }

        public ConnectLink(byte[] serialData)
        {
            PackData32 packData = new PackData32(serialData);

            //linkIndex = (ushort)packData.GetUInt(0, 16);
            //tileOffset.offsetX = (sbyte)packData.GetInt(16, 2);
            //tileOffset.offsetY = (sbyte)packData.GetInt(18, 2);
            //linkDirection = (byte)packData.GetUInt(20, 1);
            //roadType = (byte)packData.GetUInt(21, 4);
            //isOnewayReverse = (byte)packData.GetInt(25, 1);
            //hasRestrict = (byte)packData.GetInt(26, 1);
            //outDirection = (byte)packData.GetUInt(27, 3);

        }


        public byte[] Serialize()
        {
            PackData32 packData = new PackData32();


            return BitConverter.GetBytes(packData.rawData);
        }
    }

    public class LinkAttribute
    {
        public UInt64 linkId;
        public UInt64 wayId;
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

    //public class LinkHandle
    //{
    //    public SpTile tile;
    //    public MapLink mapLink;

    //    //public LinkHandle() { }

    //    //public LinkHandle(SpTile tile, MapLink mapLink)
    //    //{
    //    //    this.tile = tile;
    //    //    this.mapLink = mapLink;
    //    //}

    //    //public DLinkHandle ToDLinkHandle(byte direction)
    //    //{
    //    //    return new DLinkHandle(tile, mapLink, direction);
    //    //}

    //}


    //public class DLinkHandle : LinkHandle
    //{
    //    //public SpTile tile;
    //    //public MapLink mapLink;
    //    public byte direction;
    //    public uint tileId;

    //    //public DLinkHandle() { }

    //    //public DLinkHandle(SpTile tile, MapLink mapLink, byte direction)
    //    //{
    //    //    this.tile = tile;
    //    //    this.mapLink = mapLink;
    //    //    this.direction = direction;
    //    //}

    //    //public DLinkHandle(SpTile tile, MapLink mapLink, byte direction, uint tileId, short linkIndex)
    //    //{
    //    //    this.tile = tile;
    //    //    this.mapLink = mapLink;
    //    //    this.direction = direction;
    //    //}

    //    //public DLinkHandle(LinkHandle linkHdl, byte direction)
    //    //{
    //    //    this.tile = linkHdl.tile;
    //    //    this.mapLink = linkHdl.mapLink;
    //    //    this.direction = direction;
    //    //}

    //}

    //public class NodeHandle
    //{
    //    public SpTile tile;
    //    public MapNode mapNode;
    //    public uint tileId;

    //    //public NodeHandle() { }

    //    //public NodeHandle(SpTile tile, MapNode mapNode)
    //    //{
    //    //    this.tile = tile;
    //    //    this.mapNode = mapNode;
    //    //}

    //    //public NodeHandle(SpTile tile, MapNode mapNode, uint tileId, ushort nodeIndex)
    //    //{
    //    //    this.tile = tile;
    //    //    this.mapNode = mapNode;
    //    //}

    //}


    //public class LinkPos
    //{
    //    public SpTile tile;
    //    public MapLink mapLink;
    //    public double offset;

    //    //public LinkPos(SpTile tile, MapLink mapLink, double offset)
    //    //{
    //    //    this.tile = tile;
    //    //    this.mapLink = mapLink;
    //    //    this.offset = offset;
    //    //}

    //    //public LinkHandle ToLinkHdl()
    //    //{
    //    //    return new LinkHandle(tile, mapLink);
    //    //}
    //}

    //public class LinkDistance
    //{
    //    public LinkPos linkPos;
    //    public double distance;

    //    //public LinkDistance(LinkPos linkPos, double distance)
    //    //{
    //    //    this.linkPos = linkPos;
    //    //    this.distance = distance;
    //    //}

    //}

#if false
    public struct t_condition
    {
        int conditionId;
        int regulation1;
        int regulation2;
        int regulation3;
    }
#endif





    public class TileXY
    {
        public ushort x;
        public ushort y;

        public TileXY() { }

        public TileXY(uint tileId)
        {
            x = GisTileCode.S_CalcX(tileId);
            y = GisTileCode.S_CalcY(tileId);
        }

        public uint ToTileId()
        {
            return GisTileCode.S_CalcTileId(x, y);
        }
    }


    public struct MicroXY
    {
        public byte xy;
        // 0-1 bit: y
        // 2-3 bit: x
        // 4-7 bit: reserved

        public int X
        {
            get
            {
                if (XSignBit == 0)
                    return XNumberBit;
                else
                    return XNumberBit - 2;
            }
            set { xy = (byte)((value << 2) | (xy & 0x03)); }
        }

        public sbyte Y
        {
            get { return (sbyte)(((sbyte)((xy & 0x0f) << 4)) >> 4); }

            set { xy = (byte)(value & 0x0f | (xy & 0xf0)); }
        }
        public int XSignBit => (xy | 0x08) >> 3;
        public int YSignBit => (xy | 0x02) >> 1;
        public int XNumberBit => (xy | 0x04) >> 2;
        public int YNumberBit => (xy | 0x01) >> 0;

        public bool XisPositive()
        {
            int tmp = (xy | 0x08)>>3;
            if (tmp == 1)
                return false;
            else
                return true;
        }
        public bool YisPositive()
        {
            int tmp = (xy | 0x02);
            if (tmp == 1)
                return false;
            else
                return true;
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
            offsetX = (sbyte)(GisTileCode.S_CalcX(tileId) - GisTileCode.S_CalcX(baseTileId));
            offsetY = (sbyte)(GisTileCode.S_CalcY(tileId) - GisTileCode.S_CalcY(baseTileId));
        }

        public uint ToTileId(uint baseTileId)
        {
            return GisTileCode.S_CalcTileId((ushort)(GisTileCode.S_CalcX(baseTileId) + offsetX), (ushort)(GisTileCode.S_CalcY(baseTileId) + offsetY));
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
        Tile = 0x0010,
        All = 0xffff

    }



    public enum SpMapRefType
    {
        Selected,
        NextLink,
        BackLink,
        StartNode,
        EndNode,
        RelatedLink,
        RelatedObj,
        RelatedLine,
        RelatedNode,
        LatLon
    }
}
