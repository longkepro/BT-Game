#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class GameSetup : EditorWindow
{
    [MenuItem("Tools/1-Click Setup BT Game (Phase 7 - Đã Fix Kích Thước)")]
    public static void SetupGameScene()
    {
        // 1. Tải ảnh
        Texture2D texA = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/ship.png");
        Texture2D texB = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/meteor.png");
        Texture2D texC = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/laser.png");

        // FIX LỖI KÍCH THƯỚC KHỔNG LỒ:
        // Đặt PixelsPerUnit = kích thước ảnh (Mathf.Max(width, height)) 
        // Điều này ép Unity thu nhỏ bức ảnh lại sao cho 1 ảnh tương đương đúng 1 Đơn Vị (1 Unit) trong không gian Unity.
        // Nó sẽ to chính xác bằng cái hình vuông Quad cũ!
        Sprite sprA = texA != null ? Sprite.Create(texA, new Rect(0, 0, texA.width, texA.height), new Vector2(0.5f, 0.5f), Mathf.Max(texA.width, texA.height)) : null;
        Sprite sprB = texB != null ? Sprite.Create(texB, new Rect(0, 0, texB.width, texB.height), new Vector2(0.5f, 0.5f), Mathf.Max(texB.width, texB.height)) : null;
        Sprite sprC = texC != null ? Sprite.Create(texC, new Rect(0, 0, texC.width, texC.height), new Vector2(0.5f, 0.5f), Mathf.Max(texC.width, texC.height)) : null;

        // Tải ảnh nền vũ trụ và cấu hình WrapMode = Repeat
        string bgPath = "Assets/Sprites/space_bg.png";
        TextureImporter bgImporter = AssetImporter.GetAtPath(bgPath) as TextureImporter;
        if (bgImporter != null && bgImporter.wrapMode != TextureWrapMode.Repeat)
        {
            bgImporter.wrapMode = TextureWrapMode.Repeat;
            bgImporter.SaveAndReimport();
        }
        Texture2D texBG = AssetDatabase.LoadAssetAtPath<Texture2D>(bgPath);
        if (texBG != null)
        {
            texBG.wrapMode = TextureWrapMode.Repeat;
        }

        // Dọn dẹp object cũ
#if UNITY_2023_1_OR_NEWER
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
#endif
        foreach(GameObject go in allObjects) {
            if (go.name.Contains("Prefab") || go.name.Contains("Object A") || go.name.Contains("Object B") || go.name.Contains("Object C") || go.name == "GameManager" || go.name.Contains("Background") || go.name.Contains("GameCanvas") || go.name.Contains("EventSystem")) {
                DestroyImmediate(go);
            }
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            cam = camObj.AddComponent<Camera>();
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        cam.orthographicSize = 5f;

        // Tạo GameManager
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.bgTexture = texBG;
        gm.sourceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Arial.ttf") 
                     ?? AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Tahoma.ttf") 
                     ?? AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/SegoeUI.ttf");

        // Tạo Prefabs
        gm.prefabA = CreateSpritePrefab("Prefab A (Ship)", sprA, Color.blue);
        gm.prefabB = CreateSpritePrefab("Prefab B (Meteor)", sprB, Color.red);
        gm.prefabC = CreateSpritePrefab("Prefab C (Laser)", sprC, Color.yellow);

        // Đặt kích thước hiển thị (Scale)
        gm.objectWidth = 1.5f;
        gm.objectHeight = 1.5f;

        // Xoay góc theo đúng yêu cầu: Tàu -90 độ, Laser -180 độ
        gm.prefabA.transform.rotation = Quaternion.Euler(0, 0, -90);
        gm.prefabC.transform.rotation = Quaternion.Euler(0, 0, -180);

        // Ẩn prefab gốc đi
        gm.prefabA.SetActive(false);
        gm.prefabB.SetActive(false);
        gm.prefabC.SetActive(false);

        // Khởi tạo hệ thống Canvas UI uGUI sắc nét ngay trong Scene
        gm.SetupCanvasUI();
        gm.UpdateUIState();

        Debug.Log("=> Đã thiết lập thành công Scene với Canvas uGUI sắc nét chuẩn HD 1920x1080!");
    }

    private static GameObject CreateSpritePrefab(string name, Sprite sprite, Color backupColor)
    {
        GameObject obj = new GameObject(name);
        
        if (sprite != null)
        {
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
        }
        else
        {
            DestroyImmediate(obj);
            obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            obj.name = name;
            DestroyImmediate(obj.GetComponent<Collider>());
            Material mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = backupColor;
            obj.GetComponent<Renderer>().sharedMaterial = mat;
        }

        return obj;
    }
}
#endif
