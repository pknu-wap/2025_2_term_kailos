using UnityEngine;

// ��ȭ �� �ٿ� �ʿ��� �������� ��� Ŭ����
[System.Serializable] // �� ��Ʈ����Ʈ�� �߰��ؾ� Inspector â���� ���Դϴ�.
public class Sentence
{
    public string characterName; // ĳ���� �̸�

    [TextArea(3, 10)]
    public string text; // ��� ����

    public Sprite characterPortrait; // ĳ���� �ʻ�ȭ (ǥ��)
}

// ��ȭ �̺�Ʈ �ϳ��� ���Ե� Sentence���� �迭�� ��� Ŭ����
[System.Serializable]
public class Dialogue
{
    public Sentence[] sentences;
}