using System;
using System.Collections.Generic;

[Serializable]
public class CardSaveData
{
    // ���� ī�� ID�� (��: "Card1", "Card2")
    public List<string> owned = new List<string>();

    // ���� �� �� (���� ����)
    public List<string> deck = new List<string>();
}
