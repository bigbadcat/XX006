using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// 计算Shader持有者。
    /// </summary>
    public class ComputeShaderHolder : MonoBehaviour
    {
        /// <summary>
        /// Shader内容。
        /// </summary>
        [System.Serializable]
        public class ComputeShaderEntry
        {
            public string name;
            public ComputeShader shader;
        }

        /// <summary>
        /// 获取计算Shader。
        /// </summary>
        /// <param name="name">Shader名称。</param>
        /// <returns>计算Shader。</returns>
        public static ComputeShader GetComputeShader(string name)
        {
            if (s_Shaders == null)
            {
                ComputeShaderHolder holder = LoadHolder();
                s_Shaders = new Dictionary<string, ComputeShader>();
                if (holder != null && holder.m_Entrys != null)
                {
                    for (int i=0; i<holder.m_Entrys.Length; ++i)
                    {
                        ComputeShaderEntry entry = holder.m_Entrys[i];
                        if (entry != null && !string.IsNullOrEmpty(entry.name) && entry.shader != null && !s_Shaders.ContainsKey(entry.name))
                        {
                            s_Shaders.Add(entry.name, entry.shader);
                        }
                    }
                }
            }

            ComputeShader shader;
            if(s_Shaders.TryGetValue(name, out shader))
            {
                return shader;
            }
            return null;
        }

        /// <summary>
        /// 加载Shader持有者。
        /// </summary>
        /// <returns>Shader持有者。</returns>
        private static ComputeShaderHolder LoadHolder()
        {
#if UNITY_EDITOR
            //编辑器下固定加载Asset的prefab。
            string file = "Assets/ResourcesEx/Prefab/ComputeShaderHolder.prefab";
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(file);
            s_Holder = GameObject.Instantiate(prefab);
#else
            s_Holder = ResourceManager.Instance.LoadObject("Prefab/ComputeShaderHolder");
#endif

            GameObject.DontDestroyOnLoad(s_Holder);
            s_Holder.transform.Reset();
            return s_Holder.GetComponent<ComputeShaderHolder>();
        }

        /// <summary>
        /// 计算Shader。
        /// </summary>
        private static Dictionary<string, ComputeShader> s_Shaders = null;

        /// <summary>
        /// Shader持有者。
        /// </summary>
        private static GameObject s_Holder = null;

        /// <summary>
        /// Shader列表。
        /// </summary>
        [SerializeField]
        private ComputeShaderEntry[] m_Entrys = null;
    }
}