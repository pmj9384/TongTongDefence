using System;
using UnityEngine;

// 데미지 플로터 — 자기 연출(떠오르며 페이드)만 담당하는 벙어리 팝업.
// 생성/풀링/배치는 DamagePopupSpawner 몫 (표준 3층: 팝업 ← 스포너 ← 이벤트 소스)
[RequireComponent(typeof(TextMesh))]
public class DamagePopup : MonoBehaviour
{
    public event Action<DamagePopup> OnFinished;   // 풀 반환 신호

    private const float Lifetime = 0.6f;
    private const float RiseSpeed = 1.2f;

    private TextMesh text;
    private float timer;

    private void Awake() => text = GetComponent<TextMesh>();

    public void Show(Vector3 position, int damage, bool isCritical)
    {
        transform.position = position;
        timer = Lifetime;

        text.text = damage.ToString();
        text.color = isCritical ? new Color(1f, 0.3f, 0.25f) : Color.white;
        transform.localScale = Vector3.one * (isCritical ? 1.4f : 1f);   // 치명타 강조
    }

    private void Update()
    {
        transform.position += Vector3.up * (RiseSpeed * Time.deltaTime);

        timer -= Time.deltaTime;
        Color c = text.color;
        c.a = Mathf.Clamp01(timer / Lifetime * 2f);   // 후반부에 빠르게 사라짐
        text.color = c;

        if (timer <= 0f)
            OnFinished?.Invoke(this);
    }
}
