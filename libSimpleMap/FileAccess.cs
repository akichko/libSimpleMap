﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace libSimpleMap
{
    //public class FileAccessLib : IBinDataAccess
    //{
    //    string mapPath;
    //    //byte[] commonBuf;

    //    public FileAccessLib()
    //    {
    //        //byte[] commonBuf = new byte[1024 * 1024];

    //    }


    //    public override int Connect(string mapPath)
    //    {
    //        this.mapPath = mapPath;


    //        if (Directory.Exists(mapPath))
    //        {
    //            this.mapPath = mapPath + "\\";
    //            return 0;
    //        }
    //        else
    //        {
    //            Console.WriteLine("Map not exists!");
    //            return -1;
    //        }


    //    }

    //    public override int Disconnect()
    //    {
    //        return 0;
    //    }


    //    public override List<uint> GetMapTileIdList()
    //    {

    //        List<uint> retList = new List<uint>();
    //        string[] names = Directory.GetFiles(mapPath + "NODE", "*.txt");
    //        foreach (string name in names)
    //        {
    //            string tileName = name.Replace(mapPath + "NODE\\", "").Replace("_NODE.txt", "");
    //            retList.Add(uint.Parse(tileName));

    //        }
    //        return retList;
    //    }

    //    public override byte[] GetRawData(uint tileId, SpMapContentType contentType)
    //    {
    //        return null;
    //    }


    //    public override byte[] GetLinkData(uint tileId)
    //    {
    //        byte[] retBytes;

    //        uint subTileY = tileId / 10000000;
    //        uint subTileX = (tileId % 100000) / 100;
    //        string subTile = (subTileY * 1000 + subTileX).ToString();
    //        //TileXY baseTileXY = new TileXY(tileId);

    //        string fileName = mapPath + @"LINK\" + subTile + @"\" + tileId.ToString() + "_LINK.txt";
    //        if (!File.Exists(fileName))
    //            return null;

    //        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
    //        {
    //            retBytes = new byte[fs.Length];
    //            fs.Read(retBytes, 0, (int)fs.Length);
    //        }

    //        return retBytes;

    //    }

    //    public override byte[] GetNodeData(uint tileId)
    //    {
    //        byte[] retBytes;

    //        uint subTileY = tileId / 10000000;
    //        uint subTileX = (tileId % 100000) / 100;
    //        string subTile = (subTileY * 1000 + subTileX).ToString();
    //        //TileXY baseTileXY = new TileXY(tileId);

    //        string fileName = mapPath + @"NODE\" + subTile + @"\" + tileId.ToString() + "_NODE.txt";
    //        if (!File.Exists(fileName))
    //            return null;

    //        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
    //        {
    //            retBytes = new byte[fs.Length];
    //            fs.Read(retBytes, 0, (int)fs.Length);
    //        }

    //        return retBytes;
    //    }

    //    public override byte[] GetGeometryData(uint tileId)
    //    {

    //        byte[] retBytes;

    //        uint subTileY = tileId / 10000000;
    //        uint subTileX = (tileId % 100000) / 100;
    //        string subTile = (subTileY * 1000 + subTileX).ToString();
    //       // TileXY baseTileXY = new TileXY(tileId);

    //        string fileName = mapPath + @"LINKGEOMETRY\" + subTile + @"\" + tileId.ToString() + "_LINKGEOMETRY.txt";
    //        if (!File.Exists(fileName))
    //            return null;

    //        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
    //        {
    //            retBytes = new byte[fs.Length];
    //            fs.Read(retBytes, 0, (int)fs.Length);
    //        }


    //        return retBytes;
    //    }

    //    public override byte[] GetAttributeData(uint tileId)
    //    {
    //        return null;
    //    }

    //    public override int SaveLinkData(uint tileId, byte[] tileBuf, int size)
    //    {
    //        uint subTileY = tileId / 10000000;
    //        uint subTileX = (tileId % 100000) / 100;
    //        string subTile = (subTileY * 1000 + subTileX).ToString();

    //        //LINK
    //        if (!Directory.Exists(mapPath + @"LINK"))
    //        {
    //            Directory.CreateDirectory(mapPath + @"LINK");
    //        }
    //        if (!Directory.Exists(mapPath + @"LINK\" + subTile))
    //        {
    //            Directory.CreateDirectory(mapPath + @"LINK\" + subTile);
    //        }

    //        using (FileStream fs = new FileStream(mapPath + @"LINK\" + subTile + @"\" + tileId.ToString() + "_LINK.txt", FileMode.OpenOrCreate, FileAccess.Write))
    //        {
    //            fs.Write(tileBuf, 0, size);

    //        }

    //        return 0;
    //    }

    //    public override int SaveNodeData(uint tileId, byte[] tileBuf, int size)
    //    {
    //        uint subTileY = tileId / 10000000;
    //        uint subTileX = (tileId % 100000) / 100;
    //        string subTile = (subTileY * 1000 + subTileX).ToString();


    //        if (!Directory.Exists(mapPath + @"NODE"))
    //        {
    //            Directory.CreateDirectory(mapPath + @"NODE");
    //        }
    //        if (!Directory.Exists(mapPath + @"NODE\" + subTile))
    //        {
    //            Directory.CreateDirectory(mapPath + @"NODE\" + subTile);
    //        }

    //        using (FileStream fs = new FileStream(mapPath + @"NODE\" + subTile + @"\" + tileId.ToString() + "_NODE.txt", FileMode.OpenOrCreate, FileAccess.Write))
    //        {
    //            fs.Write(tileBuf, 0, size);
    //        }

    //        return 0;
    //    }

    //    public override int SaveGeometryData(uint tileId, byte[] tileBuf, int size)
    //    {
    //        uint subTileY = tileId / 10000000;
    //        uint subTileX = (tileId % 100000) / 100;
    //        string subTile = (subTileY * 1000 + subTileX).ToString();


    //        if (!Directory.Exists(mapPath + @"LINKGEOMETRY"))
    //        {
    //            Directory.CreateDirectory(mapPath + @"LINKGEOMETRY");
    //        }
    //        if (!Directory.Exists(mapPath + @"LINKGEOMETRY\" + subTile))
    //        {
    //            Directory.CreateDirectory(mapPath + @"LINKGEOMETRY\" + subTile);
    //        }

    //        using (FileStream fs = new FileStream(mapPath + @"LINKGEOMETRY\" + subTile + @"\" + tileId.ToString() + "_LINKGEOMETRY.txt", FileMode.OpenOrCreate, FileAccess.Write))
    //        {
    //            fs.Write(tileBuf, 0, size);
    //        }

    //        return 0;
    //    }


    //    public override int SaveAllData(uint tileId, byte[] linkBuf, byte[] nodeBuf, byte[] geometryBuf, byte[] attributeBuf)
    //    {
    //        return -1;
    //    }

    //}

}
