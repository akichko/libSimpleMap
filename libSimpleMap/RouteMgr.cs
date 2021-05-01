using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using libGis;

namespace libSimpleMap
{

    //public class CostRecord
    //{
    //    public int totalCost; //経路始点～当該リンクまでのコスト
    //    public int remainCost; //目的地側から計算した残りコスト

    //    public byte status = 0; //0:未開始　1:計算中　2:探索終了
    //    //public byte statusD = 0; //0:未開始　1:計算中　2:探索終了  終点側
    //    public bool isGoal = false; //以降は目的地側から計算済み

    //    public TileCostInfo tileCostInfo; //親参照
    //    //public Int64 linkId; //最悪なくてもよい
    //    public short linkIndex;
    //    public byte linkDirection;

    //    public CostRecord back;
    //    //public CostRecord next;

    //    public UInt64 LinkId
    //    {
    //        get { return tileCostInfo.tile.link[linkIndex].linkId; }
    //    }

    //    public MapLink MapLink
    //    {
    //        get { return tileCostInfo.tile.link[linkIndex]; }
    //    }

    //    public LinkHandle LinkHdl
    //    {
    //        get { return tileCostInfo.tile.GetMapLink(linkIndex); }
    //    }
    //}

    //public class TileCostInfo
    //{
    //    public uint tileId;
    //    public SpTile tile;
    //    public CostRecord[][] costInfo; //始点方向リンク、終点方向リンク
    //    public byte status = 0; //0:未開始　1:計算中　2:探索終了

    //    public byte maxUsableRoadType = 255; //分岐先に移動可能な道路種別
    //    public byte readStatus = 0; //読み込んだ最大道路種別
    //    public float DistFromStartTile;
    //    public float DistFromDestTile;

    //    public TileCostInfo(uint tileId)
    //    {
    //        this.tileId = tileId;
    //    }

    //    public int SetTileCostInfo(SpTile tile)
    //    {
    //        this.tileId = tile.tileId;
    //        this.tile = tile;
    //        int numLink = tile.link.Length;
    //        costInfo = new CostRecord[numLink][];

    //        for (short i = 0; i < numLink; i++)
    //        {
    //            costInfo[i] = new CostRecord[2];
    //            costInfo[i][0] = new CostRecord();
    //            costInfo[i][1] = new CostRecord();
    //            costInfo[i][0].tileCostInfo = this;
    //            costInfo[i][1].tileCostInfo = this;
    //            costInfo[i][0].linkIndex = i;
    //            costInfo[i][1].linkIndex = i;
    //            costInfo[i][0].linkDirection = 0;
    //            costInfo[i][1].linkDirection = 1;
    //        }
    //        return 0;
    //    }

    //    public int CalcTileDistance(uint startTileId, uint destTileId)
    //    {
    //        DistFromStartTile = (float)GisTileCode.S_CalcTileDistance(tileId, startTileId);
    //        DistFromDestTile = (float)GisTileCode.S_CalcTileDistance(tileId, destTileId);

    //        return 0;
    //    }



    //}


    //public class Dykstra
    //{
    //    public Dictionary<uint, TileCostInfo> dicTileCostInfo;
    //    List<CostRecord> unprocessed;
    //    List<CostRecord> goalInfo;
    //    //public int tileCount = 0;

    //    public LinkHandle startLinkHdl;
    //    public LinkHandle destLinkHdl;

    //    SpMapMgr mapMgr;


    //    //性能測定用
    //    public int[] logTickCountList;
    //    public int[] logUnprocessedCount;
    //    public int logMaxQueue = 0;
    //    public int logCalcCount = 0;

    //    public Dykstra(SpMapMgr mapMgr)
    //    {
    //        //costTab = new RouteCost();
    //        this.mapMgr = mapMgr;
    //        dicTileCostInfo = new Dictionary<uint, TileCostInfo>();
    //        goalInfo = new List<CostRecord>();

    //        unprocessed = new List<CostRecord>();
    //        logUnprocessedCount = new int[1000000];
    //        logTickCountList = new int[1000000];


