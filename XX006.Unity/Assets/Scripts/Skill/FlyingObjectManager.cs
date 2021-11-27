using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006.Fight
{
    public class FlyingObjectManager : Singleton<FlyingObjectManager>
    {
        public void AddFlyingObject(FlyingObject obj)
        {
            m_FlyingObjects.Add(obj);
        }

        /// <summary>
        /// 初始化。
        /// </summary>
        protected override void Init() { }

        private void Update()
        {
            if (m_FlyingObjects.Count <= 0)
            {
                return;
            }

            float dt = Time.deltaTime;
            s_CacheIndex.Clear();
            for (int i = 0; i < m_FlyingObjects.Count; ++i)
            {
                FlyingObject obj = m_FlyingObjects[i];
                obj.Update(dt);
                if (obj.IsEnd)
                {
                    s_CacheIndex.Add(i);
                }
            }

            //移除掉已结束的行为
            for (int i = s_CacheIndex.Count - 1; i >= 0; --i)
            {
                m_FlyingObjects.RemoveAt(s_CacheIndex[i]);
            }
        }

        /// <summary>
        /// 释放。
        /// </summary>
        protected override void Release() { }

        private static List<int> s_CacheIndex = new List<int>();

        private List<FlyingObject> m_FlyingObjects = new List<FlyingObject>();
    }
}