using System;
using System.Text;

namespace XuXiang
{
    /// <summary>
    /// 数据操作公共函数。
    /// </summary>
    public class DataUtil
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 清空数组。
        /// </summary>
        /// <param name="a">数组。</param>
        public static void ClearArray(int[] a)
        {
            for (int i = 0; i < a.Length; ++i)
            {
                a[i] = 0;
            }
        }

        /// <summary>
        /// 清空数组。
        /// </summary>
        /// <param name="a">数组。</param>
        public static void ClearArray(float[] a)
        {
            for (int i = 0; i < a.Length; ++i)
            {
                a[i] = 0;
            }
        }

        /// <summary>
        /// 将字节数组拷贝到另一个数组中。
        /// </summary>
        /// <param name="src">源数组。</param>
        /// <param name="srcindex">数据源起始索引。</param>
        /// <param name="dst">目标数组。</param>
        /// <param name="dstindex">目标数组起始索引，若src==dst则dstindex必须小于srcindex，否则会数据错误。</param>
        /// <param name="len">拷贝的数据长度。</param>
        public static void CopyTo(byte[] src, int srcindex, byte[] dst, int dstindex, int len)
        {
            for (int i = 0; i < len; ++i)
            {
                dst[dstindex + i] = src[srcindex + i];
            }
        }

        /// <summary>
        /// 写入一个64位无符号整数。
        /// </summary>
        /// <param name="src">要写入的字节数组。</param>
        /// <param name="index">要写入的起始位置。</param>
        /// <param name="value">要写入的值。</param>
        /// <returns>下一个写入索引。</returns>
        public static int WriteUInt64(byte[] src, int index, ulong value)
        {
            src[index + 7] = (byte)((value >> 56) & 0xFF);
            src[index + 6] = (byte)((value >> 48) & 0xFF);
            src[index + 5] = (byte)((value >> 40) & 0xFF);
            src[index + 4] = (byte)((value >> 32) & 0xFF);
            src[index + 3] = (byte)((value >> 24) & 0xFF);
            src[index + 2] = (byte)((value >> 16) & 0xFF);
            src[index + 1] = (byte)((value >> 8) & 0xFF);
            src[index + 0] = (byte)(value & 0xFF);
            return index + 8;
        }

        /// <summary>
        /// 写入一个64位整数。
        /// </summary>
        /// <param name="src">要写入的字节数组。</param>
        /// <param name="index">要写入的起始位置。</param>
        /// <param name="value">要写入的值。</param>
        /// <returns>下一个写入索引。</returns>
        public static int WriteInt64(byte[] src, int index, long value)
        {
            return WriteUInt64(src, index, (ulong)value);
        }

        /// <summary>
        /// 读取一个64位无符号整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <returns>无符号整数值。</returns>
        public static ulong ReadUInt64(byte[] src, int index)
        {
            ulong v = BitConverter.ToUInt64(src, index);
            return v;
        }

        /// <summary>
        /// 读取一个64位整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <returns>整数值。</returns>
        public static long ReadInt64(byte[] src, int index)
        {
            return (long)ReadUInt64(src, index);
        }

        /// <summary>
        /// 读取一个64位无符号整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <param name="pos">下一个读取位置。</param>
        /// <returns>无符号整数值。</returns>
        public static ulong ReadUInt64(byte[] src, int index, ref int pos)
        {
            ulong v = ReadUInt64(src, index);
            pos = index + 8;
            return v;
        }

        /// <summary>
        /// 读取一个64位整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <param name="pos">下一个读取位置。</param>
        /// <returns>整数值。</returns>
        public static long ReadInt64(byte[] src, int index, ref int pos)
        {
            return (long)ReadUInt64(src, index, ref pos);
        }