    //    }

    //    public int SetStartCost(LinkHandle linkHdr, int offset, sbyte direction)
    //    {
    //        //TileObjId start = new TileObjId(mapPos.tileId, mapPos.linkId);
    //        //MapLink sMapLink = mapMgr.SearchMapLink(start);

    //        if (direction == 1 || direction >= 2)
    //        {
    //            CostRecord costRecS = GetLinkCostInfo(null, linkHdr.tile.tileId, linkHdr.mapLink.index, 1);

    //            costRecS.linkDirection = 1;
    //            costRecS.totalCost = offset;
    //            costRecS.status = 1;
    //            unprocessed.Add(costRecS);
    //        }

    //        if (direction == 0 || direction >= 2)
    //        {
    //            CostRecord costRecE = GetLinkCostInfo(null, linkHdr.tile.tileId, linkHdr.mapLink.index, 0);

    //            costRecE.linkDirection = 0;
    //            costRecE.totalCost = offset;
    //            costRecE.status = 1;
    //            unprocessed.Add(costRecE);
    //        }

    //        return 0;
    //    }

    //    public int SetDestination(LinkHandle linkHdr, int offset, sbyte direction)
    //    {
    //        //offsetが暫定
    //        if (direction == 1 || direction >= 2)
    //        {
    //            CostRecord costRecS = GetLinkCostInfo(null, linkHdr.tile.tileId, linkHdr.mapLink.index, 1);

    //            costRecS.isGoal = true;
    //            costRecS.remainCost = offset;
    //            goalInfo.Add(costRecS);
    //        }

    //        if (direction == 0 || direction >= 2)
    //        {
    //            CostRecord costRecE = GetLinkCostInfo(null, linkHdr.tile.tileId, linkHdr.mapLink.index, 0);

    //            costRecE.isGoal = true;
    //            costRecE.remainCost = offset;
    //            goalInfo.Add(costRecE);
    //        }

    //        return 0;
    //    }

    //    public int SetTileInfo(uint tileId, byte maxUsableRoadType)
    //    {
    //        if (dicTileCostInfo.ContainsKey(tileId))
    //            return 0;

    //        TileCostInfo tmpTileCostInfo = new TileCostInfo(tileId);

    //        tmpTileCostInfo.CalcTileDistance(startLinkHdl.tile.tileId, destLinkHdl.tile.tileId);
    //        tmpTileCostInfo.maxUsableRoadType = maxUsableRoadType;
    //        dicTileCostInfo.Add(tileId, tmpTileCostInfo);

    //        return 0;
    //    }

    //    public int AddTileInfo(uint tileId)
    //    {
    //        if (!dicTileCostInfo.ContainsKey(tileId))
    //            return 0;

    //        TileCostInfo tmpTileCostInfo = dicTileCostInfo[tileId];
    //        if (tmpTileCostInfo.tile != null)
    //            return 0;

    //        mapMgr.LoadTile(tileId, (UInt32)(SpMapContentType.Link | SpMapContentType.Node), tmpTileCostInfo.maxUsableRoadType);
    //        CmnTile tmpTile = mapMgr.SearchTile(tileId);

    //        tmpTileCostInfo.SetTileCostInfo((SpTile)tmpTile);
    //        Console.Write($"\r {dicTileCostInfo.Count()} tiles read");


    //        return 0;
    //    }

    //    public TileCostInfo GetTileCostInfo(uint tileId)
    //    {
    //        if (dicTileCostInfo.ContainsKey(tileId))
    //            return null;

    //        return dicTileCostInfo[tileId];
    //    }

    //    public CostRecord GetLinkCostInfo(CostRecord currentInfo, uint tileId, int linkIndex, int linkDirection)
    //    {
    //        if (currentInfo != null && currentInfo.tileCostInfo.tileId == tileId)
    //        {
    //            return currentInfo.tileCostInfo.costInfo[linkIndex][linkDirection];
    //        }

