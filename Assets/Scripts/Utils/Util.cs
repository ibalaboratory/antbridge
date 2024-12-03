using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 角度の変更を行うためのスクリプト。
public static class Util {
  // radianからdegreeに変更
  public static float ConvertToEuler(float angle) {
    return angle * 180.0f / (float)Math.PI;
  }

  // degreeからradianに変更
  public static float ConvertToRad(float angle) {
    return angle * (float)Math.PI / 180.0f;
  }
}
