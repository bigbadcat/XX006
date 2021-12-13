using System;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// UI面板管理。
    /// </summary>
    public class PanelManager : MonoBehaviourCache
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 底部信息层级。
        /// </summary>
        public static float LayerBottom = 3;

        /// <summary>
        /// UI层级。
        /// </summary>
        public static float LayerUI = 2;

        /// <summary>
        /// 顶部信息层级。
        /// </summary>
        public static float LayerTop = 1;

        /// <summary>
        /// 打开一个UI。
        /// </summary>
        /// <param name="code">UI编号。</param>
        /// <param name="prefab">UI对象路径。</param>
        /// <param name="full">是否为全屏UI。</param>
        /// <param name="load_param">UI加载时的参数。</param>
        /// <param name="open_param">UI打开时的参数。</param>
        public PanelBase Open(int code, string prefab, bool full = true, string load_param="", object open_param=null)
        {
            //已经打开了的则无视，或者从被隐藏的UI里找到打开
            PanelBase ui = GetUI(code);
            bool needload = false;
            if (ui != null)
            {
                if (ui.IsShow)
                {
                    return ui;
                }
                _openList.Remove(ui);
            }
            else
            {
                ui = LoadPanel(code, prefab, out needload);
                if (ui == null)
                {
                    return null;
                }
            }

            //UI初始化
            ui.Code = code;
            ui.IsFull = full;
            ui.Manager = this;
            ui.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            ui.Canvas.worldCamera = m_UICamera;
            ui.Canvas.planeDistance = LayerUI;
            ui.CacheTransform.SetParent(CacheTransform);
            ui.CacheGameObject.SetActive(true);     //保证OnLoad在Awake之后执行
            if (needload)
            {
                ui.OnLoad(load_param);
            }           
            AddShowPanel(ui, open_param);
            return ui;
        }

        /// <summary>
        /// 关闭一个UI。
        /// </summary>
        /// <param name="code">UI编号。</param>
        public void Close(int code)
        {
            PanelBase ui = GetUI(code);
            if (ui == null)
            {
                //ui并未打开
                return;
            }

            //若处于显示状态则关闭
            bool showlastfull = false;          //是否要把上一个全屏UI显示出来
            if (ui.IsShow)
            {
                showlastfull = ui.IsFull;
                ui.OnClose();
                ui.CacheGameObject.SetActive(false);
            }            
            _openList.Remove(ui);
            GameObject.Destroy(ui.CacheGameObject);     //直接移除

            //把上一个全屏UI显示出来
            if (showlastfull)
            {
                for (int i = _openList.Count - 1; i >= 0; --i)
                {
                    ui = _openList[i];
                    if (!ui.IsShow)
                    {
                        ui.CacheGameObject.SetActive(true);
                        ui.OnOpen(null);
                        if (ui.IsFull)
                        {
                            break;
                        }
                    }                    
                }
            }
        }
        
        /// <summary>
        /// 获取UI。
        /// </summary>
        /// <param name="code">UI编号。</param>
        /// <returns>UI对象。</returns>
        public PanelBase GetUI(int code)
        {
            foreach (var ui in _openList)
            {
                if (ui.Code == code)
                {
                    return ui;
                }
            }
            return null;
        }

        /// <summary>
        /// 关闭当前顶层页面。
        /// </summary>
        public void CloseCurUI()
        {
            if (_openList.Count > 0)
            {
                PanelBase ui = _openList[_openList.Count - 1];
                if (ui != null)
                {
                    Close(ui.Code);
                }                
            }
        }

        /// <summary>
        /// 关闭所有UI。
        /// </summary>
        public void CloseAll()
        {
            //关闭所有当前显示的UI，移除UI列表到缓存中
            for (int i = _openList.Count - 1; i >= 0; --i)
            {
                PanelBase ui = _openList[i];
                if (ui == null)
                {
                    continue;
                }
                if (ui.IsShow)
                {
                    ui.OnClose();
                    ui.CacheGameObject.SetActive(false);
                }
                GameObject.Destroy(ui.CacheGameObject);     //直接移除
            }
            _openList.Clear();
        }

        /// <summary>
        /// 添加对象到常驻画布中。
        /// </summary>
        /// <param name="rt">要添加的对象。</param>
        /// <param name="top">是否为顶部画布。</param>
        /// <param name="layer">添加到的层级。0底层 1中层 2上层</param>
        public void AddObject(RectTransform rt, bool top, int layer = 1)
        {
            UILongCanvas canv = top ? TopCanvas : BottomCanvas;
            canv.AddObject(rt, (UILongCanvas.LayerType)layer);
        }

        /// <summary>
        /// 处理返回键。
        /// </summary>
        public void OnKeyBack()
        {
            //从最顶层UI开始，如果遇到没显示的或者OnKeyBack为false的则结束
            for (int i = _openList.Count - 1; i >= 0; --i)
            {
                PanelBase ui = _openList[i];
                if (!ui.IsShow || !ui.OnKeyBack())
                {
                    break;
                }
            }
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取UI摄像机。
        /// </summary>
        public Camera UICamera
        {
            get { return m_UICamera; }
        }

        /// <summary>
        /// 获取底部画布。
        /// </summary>
        public UILongCanvas BottomCanvas
        {
            get
            {
                if (m_BottomCanvas == null)
                {
                    m_BottomCanvas = LoadNewLongCanvas();
                    m_BottomCanvas.Canvas.planeDistance = LayerBottom;
                    m_BottomCanvas.Canvas.sortingOrder = START_ORDER - 1;
                    m_BottomCanvas.name = "BottomCanvas";
                }
                return m_BottomCanvas;
            }
        }

        /// <summary>
        /// 获取顶部画布。
        /// </summary>
        public UILongCanvas TopCanvas
        {
            get
            {
                if (m_TopCanvas == null)
                {
                    m_TopCanvas = LoadNewLongCanvas();
                    m_TopCanvas.Canvas.planeDistance = LayerTop;
                    m_TopCanvas.Canvas.sortingOrder = END_ORDER + 1;
                    m_TopCanvas.name = "TopCanvas";
                }
                return m_TopCanvas;
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 销毁。
        /// </summary>
        private void OnDestroy()
        {
            CloseAll();
        }

        /// <summary>
        /// 加载一个新的常驻画布。
        /// </summary>
        /// <returns>画布对象。</returns>
        private UILongCanvas LoadNewLongCanvas()
        {
            //加载
            if (string.IsNullOrEmpty(m_LongCanvasPath))
            {
                Log.Error("The LongCanvasPrefab is null.");
                return null;
            }
            UILongCanvas ui = ResourceManager.Instance.LoadObject<UILongCanvas>(m_LongCanvasPath);
            if (ui == null)
            {
                Log.Error("The object can not found UILongCanvas.");
                return null;
            }
            ui.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            ui.Canvas.worldCamera = m_UICamera;            
            ui.CacheTransform.SetParent(CacheTransform);
            ui.CacheGameObject.SetActive(true);
            return ui;
        }

        /// <summary>
        /// 加载一个UI。
        /// </summary>
        /// <param name="code">UI编号。</param>
        /// <param name="path">UI对象路径。</param>
        /// <param name="isnew">是否新加载的。</param>
        /// <returns>UI对象。</returns>
        private PanelBase LoadPanel(int code, string path, out bool isnew)
        {
            //查找缓存是否有
            PanelBase ui;
            if (_cacheSet.TryGetValue(code, out ui))
            {
                isnew = false;
                _cacheSet.Remove(code);
            }
            else
            {
                isnew = true;

                ui = ResourceManager.Instance.LoadObject<PanelBase>(path);

                if (ui == null)
                {
                    Log.Error("Can not load ui. path:{0}", path);
                    return null;
                }
                else
                {
                    if (ui.CacheGameObject.activeSelf)
                    {
                        Log.Warning("The ui prefab should be deactivated. path:{0}", path);
                    }
                }
            }
            return ui;
        }

        /// <summary>
        /// 添加显示一个UI。
        /// </summary>
        /// <param name="ui">UI对象。</param>
        /// <param name="param">打开参数。</param>
        private void AddShowPanel(PanelBase ui, object param)
        {
            if (ui.IsFull)
            {
                //隐藏所有当前显示的UI
                foreach (var tmp in _openList)
                {
                    if (tmp.IsShow)
                    {
                        tmp.OnClose();
                        tmp.CacheGameObject.SetActive(false);
                    }
                }
            }

            //显示UI
            _openList.Add(ui);
            ui.Canvas.sortingOrder = _showOrder;
            _showOrder += 10;           //留9个给UI插入特效
            ui.OnOpen(param);

            //检查层级
            if (_showOrder >= END_ORDER)
            {
                _showOrder = START_ORDER;
                for (int i = 0; i < _openList.Count; ++i)
                {
                    _openList[i].Canvas.sortingOrder = _showOrder;
                    _showOrder += 10;
                }
            }
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// UI的起始层级序号。
        /// </summary>
        private const int START_ORDER = 10;

        /// <summary>
        /// UI的最大序号。
        /// </summary>
        private const int END_ORDER = 1000;

        /// <summary>
        /// 当前打开的UI列表。完全包含_showList。
        /// </summary>
        private List<PanelBase> _openList = new List<PanelBase>();

        /// <summary>
        /// UI缓存集合。与_openList交集为空。
        /// </summary>
        private Dictionary<int, PanelBase> _cacheSet = new Dictionary<int, PanelBase>();

        /// <summary>
        /// 显示层级顺序，达到END_ORDER时会重新设置。
        /// </summary>
        private int _showOrder = START_ORDER;

        /// <summary>
        /// UI摄像机。
        /// </summary>
        [SerializeField]
        private Camera m_UICamera;

        /// <summary>
        /// 常驻画布的原型。
        /// </summary>
        [SerializeField]
        private string m_LongCanvasPath;

        /// <summary>
        /// 底部画布。
        /// </summary>
        private UILongCanvas m_BottomCanvas;

        /// <summary>
        /// 顶部画布。
        /// </summary>
        private UILongCanvas m_TopCanvas;

        #endregion        
    }
}