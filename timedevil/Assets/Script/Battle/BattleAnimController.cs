using UnityEngine;
using System.Collections;

public class BattleAnimController : MonoBehaviour
{
    [Header("이동 설정")]
    public float tileSize = 1f;         // 한 칸 크기
    public float moveDuration = 0.15f;  // 한 칸 이동 시간

    [Header("Idle Sprites (정지 그림)")]
    public Sprite idleDown;
    public Sprite idleLeft;
    public Sprite idleRight;
    public Sprite idleUp;

    [Header("Animator State Names (걷기 애니메이션 이름)")]
    public string walkDownState = "Walk_Down";
    public string walkLeftState = "Walk_Left";
    public string walkRightState = "Walk_Right";
    public string walkUpState = "Walk_Up";

    [Header("보드 경계 (선택)")]
    public Transform gridOrigin;         // (0,0) 기준점
    public Vector2Int minGrid = new Vector2Int(0, 0);
    public Vector2Int maxGrid = new Vector2Int(3, 3);

    private Animator anim;
    private SpriteRenderer sr;
    private bool isMoving = false;

    // 0=Down, 1=Left, 2=Right, 3=Up
    private int lastDir = 0; // 시작 시 아래쪽 본다고 가정

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // 시작은 Idle 상태 (움직이지 않음)
        SetAnimatorActive(false);
        SetIdleSprite(lastDir);
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.UpArrow)) TryMove(Vector2Int.up, 3);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) TryMove(Vector2Int.down, 0);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) TryMove(Vector2Int.left, 1);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) TryMove(Vector2Int.right, 2);
    }

    void TryMove(Vector2Int dir, int dirNum)
    {
        Vector3 from = transform.position;
        Vector3 to = from + new Vector3(dir.x * tileSize, dir.y * tileSize, 0);

        // 경계 설정이 있다면 넘어가지 않게 막기
        if (gridOrigin != null)
        {
            Vector3 local = transform.position - gridOrigin.position;
            Vector2Int current = new Vector2Int(
                Mathf.RoundToInt(local.x / tileSize),
                Mathf.RoundToInt(local.y / tileSize)
            );
            Vector2Int next = current + dir;

            if (next.x < minGrid.x || next.y < minGrid.y || next.x > maxGrid.x || next.y > maxGrid.y)
            {
                // 이동 못하면 바라보는 방향만 변경
                lastDir = dirNum;
                SetAnimatorActive(false);
                SetIdleSprite(lastDir);
                return;
            }
        }

        StartCoroutine(MoveRoutine(from, to, dirNum));
    }

    IEnumerator MoveRoutine(Vector3 from, Vector3 to, int dirNum)
    {
        isMoving = true;
        lastDir = dirNum;

        // 이동 시작 → 걷기 애니메이션 재생
        SetAnimatorActive(true);
        PlayWalk(dirNum);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }

        // 이동 종료 → 애니메이터 끄고 Idle 스프라이트로 고정
        SetAnimatorActive(false);
        SetIdleSprite(lastDir);

        isMoving = false;
    }

    void PlayWalk(int dirNum)
    {
        if (!anim) return;

        switch (dirNum)
        {
            case 0: anim.Play(walkDownState, 0, 0f); break;
            case 1: anim.Play(walkLeftState, 0, 0f); break;
            case 2: anim.Play(walkRightState, 0, 0f); break;
            case 3: anim.Play(walkUpState, 0, 0f); break;
        }
    }

    void SetIdleSprite(int dirNum)
    {
        if (!sr) return;
        sr.sprite =
            dirNum == 0 ? idleDown :
            dirNum == 1 ? idleLeft :
            dirNum == 2 ? idleRight :
                          idleUp;
    }

    void SetAnimatorActive(bool on)
    {
        if (!anim) return;
        anim.enabled = on;
        if (on)
        {
            anim.Rebind();
            anim.Update(0f);
        }
    }
}
