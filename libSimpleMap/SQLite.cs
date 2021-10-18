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
using System.Threading;
using System.Data.SQLite;
using System.IO;

namespace libSimpleMap
{
    public class SQLiteAccess : IBinDataAccess
    {
        string mapPath;
        //byte[] commonBuf;

        //string dbDirectoryPath;
        //string dbPath;
        SQLiteConnection con;
        //SQLite handle;
        SemaphoreSlim semaphore;

        public SQLiteAccess()
        {
            semaphore = new SemaphoreSlim(1, 1);
        }


        public override int Connect(string mapPath)
        {
            this.mapPath = mapPath;

            if (!File.Exists(mapPath))
            {
                Console.WriteLine("Map not exists!");
                return -1;
            }

            con = new SQLiteConnection("Data Source=" + mapPath + ";Version=3;");
            con.Open();

            Console.WriteLine($"connected to DB: {mapPath}");

            return 0;

        }

        public override int Disconnect()
        {
            con.Close();
            Console.WriteLine("DisConnected");
            return 0;
        }


        public override List<uint> GetMapTileIdList()
        {

            List<uint> retList = new List<uint>();
            string sql = $"select distinct tile_id from map_tile";

            SQLiteCommand com = new SQLiteCommand(sql, con);
            SQLiteDataReader reader = com.ExecuteReader();

            while (reader.Read() == true)
            {
                Int64 tileId = (Int64)reader["tile_id"];

                retList.Add((uint)tileId);
            }


            return retList;
        }


        public override byte[] GetRawData(uint tileId, SpMapContentType contentType)
        {
            return null;
        }


        public override byte[] GetLinkData(uint tileId)
        {
            byte[] retBytes = null;

            string sql = $"select length(link) size, link from map_tile where tile_id = {tileId}";

            SQLiteCommand com = new SQLiteCommand(sql, con);
            semaphore.Wait();
            SQLiteDataReader reader = com.ExecuteReader();

            while (reader.Read() == true)
            {
                Int64 size = (Int64)reader["size"];
                retBytes = (byte[])reader["link"];

                //Console.WriteLine($"tileID = {tileId}, size(link) = {size}");
                //reader.GetBytes()
            }
            reader.Close();
            semaphore.Release();


            return retBytes;

        }

        public override byte[] GetNodeData(uint tileId)
        {
            byte[] retBytes = null;

            string sql = $"select length(node) size, node from map_tile where tile_id = {tileId}";

            SQLiteCommand com = new SQLiteCommand(sql, con);
            semaphore.Wait();
            SQLiteDataReader reader = com.ExecuteReader();

            while (reader.Read() == true)
            {
                Int64 size = (Int64)reader["size"];
                retBytes = (byte[])reader["node"];

                // Console.WriteLine($"tileID = {tileId}, size(node) = {size}");
                //reader.GetBytes()
            }

            reader.Close();
            semaphore.Release();

            return retBytes;
        }

        public override byte[] GetGeometryData(uint tileId)
        {

            byte[] retBytes = null;

            string sql = $"select length(geometry) size, geometry from map_tile where tile_id = {tileId}";

            SQLiteCommand com = new SQLiteCommand(sql, con);
            semaphore.Wait();
            SQLiteDataReader reader = com.ExecuteReader();

            while (reader.Read() == true)
            {
                Int64 size = (Int64)reader["size"];
                retBytes = (byte[])reader["geometry"];

                //Console.WriteLine($"tileID = {tileId}, size(geometry) = {size}");
                //reader.GetBytes()
            }
            reader.Close();
            semaphore.Release();

            return retBytes;
        }

        public override byte[] GetAttributeData(uint tileId)
        {
            //
            //return null;

            byte[] retBytes = null;

            string sql = $"select length(attribute) size, attribute from map_tile where tile_id = {tileId}";

            SQLiteCommand com = new SQLiteCommand(sql, con);
            semaphore.Wait();
            SQLiteDataReader reader = com.ExecuteReader();

            while (reader.Read() == true)
            {
                Int64 size = (Int64)reader["size"];
                retBytes = (byte[])reader["attribute"];

            }
            reader.Close();
            semaphore.Release();

            return retBytes;
        }

