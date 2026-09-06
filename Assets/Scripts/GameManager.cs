using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs (Chỉ cần Hình Vuông 2D, Không Physics)")]
    public GameObject prefabA;
    public GameObject prefabB;
    public GameObject prefabC;

    // Các đối tượng trong game
    private GameObject objA;
    private GameObject objB;
    private List<GameObject> listC = new List<GameObject>();

    [Header("Configs - Object A")]
    public float speedA = 5f;
    public Vector2 dirA = new Vector2(0, 1); // Đi lên

    [Header("Configs - Object B")]
    public float speedB = 3f;
    public Vector2 dirB = new Vector2(-1, 0); // Đi sang trái

    [Header("Configs - Object C")]
    public float speedC = 10f;
    public Vector2 dirC = new Vector2(1, 0); // Đi sang phải

    [Header("Kích thước chung (A & B)")]
    public float objectWidth = 1f;
    public float objectHeight = 1f;

    // Biên màn hình (World Space)
    private float left, right, top, bottom;

    void Start()
    {
        // PHASE 1 - TASK 01: Lấy kích thước màn hình và xác định vùng tọa độ
        Camera cam = Camera.main;
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        
        // Cấu hình biên (theo chuẩn Unity, y dương là bên trên)
        left = cam.transform.position.x - width / 2f;
        right = cam.transform.position.x + width / 2f;
        top = cam.transform.position.y + height / 2f;
        bottom = cam.transform.position.y - height / 2f;

        // PHASE 2 - TASK 02 & TASK 03: Tạo và đặt A, B đúng vị trí ban đầu
        
        // Tạo A
        objA = Instantiate(prefabA);
        objA.name = "Object A";
        objA.transform.localScale = new Vector3(objectWidth, objectHeight, 1);
        // A nằm giữa biên trái (TASK 03 - Phương án 1)
        objA.transform.position = new Vector3(left + objectWidth / 2f, 0, 0);

        // Tạo B
        objB = Instantiate(prefabB);
        objB.name = "Object B";
        objB.transform.localScale = new Vector3(objectWidth, objectHeight, 1);
        // B nằm giữa biên phải (đối diện A)
        objB.transform.position = new Vector3(right - objectWidth / 2f, 0, 0);
        
        // Kích hoạt hiển thị
        objA.SetActive(true);
        objB.SetActive(true);
    }

    void Update()
    {
        // PHASE 5 - TASK 16: Thứ tự xử lý mỗi frame
        
        // 1. Đọc input (TASK 09)
        // Click hoặc Touch
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            SpawnC();
        }

        // 2. Cập nhật A (TASK 04)
        if (objA != null)
        {
            objA.transform.position += (Vector3)dirA * speedA * Time.deltaTime;
            
            // Tùy chọn: Chặn A lại ở biên để A không bay mất (Không có trong đề, nhưng giúp dễ test)
            if (objA.transform.position.y + objectHeight / 2f >= top) dirA.y = -1;
            if (objA.transform.position.y - objectHeight / 2f <= bottom) dirA.y = 1;
        }

        // 3. Cập nhật B (TASK 05)
        if (objB != null)
        {
            objB.transform.position += (Vector3)dirB * speedB * Time.deltaTime;

            // 4. Kiểm tra B chạm biên (PHASE 4 - TASK 10)
            CheckBoundariesB();
        }

        // 5. Cập nhật C (PHASE 3 - TASK 08)
        for (int i = listC.Count - 1; i >= 0; i--)
        {
            if (listC[i] != null)
            {
                listC[i].transform.position += (Vector3)dirC * speedC * Time.deltaTime;
                
                // Hủy C nếu ra khỏi màn hình (tránh leak bộ nhớ)
                if (listC[i].transform.position.x > right + 2f)
                {
                    Destroy(listC[i]);
                    listC.RemoveAt(i);
                }
            }
        }
        
        // 6, 7, 8. Unity tự động lo phần vẽ (Render) cuối frame
    }

    // PHASE 3 - TASK 06 & TASK 07: C xuất hiện từ A
    void SpawnC()
    {
        if (objA == null) return;

        GameObject c = Instantiate(prefabC);
        c.name = "Object C";
        // C nhỏ hơn A/B một chút cho dễ phân biệt
        c.transform.localScale = new Vector3(objectWidth * 0.4f, objectHeight * 0.4f, 1);
        
        // Vị trí xuất hiện bắt nguồn từ A (TASK 07)
        c.transform.position = objA.transform.position;
        c.SetActive(true);
        
        listC.Add(c);
    }

    // PHASE 4 - TASK 10, 11, 12, 13
    void CheckBoundariesB()
    {
        Vector3 posB = objB.transform.position;
        bool changed = false;

        // TASK 11: B chạm biên trái -> Sang phải, Y random
        if (posB.x - objectWidth / 2f <= left)
        {
            posB.x = right - objectWidth / 2f;
            posB.y = Random.Range(bottom + objectHeight / 2f, top - objectHeight / 2f); // TASK 13
            changed = true;
        }
        // TASK 12: B chạm biên trên -> Xuống dưới, X random
        else if (posB.y + objectHeight / 2f >= top)
        {
            posB.y = bottom + objectHeight / 2f;
            posB.x = Random.Range(left + objectWidth / 2f, right - objectWidth / 2f); // TASK 13
            changed = true;
        }

        if (changed)
        {
            objB.transform.position = posB;
        }
    }
}
