using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

// Component hỗ trợ bắt sự kiện giữ ngón tay trên nút ảo Mobile
public class VirtualHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool isPressed { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPressed = false;
    }
}

public class GameManager : MonoBehaviour
{
    [Header("Prefabs (Hình ảnh Sprite 2D)")]
    public GameObject prefabA;
    public GameObject prefabB;
    public GameObject prefabC;

    // Các đối tượng trong game
    private GameObject objA;
    private GameObject objB;
    private List<GameObject> listC = new List<GameObject>();

    [Header("Configs - Object A (Phi Thuyền)")]
    public float speedA = 5f;

    [Header("Configs - Object B (Thiên Thạch)")]
    public float speedB = 3.5f;
    public Vector2 dirB = new Vector2(-1, 0); // Đi sang trái
    public bool useFlexibleMovementB = true;  // Chuyển động lượn sóng linh hoạt (Lên/Xuống/Trái)
    public float waveAmplitude = 2.0f;        // Biên độ lượn sóng dọc (Lên/Xuống)
    public float waveFrequency = 2.2f;        // Tần số lượn sóng
    private float waveTimerB = 0f;
    private float baseY_B = 0f;

    [Header("Configs - Object C (Đạn Laser)")]
    public float speedC = 10f;
    public Vector2 dirC = new Vector2(1, 0); // Đi sang phải

    [Header("Kích thước chung (A & B)")]
    public float objectWidth = 1.5f;
    public float objectHeight = 1.5f;

    [Header("Background Configs (UV Offset Scroll)")]
    public Texture2D bgTexture;
    public float bgScrollSpeed = 0.25f;
    private Material bgMaterial;
    private GameObject bgQuad;

    public enum GameState
    {
        StartMenu,
        Playing,
        GameOver
    }

    [Header("Game State & Score")]
    public GameState gameState = GameState.StartMenu;
    public bool isGameOver = false;
    public int score = 0;

    [Header("Fonts (TextMeshPro SDF Vector Font Sắc Nét 100%)")]
    public Font sourceFont;
    public Font gameFont; // Tương thích ngược Inspector
    public Font gameBoldFont;
    public TMP_FontAsset tmpFontAsset;

    [Header("UI Canvas (uGUI TextMeshPro)")]
    public Canvas gameCanvas;
    public GameObject startMenuPanel;
    public GameObject hudPanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI hudScoreText;
    public TextMeshProUGUI gameOverScoreText;

    [Header("Mobile Virtual Controls (Cảm Ứng Android - D-Pad 4 Hướng)")]
    public VirtualHoldButton btnUpHold;
    public VirtualHoldButton btnDownHold;
    public VirtualHoldButton btnLeftHold;
    public VirtualHoldButton btnRightHold;

    private Sprite panelSprite;
    private Camera mainCam;

    // Bán kính va chạm
    private float radiusA;
    private float radiusB;
    private float radiusC;

    // Biên màn hình (World Space)
    private float left, right, top, bottom;

    void Awake()
    {
        // Ép các thông số chuẩn xác ngay tại thời điểm khởi chạy, 
        // đảm bảo không bị ghi đè bởi các giá trị cũ lưu trong file Scene YAML hoặc Inspector máy khác
        objectWidth = 1.5f;
        objectHeight = 1.5f;
        speedA = 5f;
        speedB = 3.5f;
        dirB = new Vector2(-1, 0);
        useFlexibleMovementB = true;
        waveAmplitude = 2.0f;
        waveFrequency = 2.2f;
        speedC = 12f;
        dirC = new Vector2(1, 0);

        LoadTMPFont();
    }

    void Start()
    {
        // 1. Lấy kích thước màn hình và xác định vùng tọa độ World Space
        mainCam = Camera.main;
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }
        float height = mainCam.orthographicSize * 2f;
        float width = height * mainCam.aspect;
        
        // Cấu hình biên (theo chuẩn Unity, y dương là bên trên)
        left = mainCam.transform.position.x - width / 2f;
        right = mainCam.transform.position.x + width / 2f;
        top = mainCam.transform.position.y + height / 2f;
        bottom = mainCam.transform.position.y - height / 2f;

        // 2. Khởi tạo Quad ảnh nền cuộn UV Offset
        SetupBackground(mainCam, width, height);

        // 3. Khởi tạo Hệ thống UI Canvas TextMeshPro sắc nét kèm nút ảo Android
        SetupCanvasUI();
        UpdateUIState();

        // 4. Bán kính va chạm (ước tính theo kích thước)
        radiusA = objectHeight * 0.45f;
        radiusB = objectWidth * 0.45f;
        radiusC = objectWidth * 0.25f;

        // 5. Tạo và đặt A, B đúng vị trí ban đầu
        if (prefabA != null)
        {
            objA = Instantiate(prefabA);
            objA.name = "Object A";
            objA.transform.localScale = new Vector3(objectWidth, objectHeight, 1);
            objA.transform.position = new Vector3(left + objectWidth / 2f, 0, 0);
            objA.SetActive(true);
        }

