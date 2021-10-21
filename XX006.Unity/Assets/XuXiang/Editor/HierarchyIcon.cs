using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;
using UnityEditor;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// Hierarchy图标扩展。
    /// </summary>
    [InitializeOnLoad]
    public class HierarchyIcon
    {
        #region 对外操作----------------------------------------------------------------

        static HierarchyIcon()
        {
            bool show = EditorPrefs.GetBool(PREF_KEY_ACTIVE_CONTROL, true);
            IsActiveControl = show;
        }

        /// <summary>
        /// 控制菜单前的检查。
        /// </summary>
        /// <returns></returns>
        [MenuItem(MENU_KEY_SWITCH, true)]
        public static bool ShowActiveControlCheck()
        {
            Menu.SetChecked(MENU_KEY_SWITCH, IsActiveControl);          //刷新Checked状态
            return true;                //菜单固定可用
        }

        /// <summary>
        /// 切换显示状态。
        /// </summary>
        [MenuItem(MENU_KEY_SWITCH)]
        public static void ShowActiveControl()
        {
            IsActiveControl = !IsActiveControl;            
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 控制菜单项。
        /// </summary>
        public const string MENU_KEY_SWITCH = "Tools/Hierarchy/ActiveControl";

        /// <summary>
        /// 控制状态存储Key。
        /// </summary>
        public const string PREF_KEY_ACTIVE_CONTROL = "HIERARCHY_ACTIVE_CONTROL";

        /// <summary>
        /// 获取或设置是否显示激活控制按钮。
        /// </summary>
        public static bool IsActiveControl
        {
            get { return _isActiveControl; }
            set
            {
                if (_isActiveControl == value)
                {
                    return;
                }

                _isActiveControl = value;
                EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
                if (_isActiveControl)
                {
                    EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
                }
                EditorApplication.RepaintHierarchyWindow();
                EditorPrefs.SetBool(PREF_KEY_ACTIVE_CONTROL, _isActiveControl);
            }
        }

        /// <summary>
        /// 获取激活状态的图标。
        /// </summary>
        public static Texture2D ActiveIco
        {
            get
            {
                if (_activeIco == null)
                {
                    _activeIco = AssetDatabase.LoadAssetAtPath("Assets/XuXiang/Editor/Res/eye_active.png", typeof(Texture2D)) as Texture2D;
                }
                return _activeIco;
            }
        }

        /// <summary>
        /// 获取未激活状态的图标。
        /// </summary>
        public static Texture2D InactiveIco
        {
            get
            {
                if (_inactiveIco == null)
                {
                    _inactiveIco = AssetDatabase.LoadAssetAtPath("Assets/XuXiang/Editor/Res/eye_inactive.png", typeof(Texture2D)) as Texture2D;
                }
                return _inactiveIco;
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null)
            {
                return;
            }

            if (_isActiveControl)
            {
                Texture2D ico = go.activeSelf ? ActiveIco : InactiveIco;
                if (GUI.Button(new Rect(selectionRect.xMax - ico.width - 4, selectionRect.center.y - (ico.height / 2f), ico.width, ico.height), ico, GUIStyle.none))
                {
                    go.SetActive(!go.activeSelf);
                }
            }
        }

        #endregion

        #region 内部数据----------------------------------------------------------------


        /// <summary>
        /// 是否绘制Active控制按钮。
        /// </summary>
        private static bool _isActiveControl = false;

        /// <summary>
        /// 活动状态的图标。
        /// </summary>
        private static Texture2D _activeIco = null;

        /// <summary>
        /// 不活动状态的图标。
        /// </summary>
        private static Texture2D _inactiveIco = null;

        #endregion
    }
}