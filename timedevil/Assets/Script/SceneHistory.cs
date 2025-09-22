using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneHistory
{
    private static Stack<string> history = new Stack<string>();

    /// <summary>현재 씬을 스택에 기록</summary>
    public static void PushCurrent()
    {
        var current = SceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(current)) return;

        // 같은 씬 연속 기록 방지
        if (history.Count == 0 || history.Peek() != current)
            history.Push(current);
    }

    /// <summary>이전 씬 이름 반환 (없으면 null)</summary>
    public static string PopPrevious()
    {
        if (history.Count == 0) return null;

        history.Pop(); // 현재 씬 버리기
        return (history.Count > 0) ? history.Pop() : null;
    }

    public static void Clear()
    {
        history.Clear();
    }
}
