using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 적 이름(예: "Enemy1")을 받아 해당 타입 컴포넌트를 enemyHost에 붙이고,
/// public 필드 기본값을 읽어 스냅샷으로 보관.
/// HPUIBinder와도 연결해 UI에 즉시 반영.
/// </summary>
public class EnemyDataManager : MonoBehaviour
{
    public static EnemyDataManager Instance { get; private set; }

    [Header("Where to attach enemy component (ex: 'none' GameObject)")]
    [SerializeField] private Transform enemyHost;

    [Header("Debug / Fallback")]
    [SerializeField] private string enemyNameOverride = "Enemy1";

    [Header("Runtime")]
    public MonoBehaviour currentEnemyComp;   // Enemy1 등 실제 컴포넌트
    public EnemySnapshot snapshot;           // 읽어온 값 보관

    /// <summary>
    /// TurnManager 등에서 읽기 좋게 노출하는 읽기 전용 프로퍼티(추천).
    /// </summary>
    public MonoBehaviour CurrentEnemyComponent => currentEnemyComp;

    [Serializable]
    public struct EnemySnapshot
    {
        public string enemyName;
        public int maxHP, currentHP, attack, defense, speed;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // 1) 적 이름 결정
        string enemyName = SceneLoadContext.Instance != null
            ? SceneLoadContext.Instance.pendingEnemyName
            : null;
        if (string.IsNullOrWhiteSpace(enemyName))
            enemyName = enemyNameOverride;

        // 2) 호스트가 비었으면 이름으로 찾아보기
        if (enemyHost == null)
        {
            var found = GameObject.Find("none");
            if (found != null) enemyHost = found.transform;
        }
        if (enemyHost == null)
        {
            Debug.LogError("[EnemyDataManager] enemyHost가 없습니다. 'none' 같은 빈 오브젝트를 지정하세요.");
            return;
        }

        // 3) 적 컴포넌트 부착 + 스냅샷 읽기
        AttachAndRead(enemyName);

        // 4) HPUIBinder 연결
        var hp = FindObjectOfType<HPUIBinder>();
        if (hp != null)
        {
            // PlayerDataRuntime에서 플레이어 데이터 가져와 바인딩(있으면)
            var pdr = FindObjectOfType<PlayerDataRuntime>();
            if (pdr != null) hp.BindPlayer(pdr.Data);

            if (currentEnemyComp != null) hp.BindEnemy(currentEnemyComp);
            hp.Refresh();
        }
        else
        {
            Debug.LogWarning("[EnemyDataManager] HPUIBinder를 씬에서 찾지 못했습니다.");
        }
    }

    /// <summary>기존 적 컴포넌트를 제거하고 새로운 타입을 붙인 뒤 public 필드 기본값을 읽어온다.</summary>
    public void AttachAndRead(string enemyTypeName)
    {
        // 이전 것 제거
        if (currentEnemyComp != null)
        {
            Destroy(currentEnemyComp);
            currentEnemyComp = null;
        }

        // 타입 찾기 (대소문자 무시)
        var asm = typeof(EnemyDataManager).Assembly;
        var type = asm.GetTypes()
            .FirstOrDefault(t =>
                typeof(MonoBehaviour).IsAssignableFrom(t) &&
                string.Equals(t.Name, enemyTypeName, StringComparison.OrdinalIgnoreCase));

        if (type == null)
        {
            Debug.LogError($"[EnemyDataManager] 타입 '{enemyTypeName}'을(를) 찾지 못했습니다. (Enemy1 같은 클래스명 필요)");
            return;
        }

        // 컴포넌트 부착
        var comp = enemyHost.gameObject.AddComponent(type) as MonoBehaviour;
        currentEnemyComp = comp;

        // public 필드 읽기
        snapshot.enemyName = type.Name;
        snapshot.maxHP = GetIntField(type, comp, "maxHP", 1);
        snapshot.currentHP = GetIntField(type, comp, "currentHP", snapshot.maxHP);
        snapshot.attack = GetIntField(type, comp, "attack", 0);
        snapshot.defense = GetIntField(type, comp, "defense", 0);
        snapshot.speed = GetIntField(type, comp, "speed", 0);

        Debug.Log($"[EnemyDataManager] Attached {type.Name} @ {enemyHost.name} | HP {snapshot.currentHP}/{snapshot.maxHP}, ATK {snapshot.attack}, DEF {snapshot.defense}, SPD {snapshot.speed}");
    }

    int GetIntField(Type t, object inst, string fieldName, int fallback)
    {
        var f = t.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (f == null) { Debug.LogWarning($"[EnemyDataManager] {t.Name}에 '{fieldName}' 필드가 없습니다."); return fallback; }
        try { return Mathf.Max(0, (int)f.GetValue(inst)); }
        catch { return fallback; }
    }
}
