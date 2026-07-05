using UnityEngine;

// 코드 생성 UI(체력바 등)용 최소 스프라이트 유틸 — 에셋 의존 없이 색/스케일만으로 바를 그린다
public static class RuntimeSprites
{
    private static Sprite white;

    // 1×1 월드유닛 흰색 스프라이트 (캐시)
    public static Sprite White
    {
        get
        {
            if (white != null) return white;

            var tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return white;
        }
    }
}
