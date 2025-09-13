using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{
    // ===== Public =====
    public float Speed;
    public GameManager manager;
    public NextScene Next_Scene;
    public GetManager get_manager;

    // ===== Private =====
    Rigidbody2D rigid;
    Animator anim;

    float h;
    float v;
    bool isHorizonMove;

    Vector3 dirVec;
    GameObject scanObject;

    // ===== Unity =====
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // 매 프레임 호출
    void Update()
    {
        // 입력 (액션 중에는 입력 무시)
        h = manager.isAction ? 0 : Input.GetAxisRaw("Horizontal"); // 수평
        v = manager.isAction ? 0 : Input.GetAxisRaw("Vertical");   // 수직

        bool hDown = manager.isAction ? false : Input.GetButtonDown("Horizontal");
        bool vDown = manager.isAction ? false : Input.GetButtonDown("Vertical");
        bool hUp = manager.isAction ? false : Input.GetButtonUp("Horizontal");
        bool vUp = manager.isAction ? false : Input.GetButtonUp("Vertical");

        // 애니메이션 전환 기준 (가로/세로 우선)
        if (hDown)
        {
            isHorizonMove = true;
        }
        else if (vDown)
        {
            isHorizonMove = false;
        }
        else if (hUp || vUp)
        {
            isHorizonMove = h != 0;
        }

        if (anim.GetInteger("hAxisRaw") != h)
        {
            anim.SetBool("isChange", true);
            anim.SetInteger("hAxisRaw", (int)h);
        }
        else if (anim.GetInteger("vAxisRaw") != v)
        {
            anim.SetBool("isChange", true);
            anim.SetInteger("vAxisRaw", (int)v);
        }
        else
        {
            anim.SetBool("isChange", false);
        }

        // 바라보는 방향 갱신
        if (vDown && v == 1)
        {
            dirVec = Vector3.up;
        }
        else if (vDown && v == -1)
        {
            dirVec = Vector3.down;
        }
        else if (hDown && h == 1)
        {
            dirVec = Vector3.right;
        }
        else if (hDown && h == -1)
        {
            dirVec = Vector3.left;
        }

        // 상호작용
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (scanObject != null && scanObject.layer == LayerMask.NameToLayer("teleport"))
            {
                if (Next_Scene != null)
                    Next_Scene.LoadBattleScene(scanObject);
            }
            else if (scanObject != null && scanObject.layer == LayerMask.NameToLayer("item_get"))
            {
                if (manager != null)
                    get_manager.Action(scanObject);
            }
            else
            {
                if (manager != null)
                    manager.Action(scanObject);
            }
        }
    }

    // 물리 업데이트
    void FixedUpdate()
    {
        // 이동
        rigid.velocity = new Vector2(h, v).normalized * Speed;
        Vector2 moveVec = new Vector2(h, v).normalized;

        // 앞 방향 레이캐스트
        Debug.DrawRay(rigid.position, dirVec * 0.5f, new Color(0, 1, 0));

        RaycastHit2D rayhit = Physics2D.Raycast(
            rigid.position,
            dirVec,
            0.7f,
            LayerMask.GetMask("Object", "teleport", "item_get")
        );

        if (rayhit.collider != null)
        {
            scanObject = rayhit.collider.gameObject;
            // Debug.Log($"Raycast hit: {scanObject.name} (Layer: {LayerMask.LayerToName(scanObject.layer)})");
        }
        else
        {
            scanObject = null;
        }
    }
}
