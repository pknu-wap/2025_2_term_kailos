using System;
using UnityEngine;

public class EnemyTurn : MonoBehaviour
{
    /// <summary>
    /// ���� �ܼ��� ������ �� �ϡ�: �α׸� ��� ��� �Ϸ� �ݹ� ȣ��.
    /// ���߿� �� ī�� ���/�̵�/���� ���� ���⿡ �߰��ϸ� ��.
    /// </summary>
    public void RunOnceImmediate(Action onDone)
    {
        Debug.Log("������Դϴ�!");
        onDone?.Invoke();
    }
}
