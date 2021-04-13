using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace libSimpleMap
{
    public class SQLiteAccess : IDataAccess
    {
        string mapPath;
        //byte[] commonBuf;

        //string dbDirectoryPath;
        //string dbPath;
        SQLiteConnection con;
        //SQLite handle;

        public SQLiteAccess() { }


        public int Connect(string mapPath)
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

        public int Disconnect()
        {
            con.Close();
            Console.WriteLine("DisConnected");
            return 0;
        }



        public byte[] GetRawData(uint tileId, SpMapContentType contentType)
        {
            return null;
        }


        public byte[] GetLinkData(uint tileId)
        {
            byte[] retBytes = null;

            string sql = $"select length(link) size, link from map_tile where tile_id = {tileId}";

            SQLiteCommand com = new SQLiteCommand(sql, con);
            SQLiteDataReader reader = com.ExecuteReader();

            while (reader.Read() == true)
            {
                Int64 size = (Int64)reader["size"];
                retBytes = (byte[])reader["link"];

                //Console.WriteLine($"tileID = {tileId}, size(link) = {size}");
                //reader.GetBytes()
            }


            return retBytes;

        }

        public byte[] GetNodeData(uint tileId)
        {
            byte[] retBytes = null;

            string sql = $"select length(node) size, node from map_tile where tile_id = {tileId}";

            SQLiteCommand com = new SQLiteCommand(sql, con);
            SQLiteDataReader reader = com.ExecuteReader();

            while (reader.Read() == true)
            {
                Int64 size = (Int64)reader["size"];
                retBytes = (byte[])reader["node"];

                // Console.WriteLine($"tileID = {tileId}, size(node) = {size}");
                //reader.GetBytes()
            }

            return retBytes;
        }

        public byte[] GetGeometryData(uint tileId)
        {

            byte[] retBytes = null;

            string sql = $"select length(geometry) size, geometry from map_tile where tile_id = {tileId}";

            SQLiteCommand com = new SQLiteCommand(sql, con);
            SQLiteDataReader reader = com.ExecuteReader();

            while (reader.Read() == true)
            {
                Int64 size = (Int64)reader["size"];
                retBytes = (byte[])reader["geometry"];

                //Console.WriteLine($"tileID = {tileId}, size(geometry) = {size}");
                //reader.GetBytes()
            }

            return retBytes;
        }


        public int SaveLinkData(uint tileId, byte[] tileBuf, int size)
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


        public int SaveNodeData(uint tileId, byte[] tileBuf, int size)
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

        public int SaveGeometryData(uint tileId, byte[] tileBuf, int size)
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


        public int SaveAllData(uint tileId, byte[] linkBuf, byte[] nodeBuf, byte[] geometryBuf)
        {
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, link, node, geometry) VALUES ({tileId}, @linkBlob, @nodeBlob, @geometryBlob);");

            SQLiteParameter param1 = new SQLiteParameter("@linkBlob", System.Data.DbType.Binary);
            param1.Value = linkBuf;
            cmd.Parameters.Add(param1);

            SQLiteParameter param2 = new SQLiteParameter("@nodeBlob", System.Data.DbType.Binary);
            param2.Value = nodeBuf;
            cmd.Parameters.Add(param2);

            SQLiteParameter param3 = new SQLiteParameter("@geometryBlob", System.Data.DbType.Binary);
            param3.Value = geometryBuf;
            cmd.Parameters.Add(param3);

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
