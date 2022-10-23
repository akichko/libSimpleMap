using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akichko.libGis;

namespace libSimpleMap
{
    public class SpMapConvertSetting
    {
        public string inMapPath;
        public string outMapFilePath;
    }

    public class SpMapConverter
    {
        SpMapConvertSetting setting;

        public SpMapConverter(SpMapConvertSetting setting)
        {
            this.setting = setting;
        }

        public int ConvertTextToSqlite()
        {
            SQLite sqLite = new SQLite(setting.outMapFilePath);

            //空DB生成
            sqLite.CreateDB();

            //入力DB
            SpMapMgr textMapMgr = new SpMapMgr(MapDataType.TextFile);
            textMapMgr.Connect(setting.inMapPath);

            //出力DB
            SpMapMgr sqliteMapMgr = new SpMapMgr(MapDataType.SQLite);
            sqliteMapMgr.Connect(setting.outMapFilePath);

            //タイルリスト取得
            List<uint> tileList = textMapMgr.GetMapTileIdList();
            Console.WriteLine($"tileNum = {tileList.Count}");

            //タイルループ
            int count = 0;
            foreach (uint tileId in tileList)
            {
                Console.WriteLine($"[{++count}] {tileId}");

                textMapMgr.LoadTile(tileId);
                SpTile tmpTile = (SpTile)textMapMgr.SearchTile(tileId);

                tmpTile.CalcLinkCost(); //リンク長計算

                sqliteMapMgr.SaveTile(tmpTile);

                textMapMgr.UnloadTile(tileId);
            }

            sqliteMapMgr.Disconnect();

            //インデックス生成
            sqLite.ConnectDB();
            sqLite.CreateIndex();
            sqLite.DisconnectDB();

            return 0;
        }
    }
}