        public override int GetTileData(uint tileId, uint reqObjType, out byte[] outLinkData, out byte[] outNodeData, out byte[] outGeometryData, out byte[] outAttributeData)
        {
            outLinkData = null;
            outNodeData = null;
            outGeometryData = null;
            outAttributeData = null;

            string sqlStr = "select tile_id";

            if ((reqObjType & (uint)SpMapContentType.Link) != 0)
                sqlStr += ", link";
            if ((reqObjType & (uint)SpMapContentType.Node) != 0)
                sqlStr += ", node";
            if ((reqObjType & (uint)SpMapContentType.LinkGeometry) != 0)
                sqlStr += ", geometry";
            if ((reqObjType & (uint)SpMapContentType.LinkAttribute) != 0)
                sqlStr += ", attribute";
            sqlStr += $" from map_tile where tile_id = {tileId}";

            SQLiteCommand com = new SQLiteCommand(sqlStr, con);
            semaphore.Wait();
            SQLiteDataReader reader = com.ExecuteReader();

            while (reader.Read() == true)
            {
                if ((reqObjType & (uint)SpMapContentType.Link) != 0)
                    outLinkData = (byte[])reader["link"];
                if ((reqObjType & (uint)SpMapContentType.Node) != 0)
                    outNodeData = (byte[])reader["node"];
                if ((reqObjType & (uint)SpMapContentType.LinkGeometry) != 0)
                    outGeometryData = (byte[])reader["geometry"];
                if ((reqObjType & (uint)SpMapContentType.LinkAttribute) != 0)
                    outAttributeData = (byte[])reader["attribute"];
            }
            reader.Close();
            semaphore.Release();

            return 0;
        }

