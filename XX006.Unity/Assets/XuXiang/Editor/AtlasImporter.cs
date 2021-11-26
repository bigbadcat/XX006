using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace U3dFrameWork.Editor
{
    /// <summary>
    /// 碎图集合导入。
    /// 使用方法:
    ///     1、用TexturePacker整合碎图并导出xml和png文件。
    ///        PS:Output的DataFormat选择Generic XML，勾选Geometry中的Force squared，取消Layout的Allow ratation。
    ///           Data file的后缀名改为xml，Texture file使用png格式，与xml同文件名。
    ///     2、将导出的xml文件与png文件一起添加到Asset中。
    ///     3、在Unity编辑器Asset中选中png文件，择菜单"Tools->Atlas->Import"项进行导入。
    /// </summary>
    public static class AtlasImporter
    {
        /// <summary>
        /// 精灵信息。可以考虑用SpriteMetaData替换
        /// </summary>
        public class SpriteInfo
        {
            /// <summary>
            /// 名称。
            /// </summary>
            public String Name = string.Empty;

            /// <summary>
            /// X坐标。(相对纹理图片左下角)
            /// </summary>
            public int X;

            /// <summary>
            /// Y坐标。(相对纹理图片左下角)
            /// </summary>
            public int Y;

            /// <summary>
            /// 宽度。
            /// </summary>
            public int Width;

            /// <summary>
            /// 高度。
            /// </summary>
            public int Height;

            /// <summary>
            /// 精灵边框。
            /// </summary>
            public Vector4 Border;
        };

        [MenuItem("Tools/Atlas/Import")]
        public static void ImportAtlas()
        {
            //判断选择的资源
            Texture selected = Selection.activeObject as Texture;
            if (selected == null)
            {
                Debug.unityLogger.LogError("AtlasImporter", "Did not select the Texture!");
                return;
            }

            string texpath = AssetDatabase.GetAssetPath(selected);
            ImportAtlasAsset(texpath);
        }

        /// <summary>
        /// 导入图集资源。
        /// </summary>
        /// <param name="path">图集路径，Assets打头。</param>
        public static void ImportAtlasAsset(string path)
        {
            //加载XML
            string name = Path.GetFileNameWithoutExtension(path);
            string rootpath = Path.GetDirectoryName(path);
            string xmlpath = rootpath + "/" + name + ".xml";
            TextAsset xml = AssetDatabase.LoadAssetAtPath<TextAsset>(xmlpath);
            if (xml == null)
            {
                Debug.unityLogger.LogError("AtlasImporter", "Can not find xml file!" + xmlpath);
                return;
            }

            //纹理基础设置
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;

            //拆分图集
            Dictionary<string, int> shrink = LoadShrinkInfo(name);
            List<SpriteInfo> infos = LoadSpriteInfo(xml.text);
            ShrinkSprite(infos, shrink);
            UpdateBorder(textureImporter.spritesheet, infos);
            textureImporter.spritesheet = BuildSpriteMetaData(infos);

            //平台设置
            TextureImporterPlatformSettings android = new TextureImporterPlatformSettings();
            android.name = "Android";
            android.compressionQuality = 50;
            android.maxTextureSize = 1024;
            android.overridden = true;
            android.format = TextureImporterFormat.ETC2_RGBA8;
            textureImporter.SetPlatformTextureSettings(android);
            textureImporter.SaveAndReimport();

            //删除XML 保存 刷新
            AssetDatabase.DeleteAsset(xmlpath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 加载图集精灵信息。
        /// </summary>
        /// <param name="text">图集xml配置内容。</param>
        /// <returns>图集精灵列表。</returns>
        private static List<SpriteInfo> LoadSpriteInfo(string text)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(text);

            List<SpriteInfo> infos = new List<SpriteInfo>();
            XmlNode root = xml.GetElementsByTagName("TextureAtlas")[0];
            int th = int.Parse(root.Attributes["height"].Value);
            foreach (XmlNode sprite in root.ChildNodes)
            {
                //解析 <sprite n="back_text1.png" x="2" y="176" w="60" h="40"/>
                string name = sprite.Attributes["n"].Value;
                int x = int.Parse(sprite.Attributes["x"].Value);
                int y = int.Parse(sprite.Attributes["y"].Value);
                int w = int.Parse(sprite.Attributes["w"].Value);
                int h = int.Parse(sprite.Attributes["h"].Value);

                //生成精灵信息
                SpriteInfo info = new SpriteInfo();
                info.Name = Path.GetFileNameWithoutExtension(name); //名称去掉后缀名
                info.X = x;
                info.Y = th - (y + h);
                info.Width = w;
                info.Height = h;
                infos.Add(info);
            }

            return infos;
        }

        /// <summary>
        /// 收缩精灵区域。
        /// </summary>
        /// <param name="infos">精灵列表。</param>
        /// <param name="shrink">收缩配置。</param>
        private static void ShrinkSprite(List<SpriteInfo> infos, Dictionary<string, int> shrink)
        {
            foreach (var info in infos)
            {
                //收缩调整
                int t;
                if (shrink.TryGetValue(info.Name, out t))
                {
                    if (t == 0 || t == 1)
                    {
                        info.X = info.X + 1;
                        info.Width = info.Width - 2;
                    }
                    if (t == 0 || t == 2)
                    {
                        info.Y = info.Y + 1;
                        info.Height = info.Height - 2;
                    }
                }
            }
        }

        /// <summary>
        /// 加载收缩信息。
        /// </summary>
        /// <param name="name">图集名称。</param>
        /// <returns>收缩信息。</returns>
        private static Dictionary<string, int> LoadShrinkInfo(string name)
        {
            Dictionary<string, int> ret = new Dictionary<string, int>();
            string path = Path.Combine(Application.dataPath, "../AtlasImages/shrink.xml");
            if (!File.Exists(path))
            {
                return ret;
            }
            StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read));
            string text = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
            reader = null;

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(text);
            XmlNode root = xml.GetElementsByTagName("Atlases")[0];
            XmlNodeList atlasnodes = root.ChildNodes;
            XmlNodeList types = null;
            for (int i = 0; i < atlasnodes.Count; ++i)
            {
                XmlNode node = atlasnodes[i];
                if (node.Attributes["name"].InnerText.CompareTo(name) == 0)
                {
                    types = node.ChildNodes;
                    break;
                }
            }
            if (types == null)
            {
                return ret;
            }
            for (int i = 0; i < types.Count; ++i)
            {
                XmlNode node = types[i];
                string n = node.Attributes["name"].InnerText;
                int t = int.Parse(node.Attributes["type"].InnerText);
                ret.Add(n, t);
            }

            return ret;
        }

        /// <summary>
        /// 更新边框信息。
        /// </summary>
        /// <param name="metadata">原来的sprite信息。</param>
        /// <param name="infos">sprite列表。</param>
        private static void UpdateBorder(SpriteMetaData[] metadata, List<SpriteInfo> infos)
        {
            Dictionary<string, SpriteInfo> findmap = new Dictionary<string, SpriteInfo>();
            foreach (SpriteInfo si in infos)
            {
                findmap.Add(si.Name, si);
            }

            foreach (SpriteMetaData smd in metadata)
            {
                SpriteInfo si;
                if (findmap.TryGetValue(smd.name, out si))
                {
                    si.Border = smd.border;
                }
            }
        }

        /// <summary>
        /// 生成sprite信息。
        /// </summary>
        /// <param name="infos">>sprite列表。</param>
        /// <returns>prite信息。</returns>
        private static SpriteMetaData[] BuildSpriteMetaData(List<SpriteInfo> infos)
        {
            if (infos == null)
            {
                return new SpriteMetaData[0];
            }

            SpriteMetaData[] ret = new SpriteMetaData[infos.Count];
            for (int i = 0; i < ret.Length; ++i)
            {
                SpriteMetaData smd = new SpriteMetaData();
                SpriteInfo si = infos[i];
                smd.name = si.Name;
                smd.border = si.Border;
                smd.alignment = 0;
                smd.pivot = new Vector2(0.5f, 0.5f);
                smd.rect = new Rect(si.X, si.Y, si.Width, si.Height);
                ret[i] = smd;
            }
            return ret;
        }
    }
}