        /// <summary>
        /// 写入一个无符号整数。
        /// </summary>
        /// <param name="src">要写入的字节数组。</param>
        /// <param name="index">要写入的起始位置。</param>
        /// <param name="value">要写入的值。</param>
        /// <returns>下一个写入索引。</returns>
        public static int WriteUInt32(byte[] src, int index, uint value)
        {
            src[index + 3] = (byte)((value >> 24) & 0xFF);                  //最高位
            src[index + 2] = (byte)((value >> 16) & 0xFF);                  //次高位            
            src[index + 1] = (byte)((value >> 8) & 0xFF);                   //次低位
            src[index + 0] = (byte)(value & 0xFF);                          //最低位
            return index + 4;
        }

        /// <summary>
        /// 写入一个整数。
        /// </summary>
        /// <param name="src">要写入的字节数组。</param>
        /// <param name="index">要写入的起始位置。</param>
        /// <param name="value">要写入的值。</param>
        /// <returns>下一个写入索引。</returns>
        public static int WriteInt32(byte[] src, int index, int value)
        {
            return WriteUInt32(src, index, (uint)value);
        }

        /// <summary>
        /// 读取一个无符号整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <returns>无符号整数值。</returns>
        public static uint ReadUInt32(byte[] src, int index)
        {
            uint v = BitConverter.ToUInt32(src, index);
            return v;
        }

        /// <summary>
        /// 读取一个整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <returns>整数值。</returns>
        public static int ReadInt32(byte[] src, int index)
        {
            return (int)ReadUInt32(src, index);
        }

        /// <summary>
        /// 读取一个无符号整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <param name="pos">下一个读取位置。</param>
        /// <returns>无符号整数值。</returns>
        public static uint ReadUInt32(byte[] src, int index, ref int pos)
        {
            uint v = ReadUInt32(src, index);
            pos = index + 4;
            return v;
        }

        /// <summary>
        /// 读取一个整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <param name="pos">下一个读取位置。</param>
        /// <returns>整数值。</returns>
        public static int ReadInt32(byte[] src, int index, ref int pos)
        {
            return (int)ReadUInt32(src, index, ref pos);
        }

        /// <summary>
        /// 写入一个无符号整数。
        /// </summary>
        /// <param name="src">要写入的字节数组。</param>
        /// <param name="index">要写入的起始位置。</param>
        /// <param name="value">要写入的值。</param>
        /// <returns>下一个写入索引。</returns>
        public static int WriteUInt16(byte[] src, int index, ushort value)
        {
            src[index + 1] = (byte)((value >> 8) & 0xFF);                   //次低位
            src[index + 0] = (byte)(value & 0xFF);                          //最低位
            return index + 2;
        }
        
        /// <summary>
        /// 写入一个16位整数。
        /// </summary>
        /// <param name="src">要写入的字节数组。</param>
        /// <param name="index">要写入的起始位置。</param>
        /// <param name="value">要写入的值。</param>
        /// <returns>下一个写入索引。</returns>
        public static int WriteInt16(byte[] src, int index, short value)
        {
            return WriteUInt16(src, index, (ushort)value);
        }

        /// <summary>
        /// 读取一个16位无符号整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <returns>无符号整数值。</returns>
        public static ushort ReadUInt16(byte[] src, int index)
        {
            ushort v = BitConverter.ToUInt16(src, index);
            return v;
        }

        /// <summary>
        /// 读取一个16位整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <returns>整数值。</returns>
        public static short ReadInt16(byte[] src, int index)
        {
            return (short)ReadUInt16(src, index);
        }

        /// <summary>
        /// 读取一个16位无符号整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <param name="pos">下一个读取位置。</param>
        /// <returns>无符号整数值。</returns>
        public static ushort ReadUInt16(byte[] src, int index, ref int pos)
        {
            ushort v = ReadUInt16(src, index);
            pos = index + 2;
            return v;
        }

        /// <summary>
        /// 读取一个16位整数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <param name="pos">下一个读取位置。</param>
        /// <returns>整数值。</returns>
        public static short ReadInt16(byte[] src, int index, ref int pos)
        {
            return (short)ReadUInt16(src, index, ref pos);
        }
        
