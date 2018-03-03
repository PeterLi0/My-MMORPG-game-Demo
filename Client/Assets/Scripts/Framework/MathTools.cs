using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 范围检测
/// </summary>
public class MathTools
{
    /// <summary>
    /// 点是否在圆内
    /// </summary>
    /// <param name="center">圆心</param>
    /// <param name="p">要检测的一点</param>
    /// <param name="radius">半径</param>
    /// <returns></returns>
    public static bool PointInCircle(Vector3 center, Vector3 p, float radius)
    {
        float distance = Vector3.Distance(center, p);
        return distance < radius;
    }

    /// <summary>
    /// 点是否在扇形内
    /// </summary>
    /// <param name="a">扇形的朝向</param>
    /// <param name="center">扇形所在圆的圆心</param>
    /// <param name="p">要检测的一点P</param>
    /// <param name="radius">半径</param>
    /// <param name="angle">扇形角度</param>
    /// <returns></returns>
    public static bool PointInFan(Vector3 a,  Vector3 center, Vector3 p, float radius, float angleA)
    {
        Vector3 b = (p - center).normalized;
        float angleB = Mathf.Acos(Vector3.Dot(a, b)) * Mathf.Rad2Deg;
        return PointInCircle(center, p, radius) && angleB <= angleA / 2;
    }

    public static Vector3 GetPosition(string pos)
    {
        string[] strs = pos.Split(',');
        return new Vector3(Convert.ToSingle(strs[0]), Convert.ToSingle(strs[1]), Convert.ToSingle(strs[2]));
    }
}
