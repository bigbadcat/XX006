using UnityEngine;
using System.Collections;
using System;

namespace XuXiang
{
    /// <summary>
    /// 圆形数据。
    /// </summary>
    [Serializable]
    public struct ShapeCircle
    {
        /// <summary>
        /// 圆心坐标。
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// 半径。
        /// </summary>
        public float radius;

        public ShapeCircle(Vector2 pos, float r)
        {
            position = pos;
            radius = r;
        }
    }

    /// <summary>
    /// 矩形数据。
    /// </summary>
    [Serializable]
    public struct ShapeRect
    {
        /// <summary>
        /// 中心坐标。
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// 朝向。
        /// </summary>
        public float direction;

        /// <summary>
        /// 宽度，与朝向垂直的尺寸。
        /// </summary>
        public float width;

        /// <summary>
        /// 高度，与朝向平行的尺寸。
        /// </summary>
        public float height;

        public ShapeRect(Vector2 pos, float dir, float w, float h)
        {
            position = pos;
            direction = dir;
            width = w;
            height = h;
        }
    }

    /// <summary>
    /// 发射数据。(矩形的另一种描述)
    /// </summary>
    [Serializable]
    public struct ShapeTransmit
    {
        /// <summary>
        /// 起点坐标。
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// 朝向。
        /// </summary>
        public float direction;

        /// <summary>
        /// 宽度，与朝向垂直的尺寸。
        /// </summary>
        public float width;

        /// <summary>
        /// 长度，与朝向平行的尺寸。
        /// </summary>
        public float lenghth;

        public ShapeTransmit(Vector2 pos, float dir, float w, float l)
        {
            position = pos;
            direction = dir;
            width = w;
            lenghth = 1;
        }
    }

    /// <summary>
    /// 扇形数据。
    /// </summary>
    [Serializable]
    public struct ShapeSector
    {
        /// <summary>
        /// 圆心坐标。
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// 半径。
        /// </summary>
        public float radius;

        /// <summary>
        /// 朝向。
        /// </summary>
        public float direction;

        /// <summary>
        /// 张开角度。
        /// </summary>
        public float angle;

        public ShapeSector(Vector2 pos, float r, float dir, float a)
        {
            position = pos;
            radius = r;
            direction = dir;
            angle = a;
        }
    }

    /// <summary>
    /// 环形数据。
    /// </summary>
    [Serializable]
    public struct ShapeRing
    {
        /// <summary>
        /// 圆心坐标。
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// 内圆半径。
        /// </summary>
        public float radius_in;

        /// <summary>
        /// 外圆半价。
        /// </summary>
        public float radius_out;

        public ShapeRing(Vector2 pos, float r_in, float r_out)
        {
            position = pos;
            radius_in = r_in;
            radius_out = r_out;
        }
    }
}
