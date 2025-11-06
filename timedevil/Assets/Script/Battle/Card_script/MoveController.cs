using System.Collections;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    public IEnumerator Execute(MoveCardSO so, Faction self, Faction foe)
    {
        Debug.Log($"[MoveController] Execute: id={so.id}, name={so.displayName}, " +
                  $"mode={so.moveMode}, dir={so.where}, amount={so.amount}, self={self}, foe={foe}");

        if (so.moveMode == MoveMode.UpMove)
        {
            // 자신 이동
            Debug.Log($"[MoveController] Move SELF {so.where} by {so.amount} (stub).");
            // TODO: 그리드에서 self의 위치를 so.where로 so.amount칸 이동
        }
        else // AntiMove
        {
            // 상대 이동
            Debug.Log($"[MoveController] Move FOE {so.where} by {so.amount} (stub).");
            // TODO: 그리드에서 foe의 위치를 so.where로 so.amount칸 이동
        }

        yield return null;
    }
}
