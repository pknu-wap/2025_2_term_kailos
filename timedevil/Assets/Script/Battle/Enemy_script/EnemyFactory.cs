using System;
using System.Linq;
using UnityEngine;

public static class EnemyFactory
{
    public static MonoBehaviour AttachEnemyByName(GameObject host, string enemyName)
    {
        if (!host) { Debug.LogError("[EnemyFactory] host is null"); return null; }
        if (string.IsNullOrEmpty(enemyName)) { Debug.LogError("[EnemyFactory] enemyName empty"); return null; }

        var type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => {
                try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
            })
            .FirstOrDefault(t =>
                typeof(MonoBehaviour).IsAssignableFrom(t) &&
                string.Equals(t.Name, enemyName, StringComparison.OrdinalIgnoreCase));

        if (type == null)
        {
            Debug.LogError($"[EnemyFactory] Enemy type '{enemyName}' not found");
            return null;
        }

        var existing = host.GetComponent(type) as MonoBehaviour;
        if (existing) return existing;

        var added = host.AddComponent(type) as MonoBehaviour;
        return added;
    }
}
