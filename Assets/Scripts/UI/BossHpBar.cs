using UnityEngine;
using UnityEngine.UI;

// 화면 상단 보스 전용 HP바 (AnimalBreakOut BossHpSlider 패턴 관찰 2026-07-15: 스폰 시 표시 →
// HP 이벤트로 갱신 → 사망/게임오버 시 숨김. 경고 배너 없음 — 레퍼런스 스코프 그대로).
// 진행도 슬라이더와 같은 자리 오버레이 — 보스 동안 BossProgress가 0이라 겹쳐도 정보 손실 없음 [가정].
// 레퍼런스의 static 이벤트 대신 프로젝트 관례(GameManager.MonsterManager 경유 C# 이벤트)로 배선.
public class BossHpBar : UIElement
{
    [SerializeField] private Slider hpSlider;

    private Monster boss;   // 표시 중 보스 — 구독 해제용

    public override void Initialize()
    {
        gameManager.MonsterManager.OnBossSpawned += HandleBossSpawned;
        // 보스 생존 중 게임오버(바닥 지속공격 패배) → 필드 정리로 OnDied 없이 사라지므로 여기서 숨김
        gameManager.AddGameStateEnterAction(GameManager.GameState.GameOver, Hide);
        gameObject.SetActive(false);
    }

    private void OnDestroy()   // 씬 언로드(재시작) — 매니저 쪽 구독 잔류 방지
    {
        if (gameManager == null) return;
        if (gameManager.MonsterManager != null)
            gameManager.MonsterManager.OnBossSpawned -= HandleBossSpawned;
        gameManager.RemoveGameStateEnterAction(GameManager.GameState.GameOver, Hide);
    }

    private void HandleBossSpawned(Monster monster)
    {
        boss = monster;
        boss.OnHpChanged += HandleHpChanged;   // 구독/해제 쌍: Hide()에서 해제
        boss.OnDied += HandleBossDied;
        hpSlider.value = 1f;
        gameObject.SetActive(true);
    }

    private void HandleHpChanged(int current, int max) => hpSlider.value = (float)current / max;

    private void HandleBossDied(Monster _) => Hide();

    public override void Hide()
    {
        if (boss != null)
        {
            boss.OnHpChanged -= HandleHpChanged;
            boss.OnDied -= HandleBossDied;
            boss = null;
        }
        gameObject.SetActive(false);
    }
}