    //        if (!dicTileCostInfo.ContainsKey(tileId))
    //            return null;

    //        return dicTileCostInfo[tileId].costInfo[linkIndex][linkDirection];

    //    }

    //    public CostRecord GetLinkCostInfo(CostRecord currentInfo, DLinkHandle linkRef)
    //    {
    //        return GetLinkCostInfo(currentInfo, linkRef.tile.tileId, linkRef.mapLink.index, linkRef.direction);
    //    }


    //    /****** 経路計算メイン ******************************************************************************/

    //    public int CalcRouteStep()
    //    {
    //        //計算対象選定　処理未完了＆コスト最小を探す            
    //        var minRecord = unprocessed
    //            .Select((p, i) => new { Content = p, Index = i })
    //            .OrderBy(x => x.Content.totalCost)
    //            .FirstOrDefault();

    //        if (minRecord == null)
    //        {
    //            //currentCostInfo = costTab.GetMinCostRecord();
    //            //if(currentCostInfo == null)
    //            //    return -1;
    //            Console.WriteLine($"[{Environment.TickCount / 1000.0:F3}] All Calculation Finished! Destination not Found");
    //            return -1;
    //        }
    //        int minIndex = minRecord.Index;

    //        CostRecord currentCostInfo = unprocessed[minIndex];

    //        if (currentCostInfo == null)
    //        {
    //            Console.WriteLine("Fatal Error");
    //            return -1;

    //        }
    //        if (currentCostInfo.isGoal)
    //        {
    //            Console.WriteLine($"[{Environment.TickCount / 1000.0:F3}] Goal Found ! (CalcCount = {logCalcCount}, totalCost = {currentCostInfo.totalCost})");
    //            currentCostInfo.status = 2;
    //            return -1;
    //        }

    //        if (currentCostInfo.status == 2)
    //        {
    //            FastDelete(unprocessed, minIndex);
    //            return 0;
    //        }

    //        //LinkHandle currentLinkHdl = currentCostInfo.tileCostInfo.tile.GetMapLink(currentCostInfo.linkIndex);
    //        LinkHandle currentLinkHdl = currentCostInfo.LinkHdl;

    //        //起点ノードのTileがキャッシュされているか
    //        NodeHandle baseNodeHdl = mapMgr.GetEdgeNode(currentLinkHdl.tile, currentLinkHdl.mapLink, currentCostInfo.linkDirection, false);
    //        if (baseNodeHdl.mapNode == null && baseNodeHdl.tileId != 0)
    //        {
    //            //タイル追加読み込み
    //            AddTileInfo(baseNodeHdl.tileId);

    //            //ハンドル再取得
    //            baseNodeHdl = mapMgr.GetEdgeNode(currentLinkHdl.tile, currentLinkHdl.mapLink, currentCostInfo.linkDirection, false);
    //        }

    //        List<DLinkHandle> connectLinkList = mapMgr.GetConnectLinks(currentLinkHdl, currentCostInfo.linkDirection, true, true);

    //        List<DLinkHandle> connectLinkListTest = mapMgr.GetConnectLinks(baseNodeHdl, false);
    //        foreach (DLinkHandle a in connectLinkListTest.Where(x => x.mapLink == null))
    //        {
    //            //タイル追加読み込み
    //            AddTileInfo(a.tileId);
    //        }

    //        connectLinkList = mapMgr.GetConnectLinks(currentLinkHdl, currentCostInfo.linkDirection, true, true);


    //        //接続リンクとコスト参照
    //        foreach (DLinkHandle nextLinkRef in connectLinkList)
    //        {
    //            if (nextLinkRef.mapLink == currentLinkHdl.mapLink
    //                || nextLinkRef.mapLink.roadType > currentCostInfo.tileCostInfo.maxUsableRoadType)
    //                continue;
    //            //.Where(x => x.mapLink != currentLinkHdl.mapLink && x.mapLink.roadType <= currentCostInfo.tileCostInfo.maxUsableRoadType) )


