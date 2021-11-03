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
using System.IO;
using Akichko.libGis;
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

        //static List<UInt32> mapContentTypeList;

        override public UInt32 Type { get { return 0; } }

        public SpTile() { }
        public SpTile(uint tileId)
        {
            tileCode = new GisTileCode(tileId);


            //objDic = new Dictionary<UInt16, CmnObjGroup>();
            //objDic.Add((UInt16)SpMapContentType.Link, new SpObjGroup());
            //objDic.Add((UInt16)SpMapContentType.Node, new SpObjGroup());



        }

        //static SpTile()
        //{
        //    mapContentTypeList = ((uint[])Enum.GetValues(typeof(SpMapContentType)))
        //        .Where(x => x != 0xFFFF && x != (UInt32)SpMapContentType.Tile)
        //        .ToList();
        //}

        override public CmnTile CreateTile(uint tileId)
        {
            return new SpTile(tileId);
        }



        public override int UpdateObjGroup(CmnObjGroup objGroup) /* abstract ?? */
        {
            UInt32 objType = objGroup.Type;

            //上書き
            objGroupDic[objType] = objGroup;

            switch ((SpMapContentType)objType)
            {
                case SpMapContentType.Link:
                    link = (MapLink[])objGroupDic[objType].ObjArray;

                    break;

                case SpMapContentType.Node:
                    node = (MapNode[])objGroupDic[objType].ObjArray;
                    objGroup.isDrawable = false;
                    objGroup.isGeoSearchable = false;
                    break;

                case SpMapContentType.LinkGeometry:

                    //リンクがないものは読み込まない
                    int loadObjNum = objGroup.ObjArray.Length;
                    int linkNum = link?.Length ?? 0;

                    if (loadObjNum > linkNum)
                    {
                        loadObjNum = linkNum;
                    }

                    //linkGeometry = (MapLink[])objDic[objType].objArray;
                    geometry = (MapLinkGeometry[])objGroupDic[objType].ObjArray;

                    //geometry = geometry.Take(loadObjNum).ToArray();
                    Array.Resize(ref geometry, loadObjNum);
                    MergeGeometry2(geometry, true);
                    objGroup.isDrawable = false;
                    objGroup.isGeoSearchable = false;
                    break;

                case SpMapContentType.LinkAttribute:
                    //linkAttr = (MapLink[])objDic[objType].objArray;
                    attribute = (MapLinkAttribute[])objGroupDic[objType].ObjArray;
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
            Array.ForEach(link, x =>
            {
                x.linkLength = (ushort)LatLon.CalcLength(x.Geometry);
            });
            //Array.ForEach(link, x => x.CalcCost());
            //link.ForEach(x => x.CalcCost());

            return 0;
        }



        //public static List<UInt32> GetMapContentTypeList() => mapContentTypeList;
        //{
        //    return ((uint[])Enum.GetValues(typeof(SpMapContentType)))
        //        .Select(x => (UInt32)x)
        //        .Where(x => x != 0xFFFF && x != (UInt32)SpMapContentType.Tile)
        //        .ToList();
        //}


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

        public SpLinkHandle(CmnTile tile, MapLink obj, DirectionCode direction = DirectionCode.None) : base(tile, obj, direction) { }

        public override LatLon[] Geometry => ((SpTile)tile).geometry?[obj.Index].Geometry;


        public override List<AttrItemInfo> GetAttributeListItem()
        {
            List<AttrItemInfo> listItem = new List<AttrItemInfo>();
            AttrItemInfo item;

            MapLink link = (MapLink)obj;
            //LinkAttribute attribute = ((SpTile)tile).linkAttr[obj.Index].attribute;
            MapLinkAttribute attribute = ((SpTile)tile).attribute[obj.Index];

            //基本属性

            listItem.Add(new AttrItemInfo(new string[] { "Id", $"{ObjId}" }, new AttrTag(0, new CmnSearchKey((int)SpMapContentType.Link).AddObjHandle(tile, this.obj), null)));
            listItem.Add(new AttrItemInfo(new string[] { "TileId", $"{tile.TileId}" }));
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

            listItem.AddRange(obj.GetAttributeListItemGeometry(tile, true));


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


        public override uint EdgeNodeTileId(CmnTile tile, DirectionCode direction)
        {
            if (direction == DirectionCode.Negative)
                return edgeNodeTileId[0];
            else if (direction == DirectionCode.Positive)
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
        public override UInt16 Index => index;


        public override int Cost
        {
            get
            {
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

        public override DirectionCode Oneway
        {
            get
            {
                if (fOneWay == 1)
                    return DirectionCode.Positive;
                else if (fOneWay == -1)
                    return DirectionCode.Negative;
                else
                    return DirectionCode.None;
            }
        }


        public virtual UInt64 LinkId { get { return 0; } set { return; } } //リンク属性の方に移動
        public virtual LinkAttribute Attribute { get { return null; } set { return; } }


        public virtual CmnTile EdgeNodeTile(CmnTile tile, DirectionCode direction)
        {
            if (direction == DirectionCode.Negative)
                return tile;
            if (endNodeTileOffset.offsetX == 0 && endNodeTileOffset.offsetY == 0)
                return tile;
            else
                return null;
        }

        public virtual uint EdgeNodeTileId(CmnTile tile, DirectionCode direction)
        {
            if (direction == DirectionCode.Negative)
                return tile.TileId;
            else if (direction == DirectionCode.Positive)
                return tile.tileCode.CalcOffsetTileId(endNodeTileOffset.offsetX, endNodeTileOffset.offsetY);
            else
                throw new ArgumentException();
        }

        public ushort EdgeNodeIndex(DirectionCode direction)
        {
            if (direction == DirectionCode.Positive)
                return edgeNodeIndex[1];
            else if (direction == DirectionCode.Negative)
                return edgeNodeIndex[0];
            else
                return edgeNodeIndex[1];
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


        public override List<CmnObjRef> GetObjAllRefList(CmnTile tile, DirectionCode direction)
        {
            List<CmnObjRef> ret = new List<CmnObjRef>();

            //始点ノード側接続リンク
            CmnObjRef nodeRef = new CmnObjRef((int)SpMapRefType.BackLink, (UInt32)SpMapContentType.Node, false);
            nodeRef.key.tile = tile;
            nodeRef.key.objIndex = edgeNodeIndex[0];
            ret.Add(nodeRef);

            //始点ノード側接続リンク
            nodeRef = new CmnObjRef((int)SpMapRefType.NextLink, (UInt32)SpMapContentType.Node, false);
            nodeRef.key.tileId = EdgeNodeTileId(tile, DirectionCode.Positive);
            //nodeRef.key.tileId = edgeNodeTileId[1];
            nodeRef.key.objIndex = edgeNodeIndex[1];
            ret.Add(nodeRef);

            return ret;
        }





        override public List<CmnObjHdlRef> GetObjRefHdlList(int refType, CmnTile tile, DirectionCode direction = DirectionCode.None)
        {
            List<CmnObjHdlRef> ret = new List<CmnObjHdlRef>();

            CmnObjHdlRef refNode;

            if (direction == DirectionCode.None)
                direction = DirectionCode.Positive;

            switch ((SpMapRefType)refType)
            {
                case SpMapRefType.NextLink:

                    refNode = new CmnObjHdlRef(null, refType, (UInt32)SpMapContentType.Node);
                    refNode.nextRef.key.tile = EdgeNodeTile(tile, direction);
                    refNode.nextRef.key.tileId = EdgeNodeTileId(tile, direction);
                    //refNode.nextRef.key.tileId = edgeNodeTileId[direction];
                    //refNode.nextRef.key.objIndex = edgeNodeIndex[(int)direction];
                    refNode.nextRef.key.objIndex = EdgeNodeIndex(direction);
                    refNode.nextRef.final = false;

                    ret.Add(refNode);
                    return ret;

                case SpMapRefType.BackLink:

                    refNode = new CmnObjHdlRef(null, refType, (UInt32)SpMapContentType.Node);
                    refNode.nextRef.key.tile = EdgeNodeTile(tile, 1 - direction);
                    refNode.nextRef.key.tileId = EdgeNodeTileId(tile, 1 - direction);
                    //refNode.nextRef.key.tileId = edgeNodeTileId[1 - direction];
                    //refNode.nextRef.key.objIndex = edgeNodeIndex[1 - (int)direction];
                    refNode.nextRef.key.objIndex = EdgeNodeIndex(direction.Reverse());
                    refNode.nextRef.final = false;

                    ret.Add(refNode);
                    return ret;
                default:
                    break;
            }
            return ret;
        }


        public override CmnObjHandle ToCmnObjHandle(CmnTile tile, DirectionCode direction = DirectionCode.None)
        {
            return new SpLinkHandle(tile, this, direction);
        }


        public override bool CheckTimeStamp(long timeStamp)
        {
            if (timeStamp >= 1000 && timeStamp <= 2000)
                return false;
            else
                return true;
        }

    }



    public class MapLinkGeometry : CmnObj
    {
        public UInt64 id;
        public LatLon[] geometry { get; set; }

        public override UInt64 Id => id;
        public override UInt32 Type { get { return (UInt16)SpMapContentType.LinkGeometry; } }

        public override LatLon[] Geometry => geometry;

        override public List<CmnObjHdlRef> GetObjRefHdlList(int refType, CmnTile thisTile, DirectionCode direction = DirectionCode.None)
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

        public override List<CmnObjRef> GetObjAllRefList(CmnTile tile, DirectionCode direction = DirectionCode.Positive)
        {
            List<CmnObjRef> ret = new List<CmnObjRef>();

            foreach (var connLink in connectLink)
            {

                CmnObjRef linkRef = new CmnObjRef((int)SpMapRefType.RelatedLink, (UInt32)SpMapContentType.Link);
                linkRef.key.tileId = connLink.tileId;
                linkRef.key.objIndex = connLink.linkIndex;

                ret.Add(linkRef);
            }

            return ret;
        }



        override public List<CmnObjHdlRef> GetObjRefHdlList(int refType, CmnTile tile, DirectionCode direction = DirectionCode.Positive)
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
                        refLink.nextRef.key.objDirection = x.linkDirection.Reverse();

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
        public DirectionCode linkDirection; //接続リンクへ退出する方向がリンクの順逆方向かどうか。１：順方向。遷移禁止フラグ？いずれbool型
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
            int tmp = (xy | 0x08) >> 3;
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

    public enum SpMapContentType : uint
    {
        Link = 1,
        Node = 2,
        LinkGeometry = 3,
        LinkAttribute = 4,
        Tile = 5

    }


    public enum SpMapRefType : uint
    {
        Selected = 1,
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
