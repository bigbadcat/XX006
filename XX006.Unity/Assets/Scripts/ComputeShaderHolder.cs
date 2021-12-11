using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// ����Shader�����ߡ�
    /// </summary>
    public class ComputeShaderHolder : MonoBehaviour
    {
        /// <summary>
        /// Shader���ݡ�
        /// </summary>
        [System.Serializable]
        public class ComputeShaderEntry
        {
            public string name;
            public ComputeShader shader;
        }

        /// <summary>
        /// ��ȡ����Shader��
        /// </summary>
        /// <param name="name">Shader���ơ�</param>
        /// <returns>����Shader��</returns>
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
        /// ����Shader�����ߡ�
        /// </summary>
        /// <returns>Shader�����ߡ�</returns>
        private static ComputeShaderHolder LoadHolder()
        {
#if UNITY_EDITOR
            //�༭���¹̶�����Asset��prefab��
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
        /// ����Shader��
        /// </summary>
        private static Dictionary<string, ComputeShader> s_Shaders = null;

        /// <summary>
        /// Shader�����ߡ�
        /// </summary>
        private static GameObject s_Holder = null;

        /// <summary>
        /// Shader�б�
        /// </summary>
        [SerializeField]
        private ComputeShaderEntry[] m_Entrys = null;
    }
}