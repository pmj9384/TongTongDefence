using UnityEngine;

// 타입 스프라이트 적용만 담당 — Monster(HP)/MonsterMover(이동)와 책임 분리
public class MonsterVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer blockRenderer;
    [SerializeField] private SpriteRenderer bodyRenderer;

    public void Apply(MonsterTypeData type)
    {
        blockRenderer.sprite = type.blockSprite;
        bodyRenderer.sprite = type.bodySprite;
    }
}
