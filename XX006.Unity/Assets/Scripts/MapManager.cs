using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// 地图管理器。
    /// </summary>
    public class MapManager : Singleton<MapManager>
    {
        /// <summary>
        /// 添加地图区块。
        /// </summary>
        /// <param name="chunk"></param>
        public void AddChunk(MapChunk chunk)
        {
#if UNITY_EDITOR
            if (m_MapChunks.ContainsKey(chunk.ID))
            {
                Log.Error("The map chunk with ID {0} already exists", chunk.ID);
                return;
            }
#endif
            m_MapChunks.Add(chunk.ID, chunk);
        }

        /// <summary>
        /// 移除并释放区块。
        /// </summary>
        /// <param name="id">区块id。</param>
        public void RemoveChunk(int id)
        {
            MapChunk chunk;
            if (m_MapChunks.TryGetValue(id, out chunk))
            {
                m_MapChunks.Remove(id);
                chunk.Release();
                chunk = null;
            }
        }

        /// <summary>
        /// 当前区块集合。
        /// </summary>
        private Dictionary<int, MapChunk> m_MapChunks = new Dictionary<int, MapChunk>();
    }
}