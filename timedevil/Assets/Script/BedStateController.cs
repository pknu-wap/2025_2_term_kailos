using UnityEngine;

public class BedStateController : MonoBehaviour, IInteractable
{
    [Header("���� ������Ʈ ����")]
    public GameObject normalStateObject;    // '���� ħ��' ������Ʈ
    public GameObject lyingDownStateObject; // '�����ִ� ħ��' ������Ʈ

    private bool isLyingDown = false;
    private SpriteRenderer playerSprite;

    public void Interact()
    {
        if (playerSprite == null)
        {
            PlayerAction player = FindObjectOfType<PlayerAction>();
            if (player != null)
                playerSprite = player.GetComponent<SpriteRenderer>();
            else return;
        }

        if (!isLyingDown)
        {
            playerSprite.enabled = false;
            normalStateObject.SetActive(false);
            lyingDownStateObject.SetActive(true);
            isLyingDown = true;
        }
        else
        {
            playerSprite.enabled = true;
            normalStateObject.SetActive(true);
            lyingDownStateObject.SetActive(false);
            isLyingDown = false;
        }
    }
}