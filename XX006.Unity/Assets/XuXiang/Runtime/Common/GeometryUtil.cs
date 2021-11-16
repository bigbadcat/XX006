using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XuXiang
{
    public static class GeometryUtil
    {
        /// <summary>
        /// 获取垂直与某个法线且经过特定点的平面方程。
        /// </summary>
        /// <param name="normal">垂直平面的法线。</param>
        /// <param name="point">要经过的点。</param>
        /// <returns>xyzw分别表示平面方程Ax+By+Cz+D=0里的ABCD。</returns>
        public static Vector4 GetPlane(Vector3 normal, Vector3 point)
        {
            return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, point));
        }

        /// <summary>
        /// 通过三点获取一个平面方程。点的顺时针的面表示正面。
        /// </summary>
        /// <param name="a">A点。</param>
        /// <param name="b">B点。</param>
        /// <param name="c">C点。</param>
        /// <returns>xyzw分别表示平面方程Ax+By+Cz+D=0里的ABCD。</returns>
        public static Vector4 GetPlane(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            return GetPlane(normal, a);
        }

        /// <summary>
        /// 获取摄像机视锥体远平面的四个点。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左下，右下，左上，右上四个点的世界坐标。</returns>
        public static Vector3[] GetCameraFarClipPlanePoint(Camera camera)
        {
            Vector3[] points = new Vector3[4];
            GetCameraFarClipPlanePoint(camera, points);
            return points;
        }

        /// <summary>
        /// 获取摄像机视锥体远平面的四个点。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左下，右下，左上，右上四个点的世界坐标。</returns>
        public static Vector3[] GetCameraFarClipPlanePoint(Camera camera, Vector3[] points)
        {
            Transform transform = camera.transform;
            float distance = camera.farClipPlane;
            float halfFovRad = Mathf.Deg2Rad * camera.fieldOfView * 0.5f;
            float upLen = distance * Mathf.Tan(halfFovRad);
            float rightLen = upLen * camera.aspect;
            Vector3 farCenterPoint = transform.position + distance * transform.forward;
            Vector3 up = upLen * transform.up;
            Vector3 right = rightLen * transform.right;
            points[0] = farCenterPoint - up - right;//left-bottom
            points[1] = farCenterPoint - up + right;//right-bottom
            points[2] = farCenterPoint + up - right;//left-up
            points[3] = farCenterPoint + up + right;//right-up
            return points;
        }

        /// <summary>
        /// 缓存计算过程中的顶点数组，减少GC。
        /// </summary>
        private static Vector3[] s_CachePoints = new Vector3[4];

        /// <summary>
        /// 获取摄像机视锥体的六个平面。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左右下上近远六个面方程数组。</returns>
        public static Vector4[] GetFrustumPlane(Camera camera)
        {
            Vector4[] planes = new Vector4[6];
            GetFrustumPlane(camera, planes);
            return planes;
        }

        /// <summary>
        /// 获取摄像机视锥体的六个平面。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左右下上近远六个面方程数组。</returns>
        public static Vector4[] GetFrustumPlane(Camera camera, Vector4[] planes)
        {
            Transform transform = camera.transform;
            Vector3 cameraPosition = transform.position;
            Vector3[] points = GetCameraFarClipPlanePoint(camera, s_CachePoints);

            //按顺时针传入点坐标。
            planes[0] = GetPlane(cameraPosition, points[0], points[2]);     //left
            planes[1] = GetPlane(cameraPosition, points[3], points[1]);     //right
            planes[2] = GetPlane(cameraPosition, points[1], points[0]);     //bottom
            planes[3] = GetPlane(cameraPosition, points[2], points[3]);     //up
            planes[4] = GetPlane(-transform.forward, transform.position + transform.forward * camera.nearClipPlane);//near
            planes[5] = GetPlane(transform.forward, transform.position + transform.forward * camera.farClipPlane);//far
            return planes;
        }

        /// <summary>
        /// 判断一个点是否在平面法线指向的那一侧(外侧)。
        /// </summary>
        /// <param name="plane">平面方程。</param>
        /// <param name="point">点坐标。</param>
        /// <returns>是否在外侧。</returns>
        public static bool IsOutsideThePlane(Vector4 plane, Vector3 point)
        {
            return Vector3.Dot(plane, point) + plane.w > 0;
        }

        /// <summary>
        /// 获取边框的8个点。
        /// </summary>
        /// <param name="center">边框中心。</param>
        /// <param name="size">边框大小。</param>
        /// <param name="points">保存边框八个点的数组。</param>
        /// <returns>保存边框八个点的数组。/returns>
        public static Vector4[] GetBoundPoints(Vector3 center, Vector3 size, Vector4[] points)
        {
            Vector3 boundMin = center - size / 2;
            Vector3 boundMax = center + size / 2;
            points[0] = new Vector4(boundMin.x, boundMin.y, boundMin.z, 1);
            points[1] = new Vector4(boundMax.x, boundMax.y, boundMax.z, 1);
            points[2] = new Vector4(boundMax.x, boundMax.y, boundMin.z, 1);
            points[3] = new Vector4(boundMax.x, boundMin.y, boundMax.z, 1);
            points[4] = new Vector4(boundMax.x, boundMin.y, boundMin.z, 1);
            points[5] = new Vector4(boundMin.x, boundMax.y, boundMax.z, 1);
            points[6] = new Vector4(boundMin.x, boundMax.y, boundMin.z, 1);
            points[7] = new Vector4(boundMin.x, boundMin.y, boundMax.z, 1);
            return points;
        }
    }
}