        if (prefabB != null)
        {
            objB = Instantiate(prefabB);
            objB.name = "Object B";
            objB.transform.localScale = new Vector3(objectWidth, objectHeight, 1);
            objB.transform.position = new Vector3(right - objectWidth / 2f, 0, 0);
            baseY_B = 0f;
            waveTimerB = 0f;
            objB.SetActive(true);
        }
    }

    void Update()
    {
        // Cuộn ảnh nền theo trục X (từ phải sang trái tạo cảm giác tàu lao về phía trước)
        if (gameState != GameState.GameOver && bgMaterial != null)
        {
            float currentScrollSpeed = (gameState == GameState.StartMenu) ? bgScrollSpeed * 0.4f : bgScrollSpeed;
            bgMaterial.mainTextureOffset += new Vector2(currentScrollSpeed * Time.deltaTime, 0);
        }

        // Trạng thái 1: Màn hình bắt đầu (StartMenu)
        if (gameState == GameState.StartMenu)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StartGame();
            }
            return; // Chưa bắt đầu thì chưa cho di chuyển tàu, đạn và thiên thạch
        }

        // Trạng thái 2: Game Over - Lắng nghe phím R để Chơi lại hoặc Escape/M để về Menu
        if (gameState == GameState.GameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
            {
                ReturnToMenu();
            }
            return; // Dừng toàn bộ chuyển động khi đã Game Over
        }

        // 1. ĐỌC INPUT ĐIỀU KHIỂN TÀU A (4 HƯỚNG: LÊN, XUỐNG, TRÁI, PHẢI):
        // Hỗ trợ đồng thời 3 cơ chế: Bàn phím máy tính + Nút ảo D-pad Android + Vuốt chạm 2D trực tiếp
        float inputX = 0f;
        float inputY = 0f;

        // Cơ chế A1: Phím cứng PC (W / S / A / D hoặc Mũi tên 4 chiều)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputY += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputY -= 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputX -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputX += 1f;

        // Cơ chế A2: Cụm nút ảo Mobile D-Pad 4 chiều (Giữ nút ▲, ▼, ◄, ►)
        if (btnUpHold != null && btnUpHold.isPressed) inputY += 1f;
        if (btnDownHold != null && btnDownHold.isPressed) inputY -= 1f;
        if (btnLeftHold != null && btnLeftHold.isPressed) inputX -= 1f;
        if (btnRightHold != null && btnRightHold.isPressed) inputX += 1f;

        // Cập nhật vị trí A theo input người chơi
        if (objA != null)
        {
            Vector3 posA = objA.transform.position;

            if (inputX != 0f || inputY != 0f)
            {
                Vector3 moveDir = new Vector3(inputX, inputY, 0).normalized;
                posA += moveDir * speedA * Time.deltaTime;
            }

            // Cơ chế A3: Vuốt / Kéo ngón tay trực tiếp trên màn hình Android (cả trục X và Y)
            if (Input.touchCount > 0 && mainCam != null)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    // Bỏ qua nếu ngón tay đang bấm vào nút ảo UI để không bị xung đột
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        continue;
                    }

                    // Khi người chơi vuốt hoặc giữ ngón tay trên màn hình, tàu lướt theo ngón tay 2 chiều
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        Vector3 touchWorldPos = mainCam.ScreenToWorldPoint(touch.position);
                        float targetX = Mathf.Clamp(touchWorldPos.x, left + objectWidth / 2f, right - objectWidth / 2f);
                        float targetY = Mathf.Clamp(touchWorldPos.y, bottom + objectHeight / 2f, top - objectHeight / 2f);
                        posA = Vector3.MoveTowards(posA, new Vector3(targetX, targetY, posA.z), speedA * 2.0f * Time.deltaTime);
                        break; // Ưu tiên ngón tay điều khiển chính
                    }
                }
            }

            // Giữ A không chạy ra khỏi 4 mép màn hình
            posA.x = Mathf.Clamp(posA.x, left + objectWidth / 2f, right - objectWidth / 2f);
            posA.y = Mathf.Clamp(posA.y, bottom + objectHeight / 2f, top - objectHeight / 2f);
            objA.transform.position = posA;
        }

        // 2. ĐỌC INPUT BẮN ĐẠN C:
        // Hỗ trợ: Phím Space, Click chuột (PC/AVD) hoặc Chạm màn hình tự do (ngoài nút UI)
        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(0) && !isPointerOverUI))
        {
            SpawnC();
        }

        // 3. Cập nhật B (Di chuyển linh hoạt sang trái kết hợp lượn sóng lên/xuống)
        if (objB != null)
        {
            if (useFlexibleMovementB)
            {
                waveTimerB += Time.deltaTime * waveFrequency;
                Vector3 posB = objB.transform.position;
                posB.x += dirB.x * speedB * Time.deltaTime;
                posB.y = baseY_B + Mathf.Sin(waveTimerB) * waveAmplitude;
                objB.transform.position = posB;
            }
            else
            {
                objB.transform.position += (Vector3)dirB * speedB * Time.deltaTime;
            }

            // Xoay nhẹ thiên thạch tạo hiệu ứng thị giác 2D sống động
            objB.transform.Rotate(0, 0, 35f * Time.deltaTime);

            // Kiểm tra B chạm biên trái -> Bọc biên sang phải (Boundary Wrapping)
            CheckBoundariesB();
        }

        // 4. Cập nhật C và kiểm tra va chạm C trúng B
        for (int i = listC.Count - 1; i >= 0; i--)
        {
            if (listC[i] != null)
            {
                listC[i].transform.position += (Vector3)dirC * speedC * Time.deltaTime;

                // KIỂM TRA VA CHẠM: Đạn C trúng Thiên thạch B
                if (objB != null)
                {
                    float distCB = Vector2.Distance(listC[i].transform.position, objB.transform.position);
                    if (distCB < (radiusC + radiusB))
                    {
                        // 1. Hủy viên đạn C
                        Destroy(listC[i]);
                        listC.RemoveAt(i);

                        // 2. Tăng điểm & cập nhật HUD TextMeshPro
                        score++;
                        if (hudScoreText != null) hudScoreText.text = $"Điểm số: {score}";

                        // 3. Tạo hiệu ứng nổ tại vị trí B và Respawn B
                        CreateExplosionEffect(objB.transform.position);
                        RespawnB();
                        continue;
                    }
                }
                
                // Hủy C nếu ra khỏi màn hình (tránh leak bộ nhớ)
                if (listC[i].transform.position.x > right + 2f)
                {
                    Destroy(listC[i]);
                    listC.RemoveAt(i);
                }
            }
        }

        // 5. KIỂM TRA VA CHẠM: Thiên thạch B đâm trúng Phi thuyền A -> Game Over
        if (objA != null && objB != null && objA.activeSelf)
        {
            float distAB = Vector2.Distance(objA.transform.position, objB.transform.position);
            if (distAB < (radiusA + radiusB) * 0.85f)
            {
                TriggerGameOver();
            }
        }
    }

    // Bắn đạn C xuất phát từ A
    public void SpawnC()
    {
        if (objA == null || !objA.activeSelf) return;

        GameObject c = Instantiate(prefabC);
        c.name = "Object C";
        c.transform.localScale = new Vector3(objectWidth * 0.4f, objectHeight * 0.4f, 1);
        c.transform.position = objA.transform.position;
        c.SetActive(true);
        
        listC.Add(c);
    }

    // Respawn B về lại mép phải với độ cao ngẫu nhiên
    void RespawnB()
    {
        if (objB == null) return;
        baseY_B = Random.Range(bottom + objectHeight * 1.5f, top - objectHeight * 1.5f);
        waveTimerB = Random.Range(0f, Mathf.PI * 2f);
        Vector3 posB = objB.transform.position;
        posB.x = right - objectWidth / 2f;
        posB.y = baseY_B;
        objB.transform.position = posB;
    }

    // Hiệu ứng nổ hình ảnh nhẹ nhàng
    void CreateExplosionEffect(Vector3 pos)
    {
        GameObject exp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        exp.name = "ExplosionEffect";
        exp.transform.position = pos;
        exp.transform.localScale = Vector3.one * objectWidth * 0.9f;
        Destroy(exp.GetComponent<Collider>());
        
        Renderer r = exp.GetComponent<Renderer>();
        if (r != null)
        {
            Shader s = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (s != null)
            {
                Material mat = new Material(s);
                mat.color = new Color(1f, 0.4f, 0f, 0.9f); // Màu cam lửa
                r.material = mat;
            }
        }
        Destroy(exp, 0.2f); // Tự hủy sau 0.2 giây
    }

    // Bắt đầu chơi game từ Menu
    public void StartGame()
    {
        gameState = GameState.Playing;
        isGameOver = false;
        score = 0;

        // Khôi phục vị trí A
        if (objA != null)
        {
            objA.transform.position = new Vector3(left + objectWidth / 2f, 0, 0);
            objA.SetActive(true);
        }

        // Khôi phục vị trí B
        RespawnB();

        // Xóa sạch đạn C trên màn hình
        for (int i = listC.Count - 1; i >= 0; i--)
        {
            if (listC[i] != null) Destroy(listC[i]);
        }
        listC.Clear();

        UpdateUIState();
    }

    // Quay lại màn hình chính (Menu)
    public void ReturnToMenu()
    {
        gameState = GameState.StartMenu;
        isGameOver = false;
        score = 0;

        if (objA != null)
        {
            objA.transform.position = new Vector3(left + objectWidth / 2f, 0, 0);
            objA.SetActive(true);
        }

        RespawnB();

        for (int i = listC.Count - 1; i >= 0; i--)
        {
            if (listC[i] != null) Destroy(listC[i]);
        }
        listC.Clear();

        UpdateUIState();
    }

    // Xử lý khi Game Over
    void TriggerGameOver()
    {
        gameState = GameState.GameOver;
        isGameOver = true;
        if (objA != null)
        {
            CreateExplosionEffect(objA.transform.position);
            objA.SetActive(false); // Ẩn phi thuyền A
        }

        UpdateUIState();
    }

    // Chơi lại từ đầu
    public void RestartGame()
    {
        StartGame();
    }

    // Bọc biên chuyển động cho B
    void CheckBoundariesB()
    {
        Vector3 posB = objB.transform.position;
        bool changed = false;

        // 1. B chạm biên trái -> Xuất hiện lại ở biên đối diện (biên phải) với Y ngẫu nhiên
        if (posB.x - objectWidth / 2f <= left)
        {
            posB.x = right - objectWidth / 2f;
            baseY_B = Random.Range(bottom + objectHeight * 1.5f, top - objectHeight * 1.5f);
            posB.y = baseY_B;
            waveTimerB = Random.Range(0f, Mathf.PI * 2f);
            changed = true;
        }
        // 2. B chạm biên trên -> Xuất hiện lại ở biên đối diện (biên dưới) với X ngẫu nhiên
        else if (posB.y + objectHeight / 2f >= top)
        {
            posB.y = bottom + objectHeight / 2f;
            baseY_B = posB.y;
            posB.x = Random.Range(left + objectWidth / 2f, right - objectWidth / 2f);
            changed = true;
        }
        // 3. B chạm biên dưới -> Xuất hiện lại ở biên đối diện (biên trên) với X ngẫu nhiên
        else if (posB.y - objectHeight / 2f <= bottom)
        {
            posB.y = top - objectHeight / 2f;
            baseY_B = posB.y;
            posB.x = Random.Range(left + objectWidth / 2f, right - objectWidth / 2f);
            changed = true;
        }

        if (changed)
        {
            objB.transform.position = posB;
        }
    }

    // Nạp Font và tạo SDF Dynamic Font Asset cho TextMeshPro
    void LoadTMPFont()
    {
        if (tmpFontAsset != null) return;

        Font src = sourceFont != null ? sourceFont : gameFont;
#if UNITY_EDITOR
        if (src == null)
        {
            src = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Arial.ttf")
               ?? UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Tahoma.ttf")
               ?? UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/SegoeUI.ttf");
        }
#endif
        if (src == null)
        {
            src = Font.CreateDynamicFontFromOSFont("Arial", 48)
               ?? Font.CreateDynamicFontFromOSFont("Tahoma", 48)
               ?? Font.CreateDynamicFontFromOSFont("Segoe UI", 48);
        }

        if (src != null)
        {
            sourceFont = src;
            // Tạo TMP Font Asset động với SDFAA (Signed Distance Field Anti-Aliasing), Padding 9px ngăn hoàn toàn hiện tượng tràn pixel
            tmpFontAsset = TMP_FontAsset.CreateFontAsset(src, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        }

        // Fallback font mặc định
        if (tmpFontAsset == null)
        {
            tmpFontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
    }

    // Tạo Sprite 9-Slice bo góc tròn mượt mà cho Panel và Button
    private Sprite CreateRoundedSprite(int width, int height, int radius)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[width * height];
        Color32 white = new Color32(255, 255, 255, 255);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int cx = (x < radius) ? radius : ((x >= width - radius) ? width - radius - 1 : x);
                int cy = (y < radius) ? radius : ((y >= height - radius) ? height - radius - 1 : y);
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist > radius)
                {
                    float alpha = Mathf.Clamp01(radius + 0.5f - dist);
                    pixels[y * width + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
                }
                else
                {
                    pixels[y * width + x] = white;
                }
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    // Helper tạo đối tượng TextMeshProUGUI sắc nét chuẩn vector SDF
    private TextMeshProUGUI CreateTMPText(string name, Transform parent, string content, float fontSize, FontWeight fontWeight, Color color, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        if (tmpFontAsset != null)
        {
            tmp.font = tmpFontAsset;
        }
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontWeight = fontWeight;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        return tmp;
    }

    // Helper tạo đối tượng Button phong cách hiện đại với hiệu ứng Hover/Press và chữ TextMeshPro
    private Button CreateButton(string name, Transform parent, string labelText, float fontSize, Color normalColor, Color hoverColor, Color pressedColor, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        if (panelSprite != null)
        {
            img.sprite = panelSprite;
            img.type = Image.Type.Sliced;
        }
        img.color = Color.white;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.normalColor = normalColor;
        cb.highlightedColor = hoverColor;
        cb.pressedColor = pressedColor;
        cb.selectedColor = normalColor;
        cb.colorMultiplier = 1f;
        btn.colors = cb;

        if (onClickAction != null)
        {
            btn.onClick.AddListener(onClickAction);
        }

        TextMeshProUGUI tmpText = CreateTMPText("Text", btnObj.transform, labelText, fontSize, FontWeight.Bold, Color.white, TextAlignmentOptions.Center);
        RectTransform rtText = tmpText.rectTransform;
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.sizeDelta = Vector2.zero;
        rtText.anchoredPosition = Vector2.zero;

        return btn;
    }

    // Helper tạo nút bấm ảo Mobile có thể giữ (Virtual Button)
    private GameObject CreateVirtualButton(string name, Transform parent, string labelText, float fontSize, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = position;

        Image img = btnObj.AddComponent<Image>();
        if (panelSprite != null)
        {
            img.sprite = panelSprite;
            img.type = Image.Type.Sliced;
        }
        img.color = Color.white;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.normalColor = bgColor;
        cb.highlightedColor = new Color(Mathf.Min(1f, bgColor.r * 1.25f), Mathf.Min(1f, bgColor.g * 1.25f), Mathf.Min(1f, bgColor.b * 1.25f), bgColor.a);
        cb.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 1f);
        cb.selectedColor = bgColor;
        cb.colorMultiplier = 1f;
        btn.colors = cb;

        TextMeshProUGUI tmpText = CreateTMPText("Text", btnObj.transform, labelText, fontSize, FontWeight.Bold, Color.white, TextAlignmentOptions.Center);
        RectTransform rtText = tmpText.rectTransform;
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.sizeDelta = Vector2.zero;
        rtText.anchoredPosition = Vector2.zero;

        return btnObj;
    }

    // KHỞI TẠO TOÀN BỘ GIAO DIỆN CANVAS TEXTMESHPRO CHUẨN VECTOR FULL HD
    public void SetupCanvasUI()
    {
        if (gameCanvas != null && startMenuPanel != null && hudPanel != null && gameOverPanel != null)
        {
            return;
        }

        LoadTMPFont();
        panelSprite = CreateRoundedSprite(32, 32, 8);

        // 1. Đảm bảo EventSystem tồn tại để bắt sự kiện chuột / phím
#if UNITY_2023_1_OR_NEWER
        if (FindFirstObjectByType<EventSystem>() == null)
#else
        if (FindObjectOfType<EventSystem>() == null)
#endif
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // 2. Dọn dẹp GameCanvas cũ nếu có và tạo Canvas mới với CanvasScaler 1920x1080
        GameObject oldCanvas = GameObject.Find("GameCanvas");
        if (oldCanvas != null)
        {
            Destroy(oldCanvas);
        }

        GameObject canvasObj = new GameObject("GameCanvas");
        gameCanvas = canvasObj.AddComponent<Canvas>();
        gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 3. START MENU PANEL
        startMenuPanel = new GameObject("StartMenuPanel");
        startMenuPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform rtStart = startMenuPanel.AddComponent<RectTransform>();
        rtStart.anchorMin = Vector2.zero;
        rtStart.anchorMax = Vector2.one;
        rtStart.sizeDelta = Vector2.zero;

        Image startOverlay = startMenuPanel.AddComponent<Image>();
        startOverlay.color = new Color(0.02f, 0.03f, 0.07f, 0.65f); // Làm mờ nhẹ nền sao khi ở menu

        // Card trung tâm của Start Menu
        GameObject startCard = new GameObject("MenuCard");
        startCard.transform.SetParent(startMenuPanel.transform, false);
        RectTransform rtCard = startCard.AddComponent<RectTransform>();
        rtCard.anchorMin = new Vector2(0.5f, 0.5f);
        rtCard.anchorMax = new Vector2(0.5f, 0.5f);
        rtCard.pivot = new Vector2(0.5f, 0.5f);
        rtCard.sizeDelta = new Vector2(800, 550);
        rtCard.anchoredPosition = Vector2.zero;

        Image cardImg = startCard.AddComponent<Image>();
        cardImg.sprite = panelSprite;
        cardImg.type = Image.Type.Sliced;
        cardImg.color = new Color(0.05f, 0.08f, 0.16f, 0.94f); // Màu xanh đêm vũ trụ sang trọng

        // Tiêu đề
        TextMeshProUGUI titleTMP = CreateTMPText("Title", startCard.transform, "CHIẾN BINH VŨ TRỤ", 48, FontWeight.Bold, new Color(0.25f, 0.85f, 1f), TextAlignmentOptions.Center);
        RectTransform rtTitle = titleTMP.rectTransform;
        rtTitle.anchorMin = new Vector2(0.5f, 1f);
        rtTitle.anchorMax = new Vector2(0.5f, 1f);
        rtTitle.pivot = new Vector2(0.5f, 1f);
        rtTitle.sizeDelta = new Vector2(740, 60);
        rtTitle.anchoredPosition = new Vector2(0, -30);
        titleTMP.outlineWidth = 0.2f;
        titleTMP.outlineColor = Color.black;

        // Phụ đề
        TextMeshProUGUI subTMP = CreateTMPText("SubTitle", startCard.transform, "2D Space Shooter - Báo Cáo Nhóm", 24, FontWeight.Regular, new Color(1f, 0.84f, 0.2f), TextAlignmentOptions.Center);
        RectTransform rtSub = subTMP.rectTransform;
        rtSub.anchorMin = new Vector2(0.5f, 1f);
        rtSub.anchorMax = new Vector2(0.5f, 1f);
        rtSub.pivot = new Vector2(0.5f, 1f);
        rtSub.sizeDelta = new Vector2(740, 35);
        rtSub.anchoredPosition = new Vector2(0, -95);

        // Thanh phân cách trang trí
        GameObject divObj = new GameObject("Divider");
        divObj.transform.SetParent(startCard.transform, false);
        RectTransform rtDiv = divObj.AddComponent<RectTransform>();
        rtDiv.anchorMin = new Vector2(0.5f, 1f);
        rtDiv.anchorMax = new Vector2(0.5f, 1f);
        rtDiv.pivot = new Vector2(0.5f, 1f);
        rtDiv.sizeDelta = new Vector2(680, 2);
        rtDiv.anchoredPosition = new Vector2(0, -140);
        Image divImg = divObj.AddComponent<Image>();
        divImg.color = new Color(1f, 1f, 1f, 0.2f);

        // Nội dung hướng dẫn (Hỗ trợ cả PC lẫn Android)
        string guideText = "• Di chuyển: Phím [W][A][S][D], Cụm nút ảo [▲][▼][◄][►] hoặc Vuốt màn hình\n" +
                          "• Khai hỏa: Phím [Space], Chạm màn hình hoặc Nút [BẮN]\n" +
                          "• Bắn hạ Thiên thạch để ghi điểm, tránh va chạm trực diện!";
        TextMeshProUGUI infoTMP = CreateTMPText("Instructions", startCard.transform, guideText, 21, FontWeight.Regular, Color.white, TextAlignmentOptions.MidlineLeft);
        infoTMP.lineSpacing = 14f;
        RectTransform rtInfo = infoTMP.rectTransform;
        rtInfo.anchorMin = new Vector2(0.5f, 1f);
        rtInfo.anchorMax = new Vector2(0.5f, 1f);
        rtInfo.pivot = new Vector2(0.5f, 1f);
        rtInfo.sizeDelta = new Vector2(680, 140);
        rtInfo.anchoredPosition = new Vector2(0, -155);

        // Nút Bắt Đầu
        Color blueNorm = new Color(0.12f, 0.45f, 0.95f, 1f);
        Color blueHover = new Color(0.22f, 0.58f, 1f, 1f);
        Color bluePress = new Color(0.08f, 0.35f, 0.8f, 1f);
        Button btnStart = CreateButton("StartButton", startCard.transform, "BẮT ĐẦU CHƠI", 24, blueNorm, blueHover, bluePress, StartGame);
        RectTransform rtBtnStart = btnStart.GetComponent<RectTransform>();
        rtBtnStart.anchorMin = new Vector2(0.5f, 1f);
        rtBtnStart.anchorMax = new Vector2(0.5f, 1f);
        rtBtnStart.pivot = new Vector2(0.5f, 1f);
        rtBtnStart.sizeDelta = new Vector2(340, 62);
        rtBtnStart.anchoredPosition = new Vector2(0, -355);

        // Gợi ý phím tắt
        TextMeshProUGUI hintTMP = CreateTMPText("Hint", startCard.transform, "(Nhấp nút hoặc nhấn [SPACE] / [ENTER] để vào game)", 18, FontWeight.Regular, new Color(0.75f, 0.8f, 0.9f, 0.85f), TextAlignmentOptions.Center);
        RectTransform rtHint = hintTMP.rectTransform;
        rtHint.anchorMin = new Vector2(0.5f, 1f);
        rtHint.anchorMax = new Vector2(0.5f, 1f);
        rtHint.pivot = new Vector2(0.5f, 1f);
        rtHint.sizeDelta = new Vector2(740, 30);
        rtHint.anchoredPosition = new Vector2(0, -450);

        // 4. HUD PANEL (Hiển thị khi chơi game)
        hudPanel = new GameObject("HUDPanel");
        hudPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform rtHUD = hudPanel.AddComponent<RectTransform>();
        rtHUD.anchorMin = Vector2.zero;
        rtHUD.anchorMax = Vector2.one;
        rtHUD.sizeDelta = Vector2.zero;
        rtHUD.anchoredPosition = Vector2.zero;

        // Badge góc trên bên trái: Điểm số & Dòng hướng dẫn
        GameObject hudBadgeObj = new GameObject("HUDBadge");
        hudBadgeObj.transform.SetParent(hudPanel.transform, false);
        RectTransform rtBadge = hudBadgeObj.AddComponent<RectTransform>();
        rtBadge.anchorMin = new Vector2(0f, 1f);
        rtBadge.anchorMax = new Vector2(0f, 1f);
        rtBadge.pivot = new Vector2(0f, 1f);
        rtBadge.sizeDelta = new Vector2(600, 100);
        rtBadge.anchoredPosition = new Vector2(30, -30);

        Image hudBadge = hudBadgeObj.AddComponent<Image>();
        hudBadge.sprite = panelSprite;
        hudBadge.type = Image.Type.Sliced;
        hudBadge.color = new Color(0.04f, 0.06f, 0.12f, 0.8f);

        hudScoreText = CreateTMPText("ScoreText", hudBadgeObj.transform, "Điểm số: 0", 32, FontWeight.Bold, new Color(1f, 0.9f, 0.25f), TextAlignmentOptions.MidlineLeft);
        hudScoreText.outlineWidth = 0.2f;
        hudScoreText.outlineColor = Color.black;
        RectTransform rtHUDScore = hudScoreText.rectTransform;
        rtHUDScore.anchorMin = new Vector2(0f, 1f);
        rtHUDScore.anchorMax = new Vector2(0f, 1f);
        rtHUDScore.pivot = new Vector2(0f, 1f);
        rtHUDScore.sizeDelta = new Vector2(540, 45);
        rtHUDScore.anchoredPosition = new Vector2(24, -12);

        TextMeshProUGUI hudInfoTMP = CreateTMPText("ControlsText", hudBadgeObj.transform, "Điều khiển: [W][A][S][D], Nút ảo 4 chiều hoặc Vuốt | Nút [BẮN] để khai hỏa", 17, FontWeight.Regular, new Color(0.9f, 0.95f, 1f, 0.9f), TextAlignmentOptions.MidlineLeft);
        RectTransform rtHUDInfo = hudInfoTMP.rectTransform;
        rtHUDInfo.anchorMin = new Vector2(0f, 1f);
        rtHUDInfo.anchorMax = new Vector2(0f, 1f);
        rtHUDInfo.pivot = new Vector2(0f, 1f);
        rtHUDInfo.sizeDelta = new Vector2(560, 30);
        rtHUDInfo.anchoredPosition = new Vector2(24, -58);

        // Cụm Nút Ảo Mobile - Góc Trái Dưới (D-Pad 4 Hướng: ▲ Lên, ▼ Xuống, ◄ Trái, ► Phải)
        Color dpadColor = new Color(0.12f, 0.45f, 0.85f, 0.75f);
        Vector2 dpadCenter = new Vector2(170, 160);
        float dpadOffset = 85f;
        Vector2 btnSize = new Vector2(85, 85);

        // Nút Lên ▲
        GameObject upObj = CreateVirtualButton("BtnUp", hudPanel.transform, "▲", 42, Vector2.zero, new Vector2(0.5f, 0.5f), dpadCenter + new Vector2(0, dpadOffset), btnSize, dpadColor);
        btnUpHold = upObj.AddComponent<VirtualHoldButton>();

        // Nút Xuống ▼
        GameObject downObj = CreateVirtualButton("BtnDown", hudPanel.transform, "▼", 42, Vector2.zero, new Vector2(0.5f, 0.5f), dpadCenter + new Vector2(0, -dpadOffset), btnSize, dpadColor);
        btnDownHold = downObj.AddComponent<VirtualHoldButton>();

        // Nút Trái ◄
        GameObject leftObj = CreateVirtualButton("BtnLeft", hudPanel.transform, "◄", 42, Vector2.zero, new Vector2(0.5f, 0.5f), dpadCenter + new Vector2(-dpadOffset, 0), btnSize, dpadColor);
        btnLeftHold = leftObj.AddComponent<VirtualHoldButton>();

        // Nút Phải ►
        GameObject rightObj = CreateVirtualButton("BtnRight", hudPanel.transform, "►", 42, Vector2.zero, new Vector2(0.5f, 0.5f), dpadCenter + new Vector2(dpadOffset, 0), btnSize, dpadColor);
        btnRightHold = rightObj.AddComponent<VirtualHoldButton>();

        // Cụm Nút Ảo Mobile - Góc Phải Dưới (BẮN Laser)
        GameObject fireObj = CreateVirtualButton("BtnFire", hudPanel.transform, "BẮN", 32, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-120, 130), new Vector2(140, 140), new Color(0.95f, 0.32f, 0.12f, 0.85f));
        Button btnFireComponent = fireObj.GetComponent<Button>();
        btnFireComponent.onClick.AddListener(SpawnC);

        // 5. GAME OVER PANEL
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform rtGOPanel = gameOverPanel.AddComponent<RectTransform>();
        rtGOPanel.anchorMin = Vector2.zero;
        rtGOPanel.anchorMax = Vector2.one;
        rtGOPanel.sizeDelta = Vector2.zero;

        Image goOverlay = gameOverPanel.AddComponent<Image>();
        goOverlay.color = new Color(0.08f, 0.02f, 0.02f, 0.7f);

        GameObject goCard = new GameObject("GameOverCard");
        goCard.transform.SetParent(gameOverPanel.transform, false);
        RectTransform rtGOCard = goCard.AddComponent<RectTransform>();
        rtGOCard.anchorMin = new Vector2(0.5f, 0.5f);
        rtGOCard.anchorMax = new Vector2(0.5f, 0.5f);
        rtGOCard.pivot = new Vector2(0.5f, 0.5f);
        rtGOCard.sizeDelta = new Vector2(580, 420);
        rtGOCard.anchoredPosition = Vector2.zero;

        Image goCardImg = goCard.AddComponent<Image>();
        goCardImg.sprite = panelSprite;
        goCardImg.type = Image.Type.Sliced;
        goCardImg.color = new Color(0.14f, 0.06f, 0.06f, 0.96f);

        TextMeshProUGUI goTitle = CreateTMPText("GOTitle", goCard.transform, "GAME OVER", 56, FontWeight.Bold, new Color(1f, 0.25f, 0.25f), TextAlignmentOptions.Center);
        goTitle.outlineWidth = 0.25f;
        goTitle.outlineColor = new Color(0.3f, 0f, 0f, 1f);
        RectTransform rtGOTitle = goTitle.rectTransform;
        rtGOTitle.anchorMin = new Vector2(0.5f, 1f);
        rtGOTitle.anchorMax = new Vector2(0.5f, 1f);
        rtGOTitle.pivot = new Vector2(0.5f, 1f);
        rtGOTitle.sizeDelta = new Vector2(520, 65);
        rtGOTitle.anchoredPosition = new Vector2(0, -35);

        gameOverScoreText = CreateTMPText("GOScore", goCard.transform, "Điểm đạt được: 0", 30, FontWeight.Bold, new Color(1f, 0.85f, 0.2f), TextAlignmentOptions.Center);
        RectTransform rtGOScore = gameOverScoreText.rectTransform;
        rtGOScore.anchorMin = new Vector2(0.5f, 1f);
        rtGOScore.anchorMax = new Vector2(0.5f, 1f);
        rtGOScore.pivot = new Vector2(0.5f, 1f);
        rtGOScore.sizeDelta = new Vector2(520, 45);
        rtGOScore.anchoredPosition = new Vector2(0, -115);

        // Nút Chơi Lại
        Color greenNorm = new Color(0.1f, 0.65f, 0.35f, 1f);
        Color greenHover = new Color(0.18f, 0.78f, 0.45f, 1f);
        Color greenPress = new Color(0.08f, 0.5f, 0.28f, 1f);
        Button btnRestart = CreateButton("RestartButton", goCard.transform, "Chơi lại (Phím R)", 24, greenNorm, greenHover, greenPress, RestartGame);
        RectTransform rtBtnRestart = btnRestart.GetComponent<RectTransform>();
        rtBtnRestart.anchorMin = new Vector2(0.5f, 1f);
        rtBtnRestart.anchorMax = new Vector2(0.5f, 1f);
        rtBtnRestart.pivot = new Vector2(0.5f, 1f);
        rtBtnRestart.sizeDelta = new Vector2(320, 56);
        rtBtnRestart.anchoredPosition = new Vector2(0, -195);

        // Nút Về Menu
        Color slateNorm = new Color(0.3f, 0.36f, 0.45f, 1f);
        Color slateHover = new Color(0.4f, 0.48f, 0.58f, 1f);
        Color slatePress = new Color(0.22f, 0.28f, 0.35f, 1f);
        Button btnMenu = CreateButton("MenuButton", goCard.transform, "Về Màn Hình Chính", 20, slateNorm, slateHover, slatePress, ReturnToMenu);
        RectTransform rtBtnMenu = btnMenu.GetComponent<RectTransform>();
        rtBtnMenu.anchorMin = new Vector2(0.5f, 1f);
        rtBtnMenu.anchorMax = new Vector2(0.5f, 1f);
        rtBtnMenu.pivot = new Vector2(0.5f, 1f);
        rtBtnMenu.sizeDelta = new Vector2(320, 48);
        rtBtnMenu.anchoredPosition = new Vector2(0, -270);

        // Dòng hint GameOver
        TextMeshProUGUI goHint = CreateTMPText("GOHint", goCard.transform, "(Nhấp nút hoặc nhấn [R] để chơi lại nhanh)", 17, FontWeight.Regular, new Color(0.8f, 0.8f, 0.8f, 0.8f), TextAlignmentOptions.Center);
        RectTransform rtGOHint = goHint.rectTransform;
        rtGOHint.anchorMin = new Vector2(0.5f, 1f);
        rtGOHint.anchorMax = new Vector2(0.5f, 1f);
        rtGOHint.pivot = new Vector2(0.5f, 1f);
        rtGOHint.sizeDelta = new Vector2(520, 30);
        rtGOHint.anchoredPosition = new Vector2(0, -345);
    }

    // Cập nhật trạng thái hiển thị của các Panel theo GameState
    public void UpdateUIState()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(gameState == GameState.StartMenu);
        if (hudPanel != null) hudPanel.SetActive(gameState == GameState.Playing);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameState == GameState.GameOver);

        if (hudScoreText != null) hudScoreText.text = $"Điểm số: {score}";
        if (gameOverScoreText != null) gameOverScoreText.text = $"Điểm đạt được: {score}";
    }

    // Khởi tạo Quad nền và Material UV Offset
    void SetupBackground(Camera cam, float width, float height)
    {
        if (bgTexture == null)
        {
#if UNITY_EDITOR
            bgTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/space_bg.png");
#endif
        }

        // Tạo Quad bao phủ tầm nhìn Camera
        bgQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgQuad.name = "Background_Quad";
        Destroy(bgQuad.GetComponent<Collider>());

        // Đặt ở vị trí camera nhưng đẩy về phía sau (Z = 5)
        bgQuad.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 5f);
        bgQuad.transform.localScale = new Vector3(width, height, 1f);

        // Shader Unlit/Texture giữ trọn vẹn màu đen sâu và các vì sao sắc sảo
        Shader shader = Shader.Find("Unlit/Texture") ?? Shader.Find("Sprites/Default");
        bgMaterial = new Material(shader);

        if (bgTexture != null)
        {
            bgTexture.wrapMode = TextureWrapMode.Repeat;
            bgMaterial.mainTexture = bgTexture;
        }

        Renderer r = bgQuad.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = bgMaterial;
            r.sortingOrder = -100; // Đảm bảo luôn nằm phía sau tất cả phi thuyền và đạn
        }
    }

    void OnDestroy()
    {
        if (bgMaterial != null)
        {
            Destroy(bgMaterial);
        }
        if (panelSprite != null && panelSprite.texture != null)
        {
            Destroy(panelSprite.texture);
        }
    }
}
