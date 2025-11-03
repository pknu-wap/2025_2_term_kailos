using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Enemy Database", fileName = "EnemyDatabase")]
public class EnemyDatabaseSO : ScriptableObject
{
    public List<EnemySO> enemies = new();

    private Dictionary<string, EnemySO> map;

    void OnEnable()
    {
        map = new Dictionary<string, EnemySO>();
        foreach (var e in enemies)
        {
            if (!e) continue;
            var key = e.enemyId ?? "";
            if (key.Length == 0) continue;
            if (!map.ContainsKey(key))
                map.Add(key, e);
        }
    }

    public EnemySO GetById(string id)
    {
        if (map == null || string.IsNullOrEmpty(id)) return null;
        return map.TryGetValue(id, out var so) ? so : null;
    }
}