        public override int SaveLinkData(uint tileId, byte[] tileBuf, int size)
        {
            Array.Resize(ref tileBuf, size);

            //レコード有無チェック

            string sql = $"select tile_id from map_tile where tile_id = {tileId}";

            SQLiteCommand cmd = new SQLiteCommand(sql, con);
            SQLiteDataReader reader = cmd.ExecuteReader();
            SQLiteParameter param;

            if (reader.Read() == true)  //record有り
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"UPDATE MAP_TILE SET LINK = @0 where tile_id = {tileId}");
                param = new SQLiteParameter("@0", System.Data.DbType.Binary);
                param.Value = tileBuf;
                cmd.Parameters.Add(param);

            }
            else //レコードなし
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, LINK) VALUES ({tileId}, @0);");
                param = new SQLiteParameter("@0", System.Data.DbType.Binary);
                param.Value = tileBuf;
                cmd.Parameters.Add(param);
            }

            return cmd.ExecuteNonQuery();
        }


        public override int SaveNodeData(uint tileId, byte[] tileBuf, int size)
        {
            Array.Resize(ref tileBuf, size);

            //レコード有無チェック

            string sql = $"select tile_id from map_tile where tile_id = {tileId}";

            SQLiteCommand cmd = new SQLiteCommand(sql, con);
            SQLiteDataReader reader = cmd.ExecuteReader();
            SQLiteParameter param;

            if (reader.Read() == true)  //record有り
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"UPDATE MAP_TILE SET node = @0 where tile_id = {tileId}");
                param = new SQLiteParameter("@0", System.Data.DbType.Binary);
                param.Value = tileBuf;
                cmd.Parameters.Add(param);

            }
            else //レコードなし
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, node) VALUES ({tileId}, @0);");
                param = new SQLiteParameter("@0", System.Data.DbType.Binary);
                param.Value = tileBuf;
                cmd.Parameters.Add(param);
            }

            return cmd.ExecuteNonQuery();
        }

        public override int SaveGeometryData(uint tileId, byte[] tileBuf, int size)
        {
            Array.Resize(ref tileBuf, size);

            //レコード有無チェック

            string sql = $"select tile_id from map_tile where tile_id = {tileId}";

            SQLiteCommand cmd = new SQLiteCommand(sql, con);
            SQLiteDataReader reader = cmd.ExecuteReader();
            SQLiteParameter param;

            if (reader.Read() == true)  //record有り
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"UPDATE MAP_TILE SET geometry = @0 where tile_id = {tileId}");
                param = new SQLiteParameter("@0", System.Data.DbType.Binary);
                param.Value = tileBuf;
                cmd.Parameters.Add(param);

            }
            else //レコードなし
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, geometry) VALUES ({tileId}, @0);");
                param = new SQLiteParameter("@0", System.Data.DbType.Binary);
                param.Value = tileBuf;
                cmd.Parameters.Add(param);
            }

            return cmd.ExecuteNonQuery();
        }


        public override int SaveAllData(uint tileId, byte[] linkBuf, byte[] nodeBuf, byte[] geometryBuf, byte[] attributeBuf)
        {
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, link, node, geometry, attribute) VALUES ({tileId}, @linkBlob, @nodeBlob, @geometryBlob, @attributeBlob);");

            SQLiteParameter param1 = new SQLiteParameter("@linkBlob", System.Data.DbType.Binary);
            param1.Value = linkBuf;
            cmd.Parameters.Add(param1);

            SQLiteParameter param2 = new SQLiteParameter("@nodeBlob", System.Data.DbType.Binary);
            param2.Value = nodeBuf;
            cmd.Parameters.Add(param2);

            SQLiteParameter param3 = new SQLiteParameter("@geometryBlob", System.Data.DbType.Binary);
            param3.Value = geometryBuf;
            cmd.Parameters.Add(param3);

            SQLiteParameter param4 = new SQLiteParameter("@attributeBlob", System.Data.DbType.Binary);
            param4.Value = attributeBuf;
            cmd.Parameters.Add(param4);

            return cmd.ExecuteNonQuery();

        }

    }



    public class SQLite
    {
        string dbDirectoryPath;
        string dbPath;
        SQLiteConnection con;

        public SQLite(string dbDirectoryPath, string dbFileName)
        {
            this.dbDirectoryPath = dbDirectoryPath;
            this.dbPath = dbDirectoryPath + @"\" + dbFileName;
        }

        public int CreateDB()
        {
            if (!Directory.Exists(dbDirectoryPath))
            {
                Console.WriteLine("DB directory not exists");
                return -1;
            }

            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
                Console.WriteLine("DB already exists. delete.");
            }

            SQLiteConnection.CreateFile(dbPath);
            Console.WriteLine("DB created");

            ConnectDB();

            CreateTable();



            DisconnectDB();

            return 0;
        }



        public int ConnectDB()
        {

            con = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;");
            con.Open();

            Console.WriteLine("connected");

            return 0;
        }

        public int DisconnectDB()
        {
            con.Close();
            Console.WriteLine("DisConnected");
            return 0;
        }

        public int CreateTable()
        {
            string sql = "create table map_tile ( tile_id integer, link blob, node blob, geometry blob, attribute blob )";

            SQLiteCommand com = new SQLiteCommand(sql, con);
            int ret = com.ExecuteNonQuery();

            Console.WriteLine($"SQL exec returned {ret}");

            return 0;
        }

        public int CreateIndex()
        {
            string sql = "create index idx_map_tile on map_tile(tile_id)";

            SQLiteCommand com = new SQLiteCommand(sql, con);
            int ret = com.ExecuteNonQuery();

            Console.WriteLine($"SQL exec returned {ret}");

            return 0;
        }


        public int ExecNonQuerySQL(string sql)
        {
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = String.Format(sql);
            return cmd.ExecuteNonQuery();
        }

        public void InsertData(int tileId, byte[] blobLink)
        {

            blobLink = new byte[] { 1, 2, 3, 4, 5 };


            byte[] pic = new byte[] { 1, 2, 3, 4, 5 };

            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, LINK) VALUES ({tileId}, @0);");
            SQLiteParameter param = new SQLiteParameter("@0", System.Data.DbType.Binary);
            param.Value = pic;
            cmd.Parameters.Add(param);
            cmd.ExecuteNonQuery();
        }

        private void InsertData2(int tileId, byte[] blobLink)
        {


            byte[] photo = new byte[] { 1, 2, 3, 4, 5 };

            SQLiteCommand command = new SQLiteCommand("INSERT INTO MAP_MESH (LINK) VALUES (@photo)", con);
            //command.CommandText = "INSERT INTO MAP_MESH (LINK) VALUES (@photo)";
            command.Parameters.Add("@photo", System.Data.DbType.Binary, 20).Value = photo;
            command.ExecuteNonQuery();


        }

        public void test()
        {
            var constr = new SQLiteConnectionStringBuilder { DataSource = "sample.sqlite3" };  //データベースのファイルパスを指定
            using (var cn = new SQLiteConnection(constr.ToString()))
            {
                //データベースへ接続
                cn.Open();

                using (var cmd = new SQLiteCommand(cn))
                {
                    //データ抽出
                    cmd.CommandText = $"SELECT * FROM t_sample";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(reader.GetValues().Get("data"));
                        }
                    }
                }
            }
        }


    }
}
