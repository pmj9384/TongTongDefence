using UnityEngine;
using UnityEngine.AddressableAssets;

// 장착 스킨을 파일럿 3파츠에 반영 (MonsterVisual 패턴 — 시각은 컴포넌트가, 데이터는 SkinUserData가).
// 스킨 에셋 = Addressables (유저 결정 2026-07-15: 스킨만 전환 — "늘어나면 원격 배포될 카테고리"라 명분 있는
// 영역만. 사운드·테이블 등은 Resources 유지). 어드레스 규약: Skins/{id}_head|_body|_weapon, 아이콘 Skins/{id}.
// 로컬 소량 + 씬 시작 1회 로드라 WaitForCompletion(동기)로 단순하게 [비동기 전환 여지].
public class PilotSkinApplier : MonoBehaviour
{
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer weaponRenderer;

    private void Start()   // 매니저 Initialize 완료 후 (관례) — GameDataManager는 접근 시 자동 생성·로드
    {
        string id = GameDataManager.Instance.SkinUserData.EquippedSkinId;
        if (string.IsNullOrEmpty(id)) return;

        Apply(headRenderer, $"Skins/{id}_head");
        Apply(bodyRenderer, $"Skins/{id}_body");
        Apply(weaponRenderer, $"Skins/{id}_weapon");
    }

    private static void Apply(SpriteRenderer renderer, string address)
    {
        if (renderer == null) return;
        var sprite = SkinSprites.Load(address);
        if (sprite != null) renderer.sprite = sprite;   // 없으면 기존(기본 파일럿) 유지
    }
}

// 스킨 스프라이트 로딩의 단일 지점 — Resources→Addressables 전환이 이 안에서 끝났듯,
// 원격 카탈로그/비동기로 갈 때도 여기만 바뀐다 (호출부 3곳: 파츠·아이콘·장착 아이콘)
public static class SkinSprites
{
    public static Sprite Load(string address)
    {
        try { return Addressables.LoadAssetAsync<Sprite>(address).WaitForCompletion(); }
        catch { return null; }   // 미등록 어드레스 — 호출부가 폴백 처리
    }
}
