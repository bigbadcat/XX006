using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace XuXiang
{
    /// <summary>
    /// 用于对一些类进行方法扩展。
    /// </summary>
	public static class MethodExt
    {
        /// <summary>
        /// 添加或更新字典内容。
        /// </summary>
        /// <typeparam name="T">键类型。</typeparam>
        /// <typeparam name="V">值类型。</typeparam>
        /// <param name="dic">字典对象。</param>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        public static void AddOrUpdate<T, V>(this Dictionary<T, V> dic, T key, V value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        /// <summary>
        /// 移除列表内的空元素。
        /// </summary>
        /// <typeparam name="T">列表元素类型。</typeparam>
        /// <param name="list">列表对象。</param>
        public static void RemoveNull<T>(this List<T> list)
        {
            //从后向前遍历，移除时可以减少复制量
            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 获取或添加组建。
        /// </summary>
        /// <typeparam name="T">组建类型。</typeparam>
        /// <param name="obj">游戏对象。</param>
        /// <returns>组建对象。</returns>
		public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T t = obj.GetComponent<T>();
            if (t == null)
            {
                t = obj.AddComponent<T>();
            }
            return t;
        }

        /// <summary>
        /// 清除子节点。
        /// </summary>
        /// <param name="t">父节点。</param>
        /// <param name="immediate">是否立即清除。</param>
        public static void ClearChild(this Transform t, bool immediate = false)
        {
            if (immediate)
            {
                while (t.childCount > 0)
                {
                    UnityEngine.Object.DestroyImmediate(t.GetChild(0).gameObject);
                }
            }
            else
            {
                foreach (Transform child in t)
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 添加子节点。
        /// </summary>
        /// <param name="p">父节点。</param>
        /// <param name="t">子节点。(添加后会被Reset)</param>
        public static void AddChild(this Transform p, Transform t)
        {
            t.SetParent(p);
            t.Reset();
        }

        /// <summary>
        /// 重置变换。
        /// </summary>
        /// <param name="t">要重置的节点。</param>
        public static void Reset(this Transform t)
        {
            RectTransform rt = t as RectTransform;
            if (rt == null)
            {
                t.localPosition = Vector3.zero;
            }
            else
            {
                rt.anchoredPosition3D = Vector3.zero;
            }            
            t.localScale = Vector3.one;
            t.localRotation = Quaternion.Euler(Vector3.zero);
        }

        /// <summary>
        /// 缓存路径名称。
        /// </summary>
        private static List<string> CachePath = new List<string>(8);

        /// <summary>
        /// 获取路径缓存。
        /// </summary>
        private static StringBuilder CachePathBuilder = new StringBuilder(64);

        /// <summary>
        /// 获取对象路径。
        /// </summary>
        /// <param name="t">要获取路径的节点。</param>
        public static string GetPath(this Transform t)
        {
            CachePath.Clear();
            CachePathBuilder.Length = 0;

            //节点列表
            CachePath.Add(t.name);
            Transform pt = t.parent;
            while (pt != null)
            {
                CachePath.Add(pt.name);
                pt = pt.parent;
            }

            //生成路径
            for (int i=CachePath.Count - 1; i >= 0;--i)
            {
                CachePathBuilder.Append(CachePath[i]);
                if (i > 0)
                {
                    CachePathBuilder.Append(".");
                }
            }

            return CachePathBuilder.ToString();
        }

        /// <summary>
        /// 设置控件位置(将锚点也考虑进去)。
        /// </summary>
        /// <param name="rt">控件。</param>
        /// <param name="pos">相对父节点位置。</param>
        public static void SetUIPosition(this RectTransform rt, Vector3 pos)
        {
            //只有对齐位置为一个点时，设置坐标才有意义
            Vector2 amin = rt.anchorMin;
            Vector2 amax = rt.anchorMax;
            if (amin.x.Equals(amax.x) && amin.y.Equals(amax.y))
            {
                //父节点的锚点偏移
                RectTransform prt = rt.parent as RectTransform;
                Vector2 sz = prt.sizeDelta;
                Vector2 pivot = prt.pivot;
                float ox = sz.x * (0.5f - pivot.x);
                float oy = sz.y * (0.5f - pivot.y);

                //对齐位置偏移
                ox += sz.x * (amin.x - 0.5f);
                oy += sz.y * (amin.y - 0.5f);
                rt.localPosition = new Vector3(pos.x + ox, pos.y + oy, pos.z);
            }
            else
            {
                rt.localPosition = pos;
            }
        }

        /// <summary>
        /// 拆分字符串为整数数组。
        /// </summary>
        /// <param name="str">要拆分的字符串。</param>
        /// <param name="pattern">分割字符串。</param>
        /// <returns>整数数组。</returns>
        public static List<int> SpiltToInt(this string str, string pattern)
        {
            List<int> ret = new List<int>();
            string[] valuestrs = str.Split(pattern.ToCharArray());
            foreach (string valuestr in valuestrs)
            {
                string s = valuestr.Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    int value;
                    if (int.TryParse(valuestr, out value))
                    {
                        ret.Add(value);
                    }
                    else
                    {
                        Log.Warning("The string({0}) is not a number in text:\n{1}", valuestr, str);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 拆分字符串为整数数组。
        /// </summary>
        /// <param name="str">要拆分的字符串。</param>
        /// <param name="pattern1">一级分割字符串。</param>
        /// <param name="pattern2">二级分割字符串。</param>
        /// <returns>整数数组。</returns>
        public static List<List<int>> SpiltToInt(this string str, string pattern1, string pattern2)
        {
            List<List<int>> ret = new List<List<int>>();
            string[] valuestrs = str.Split(pattern1.ToCharArray());
            foreach (string valuestr in valuestrs)
            {
                string s = valuestr.Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    ret.Add(s.SpiltToInt(pattern2));
                }
            }
            return ret;
        }

        /// <summary>
        /// 拆分字符串为整数键值对。
        /// </summary>
        /// <param name="str">要拆分的字符串。</param>
        /// <param name="pattern1">一级分割字符串。</param>
        /// <param name="pattern2">二级分割字符串。</param>
        /// <returns>整数键值对。</returns>
        public static Dictionary<int, int> SpiltToIntKeyValue(this string str, string pattern1, string pattern2)
        {
            Dictionary<int, int> ret = new Dictionary<int, int>();
            List<List<int>> keyvalue = str.SpiltToInt(pattern1, pattern2);
            foreach (List<int> kv in keyvalue)
            {
                ret.Add(kv[0], kv[1]);
            }
            return ret;
        }

        /// <summary>
        /// 初始化类。
        /// </summary>
        static MethodExt()
        {
            HexToDec.Add('0', 0);
            HexToDec.Add('1', 1);
            HexToDec.Add('2', 2);
            HexToDec.Add('3', 3);
            HexToDec.Add('4', 4);
            HexToDec.Add('5', 5);
            HexToDec.Add('6', 6);
            HexToDec.Add('7', 7);
            HexToDec.Add('8', 8);
            HexToDec.Add('9', 9);
            HexToDec.Add('a', 10);
            HexToDec.Add('b', 11);
            HexToDec.Add('c', 12);
            HexToDec.Add('d', 13);
            HexToDec.Add('e', 14);
            HexToDec.Add('f', 15);
            HexToDec.Add('A', 10);
            HexToDec.Add('B', 11);
            HexToDec.Add('C', 12);
            HexToDec.Add('D', 13);
            HexToDec.Add('E', 14);
            HexToDec.Add('F', 15);
        }

        /// <summary>
        /// 十六进制字符到数字的映射。
        /// </summary>
        private static Dictionary<char, int> HexToDec = new Dictionary<char, int>() ;

        /// <summary>
        /// 数字到十六进制字符的映射。
        /// </summary>
        private static char[] DecToHex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// 将字符串转换为颜色值。
        /// </summary>
        /// <param name="str">转换的十六进制字符串，只能是6RGB位或者8位ARGB。</param>
        /// <param name="def">默认颜色值。</param>
        /// <returns>颜色值。</returns>
        public static Color ToColor(this string str, Color def)
        {
            int[] values = new int[8];
            if (str.Length != 6 && str.Length != 8)
            {
                return def;
            }

            //默认A通道是FF
            values[6] = 15;
            values[7] = 15;
            for (int i = 0; i < str.Length; ++i)
            {
                int v;
                if (!HexToDec.TryGetValue(str[i], out v))
                {
                    return def;
                }
                values[i] = v;
            }

            //生成颜色
            int r = values[0] * 16 + values[1];
            int g = values[2] * 16 + values[3];
            int b = values[4] * 16 + values[5];
            int a = values[6] * 16 + values[7];
            Color c = new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
            return c;
        }

        /// <summary>
        /// 将颜色值转换为RGBA格式的字符串。
        /// </summary>
        /// <param name="c">颜色值。</param>
        /// <returns>大写RGBA格式字符串。</returns>
        public static string ToHexString(this Color c)
        {
            int r = (int)(c.r * 255);
            int g = (int)(c.g * 255);
            int b = (int)(c.b * 255);
            int a = (int)(c.a * 255);
            char[] chars = new char[8];
            chars[0] = DecToHex[r/16];
            chars[1] = DecToHex[r%16];
            chars[2] = DecToHex[g/16];
            chars[3] = DecToHex[g%16];
            chars[4] = DecToHex[b/16];
            chars[5] = DecToHex[b%16];
            chars[6] = DecToHex[a/16];
            chars[7] = DecToHex[a%16];
            string str = new string(chars);
            return str;
        }

        /// <summary>
        /// 将2D坐标转换到3D，y赋值给Z。
        /// </summary>
        /// <param name="v2">2D坐标。</param>
        /// <param name="y">3D坐标y分量值。</param>
        /// <returns>3D坐标。</returns>
        public static Vector3 ToVector3(this Vector2 v2, float y = 0)
        {
            return new Vector3(v2.x, y, v2.y);
        }

        /// <summary>
        /// 将3D坐标转换到2D，z赋值给y。
        /// </summary>
        /// <param name="v3">3D坐标。</param>
        /// <returns>2D坐标。</returns>
        public static Vector2 ToVector2(this Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }

        /// <summary>
        /// 将数组转换成内存文本B:0-10K K:10K-10M M:10M+。
        /// </summary>
        /// <param name="n">字节数量。</param>
        /// <returns>带单位的内存文本。</returns>
        public static string ToMemroyText(this long n)
        {
            if (n < 1024 * 10)
            {
                return string.Format("{0}B", n);
            }

            long k = n / 1024;
            if (k < 1024 * 10)
            {
                return string.Format("{0}KB", k);
            }

            long m = k / 1024;
            return string.Format("{0}MB", m);
        }

        /// <summary>
        /// 将Vector3转化为Vector4.
        /// </summary>
        /// <param name="v">Vector3值。</param>
        /// <param name="w">w分量值，默认为0.</param>
        /// <returns>Vector4值。</returns>
        public static Vector4 ToVector4(this Vector3 v, float w = 0)
        {
            return new Vector4(v.x, v.y, v.z, w);
        }
    }
}

