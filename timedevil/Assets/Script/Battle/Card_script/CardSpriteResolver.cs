using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TimeDevil/Card Sprite Resolver")]
public class CardSpriteResolver : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string cardId;          // ��: "Card1"
        public Sprite sprite;          // ���� �����ϸ� �̰� ���
        public string resourcesPath;   // ����θ� "my_asset/<cardId>"�� �⺻ ��η� �õ�
    }

    [SerializeField] private List<Entry> table = new List<Entry>();

    private Dictionary<string, Sprite> map;

    void OnEnable()
    {
        BuildMap();
    }

    private void BuildMap()
    {
        map = new Dictionary<string, Sprite>();
        foreach (var e in table)
        {
            if (!string.IsNullOrEmpty(e.cardId) && e.sprite != null)
                map[e.cardId] = e.sprite;
        }
    }

    public Sprite GetSprite(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return null;

        // 1) �ν����Ϳ� ���� ���εǾ� ������ ���
        if (map != null && map.TryGetValue(cardId, out var sp) && sp != null)
            return sp;

        // 2) ����: Resources ���� �ε� (�⺻: my_asset/Card1)
        string path = $"my_asset/{cardId}";
        var loaded = Resources.Load<Sprite>(path);
        return loaded;
    }
}
