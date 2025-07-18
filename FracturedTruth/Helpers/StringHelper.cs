﻿using System.Text;
using UnityEngine;

namespace FracturedTruth.Helpers;

public static class StringHelper
{
    public static readonly Encoding shiftJIS = CodePagesEncodingProvider.Instance.GetEncoding("Shift_JIS");

    /// <summary>给字符串添加荧光笔样式的装饰</summary>
    /// <param name="self">字符串</param>
    /// <param name="color">原始颜色，将自动转换为半透明的荧光色</param>
    /// <param name="bright">是否设置为最大亮度。如果想要保持较暗的颜色不变，则设置为false</param>
    /// <returns>标记后的字符串</returns>
    public static string Mark(this string self, Color color, bool bright = true)
    {
        var markingColor = color.ToMarkingColor(bright);
        var markingColorCode = ColorUtility.ToHtmlStringRGBA(markingColor);
        return $"<mark=#{markingColorCode}>{self}</mark>";
    }
    /// <summary>
    /// 计算使用SJIS编码时的字节数
    /// </summary>
    public static int GetByteCount(this string self) => shiftJIS.GetByteCount(self);
}