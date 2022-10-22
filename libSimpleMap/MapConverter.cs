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


        public string outDbFile;

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
                textMapMgr.LoadTile(tileId);

                Console.WriteLine($"[{++count}] {tileId.ToString()}");

                CmnTile tmpTile = textMapMgr.SearchTile(tileId);

                ((SpTile)tmpTile).CalcLinkCost(); //リンク長計算

                sqliteMapMgr.SaveTile((SpTile)tmpTile);

                textMapMgr.UnloadTile(tileId);

            }

            sqliteMapMgr.Disconnect();

            //インデックス生成
            sqLite.ConnectDB();
            sqLite.CreateIndex();
            sqLite.DisconnectDB();

            //Console.WriteLine("GenerateSQLiteMap End");
            //Console.ReadKey();

            return 0;
        }
    }
}
