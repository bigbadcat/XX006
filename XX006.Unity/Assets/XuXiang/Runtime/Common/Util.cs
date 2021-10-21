using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace XuXiang
{
    /// <summary>
    /// 提供通用简单操作。
    /// </summary>
    public class Util
    {
        /// <summary>
        /// 一个角度近另一个角度，走最近路径。
        /// </summary>
        /// <param name="from">起始角度。[0-360)</param>
        /// <param name="to">逼近目标。[0-360)</param>
        /// <param name="d">逼近大小。[0-360)</param>
        /// <returns>逼近后的角度。[0-360)</returns>
        public static float ApproachAngle(float from, float to, float d)
        {
            float ret = 0;
            if (Mathf.Abs(from - to) <= 180)
            {
                ret = from > to ? (Mathf.Max(from - d, to)) : (Mathf.Min(from + d, to));
            }
            else
            {
                if (from > to)
                {
                    ret = from + d;
                    while (ret >= 360) { ret -= 360; }
                }
                else
                {
                    ret = from - d;
                    while (ret < 0) { ret += 360; }
                }
            }    
            return ret;
        }

        /// <summary>
        /// 获取两个角度的最小夹角。
        /// </summary>
        /// <param name="a1">角度1。[0-360)</param>
        /// <param name="a2">角度2。[0-360)</param>
        /// <returns>最小夹角。[0-360)</returns>
        public static float GetIncludedAngle(float a1, float a2)
        {
            float maxa = Mathf.Max(a1, a2);
            float mina = Mathf.Min(a1, a2);
            float ret = 0;
            if (maxa - mina <= 180)
            {
                ret = maxa - mina;
            }
            else
            {
                ret = mina - maxa + 360;        //加360为了保证[0-360)
            }
            return ret;
        }

        /// <summary>
        /// 创建新项委托。
        /// </summary>
        /// <typeparam name="T">组建类型。</typeparam>
        /// <param name="item">新创建的项。</param>
        /// <param name="index">项列表索引。(从0开始)</param>
        public delegate void OnNewItem<T>(T item, int index);

        /// <summary>
        /// 重新设置列表的子项。
        /// </summary>
        /// <typeparam name="T">组建类型。</typeparam>
        /// <param name="n">要设置的数量。</param>
        /// <param name="parent">列表根节点。</param>
        /// <param name="prototype">子项原型。</param>
        /// <param name="list">子项列表。</param>
        /// <param name="hide">多出来的是隐藏还是删除。</param>
        /// <param name="onnew">创建新的项时回调。</param>
        public static void ResetItemNumber<T>(int n, Transform parent, GameObject prototype, List<T> list, bool hide = false, OnNewItem<T> onnew = null) where T : MonoBehaviour
        {
            //不够的不足
            while (list.Count < n)
            {
                int index = list.Count;
                GameObject obj = UnityEngine.Object.Instantiate(prototype) as GameObject;
                T item = obj.GetComponent<T>();
                item.name = string.Format("Item{0}", index + 1);
                item.transform.SetParent(parent, false);
                list.Add(item);
                if (onnew != null)
                {
                    onnew(item, index);
                }
            }

            //超出的删除或隐藏
            if (hide)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    list[i].gameObject.SetActive(i < n);
                }
            }
            else
            {
                while (list.Count > n)
                {
                    int i = list.Count - 1;
                    T item = list[i];
                    list.RemoveAt(i);
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
        }

        /// <summary>
        /// 时间戳用的起始时间。
        /// </summary>
        private static DateTime StartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// 获取日期时间。
        /// </summary>
        /// <param name="sec">秒级时间戳。</param>
        /// <returns></returns>
        public static string GetDateTime(long stamp)
        {
            DateTime dt = StartTime.AddSeconds(stamp);
            return string.Format("{0}.{1:D2}.{2:D2} {3:D2}:{4:D2}:{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        }

        /// <summary>
        /// 获取时间字符串。
        /// </summary>
        /// <param name="sec">时间秒。</param>
        /// <param name="showhour">是否包含小时。</param>
        /// <returns>时间字符串。</returns>
        public static string GetTimeString(int sec, bool showhour = false)
        {
            sec = Math.Max(sec, 0);
            int h = sec / 3600;
            int m = (showhour ? (sec % 3600) : sec) / 60;       //如果显示小时就要进行3600取余
            int s = sec % 60;
            string str = showhour ? string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s) : string.Format("{0:D2}:{1:D2}", m, s);
            return str;
        }

        /// <summary>
        /// 获取当前毫秒级时间戳。
        /// </summary>
        /// <returns>毫秒级时间戳。</returns>
        public static long GetNowTimeStamp()
        {
            long ticks = DateTime.Now.Ticks - StartTime.Ticks;
            return ticks / 10000;
        }
    }
}