    //            if (nextLinkRef.mapLink.roadType >= 6
    //                && currentLinkHdl.mapLink.roadType < nextLinkRef.mapLink.roadType
    //                && currentCostInfo.tileCostInfo.DistFromDestTile > 8000)
    //                continue;

    //            //int nextLinkTileId = nextLinkRef.tile.tileId;
    //            //int nextLinkIndex = nextLinkRef.mapLink.index;
    //            //int nextLinkDirection = nextLinkRef.direction;
    //            //CostRecord nextCostInfo = GetLinkCostInfo(currentCostInfo, nextLinkTileId, nextLinkIndex, nextLinkDirection);

    //            CostRecord nextCostInfo = GetLinkCostInfo(currentCostInfo, nextLinkRef);

    //            int nextTotalCost;

    //            //ゴールフラグの場合は、残コストを足す。足すけど保存NG？ゴール側statusを見るべき？
    //            if (nextCostInfo.isGoal)
    //            {
    //                nextTotalCost = currentCostInfo.totalCost + nextCostInfo.remainCost;
    //            }
    //            else
    //            {
    //                nextTotalCost = currentCostInfo.totalCost + nextLinkRef.mapLink.linkCost;
    //            }
    //            //コストを足した値を、接続リンクの累積コストを見て、より小さければ上書き
    //            if (nextCostInfo.status == 0 || nextTotalCost < nextCostInfo.totalCost)
    //            {
    //                //nextCostInfo.linkDirection = nextLinkRef.direction;
    //                nextCostInfo.totalCost = nextTotalCost;
    //                nextCostInfo.status = 1;
    //                //nextCostInfo.tileCostInfo.status = 1;
    //                nextCostInfo.back = currentCostInfo;
    //                unprocessed.Add(nextCostInfo);
    //            }

    //        }

    //        //リンクの探索ステータス更新
    //        currentCostInfo.status = 2;
    //        FastDelete(unprocessed, minIndex);

    //        return 0;
    //    }


    //    public void FastDelete(List<CostRecord> list, int index)
    //    {
    //        list[index] = list[list.Count - 1];
    //        list.RemoveAt(list.Count - 1);
    //    }


    //    public int CalcRoute()
    //    {
    //        int ret;

    //        int pastTickCount = Environment.TickCount;
    //        int nowTickCount;
    //        //計算
    //        while (true)
    //        {
    //            ret = CalcRouteStep();

    //            nowTickCount = Environment.TickCount;
    //            logTickCountList[logCalcCount] = nowTickCount - pastTickCount;
    //            pastTickCount = nowTickCount;
    //            logUnprocessedCount[logCalcCount] = unprocessed.Count;
    //            if (unprocessed.Count > logMaxQueue)
    //                logMaxQueue = unprocessed.Count;
    //            logCalcCount++;

    //            if (ret != 0) break;
    //        }

    //        return 0;
    //    }



    //    public int PrintResult()
    //    {
    //        //List<int> routeIdList = new 
    //        CostRecord Bestgoal = goalInfo.Where(x => x.status == 2).OrderBy(x => x.totalCost).FirstOrDefault();
    //        if (Bestgoal == null)
    //        {
    //            Console.WriteLine("No Result!");
    //            return -1;
    //        }
    //        Console.WriteLine($"Goal: {Bestgoal.tileCostInfo.tile.link[Bestgoal.linkIndex]}");
    //        CostRecord tmp = Bestgoal;
    //        while (tmp.back != null)
    //        {
    //            Console.WriteLine($" {tmp.tileCostInfo.tileId}\t{tmp.LinkId}\t{tmp.linkIndex}\t{tmp.linkDirection}\t{tmp.MapLink.roadType}\t{tmp.totalCost}");
    //            //Console.WriteLine($" {tmp.linkId}\t{tmp.totalCost}");
    //            tmp = tmp.back;
    //        }


    //        return 0;
    //    }

    //    public int WriteResult()
    //    {
    //        return 0;
    //    }

