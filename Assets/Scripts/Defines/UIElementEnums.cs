// 순서 = GameUIManager.uiElements 리스트 인덱스 (Generate 버튼이 씬 스캔으로 재생성).
// 실물 없는 멤버(Pause/Settings)는 #9에서 패널 추가 예정 — 호출되면 가드가 경고만 찍음
public enum UIElementEnums
{
	ResultPanel,      // 성공/실패 공용 결과 팝업 (씬 등록 인덱스 0)
	PausePanel,
	SettingsPanel,
}
