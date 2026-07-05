using System;
using UnityEngine;

// 몬스터 종류 정의 — 데이터만. MVP: 점유는 전부 1×1 (멀티셀은 가산점 이월)
[Serializable]
public class MonsterTypeData
{
    public string typeName;
    public Sprite bodySprite;
    public Sprite blockSprite;
    public float hpMultiplier = 1f;   // 웨이브 기본 HP에 곱해짐 (종류별 차등용, 관찰 미확정이라 기본 1)
}