    //    public List<DLinkHandle> GetResult()
    //    {
    //        //List<List<LinkRef>> resultInfo = new List<List<LinkRef>>();
    //        CostRecord goal = goalInfo.Where(x => x.status == 2).OrderBy(x => x.totalCost).FirstOrDefault();
    //        if (goal == null)
    //        {
    //            Console.WriteLine("no goal");
    //            return null;
    //        }
    //        List<DLinkHandle> routeIdList = new List<DLinkHandle>();
    //        CostRecord tmp = goal;
    //        while (tmp.back != null)
    //        {
    //            DLinkHandle tmpLinkRef = new DLinkHandle();
    //            tmpLinkRef.tile = tmp.tileCostInfo.tile;
    //            tmpLinkRef.mapLink = tmp.tileCostInfo.tile.link[tmp.linkIndex];
    //            tmpLinkRef.direction = tmp.linkDirection;
    //            tmp = tmp.back;
    //            routeIdList.Add(tmpLinkRef);
    //        }
    //        routeIdList.Reverse();
    //        return routeIdList;
    //        //return resultInfo;
    //    }


    //    public int PrintCalcCount()
    //    {
    //        Console.WriteLine($"calcCount = {logCalcCount}");
    //        return 0;
    //    }

    //}


    //public class RouteMgr
    //{

    //    SpMapMgr mapMgr;

    //    LinkHandle orgHdl;
    //    LinkHandle destHdl;

    //    //経路計算用メモリ
    //    public Dykstra dykstra;

    //    public RouteMgr(SpMapMgr mapMgr)
    //    {
    //        //startPos = new MapPos();
    //        //destPos = new MapPos();
    //        dykstra = new Dykstra(mapMgr);
    //        this.mapMgr = mapMgr;
    //    }


    //    public int SetOrginLinkHdl(LinkHandle handle)
    //    {
    //        this.orgHdl = handle;
    //        dykstra.startLinkHdl = handle;
    //        return 0;
    //    }


    //    public int SetDestinationLinkHdl(LinkHandle handle)
    //    {
    //        this.destHdl = handle;
    //        dykstra.destLinkHdl = handle;
    //        return 0;
    //    }


    //    //データ準備
    //    public int Prepare(bool allCache)
    //    {
    //        //探索レベルを決める
    //        //CalcSearchLevel();

    //        //タイル決定
    //        List<uint> searchTileId = CalcRouteTileId2();

    //        Console.WriteLine($"[{Environment.TickCount / 1000.0:F3}] calc tile num = {searchTileId.Count}");

    //        //メッシュ読み込み・コストテーブル登録
    //        ReadTiles(searchTileId, allCache);

    //        Console.WriteLine($"[{Environment.TickCount / 1000.0:F3}] read tile num = {dykstra.dicTileCostInfo.Count}");

    //        //始終点コスト設定
    //        dykstra.AddTileInfo(orgHdl.tile.tileId);
    //        dykstra.AddTileInfo(destHdl.tile.tileId);

    //        dykstra.SetStartCost(orgHdl, 10, 2);
    //        dykstra.SetDestination(destHdl, 10, 2);


    //        return 0;
    //    }



    //    //ダイクストラ計算
    //    public int CalcRoute()
    //    {
    //        //目的地側の計算（レベル２探索以上の場合?）


    //        //現在値から計算
    //        dykstra.CalcRoute();

    //        return 0;
    //    }


    //    private List<uint> CalcRouteTileId2()
    //    {
    //        //最小エリア対応


    //        double ratio = 1.2;
    //        return GisTileCode.CalcTileEllipse(orgHdl.tile.tileId, destHdl.tile.tileId, ratio);
    //    }


    //    private int ReadTiles(List<uint> tileIdList, bool allCache)
    //    {
    //        Console.WriteLine("tile reading");
    //        //int count = 0;
    //        foreach (uint tileId in tileIdList)
    //        {
    //            byte maxRoadType = CalcMaxUsableRoadType(tileId, orgHdl.tile.tileId, destHdl.tile.tileId);

