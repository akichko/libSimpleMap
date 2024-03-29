﻿/*============================================================================
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
using Npgsql;

namespace libSimpleMap
{

    public class PostgresAccess : IBinDataAccess
    {
        NpgsqlConnection con;
        //string connectInfo;
        SemaphoreSlim semaphore;

        public PostgresAccess()
        {
            semaphore = new SemaphoreSlim(1, 1);
        }


        public override int Connect(string connectStr)
        {
            //connectInfo = "Server=" + hostName + "; " //接続先サーバ 
            //                + "Port=" + port.ToString() + ";"  //ポート番号
            //                + "User Id=" + userId + ";"  //接続ユーザ
            //                + "Password=" + pass + ";" //パスワード
            //                + "Database=" + DbName + ";"; //接続先データベース

            //インスタンスを生成
            con = new NpgsqlConnection(connectStr);

            //データベース接続
            con.Open();
            //Console.WriteLine("接続開始");

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

            NpgsqlCommand com = new NpgsqlCommand(sql, con);
            semaphore.Wait();
            NpgsqlDataReader reader = com.ExecuteReader();

            while (reader.Read() == true)
            {
                Int64 tileId = (Int64)reader["tile_id"];

                retList.Add((uint)tileId);
            }
            reader.Close();
            semaphore.Release();


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

            NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            semaphore.Wait();
            NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read() == true)
            {
                Int32 size = (Int32)reader["size"];
                retBytes = (byte[])reader["link"];

            }
            reader.Close();
            semaphore.Release();

            return retBytes;

        }

        public override byte[] GetNodeData(uint tileId)
        {
            byte[] retBytes = null;

            string sql = $"select length(node) size, node from map_tile where tile_id = {tileId}";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            semaphore.Wait();
            NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read() == true)
            {
                Int32 size = (Int32)reader["size"];
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

            NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            semaphore.Wait();
            NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read() == true)
            {
                Int32 size = (Int32)reader["size"];
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

            NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            semaphore.Wait();
            NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read() == true)
            {
                Int32 size = (Int32)reader["size"];
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

            NpgsqlCommand com = new NpgsqlCommand(sqlStr, con);
            semaphore.Wait();
            NpgsqlDataReader reader = com.ExecuteReader();

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

            NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            NpgsqlParameter param;

            if (reader.Read() == true)  //record有り
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"UPDATE MAP_TILE SET LINK = @0 where tile_id = {tileId}");
                param = new NpgsqlParameter("@0", System.Data.DbType.Binary);
                param.Value = tileBuf;
                cmd.Parameters.Add(param);

            }
            else //レコードなし
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, LINK) VALUES ({tileId}, @0);");
                param = new NpgsqlParameter("@0", System.Data.DbType.Binary);
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

            NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            NpgsqlParameter param;

            if (reader.Read() == true)  //record有り
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"UPDATE MAP_TILE SET node = @0 where tile_id = {tileId}");
                param = new NpgsqlParameter("@0", System.Data.DbType.Binary);
                param.Value = tileBuf;
                cmd.Parameters.Add(param);

            }
            else //レコードなし
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, node) VALUES ({tileId}, @0);");
                param = new NpgsqlParameter("@0", System.Data.DbType.Binary);
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

            NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            NpgsqlParameter param;

            if (reader.Read() == true)  //record有り
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"UPDATE MAP_TILE SET geometry = @0 where tile_id = {tileId}");
                param = new NpgsqlParameter("@0", System.Data.DbType.Binary);
                param.Value = tileBuf;
                cmd.Parameters.Add(param);

            }
            else //レコードなし
            {
                cmd = con.CreateCommand();
                cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, geometry) VALUES ({tileId}, @0);");
                param = new NpgsqlParameter("@0", System.Data.DbType.Binary);
                param.Value = tileBuf;
                cmd.Parameters.Add(param);
            }

            return cmd.ExecuteNonQuery();
        }


        public override int SaveAllData(uint tileId, byte[] linkBuf, byte[] nodeBuf, byte[] geometryBuf, byte[] attributeBuf)
        {
            NpgsqlCommand cmd = con.CreateCommand();
            cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, link, node, geometry, attribute) VALUES ({tileId}, @linkBlob, @nodeBlob, @geometryBlob, @attributeBlob);");

            NpgsqlParameter param1 = new NpgsqlParameter("@linkBlob", System.Data.DbType.Binary);
            param1.Value = linkBuf;
            cmd.Parameters.Add(param1);

            NpgsqlParameter param2 = new NpgsqlParameter("@nodeBlob", System.Data.DbType.Binary);
            param2.Value = nodeBuf;
            cmd.Parameters.Add(param2);

            NpgsqlParameter param3 = new NpgsqlParameter("@geometryBlob", System.Data.DbType.Binary);
            param3.Value = geometryBuf;
            cmd.Parameters.Add(param3);

            NpgsqlParameter param4 = new NpgsqlParameter("@attributeBlob", System.Data.DbType.Binary);
            param4.Value = attributeBuf;
            cmd.Parameters.Add(param4);

            return cmd.ExecuteNonQuery();

        }

        public override SimpleMapDbRecord GetTileData(uint tileId, IEnumerable<uint> reqTypes)
        {
            throw new NotImplementedException();
        }
    }
    

    public class NpgsqlAccess
    {
        string connectInfo = string.Empty;
        NpgsqlConnection con;

        public NpgsqlAccess(string hostName, ushort port, string userId, string pass, string DbName)
        {
            //接続情報を作成
            connectInfo = "Server=" + hostName + "; " //接続先サーバ 
                            + "Port=" + port.ToString() + ";"  //ポート番号
                            + "User Id=" + userId + ";"  //接続ユーザ
                            + "Password=" + pass + ";" //パスワード
                            + "Database=" + DbName + ";"; //接続先データベース
        }

        public int CreateDB()
        {
            ConnectDB();

            CreateTable();



            DisconnectDB();

            return 0;
        }



        public int ConnectDB()
        {

            con = new NpgsqlConnection(connectInfo);
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
            string sql = "create table map_tile ( tile_id integer, link bytea, node bytea, geometry bytea, attribute bytea )";

            NpgsqlCommand com = new NpgsqlCommand(sql, con);
            int ret = com.ExecuteNonQuery();

            Console.WriteLine($"SQL exec returned {ret}");

            return 0;
        }

        public int CreateIndex()
        {
            string sql = "create index idx_map_tile on map_tile(tile_id)";

            NpgsqlCommand com = new NpgsqlCommand(sql, con);
            int ret = com.ExecuteNonQuery();

            Console.WriteLine($"SQL exec returned {ret}");

            return 0;
        }


        public int ExecNonQuerySQL(string sql)
        {
            NpgsqlCommand cmd = con.CreateCommand();
            cmd.CommandText = String.Format(sql);
            return cmd.ExecuteNonQuery();
        }

        public void InsertData(int tileId, byte[] blobLink)
        {

            blobLink = new byte[] { 1, 2, 3, 4, 5 };


            byte[] pic = new byte[] { 1, 2, 3, 4, 5 };

            NpgsqlCommand cmd = con.CreateCommand();
            cmd.CommandText = String.Format($"INSERT INTO MAP_TILE (tile_id, LINK) VALUES ({tileId}, @0);");
            NpgsqlParameter param = new NpgsqlParameter("@0", System.Data.DbType.Binary);
            param.Value = pic;
            cmd.Parameters.Add(param);
            cmd.ExecuteNonQuery();
        }



    }


    class Postgres
    {
        NpgsqlConnection con;
        NpgsqlDataReader dr;


        public int ConnectDB(string connectStr)
        {
            //インスタンスを生成
            con = new NpgsqlConnection(connectStr);

            //データベース接続
            con.Open();
            Console.WriteLine("接続開始");

            return 0;
        }


        public int ConnectDB(string hostName, ushort port, string userId, string pass, string DbName)
        {
            string connectInfo = string.Empty;

            //接続情報を作成
            connectInfo = "Server=" + hostName + "; " //接続先サーバ 
                            + "Port=" + port.ToString() + ";"  //ポート番号
                            + "User Id=" + userId + ";"  //接続ユーザ
                            + "Password=" + pass + ";" //パスワード
                            + "Database=" + DbName + ";"; //接続先データベース

            //インスタンスを生成
            con = new NpgsqlConnection(connectInfo);

            //データベース接続
            con.Open();
            Console.WriteLine("接続開始");

            return 0;
        }

        public void DisconnectDB()
        {
            Console.WriteLine("接続解除");
            con.Close();
        }


        public int ExecSQL(string sqlStr)
        {
            string userid = string.Empty;
            string username = string.Empty;

            NpgsqlCommand cmd = new NpgsqlCommand(sqlStr, con);

            //SQL実行
            dr = cmd.ExecuteReader();

            return 0;
        }

        public object[] ReadOneRow()
        {
            if (dr.Read() == false)
            {
                return null;
            }
            object[] retArray = new object[dr.FieldCount];
            dr.GetValues(retArray);

            return retArray;
        }


    }

}
