using System;
using UnityEngine;

public class EnemyTurn : MonoBehaviour
{
    /// <summary>
    /// 아주 단순한 “더미 적 턴”: 로그만 찍고 즉시 완료 콜백 호출.
    /// 나중에 적 카드 사용/이동/연출 등을 여기에 추가하면 됨.
    /// </summary>
    public void RunOnceImmediate(Action onDone)
    {
        Debug.Log("상대턴입니다!");
        onDone?.Invoke();
    }
}
