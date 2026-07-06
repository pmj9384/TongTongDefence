using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : InGameManager
{
    public List<UIElement> uiElements;

    public override void Initialize()
    {
        base.Initialize();
        foreach (var element in uiElements)
        {
            element.SetUIManager(GameManager, this);
        }

        GameManager.AddGameStateEnterAction(GameManager.GameState.GameStop, () =>
        {
            ShowUIElement(UIElementEnums.PausePanel);
        });

        GameManager.AddGameStateExitAction(GameManager.GameState.GameStop, () =>
        {
            HideUIElement(UIElementEnums.PausePanel);
        });

        // 결과 패널은 성공/실패 공용 — 어느 쪽인지는 패널이 CurrentState로 판별
        GameManager.AddGameStateEnterAction(GameManager.GameState.GameOver, () =>
        {
            ShowUIElement(UIElementEnums.ResultPanel);
        });

        GameManager.AddGameStateEnterAction(GameManager.GameState.GameClear, () =>
        {
            ShowUIElement(UIElementEnums.ResultPanel);
        });
    }

    public void InitializedUIElements()
    {
        foreach (var element in uiElements)
        {
            element.Initialize();
        }
    }

    // enum에는 있으나 씬 리스트에 미등록인 패널(PausePanel 등) 호출 방어 —
    // 실물 없는 유령 구독이 상태 전환 콜스택을 예외로 끊는 실버그가 있었음 (GameOver 크래시)
    public void ShowUIElement(UIElementEnums type)
    {
        if (!TryGetElement(type, out UIElement element)) return;
        element.Show();
    }

    public void HideUIElement(UIElementEnums type)
    {
        if (!TryGetElement(type, out UIElement element)) return;
        element.Hide();
    }

    private bool TryGetElement(UIElementEnums type, out UIElement element)
    {
        int index = (int)type;
        if (index < 0 || index >= uiElements.Count || uiElements[index] == null)
        {
            Debug.LogWarning($"[GameUIManager] {type} 미등록 — uiElements 리스트 확인");
            element = null;
            return false;
        }
        element = uiElements[index];
        return true;
    }

}
