using System.Collections;
using UnityEngine;

public class DrawController : MonoBehaviour
{
    public IEnumerator Execute(DrawCardSO so, Faction self, Faction foe)
    {
        Debug.Log($"[DrawController] Execute: id={so.id}, name={so.displayName}, " +
                  $"mode={so.drawMode}, amount={so.amount}, self={self}, foe={foe}");

        // 간단한 샘플 동작 (나중에 실제 규칙으로 교체)
        var deck = BattleDeckRuntime.Instance;
        if (deck == null)
        {
            Debug.LogWarning("[DrawController] BattleDeckRuntime is null.");
            yield break;
        }

        if (so.drawMode == DrawMode.UpDraw)
        {
            // 자신이 드로우
            if (self == Faction.Player)
            {
                deck.Draw(so.amount);
                Debug.Log($"[DrawController] Player draws {so.amount}.");
            }
            else
            {
                // TODO: 적 드로우 구현 (적 덱/손패 시스템 연결)
                Debug.Log($"[DrawController] Enemy would draw {so.amount} (stub).");
            }
        }
        else // AntiDraw
        {
            // 상대 손패에서 랜덤으로 버리기(아직 미구현: 디버그만)
            Debug.Log($"[DrawController] Force discard foe's {so.amount} card(s) (stub).");
            // TODO: 상대 손패 시스템 연결해서 실제 버리기 구현
        }

        yield return null;
    }
}
