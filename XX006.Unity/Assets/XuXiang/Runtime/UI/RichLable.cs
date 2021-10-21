using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XuXiang
{
    /// <summary>
    /// 扩展标签。(标签从外到内包含顺序<href><color>，<image>独立)
    /// 标签格式：
    ///     <href param=[点击参数]>[超链接显示内容]</href>
    ///     <image name=[图片名称]/>  PS:图片通过AtlasSpriteManager.Instance.GetSprite函数进行加载
    /// TextWidth和TextHeight注意事项
    ///     若RichLable还未Awake或者处于inactive状态，则设置Text后Width和Height并不会被设置，需要等Awake或active之后触发重新绘制才会设置。
    ///     如果父节点需要在Awake时就设置RichLable并且获取正确的TextWidth和TextHeight，可以在编辑状态下，将RichLable的Active设为false，
    ///     在设置Text前调用RichLable.SetActive(true)(提前手动触发RichLable的Awake)。
    /// </summary>
    [RequireComponent(typeof(Text))]
    [ExecuteInEditMode]
    public class RichLable : BaseMeshEffect, IPointerClickHandler, ICanvasRaycastFilter
    {
        #region 数据定义----------------------------------------------------------------

        /// <summary>
        /// 范围信息类。
        /// </summary>
        private class RangeInfo
        {
            /// <summary>
            /// 起始索引(包含)。
            /// </summary>
            public int StartIndex { get; set; }

            /// <summary>
            /// 结束索引(不包含)。
            /// </summary>
            public int EndIndex { get; set; }
        }

        /// <summary>
        /// 颜色信息类。
        /// </summary>
        private class ColorInfo : RangeInfo
        {
            /// <summary>
            /// 文本颜色。
            /// </summary>
            public Color TextColor { get; set; }
        }

        /// <summary>
        /// 超链接信息类。
        /// </summary>
        private class HrefInfo : RangeInfo
        {
            /// <summary>
            /// 超链接参数。
            /// </summary>
            public string Param;

            /// <summary>
            /// 点击区域列表。
            /// </summary>
            public List<Rect> Boxes = new List<Rect>();
        }

        /// <summary>
        /// 图像信息类。
        /// </summary>
        private class ImageInfo : RangeInfo
        {
            /// <summary>
            /// 图片名称。
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 图片位置。
            /// </summary>
            public Vector2 Position { get; set; }

            /// <summary>
            /// 图片尺寸。
            /// </summary>
            public Vector2 Size { get; set; }

            /// <summary>
            /// 图片缩放。
            /// </summary>
            public float Scale { get; set; }
        }

        /// <summary>
        /// 超链接点击事件。
        /// </summary>
        [Serializable]
        public class HrefClickEvent : UnityEvent<string> { }

        #endregion

        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 修改网格。
        /// </summary>
        /// <param name="vh">顶点数据访问接口。</param>
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
            {
                return;
            }

            List<UIVertex> vertices = new List<UIVertex>();
            vh.GetUIVertexStream(vertices);

            Vector2 lineuv = GetUnderLineUV(vertices);      //计算下划线纹理坐标
            GetTextSize(vertices);
            ModifyColorMesh(vertices);                      //颜色调整
            ModifyHrefMesh(vertices, lineuv);               //超链接下划线颜色依赖字符颜色，需要在颜色调整之后
            ModifyImageMesh(vertices);                      //隐藏图片的占位符

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertices);
        }

        /// <summary>
        /// 判断点击是否可用。
        /// </summary>
        /// <param name="sp">点击位置，屏幕坐标。</param>
        /// <param name="eventCamera">事件摄像机。</param>
        /// <returns>点击是否可用。(true:吞噬本次点击，触发自身点击效果。false:忽略本次点击，事件继续传递下去。)</returns>
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            //没有超链接，不接收点击
            if (m_HrefInfos.Count <= 0)
            {
                return false;
            }

            //是否在超链接区域内
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(graphic.rectTransform, sp, eventCamera, out lp);
            foreach (var hrefInfo in m_HrefInfos)
            {
                var boxes = hrefInfo.Boxes;
                for (var i = 0; i < boxes.Count; ++i)
                {
                    if (boxes[i].Contains(lp))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 点击事件检测是否点击到超链接文本
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(graphic.rectTransform, eventData.position, eventData.pressEventCamera, out lp);
            foreach (var hrefInfo in m_HrefInfos)
            {
                var boxes = hrefInfo.Boxes;
                for (var i = 0; i < boxes.Count; ++i)
                {
                    if (boxes[i].Contains(lp))
                    {
                        OnHrefClick.Invoke(hrefInfo.Param);
                        return;
                    }
                }
            }
            OnHrefClick.Invoke(string.Empty);           //通知点击了文本
        }

        /// <summary>
        /// 重新生成文本。
        /// </summary>
        public void Rebuild()
        {
            GetComponent<Text>().text = ParseText();
            this.graphic.Rebuild(CanvasUpdate.PreRender);
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取或设置文本。
        /// </summary>
        public string Text
        {
            get { return m_Text; }
            set
            {
                if (m_Text.CompareTo(value) != 0)
                {
                    m_Text = value;
                    GetComponent<Text>().text = ParseText();
                    this.graphic.Rebuild(CanvasUpdate.PreRender);
                }
            }
        }

        /// <summary>
        /// 获取超链接点击事件。
        /// </summary>
        public HrefClickEvent OnHrefClick
        {
            get { return m_HrefClickEvent; }
            set { m_HrefClickEvent = value; }
        }

        /// <summary>
        /// 获取图片挂接点。
        /// </summary>
        public RectTransform ImageHolder
        {
            get { return m_ImageHolder; }
        }

        /// <summary>
        /// 获取文本显示宽度。(必须保证已经Awake了)
        /// </summary>
        public float TextWidth
        {
            get { return m_TextWidth; }
        }

        /// <summary>
        /// 获取文本显示高度。(必须保证已经Awake了)
        /// </summary>
        public float TextHeight
        {
            get { return m_TextHeight; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 唤醒。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            m_ImageHolder = GetComponent<RectTransform>();
            m_ImageHolder.ClearChild();
            m_Images.Clear();
        }

        /// <summary>
        /// 解析文本。
        /// </summary>
        /// <returns>解析后显示的文本内容。</returns>
        private string ParseText()
        {
            //解析顺序不能改变，后边的解析在改变最终字符串内容时，会对前面解析出来的索引范围进行修正
            string temp = ParseColor(m_Text);
            temp = ParseHref(temp);
            temp = ParseImage(temp);
            return temp;
        }

        /// <summary>
        /// 解析颜色值。
        /// </summary>
        /// <returns>解析后的字符串。</returns>
        private string ParseColor(string text)
        {
            Color def = GetComponent<Text>().color;
            int index = 0;
            m_ColorInfos.Clear();
            CacheSB.Length = 0;
            CacheSB.Append(UnderLineChar);      //首个解析函数要加下划线占位符
            foreach (Match match in ColorRegex.Matches(text))
            {
                string colorstr = match.Groups[1].Value.Trim();
                string innertext = match.Groups[2].Value;
                ColorInfo info = new ColorInfo();
                CacheSB.Append(text.Substring(index, match.Index - index));       //匹配目标前的那一部分
                info.StartIndex = CacheSB.Length;
                info.EndIndex = info.StartIndex + innertext.Length;
                info.TextColor = colorstr.ToColor(def);
                m_ColorInfos.Add(info);
                CacheSB.Append(innertext);
                index = match.Index + match.Length;
            }
            CacheSB.Append(text.Substring(index, text.Length - index));
            return CacheSB.ToString();
        }

        /// <summary>
        /// 解析超链接值。
        /// </summary>
        /// <returns>解析后的字符串。</returns>
        private string ParseHref(string text)
        {
            int index = 0;
            m_HrefInfos.Clear();
            CacheSB.Length = 0;
            foreach (Match match in HrefRegex.Matches(text))
            {
                string param = match.Groups[1].Value.Trim();
                string innertext = match.Groups[2].Value;
                int cut = match.Length - innertext.Length;          //匹配调整后缩短的长度
                HrefInfo info = new HrefInfo();
                CacheSB.Append(text.Substring(index, match.Index - index));       //匹配目标前的那一部分
                info.StartIndex = CacheSB.Length;
                info.EndIndex = info.StartIndex + innertext.Length;
                info.Param = param;
                m_HrefInfos.Add(info);
                CacheSB.Append(innertext);
                index = match.Index + match.Length;
                AdujstIndex(m_ColorInfos, info.StartIndex, info.StartIndex + match.Length, cut, 7);       //7为"</href>"标签的长度
            }
            CacheSB.Append(text.Substring(index, text.Length - index));
            return CacheSB.ToString();
        }

        /// <summary>
        /// 解析图像值。
        /// </summary>
        /// <returns>解析后的字符串。</returns>
        private string ParseImage(string text)
        {
            float fs = GetComponent<Text>().fontSize;
            int index = 0;
            m_ImageInfos.Clear();
            CacheSB.Length = 0;
            foreach (Match match in ImageRegex.Matches(text))
            {
                string[] param = match.Groups[1].Value.Trim().Split(',');
                string innerchar = (param.Length >= 3 && param[2].Length > 0) ? param[2].Substring(0, 1) : "哈";          //默认用哈，最接近正方形
                string innertext = innerchar;                             //图像位置用"M"字符占位 
                int prelen = 0;
                if (param.Length >= 2)
                {
                    float scale;
                    if (float.TryParse(param[1], out scale))
                    {
                        if (Math.Abs(scale - 1) >= 0.0001f)
                        {
                            int imgsize = (int)(fs * scale);            //必须取整
                            string pre = string.Format("<size={0}>", imgsize);
                            innertext = string.Format("{0}{1}</size>", pre, innerchar);
                            prelen = pre.Length;
                        }
                    }
                }
                float imgscale = 1;
                if (param.Length >= 4)
                {
                    float scale;
                    if (float.TryParse(param[3], out scale))
                    {
                        imgscale = scale;
                    }
                }

                ImageInfo info = new ImageInfo();
                CacheSB.Append(text.Substring(index, match.Index - index));       //匹配目标前的那一部分
                info.StartIndex = CacheSB.Length + prelen;
                info.EndIndex = info.StartIndex + 1;
                info.Name = param[0];
                info.Scale = imgscale;
                m_ImageInfos.Add(info);
                CacheSB.Append(innertext);

                int cut = match.Length - innertext.Length;          //匹配调整后缩短的长度
                index = match.Index + match.Length;
                AdujstIndex(m_ColorInfos, info.StartIndex, info.StartIndex + match.Length, cut, 0);
                AdujstIndex(m_HrefInfos, info.StartIndex, info.StartIndex + match.Length, cut, 0);
            }
            CacheSB.Append(text.Substring(index, text.Length - index));
            return CacheSB.ToString();
        }

        /// <summary>
        /// 调整索引。
        /// </summary>
        /// <param name="infos">索引信息列表。</param>
        /// <param name="start">调整区域的原起始索引。（包含）</param>
        /// <param name="end">调整区域的原结束索引。（不包含）</param>
        /// <param name="cut">区域被减少的长度。</param>
        /// <param name="endcut">区域被减少的右侧长度。（当颜色区间在调整区间内时，起始索引要排除此长度）</param>
        private static void AdujstIndex<T>(List<T> infos, int start, int end, int cut, int endcut) where T : RangeInfo
        {
            //索引区间不会和传入的索引区间交叠，只有不相交或被包含两种关系
            for (int i = infos.Count - 1; i >= 0; --i)
            {
                RangeInfo info = infos[i];
                if (info.StartIndex >= end)
                {
                    //颜色区间在调整区间右侧，减去cut
                    info.StartIndex -= cut;
                    info.EndIndex -= cut;
                }
                else if (info.StartIndex >= start)
                {
                    //颜色区间在调整区间内，少减endcut
                    int tcut = cut - endcut;
                    info.StartIndex -= tcut;
                    info.EndIndex -= tcut;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 调整索引。
        /// </summary>
        /// <param name="infos">索引信息列表。</param>
        /// <param name="offset">要调整的索引值。</param>
        private static void AdujstIndex<T>(List<T> infos, int offset) where T : RangeInfo
        {
            for (int i = 0; i < infos.Count; ++i)
            {
                RangeInfo info = infos[i];
                info.StartIndex += offset;
                info.EndIndex += offset;
            }
        }

        /// <summary>
        /// 获取下划线纹理坐标。
        /// </summary>
        /// <param name="vertices">顶点数组。</param>
        /// <returns>下划线纹理坐标。(占位字符中点的uv)</returns>
        private Vector2 GetUnderLineUV(List<UIVertex> vertices)
        {
            Vector2 lineuv = Vector2.zero;
            int n = UnderLineChar.Length * 6;
            if (vertices.Count >= n)
            {
                int start = 8 * 6;      //<size=4>的长度
                for (int i = 0; i < 6; ++i)
                {
                    Vector4 uv = vertices[start + i].uv0;
                    lineuv += new Vector2(uv.x, uv.y);
                }
                lineuv /= 6;

                //隐藏额外的下划线占位字符
                UIVertex uiv = vertices[0];
                uiv.color = new Color32(0, 0, 0, 0);
                for (int i = 0; i < n; ++i)
                {
                    vertices[i] = uiv;
                }
            }
            return lineuv;
        }

        /// <summary>
        /// 获取文本尺寸。
        /// </summary>
        /// <param name="vertices">顶点数组。</param>
        private void GetTextSize(List<UIVertex> vertices)
        {
            int start = UnderLineChar.Length * 6;
            float fs = GetComponent<Text>().fontSize;
            if (vertices.Count <= start)
            {
                m_TextWidth = fs;
                m_TextHeight = fs;
                return;
            }

            //查找最高和最低点 
            float minx = vertices[start].position.x;
            float maxx = vertices[start].position.x;
            float miny = vertices[start].position.y;
            float maxy = vertices[start].position.y;
            for (int i = start+1; i < vertices.Count; ++i)
            {
                Vector3 pos = vertices[i].position;
                Vector2 uv = vertices[i].uv0;
                if (vertices[i].color.a > 0 && Math.Abs(uv.x) + Math.Abs(uv.y) > 0)        //只统计可视字符
                {
                    minx = Math.Min(minx, pos.x);
                    maxx = Math.Max(maxx, pos.x);
                    miny = Math.Min(miny, pos.y);
                    maxy = Math.Max(maxy, pos.y);
                }
            }

            m_TextWidth = Math.Max(fs, maxx - minx + 2);
            m_TextHeight = Math.Max(fs, maxy - miny + 2);
        }

        /// <summary>
        /// 调整颜色。
        /// </summary>
        /// <param name="vertices">顶点数组。</param>
        private void ModifyColorMesh(List<UIVertex> vertices)
        {
            for (int i = 0; i < m_ColorInfos.Count; ++i)
            {
                ColorInfo info = m_ColorInfos[i];
                int start = info.StartIndex * 6;
                int end = Math.Min(info.EndIndex * 6, vertices.Count);
                for (int j = start; j < end; ++j)
                {
                    UIVertex uiv = vertices[j];
                    uiv.color = info.TextColor;
                    vertices[j] = uiv;
                }
            }
        }

        /// <summary>
        /// 调整下划线。
        /// </summary>
        /// <param name="vertices">顶点数组。</param>
        /// <param name="lineuv">下划线的纹理坐标。</param>
        private void ModifyHrefMesh(List<UIVertex> vertices, Vector2 lineuv)
        {
            Vector3[] linev = new Vector3[6];
            for (int i = 0; i < m_HrefInfos.Count; ++i)
            {
                HrefInfo info = m_HrefInfos[i];
                int start = info.StartIndex * 6;
                int end = Math.Min(info.EndIndex * 6, vertices.Count);
                UIVertex startuiv = vertices[start];
                Bounds bounds = new Bounds(startuiv.position, Vector3.zero);
                info.Boxes.Clear();
                for (int j = start + 1; j < end; ++j)
                {
                    Vector3 pos = vertices[j].position;
                    if (j % 6 == 0 && pos.y < bounds.min.y) // 换行重新添加包围框，每个字符6个点
                    {
                        info.Boxes.Add(new Rect(bounds.min, bounds.size));
                        bounds = new Bounds(pos, Vector3.zero);
                    }
                    else
                    {
                        bounds.Encapsulate(pos); // 扩展包围框
                    }
                }
                info.Boxes.Add(new Rect(bounds.min, bounds.size));

                //在末尾添加下划线顶点，下划线颜色等于线条第一个字符的颜色
                for (int j = 0; j < info.Boxes.Count; ++j)
                {
                    Rect r = info.Boxes[j];
                    float xl = r.min.x;
                    float xr = r.max.x;
                    float yt = r.min.y;
                    float yb = yt - m_UnderLineHeight;
                    linev[0] = new Vector3(xl, yt);
                    linev[1] = new Vector3(xr, yt);
                    linev[2] = new Vector3(xr, yb);
                    linev[3] = new Vector3(xr, yb);
                    linev[4] = new Vector3(xl, yb);
                    linev[5] = new Vector3(xl, yt);
                    for (int k = 0; k < linev.Length; ++k)
                    {
                        UIVertex uiv = startuiv;
                        uiv.uv0 = lineuv;
                        uiv.position = linev[k];
                        vertices.Add(uiv);
                    }
                }
            }
        }

        /// <summary>
        /// 调整图像。
        /// </summary>
        /// <param name="vertices">顶点数组。</param>
        private void ModifyImageMesh(List<UIVertex> vertices)
        {
            for (int i = 0; i < m_ImageInfos.Count; ++i)
            {
                ImageInfo info = m_ImageInfos[i];
                int start = info.StartIndex * 6;
                int end = Math.Min(info.EndIndex * 6, vertices.Count);
                Vector3 startpos = vertices[start].position;
                Vector3 mpos = Vector3.zero;
                float iw = 0;
                float ih = 0;
                for (int j = start; j < end; ++j)
                {
                    UIVertex uiv = vertices[j];
                    Vector3 pos = uiv.position;
                    uiv.color = new Color32(0, 0, 0, 0);
                    vertices[j] = uiv;
                    mpos += pos;
                    iw = Math.Max(iw, pos.x - startpos.x);
                    ih = Math.Max(ih, startpos.y - pos.y);
                }
                int n = end - start;
                info.Position = new Vector2(mpos.x / n, mpos.y / n);
                info.Size = new Vector2(iw, ih);
            }

            //非运行时的编辑器状态不生成
            if (Application.isPlaying)
            {
                CancelInvoke();
                Invoke("RefreshImage", 0);      //推迟到下一帧刷新图片显示，ModifyMesh函数内不能随便搞事
            }
        }

        /// <summary>
        /// 刷新图像。
        /// </summary>
        private void RefreshImage()
        {
            //补足不够的
            while (m_Images.Count < m_ImageInfos.Count)
            {
                Image img = CreateImage();
                m_Images.Add(img);
            }

            //更新位置和图图像
            for (int i = 0; i < m_ImageInfos.Count; ++i)
            {
                ImageInfo info = m_ImageInfos[i];
                Image img = m_Images[i];
                RectTransform rt = img.GetComponent<RectTransform>();
                img.gameObject.SetActive(true);
                rt.anchoredPosition = info.Position;
                rt.sizeDelta = info.Size * info.Scale;
                if (info.Name.CompareTo(img.name) != 0)
                {
                    img.name = info.Name;
                    //img.sprite = AtlasSpriteManager.Instance.GetSprite(info.Name);
                    img.sprite = null;      //需要使用者添加sprite获取回调
                }
            }

            //隐藏多余的
            for (int i = m_ImageInfos.Count; i < m_Images.Count; ++i)
            {
                m_Images[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 创建图像控件。
        /// </summary>
        /// <returns></returns>
        private Image CreateImage()
        {
            float fs = GetComponent<Text>().fontSize;
            GameObject go = DefaultControls.CreateImage(new DefaultControls.Resources());
            RectTransform rt = go.transform as RectTransform;
            Image img = go.GetComponent<Image>();
            go.layer = gameObject.layer;
            rt.SetParent(m_ImageHolder);
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.Euler(0, 0, 0);
            rt.localScale = Vector3.one;
            rt.anchoredPosition3D = Vector3.zero;
            rt.sizeDelta = new Vector2(fs, fs);
            rt.anchorMin = m_ImageHolder.pivot;
            rt.anchorMax = rt.anchorMin;
            img.preserveAspect = true;
            return img;
        }

        

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 下划线占位符，用于计算下划线的纹理坐标。
        /// </summary>
        private static string UnderLineChar = "<size=4>█</size>";

        /// <summary>
        /// 颜色匹配正则表达式。
        /// </summary>
        private static Regex ColorRegex = new Regex(@"<color=#(.*?)>(.*?)</color>", RegexOptions.Singleline);

        /// <summary>
        /// 超链接匹配正则表达式。
        /// </summary>
        private static readonly Regex HrefRegex = new Regex(@"<href param=(.*?)>(.*?)</href>", RegexOptions.Singleline);

        /// <summary>
        /// 图像匹配正则表达式
        /// </summary>
        private static readonly Regex ImageRegex = new Regex(@"<image name=(.+?)/>", RegexOptions.Singleline);

        /// <summary>
        /// 缓存。
        /// </summary>
        private static StringBuilder CacheSB = new StringBuilder();

        /// <summary>
        /// 原始文本。
        /// </summary>
        [TextArea(3, 10)]
        [SerializeField]
        private string m_Text = string.Empty;

        /// <summary>
        /// 颜色信息。
        /// </summary>
        private List<ColorInfo> m_ColorInfos = new List<ColorInfo>();

        /// <summary>
        /// 超链接信息。
        /// </summary>
        private List<HrefInfo> m_HrefInfos = new List<HrefInfo>();

        /// <summary>
        /// 超链接点击事件对象。
        /// </summary>
        [SerializeField]
        private HrefClickEvent m_HrefClickEvent = new HrefClickEvent();

        /// <summary>
        /// 下划线高度。
        /// </summary>
        [SerializeField]
        private float m_UnderLineHeight = 1;

        /// <summary>
        /// 图片挂接点。
        /// </summary>
        private RectTransform m_ImageHolder;

        /// <summary>
        /// 图像列表。
        /// </summary>
        private List<Image> m_Images = new List<Image>();

        /// <summary>
        /// 图像信息。
        /// </summary>
        private List<ImageInfo> m_ImageInfos = new List<ImageInfo>();

        /// <summary>
        /// 显示文本宽度。
        /// </summary>
        private float m_TextWidth;

        /// <summary>
        /// 显示文本高度。
        /// </summary>
        private float m_TextHeight;

        #endregion
    }

#if UNITY_EDITOR

    /// <summary>
    /// RichLable的编辑页面。
    /// </summary>
    [CustomEditor(typeof(RichLable))]
    [ExecuteInEditMode]
    public class RichLableEditor : Editor
    {
        /// <summary>
        /// 选中RichLable组件时。
        /// </summary>
        void OnEnable()
        {
            m_TextProperty = serializedObject.FindProperty("m_Text");
            m_OnHrefClickProperty = serializedObject.FindProperty("m_HrefClickEvent");
            m_UnderLineHeightProperty = serializedObject.FindProperty("m_UnderLineHeight");
        }

        /// <summary>
        /// 绘制面板。
        /// </summary>
        public override void OnInspectorGUI()
        {
            RichLable rl = target as RichLable;
            string txt = m_TextProperty.stringValue;
            EditorGUILayout.PropertyField(m_TextProperty);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_OnHrefClickProperty);
            EditorGUILayout.PropertyField(m_UnderLineHeightProperty);
            serializedObject.ApplyModifiedProperties();

            //文本有变则重新生成显示
            if (txt.CompareTo(m_TextProperty.stringValue) != 0)
            {
                rl.Rebuild();
            }
        }

        /// <summary>
        /// 文本属性。
        /// </summary>
        private SerializedProperty m_TextProperty;

        /// <summary>
        /// 超链接点击事件。
        /// </summary>
        private SerializedProperty m_OnHrefClickProperty;

        /// <summary>
        /// 下划线高度。
        /// </summary>
        private SerializedProperty m_UnderLineHeightProperty;
    }

#endif
}