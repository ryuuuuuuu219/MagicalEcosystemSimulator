using UnityEngine;

[DisallowMultipleComponent]
public class FreeFlyCamera : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 10f;          // WASD / E/C の基本速度 (m/s)
    public float sprintMultiplier = 3f;    // Shiftで加速
    public float scrollSpeed = 40f;        // ホイール前後 (m/s 相当)
    public bool useUnscaledTime = false;   // Time.timeScaleの影響を受けない

    [Header("Look")]
    public float lookSensitivity = 2.0f;   // マウス感度
    public bool holdRightMouseToLook = true; // 右クリック押下中のみ回転
    public float pitchMin = -89f;
    public float pitchMax = 89f;

    [Header("Cursor")]
    public bool lockCursorWhileLooking = true;

    float yaw;
    float pitch;

    void Awake()
    {
        // 初期角度を現在の回転から取得
        var e = transform.rotation.eulerAngles;
        yaw = e.y;
        pitch = e.x;

        WorldGenerator wg = worldgen.GetComponent<WorldGenerator>();

        transform.position = new Vector3(
            wg.terrainSize / 2f,
            wg.heightScale + groundOffset + 1f, // 地面から少し上にスタート
            wg.terrainSize / 2f);
    }

    void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        bool looking = !holdRightMouseToLook || Input.GetMouseButton(1);
        HandleLook(looking);
        HandleMove(dt, looking);
        HandleScroll(dt);
    }

    public GameObject worldgen;
    public float groundOffset = 1.0f; // 地面からの高さオフセット

    void LateUpdate()
    {
        PreventClipping();
    }
    void PreventClipping()
    {
        if (worldgen == null) return;
        var terrain = worldgen.GetComponent<WorldGenerator>().terrain;
        if (terrain == null) return;

        Vector3 pos = transform.position;

        float terrainHeight = terrain.SampleHeight(pos) + terrain.transform.position.y;

        float minHeight = terrainHeight + groundOffset;

        if (pos.y < minHeight)
        {
            pos.y = minHeight;
            transform.position = pos;
        }
    }

    void HandleLook(bool looking)
    {
        if (!looking)
        {
            if (lockCursorWhileLooking) UnlockCursor();
            return;
        }

        // 右クリック開始フレームは回転しない
        if (holdRightMouseToLook && Input.GetMouseButtonDown(1))
        {
            if (lockCursorWhileLooking) LockCursor();
            return;
        }

        if (lockCursorWhileLooking) LockCursor();

        float mx = Input.GetAxis("Mouse X") * lookSensitivity;
        float my = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMove(float dt, bool looking)
    {

        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A)) h -= 1f;
        if (Input.GetKey(KeyCode.D)) h += 1f;
        if (Input.GetKey(KeyCode.S)) v -= 1f;
        if (Input.GetKey(KeyCode.W)) v += 1f;

        float up = 0f;
        if (Input.GetKey(KeyCode.E)) up += 1f;
        if (Input.GetKey(KeyCode.C)) up -= 1f;

        Vector3 currentforward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
        Vector3 currentright = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;

        Vector3 move = (currentright * h) + (currentforward * v) + (Vector3.up * up);
        if (move.sqrMagnitude > 1e-6f) move.Normalize();

        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            speed *= sprintMultiplier;

        transform.position += move * speed * dt;
    }

    void HandleScroll(float dt)
    {
        float scroll = Input.mouseScrollDelta.y; // ホイール上: +1, 下: -1（環境により差はある）
        if (Mathf.Abs(scroll) < 0.0001f) return;

        float speed = scrollSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            speed *= sprintMultiplier;

        transform.position += transform.forward * (scroll * speed * dt);
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnDisable()
    {
        // スクリプト無効化時にカーソルを戻す
        UnlockCursor();
    }
    public void SyncRotationFromTransform()
    {
        var e = transform.rotation.eulerAngles;
        yaw = e.y;
        pitch = e.x;
    }
}
