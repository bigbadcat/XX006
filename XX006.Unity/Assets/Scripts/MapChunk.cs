using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// 地图块。
    /// </summary>
    public class MapChunk
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id"></param>
        public MapChunk(int id)
        {
            m_ID = id;
        }

        /// <summary>
        /// 添加对象。
        /// </summary>
        /// <param name="res">资源路径。</param>
        /// <param name="positon">对象位置。</param>
        /// <param name="scale">对象缩放。</param>
        /// <param name="rotate">对象旋转。</param>
        /// <returns>对象标识。</returns>
        public GameObject AddObject(string res, Vector3 positon, Vector3 scale, Vector3 rotate)
        {
            GameObject obj = ResourceManager.Instance.LoadObject(res);
            obj.transform.localPosition = positon;
            obj.transform.localScale = scale;
            obj.transform.localEulerAngles = rotate;
            return obj;
        }

        /// <summary>
        /// 释放地图块。
        /// </summary>
        public void Release()
        {
            foreach (var kvp in m_Objects)
            {
                GameObject.Destroy(kvp.Value);
            }
            m_Objects.Clear();
        }

        /// <summary>
        /// 获取区块ID。
        /// </summary>
        public int ID
        {
            get { return m_ID; }
        }

        /// <summary>
        /// 地图区块编号。
        /// </summary>
        private int m_ID = 0;

        /// <summary>
        /// 地图对象集合。
        /// </summary>
        private Dictionary<int, GameObject> m_Objects = new Dictionary<int, GameObject>();
    }
}