        /// <summary>
        /// 写入一个浮点数。
        /// </summary>
        /// <param name="src">要写入的字节数组。</param>
        /// <param name="index">要写入的起始位置。</param>
        /// <param name="value">要写入的值。</param>
        /// <returns>下一个写入索引。</returns>
        public static int WriteFloat(byte[] src, int index, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            bytes.CopyTo(src, index);
            return index + bytes.Length;
        }

        /// <summary>
        /// 读取一个浮点数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <returns>浮点数值。</returns>
        public static float ReadFloat(byte[] src, int index)
        {
            float v = BitConverter.ToSingle(src, index);
            return v;
        }

        /// <summary>
        /// 读取一个浮点数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <param name="pos">下一个读取位置。</param>
        /// <returns>浮点数值。</returns>
        public static float ReadFloat(byte[] src, int index, ref int pos)
        {
            float v = ReadFloat(src, index);
            pos = index + 4;
            return v;
        }

        /// <summary>
        /// 写入一个浮点数。
        /// </summary>
        /// <param name="src">要写入的字节数组。</param>
        /// <param name="index">要写入的起始位置。</param>
        /// <param name="value">要写入的值。</param>
        /// <returns>下一个写入索引。</returns>
        public static int WriteDouble(byte[] src, int index, double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            bytes.CopyTo(src, index);
            return index + bytes.Length;
        }



        /// <summary>
        /// 读取一个双精度浮点数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <returns>双精度浮点数值。</returns>
        public static double ReadDouble(byte[] src, int index)
        {
            double v = BitConverter.ToDouble(src, index);
            return v;
        }

        /// <summary>
        /// 读取一个双精度浮点数。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <param name="pos">下一个读取位置。</param>
        /// <returns>双精度浮点数值。</returns>
        public static double ReadDouble(byte[] src, int index, ref int pos)
        {
            double v = ReadDouble(src, index);
            pos = index + 8;
            return v;
        }

        /// <summary>
        /// 写入一个字符串。
        /// </summary>
        /// <param name="src">要写入的字节数组。</param>
        /// <param name="index">要写入的起始位置。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="n">写入长度。</param>
        /// <returns>新的起始位置。</returns>
        public static int WriteString(byte[] src, int index, string value, int n = 0)
        {
            byte[] temp = Encoding.UTF8.GetBytes(value);
            int pos = index;
            if (n <= 0)
            {
                //变长写入
                int len = (int)temp.Length;
                pos = WriteInt32(src, index, len+1);
                temp.CopyTo(src, pos);
                pos += len;
                src[pos++] = 0;
            }
            else
            {
                //固定长度写入，不足长度补0，超出长度截断
                int num = Math.Min(n, temp.Length);
                int i = 0;
                for (; i < num; ++i)
                {
                    src[pos + i] = temp[i];
                }
                for (; i < n; ++i)
                {
                    src[pos + i] = 0;
                }
                pos += n;
            }
            return pos;
        }

        /// <summary>
        /// 读取字符串。
        /// </summary>
        /// <param name="src">要读取的字节数组。</param>
        /// <param name="index">读取索引。</param>
        /// <param name="pos">下一个读取位置。</param>
        /// <param name="n">读取长度。</param>
        /// <returns>字符串值。</returns>
        public static string ReadString(byte[] src, int index, ref int pos, int n = 0)
        {
            string ret = string.Empty;
            pos = index;
            if (n <= 0)
            {
                //变长字符串                
                int len = ReadInt32(src, index, ref pos);
                //ret = Encoding.UTF8.GetString(src, pos, len);
                ret = GetUTF8String(src, pos, len);
                pos += len;
            }
            else
            {
                //固定长度字符串
                //ret = Encoding.UTF8.GetString(src, index, n);
                ret = GetUTF8String(src, index, n);
                pos += n;
            }

            return ret;
        }

        public static string GetUTF8String(byte[] src, int index, int len)
        {
            //排除掉末尾的0，否则和其它字符串拼接会不正常
            int n = len;
            while (n > 0)
            {
                if (src[index + n - 1] != 0)
                {
                    break;
                }
                --n;
            }
            return Encoding.UTF8.GetString(src, index, n);
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        #endregion
    }
}