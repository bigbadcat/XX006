using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;
using Avatar = XuXiang.Avatar;

namespace XX006
{
    /// <summary>
    /// 大世界逻辑。
    /// </summary>
    public class BigWord : MonoBehaviour
    {
        /// <summary>
        /// 玩家出生点。
        /// </summary>
        public Transform PlayerSpawnPoint;

        /// <summary>
        /// NPC出生点。
        /// </summary>
        public Transform[] NPCSpawnPoints;

        /// <summary>
        /// 开始逻辑。
        /// </summary>
        private void Start()
        {
            InitUI();
            InitMap();
            SpawnPlayer();
            SpawnNPC();
        }

        /// <summary>
        /// 初始化UI。
        /// </summary>
        private void InitUI()
        {
            AppRoot.UIRoot.Open(1, "UI/UICharacterControl");
            //m_UI = ResourceManager.Instance.LoadObject("UI/UICharacterControl");
            //m_UI.gameObject.SetActive(true);

            //Canvas ui = m_UI.GetComponent<Canvas>();
            //Camera uicamera = GameObject.Find("UICamera").GetComponent<Camera>();
            //ui.renderMode = RenderMode.ScreenSpaceCamera;
            //ui.worldCamera = uicamera;
            //ui.planeDistance = 1.1f;            
        }

        /// <summary>
        /// 初始化地图。
        /// </summary>
        private void InitMap()
        {
            MapChunk chunk = new MapChunk(1);
            chunk.AddObject("Environment/PB_Rock_01", new Vector3(57, 39, 50), new Vector3(1, 1, 1), new Vector3(0, 90, 0));
            chunk.AddObject("Environment/PB_Rock_01", new Vector3(47, 39, 43), new Vector3(1, 1, 1), new Vector3(0, 0, 0));
            chunk.AddObject("Environment/PB_Rock_01", new Vector3(51, 39, 33), new Vector3(1, 1, 1), new Vector3(0, 90, 0));
            chunk.AddObject("Environment/PB_Rock_01", new Vector3(65, 39, 30), new Vector3(1, 1, 1), new Vector3(0, 90, 0));
            chunk.AddObject("Environment/PB_Rock_01", new Vector3(65, 39, 52), new Vector3(1, 1, 1), new Vector3(0, 90, 0));
            chunk.AddObject("Environment/PB_Rock_01", new Vector3(60, 48, 74), new Vector3(1, 1, 1), new Vector3(0, 0, -36));
            chunk.AddObject("Environment/TR_Test", new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(0, 0, 0));
            chunk.AddObject("Environment/PB_Grass", new Vector3(57, 40, 42), new Vector3(1, 1, 1), new Vector3(0, 120, 0));
            MapManager.Instance.AddChunk(chunk);
        }

        /// <summary>
        /// 生成玩家。
        /// </summary>
        private void SpawnPlayer()
        {
            GameObject obj = new GameObject("Player");
            Player player = obj.AddComponent<Player>();
            player.m_MoveSpeed = 3.2f;
            obj.AddComponent<BendArea>();

            Avatar avater = ResourceManager.Instance.LoadObject<Avatar>("Character/Qigongdou/PB_Qigongdou");
            avater.transform.SetParent(obj.transform);
            avater.transform.Reset();
            player.transform.Reset();
            player.transform.localPosition = PlayerSpawnPoint.localPosition;
            player.transform.localRotation = PlayerSpawnPoint.localRotation;
        }

        /// <summary>
        /// 生成NPC。
        /// </summary>
        private void SpawnNPC()
        {
            if (NPCSpawnPoints == null)
            {
                return;
            }

            for (int i=0;i< NPCSpawnPoints.Length; ++i)
            {
                Transform trs = NPCSpawnPoints[i];
                if(trs == null)
                {
                    continue;
                }

                GameObject obj = new GameObject(string.Format("NPC_{0}", i));
                obj.AddComponent<BendArea>();

                Avatar avater = ResourceManager.Instance.LoadObject<Avatar>("Character/Sword/PB_Sword");
                avater.transform.SetParent(obj.transform);
                avater.transform.Reset();
                obj.transform.Reset();
                obj.transform.localPosition = trs.localPosition;
                obj.transform.localRotation = trs.localRotation;
                obj.AddComponent<NPC>();
            }
        }
    }
}