    //            dykstra.SetTileInfo(tileId, maxRoadType);

    //            if (allCache)
    //            {
    //                dykstra.AddTileInfo(tileId);
    //            }

    //        }
    //        Console.WriteLine("tile read finished");

    //        return 0;
    //    }


    //    private byte CalcMaxUsableRoadType(uint targetTileId, uint startTileId, uint destTileId)
    //    {
    //        float DistFromStartTile = (float)GisTileCode.S_CalcTileDistance(targetTileId, startTileId);
    //        float DistFromDestTile = (float)GisTileCode.S_CalcTileDistance(targetTileId, destTileId);

    //        double minDist = Math.Min(DistFromStartTile, DistFromDestTile);

    //        //double minDist = MapTool.CalcTileDistance(tileId, startTileId);
    //        //double distFromDest = MapTool.CalcTileDistance(tileId, destTileId);
    //        //minDist = Math.Min(minDist, distFromDest);

    //        if (minDist < 2000)
    //        {
    //            return 7;
    //        }
    //        else if (minDist < 8000)
    //        {
    //            return 6;
    //        }
    //        else if (minDist < 15000)
    //        {
    //            return 4;
    //        }
    //        else if (minDist < 50000)
    //        {
    //            return 3;
    //        }
    //        else if (minDist < 100000)
    //        {
    //            return 2;
    //        }
    //        else //  >100km
    //        {
    //            return 1;
    //        }

    //    }


    //    //結果出力

    //    //public void WriteCacheTileXY()
    //    //{
    //    //    Console.WriteLine("Writing Cache Tile List ... ");

    //    //    using (var sw = new StreamWriter(@"D:\share\osm\cacheTile.txt"))
    //    //    {

    //    //        foreach (var x in dykstra.dicTileCostInfo.Where(x => x.Value.costInfo != null))
    //    //        {
    //    //            LatLon tmp = GisTileCode.SCalcLatLon(x.Value.tileId);
    //    //            //TileXY tmp = new TileXY(x.Value.tileId);
    //    //            //Console.WriteLine($" [{x.Value.tileId}], {tmp.lon}, {tmp.lat}");
    //    //            sw.WriteLine($"{x.Value.tileId}, {tmp.lon}, {tmp.lat}");
    //    //        }
    //    //    }
    //    //}

    //    //public void WriteCalclatedLinks()
    //    //{
    //    //    Console.WriteLine("Writing calculated Link List ... ");

    //    //    int tileCount = 0;
    //    //    using (var sw = new StreamWriter(@"D:\share\osm\calculatedLink.txt"))
    //    //    {
    //    //        foreach (var tileCost in dykstra.dicTileCostInfo.Values.Where(x => x.costInfo != null))
    //    //        {

    //    //            var linkList = tileCost.costInfo
    //    //                .SelectMany(x => x)
    //    //                .Where(x => x.status != 0)
    //    //                .Select(x => x.tileCostInfo.tile.link[x.linkIndex]);

    //    //            if (linkList.Count() > 0)
    //    //            {
    //    //                tileCount++;
    //    //                //Console.WriteLine($"tile = {tileCost.tileId} count={tileCount}");
    //    //                mapMgr.LoadTile(tileCost.tileId);
    //    //            }

    //    //            foreach (var link in linkList)
    //    //            {
    //    //                link.WriteGeometry(0, sw);
    //    //                sw.WriteLine("");

    //    //            }

    //    //        }
    //    //        Console.WriteLine($"calculated tile num = {tileCount}");

    //    //    }
    //    //}

    //    public int PrintResult()
    //    {
    //        return dykstra.PrintResult();
    //    }

    //    //public int PrintCalcCount()
    //    //{
    //    //    return dykstra.PrintCalcCount();

    //    //}

    //    //public int WriteResult()
    //    //{
    //    //    return dykstra.WriteResult();
    //    //}

    //    public List<DLinkHandle> GetResult()
    //    {
    //        return dykstra.GetResult();
    //    }



    //}

}
