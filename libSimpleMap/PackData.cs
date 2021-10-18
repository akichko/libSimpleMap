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

namespace libSimpleMap
{
    class PackData64
    {
        public UInt64 rawData;

        public PackData64() { }
        public PackData64(byte[] serialData)
        {
            rawData = BitConverter.ToUInt64(serialData, 0);
        }


        public UInt64 GetUInt(int bitStart, int bitLength)
        {
            return GetUInt(rawData, bitStart, bitLength);
        }

        public Int64 GetInt(int bitStart, int bitLength)
        {
            return GetInt(rawData, bitStart, bitLength);
        }

        public void SetUInt(int bitStart, int bitLength, UInt64 setData)
        {
            rawData = SetUInt(rawData, bitStart, bitLength, setData);
        }

        public void SetInt(int bitStart, int bitLength, Int64 setData)
        {
            rawData = SetInt(rawData, bitStart,bitLength,setData);
        }



        public static UInt64 CalcBitMask(int bitStart, int bitLength)
        {
            UInt64 mainBit = 0;
            for (int i = 0; i < bitLength; i++)
            {
                mainBit = (mainBit << 1) | 1;
            }
            mainBit <<= bitStart;

            return mainBit;
        }


        public static UInt64 GetUInt(UInt64 packData, int bitStart, int bitLength)
        {
            UInt64 bitMask = CalcBitMask(bitStart, bitLength);

            UInt64 uintData = (packData & bitMask) >> bitStart;

            return uintData;
        }

        public static Int64 GetInt(UInt64 packData, int bitStart, int bitLength)
        {
            UInt64 numberData = GetUInt(packData, bitStart, bitLength-1);
            byte signBit = (byte)GetUInt(packData, bitStart + bitLength - 1, 1);

            //UInt64 uintData = GetUInt(packData, bitStart, bitLength);

            //byte signBit = (byte)(uintData >> (bitLength - 1));

            if (signBit == 1)
                return -(Int64)numberData;
            else
                return (Int64)numberData;
        }

        public static UInt64 SetUInt(UInt64 packData, int bitStart, int bitLength, UInt64 setData)
        {
            if(setData >= Math.Pow(2, bitLength))
            {
                throw new OverflowException();
            }

            UInt64 bitMask = CalcBitMask(bitStart, bitLength);

            UInt64 uintData = (setData << bitStart ) & bitMask;

            packData &= ~bitMask;
            packData |= uintData;

            return packData;
        }

        public static UInt64 SetInt(UInt64 packData, int bitStart, int bitLength, Int64 setData)
        {
            if (Math.Abs(setData) >= Math.Pow(2, bitLength - 1))
            {
                throw new OverflowException();
            }

            UInt64 bitMask = CalcBitMask(bitStart, bitLength);

            UInt64 signBit = 0;
            UInt64 uintData;
            if (setData < 0)
            {
                signBit = (UInt64)1 << bitStart << (bitLength-1);
                uintData = (UInt64)(-setData);
            }
            else
            {
                uintData = (UInt64)setData;
            }


             uintData = ((uintData << bitStart) & bitMask) | signBit;

            packData &= ~bitMask;
            packData |= uintData;

            return packData;
        }


    }


    class PackData32
    {
        public UInt32 rawData;

        public PackData32() { }
        public PackData32(byte[] serialData)
        {
            rawData = BitConverter.ToUInt32(serialData, 0);
        }


        public UInt32 GetUInt(int bitStart, int bitLength)
        {
            return GetUInt(rawData, bitStart, bitLength);
        }

        public Int32 GetInt(int bitStart, int bitLength)
        {
            return GetInt(rawData, bitStart, bitLength);
        }

        public void SetUInt(int bitStart, int bitLength, UInt32 setData)
        {
            rawData = SetUInt(rawData, bitStart, bitLength, setData);
        }

        public void SetInt(int bitStart, int bitLength, Int32 setData)
        {
            rawData = SetInt(rawData, bitStart, bitLength, setData);
        }



        public static UInt32 CalcBitMask(int bitStart, int bitLength)
        {
            UInt32 mainBit = 0;
            for (int i = 0; i < bitLength; i++)
            {
                mainBit = (mainBit << 1) | 1;
            }
            mainBit <<= bitStart;

            return mainBit;
        }


        public static UInt32 GetUInt(UInt32 packData, int bitStart, int bitLength)
        {
            UInt32 bitMask = CalcBitMask(bitStart, bitLength);

            UInt32 uintData = (packData & bitMask) >> bitStart;

            return uintData;
        }

        public static Int32 GetInt(UInt32 packData, int bitStart, int bitLength)
        {
            UInt32 numberData = GetUInt(packData, bitStart, bitLength - 1);
            byte signBit = (byte)GetUInt(packData, bitStart + bitLength - 1, 1);

            if (signBit == 1)
                return -(Int32)numberData;
            else
                return (Int32)numberData;
        }

        public static UInt32 SetUInt(UInt32 packData, int bitStart, int bitLength, UInt32 setData)
        {
            if (setData >= Math.Pow(2, bitLength))
            {
                throw new OverflowException();
            }

            UInt32 bitMask = CalcBitMask(bitStart, bitLength);

            UInt32 uintData = (setData << bitStart) & bitMask;

            packData &= ~bitMask;
            packData |= uintData;

            return packData;
        }

        public static UInt32 SetInt(UInt32 packData, int bitStart, int bitLength, Int32 setData)
        {
            if (Math.Abs(setData) >= Math.Pow(2, bitLength - 1))
            {
                throw new OverflowException();
            }

            UInt32 bitMask = CalcBitMask(bitStart, bitLength);

            UInt32 signBit = 0;
            UInt32 uintData;
            if (setData < 0)
            {
                signBit = (UInt32)1 << bitStart << (bitLength - 1);
                uintData = (UInt32)(-setData);
            }
            else
            {
                uintData = (UInt32)setData;
            }


            uintData = ((uintData << bitStart) & bitMask) | signBit;

            packData &= ~bitMask;
            packData |= uintData;

            return packData;
        }


    }

}
