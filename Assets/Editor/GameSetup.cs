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

        // Dọn dẹp object cũ
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach(GameObject go in allObjects) {
            if (go.name.Contains("Prefab") || go.name.Contains("Object A") || go.name.Contains("Object B") || go.name.Contains("Object C") || go.name == "GameManager") {
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
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;

        // Tạo GameManager
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();

        // Tạo Prefabs
        gm.prefabA = CreateSpritePrefab("Prefab A (Ship)", sprA, Color.blue);
        gm.prefabB = CreateSpritePrefab("Prefab B (Meteor)", sprB, Color.red);
        gm.prefabC = CreateSpritePrefab("Prefab C (Laser)", sprC, Color.yellow);

        // Đặt kích thước hiển thị (Scale)
        // Nhờ phép chia PixelsPerUnit ở trên, ảnh gốc dù to 2000px hay 500px 
        // thì scale 1.5f ở đây đều cho ra kích thước bằng nhau và nhỏ gọn trên màn hình!
        gm.objectWidth = 1.5f;
        gm.objectHeight = 1.5f;

        // Xoay góc theo đúng yêu cầu: Tàu -90 độ, Laser -180 độ
        gm.prefabA.transform.rotation = Quaternion.Euler(0, 0, -90);
        gm.prefabC.transform.rotation = Quaternion.Euler(0, 0, -180);

        // Ẩn prefab gốc đi
        gm.prefabA.SetActive(false);
        gm.prefabB.SetActive(false);
        gm.prefabC.SetActive(false);

        Debug.Log("=> Đã fix lỗi Ảnh khổng lồ (chia lại PixelsPerUnit) và Đổi chỗ Thiên thạch/Laser!");
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
