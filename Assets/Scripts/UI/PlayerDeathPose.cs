using UnityEngine;

// 죽음 연출 — GameOver 진입 시 슈터 캐릭터를 옆으로 쓰러뜨리고 어둡게 (원작: 죽는 모션 후 결과 흐름).
// 죽는 스프라이트가 없어 통짜 회전+틴트로 대체. 재시작 = 씬 리로드라 복원 불필요.
public class PlayerDeathPose : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform visual;   // Shooter의 캐릭터 스프라이트 자식

    private void Start()
    {
        gameManager.AddGameStateEnterAction(GameManager.GameState.GameOver, Fall);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.RemoveGameStateEnterAction(GameManager.GameState.GameOver, Fall);
    }

    private void Fall()
    {
        visual.localRotation = Quaternion.Euler(0f, 0f, 90f);   // 픽 — 옆으로 쓰러짐
        foreach (SpriteRenderer sr in visual.GetComponentsInChildren<SpriteRenderer>())
            sr.color = new Color(0.5f, 0.45f, 0.45f);            // 죽은 톤
    }
}
