using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// 제출용 APK 원버튼 빌드 — Player Settings(패키지명/세로 고정)까지 코드가 보장해
// "설정 빠뜨린 빌드"가 나올 수 없게 한다. 산출물: Builds/TongTongDefence.apk
public static class BuildScript
{
    private const string OutputPath = "Builds/TongTongDefence.apk";

    [MenuItem("Tools/Build Android APK")]
    public static void BuildAndroid()
    {
        // 제출 요건 설정 (빌드마다 강제 — Inspector 상태에 의존하지 않음)
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.pmj.tongtongdefence");
        PlayerSettings.productName = "TongTongDefence";   // 런처 표시 이름 (유저 지정: 영문)
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;   // 세로 전용 게임
        // Mono + ARMv7: NDK(IL2CPP) 미설치 환경에서도 확실히 빌드되는 조합 — 과제 실행 검증엔 충분
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.Mono2x);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;

        Directory.CreateDirectory("Builds");
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = OutputPath,
            target = BuildTarget.Android,      // 플랫폼 스위치 포함 (첫 실행은 리임포트로 수 분)
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"[BuildScript] APK 빌드 성공 → {OutputPath} ({report.summary.totalSize / (1024 * 1024)}MB)");
        else
            Debug.LogError($"[BuildScript] 빌드 실패: {report.summary.result}");
    }
}
