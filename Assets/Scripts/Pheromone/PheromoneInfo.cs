using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// ある特定のアリに着目したときの、周辺のフェロモンの情報
[Serializable]
public class PheromoneInfo {
  [SerializeField]
  private float alpha = 0.96f;
  public float Alpha => alpha;

  [SerializeField]
  private float r = 1.0f;
  public float R => r;

  [SerializeField]
  private int threshold = 1;
  public int Threshold => threshold;

  // アリの周辺のフェロモンの数
  public int Count { get; set; }
  public Vector3 Direction { get; set; }

  public void NormalizeDirection() { Direction = Direction.normalized; }

  public void Reset() {
    Count = 0;
    Direction = Vector3.zero;
  }
}
