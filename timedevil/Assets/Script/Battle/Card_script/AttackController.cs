using System.Collections;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    public IEnumerator Execute(AttackCardSO so, Faction self, Faction foe)
    {
        Debug.Log($"[AttackController] Execute: id={so.id}, name={so.displayName}, power={so.power}, self={self}, foe={foe}");
        // TODO: 패턴/타임라인/히트 처리 + 이펙트/사운드
        yield return null;
    }
}
