// WeaponConfigEditorWindow.cs - Split View Version (FIXED)
// Place in Editor folder

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using PullGame;

public class WeaponConfigEditorWindow : EditorWindow
{
    private WeaponConfig selectedConfig;
    private Vector2 scrollPosition;
    private Vector2 configListScrollPosition;
    
    // Config management
    private List<WeaponConfig> allConfigs = new List<WeaponConfig>();
    private List<WeaponConfig> filteredConfigs = new List<WeaponConfig>();
    private string searchQuery = "";
    
    // Recent configs
    private const string RECENT_CONFIGS_KEY = "WeaponEditor_RecentConfigs";
    private List<string> recentConfigPaths = new List<string>();
    private const int MAX_RECENT_CONFIGS = 5;
    
    // Tabs
    private int selectedTab = 0;
    private readonly string[] tabNames = { "General", "Fire Mode", "Muzzles", "Projectile", "Ammo", "Recoil", "Audio", "Aim" };
    
    // Styles
    private GUIStyle headerStyle;
    private GUIStyle boxStyle;
    private GUIStyle buttonStyle;
    private GUIStyle configButtonStyle;
    private GUIStyle selectedConfigButtonStyle;
    
    // Split view
    private float splitPosition = 0.5f;
    private bool isResizingSplit = false;
    private bool showPreviewPanel = true;
    
    // Preview System
    private PreviewRenderUtility previewRender;
    private GameObject previewWeaponObject;
    private List<PreviewProjectile> previewProjectiles = new List<PreviewProjectile>();
    private List<Vector3> homingPreviewTargets = new List<Vector3>();
    private System.Random rand = new System.Random();
    private const int homingPreviewTargetCount = 8;
    private bool isPreviewActive = false;
    private float previewTime = 0f;
    private bool autoFire = false;
    private float fireTimer = 0f;
    
    // View mode
    private enum ViewMode
    {
        View2D,
        View3D
    }
    private ViewMode currentViewMode = ViewMode.View2D;
    
    // 2D Camera settings
    private Vector3 camera2DPosition = new Vector3(0, 0, -10f);
    private float camera2DSize = 5f;
    
    // 3D Camera settings
    private Vector2 previewCameraAngle = new Vector2(20f, 45f);
    private float previewCameraDistance = 5f;
    private Vector2 lastMousePosition;
    
    // Preview settings
    private Color backgroundColor = new Color(0.2f, 0.2f, 0.25f);
    private bool showGrid = true;
    private float timeScale = 1f;
    
    // Projectile simulation
    private class PreviewProjectile
    {
        public Vector3 position;
        public Vector3 velocity;
        public Quaternion rotation;
        public float lifetime;
        public float age;
        public GameObject visualObject;
        public ProjectileType type;
        public float gravityModifier;
        public Color color;
        public Vector3 homingTarget;
    }
    
    [MenuItem("Tools/Weapon Config Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<WeaponConfigEditorWindow>("Weapon Editor");
        window.minSize = new Vector2(1200, 800);
        window.Show();
    }
    
    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
        RefreshConfigList();
        LoadRecentConfigs();
    }
    
    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        SaveRecentConfigs();
        CleanupPreview();
    }
    
    private void OnEditorUpdate()
    {
        if (isPreviewActive)
        {
            float deltaTime = 0.016f * timeScale;
            previewTime += deltaTime;
            
            if (autoFire && selectedConfig != null)
            {
                fireTimer += deltaTime;
                if (fireTimer >= selectedConfig.fireModeConfig.fireRate)
                {
                    SimulateFire();
                    fireTimer = 0f;
                }
            }
            
            UpdateProjectiles(deltaTime);
            Repaint();
        }
    }
    
    private void InitializePreview()
    {
        if (previewRender == null)
        {
            previewRender = new PreviewRenderUtility();
            previewRender.camera.backgroundColor = backgroundColor;
            previewRender.camera.clearFlags = CameraClearFlags.SolidColor;
            previewRender.camera.fieldOfView = 60f;
            previewRender.camera.nearClipPlane = 0.1f;
            previewRender.camera.farClipPlane = 100f;
        }
        
        UpdatePreviewCamera();
    }
    
    private void CleanupPreview()
    {
        StopPreview();
        
        if (previewRender != null)
        {
            previewRender.Cleanup();
            previewRender = null;
        }
    }
    
    private void StartPreview()
    {
        if (selectedConfig == null)
        {
            EditorUtility.DisplayDialog("Error", "No config selected!", "OK");
            return;
        }
        
        if (selectedConfig.projectileConfig?.projectilePrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Projectile prefab not assigned!", "OK");
            return;
        }
        
        InitializePreview();
        
        previewWeaponObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        previewWeaponObject.hideFlags = HideFlags.HideAndDontSave;
        previewWeaponObject.transform.position = Vector3.zero;
        previewWeaponObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.6f);
        
        var renderer = previewWeaponObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.hideFlags = HideFlags.HideAndDontSave;
            mat.color = new Color(0.3f, 0.3f, 0.4f);
            renderer.sharedMaterial = mat;
        }
        
        previewProjectiles.Clear();
        GenerateRandomHomingTargets();
        
        isPreviewActive = true;
        previewTime = 0f;
        fireTimer = 0f;
        
        Debug.Log($"Preview started for '{selectedConfig.weaponName}'");
    }
    
    private void StopPreview()
    {
        if (previewWeaponObject != null)
        {
            var renderer = previewWeaponObject.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                DestroyImmediate(renderer.sharedMaterial);
            }
            
            DestroyImmediate(previewWeaponObject);
            previewWeaponObject = null;
        }
        
        foreach (var proj in previewProjectiles)
        {
            if (proj.visualObject != null)
            {
                var renderer = proj.visualObject.GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    DestroyImmediate(renderer.sharedMaterial);
                }
                
                var trail = proj.visualObject.GetComponent<TrailRenderer>();
                if (trail != null && trail.sharedMaterial != null)
                {
                    DestroyImmediate(trail.sharedMaterial);
                }
                
                DestroyImmediate(proj.visualObject);
            }
        }
        
        previewProjectiles.Clear();
        homingPreviewTargets.Clear();
        isPreviewActive = false;
        previewTime = 0f;
    }
    
    private void GenerateRandomHomingTargets(int count = homingPreviewTargetCount, float minRadius = 4f, float maxRadius = 10f)
    {
        homingPreviewTargets.Clear();
        for (int i = 0; i < count; i++)
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float radius = UnityEngine.Random.Range(minRadius, maxRadius);
            float y = UnityEngine.Random.Range(-2f, 2f);
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
            homingPreviewTargets.Add(pos);
        }
    }
    
    private void SimulateFire()
    {
        if (selectedConfig == null) return;
        
        var firePoints = GenerateFirePoints();
        
        foreach (var firePoint in firePoints)
        {
            int projectileCount = 1;
            float[] spreadAngles = null;
            
            if (selectedConfig.fireModeConfig.mode == FireMode.Spread)
            {
                projectileCount = selectedConfig.fireModeConfig.spreadProjectileCount;
                spreadAngles = GenerateSpreadAngles();
            }
            
            for (int i = 0; i < projectileCount; i++)
            {
                Vector3 direction = firePoint.direction;
                
                if (spreadAngles != null && i < spreadAngles.Length)
                {
                    Quaternion spreadRot = Quaternion.Euler(0, 0, spreadAngles[i]);
                    direction = spreadRot * direction;
                }
                
                CreatePreviewProjectile(firePoint.position, direction);
            }
        }
    }
    
    private void CreatePreviewProjectile(Vector3 position, Vector3 direction)
    {
        var projConfig = selectedConfig.projectileConfig;
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.hideFlags = HideFlags.HideAndDontSave;
        visual.transform.position = position;
        visual.transform.localScale = Vector3.one * 0.1f;
        
        Color projColor = projConfig.type switch
        {
            ProjectileType.Ballistic => Color.yellow,
            ProjectileType.Homing => Color.magenta,
            _ => Color.cyan
        };
        
        var renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.hideFlags = HideFlags.HideAndDontSave;
            mat.color = projColor;
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Glossiness", 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", projColor * 0.5f);
            renderer.sharedMaterial = mat;
        }
        
        var trail = visual.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = 0.1f;
        trail.endWidth = 0.01f;
        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        trailMat.hideFlags = HideFlags.HideAndDontSave;
        trail.sharedMaterial = trailMat;
        trail.startColor = projColor;
        trail.endColor = new Color(projColor.r, projColor.g, projColor.b, 0f);
        
        // Pick random homing target
        Vector3 target = position;
        if (projConfig.type == ProjectileType.Homing && homingPreviewTargets.Count > 0)
            target = homingPreviewTargets[rand.Next(homingPreviewTargets.Count)];
        
        var projectile = new PreviewProjectile
        {
            position = position,
            velocity = direction * projConfig.speed,
            rotation = Quaternion.LookRotation(direction),
            lifetime = projConfig.unlimitedLifetime ? 999f : projConfig.lifetime,
            age = 0f,
            visualObject = visual,
            type = projConfig.type,
            gravityModifier = projConfig.gravityModifier,
            color = projColor,
            homingTarget = target
        };
        
        previewProjectiles.Add(projectile);
    }
    
    private void UpdateProjectiles(float deltaTime)
    {
        var projConfig = selectedConfig?.projectileConfig;
        if (projConfig == null) return;
        
        for (int i = previewProjectiles.Count - 1; i >= 0; i--)
        {
            var proj = previewProjectiles[i];
            
            proj.age += deltaTime;
            
            bool shouldDestroy = false;
            
            if (proj.age >= proj.lifetime)
            {
                shouldDestroy = true;
            }
            else if (projConfig.destroyOnDistanceTraveled)
            {
                float distance = proj.age * projConfig.speed;
                if (distance >= projConfig.maxDistance)
                {
                    shouldDestroy = true;
                }
            }
            
            if (shouldDestroy)
            {
                if (proj.visualObject != null)
                {
                    var renderer = proj.visualObject.GetComponent<Renderer>();
                    if (renderer != null && renderer.sharedMaterial != null)
                    {
                        DestroyImmediate(renderer.sharedMaterial);
                    }
                    
                    var trail = proj.visualObject.GetComponent<TrailRenderer>();
                    if (trail != null && trail.sharedMaterial != null)
                    {
                        DestroyImmediate(trail.sharedMaterial);
                    }
                    
                    DestroyImmediate(proj.visualObject);
                }
                previewProjectiles.RemoveAt(i);
                continue;
            }
            
            // Update physics based on type
            switch (proj.type)
            {
                case ProjectileType.Ballistic:
                    proj.velocity += Physics.gravity * proj.gravityModifier * deltaTime;
                    break;
                    
                case ProjectileType.Homing:
                    Vector3 toTarget = (proj.homingTarget - proj.position).normalized;
                    proj.velocity = Vector3.Lerp(proj.velocity, toTarget * projConfig.speed, projConfig.followStrength * deltaTime);
                    break;
            }
            
            proj.position += proj.velocity * deltaTime;
            
            if (proj.velocity.magnitude > 0.01f)
            {
                proj.rotation = Quaternion.LookRotation(proj.velocity);
            }
            
            if (proj.visualObject != null)
            {
                proj.visualObject.transform.position = proj.position;
                proj.visualObject.transform.rotation = proj.rotation;
            }
        }
    }
    
    private List<FirePointData> GenerateFirePoints()
    {
        List<FirePointData> points = new List<FirePointData>();
        
        if (selectedConfig.muzzleConfigs == null || selectedConfig.muzzleConfigs.Length == 0)
        {
            points.Add(new FirePointData { position = Vector3.right * 0.3f, direction = Vector3.right });
            return points;
        }
        
        foreach (var muzzleConfig in selectedConfig.muzzleConfigs)
        {
            Vector3 muzzlePos = muzzleConfig.localPosition;
            
            if (muzzleConfig.mode == MuzzleMode.Line)
            {
                float halfDistance = (muzzleConfig.pointsCount - 1) * muzzleConfig.distanceBetweenPoints / 2f;
                
                for (int i = 0; i < muzzleConfig.pointsCount; i++)
                {
                    Vector3 offset = Vector3.up * (halfDistance - i * muzzleConfig.distanceBetweenPoints);
                    Vector3 pointPos = muzzlePos + offset;
                    Vector3 direction = Vector3.Lerp(Vector3.right, offset.normalized, muzzleConfig.fireAngleModifier);
                    
                    points.Add(new FirePointData { position = pointPos, direction = direction.normalized });
                }
            }
            else if (muzzleConfig.mode == MuzzleMode.Circle)
            {
                float stepAngle = muzzleConfig.autoCalculateStepAngle ? 360f / muzzleConfig.pointsCount : muzzleConfig.stepAngle;
                float minAngle = -((muzzleConfig.pointsCount - 1) * stepAngle) / 2f;
                
                for (int i = 0; i < muzzleConfig.pointsCount; i++)
                {
                    float angleRad = Mathf.Deg2Rad * (minAngle + i * stepAngle);
                    Vector3 offset = new Vector3(0, Mathf.Sin(angleRad), Mathf.Cos(angleRad)) * muzzleConfig.radius;
                    Vector3 pointPos = muzzlePos + offset;
                    Vector3 direction = Vector3.Lerp(offset.normalized, Vector3.right, muzzleConfig.fireAngleModifier);
                    
                    points.Add(new FirePointData { position = pointPos, direction = direction.normalized });
                }
            }
        }
        
        return points;
    }
    
    private float[] GenerateSpreadAngles()
    {
        var spreadConfig = selectedConfig.fireModeConfig;
        int count = spreadConfig.spreadProjectileCount;
        float[] angles = new float[count];
        
        if (spreadConfig.randomSpread)
        {
            for (int i = 0; i < count; i++)
            {
                angles[i] = Random.Range(spreadConfig.minSpreadAngle, spreadConfig.maxSpreadAngle) * spreadConfig.spreadFactor;
            }
        }
        else
        {
            float step = (spreadConfig.maxSpreadAngle - spreadConfig.minSpreadAngle) / Mathf.Max(count - 1, 1);
            for (int i = 0; i < count; i++)
            {
                angles[i] = (spreadConfig.minSpreadAngle + step * i) * spreadConfig.spreadFactor;
            }
        }
        
        return angles;
    }
    
    private struct FirePointData
    {
        public Vector3 position;
        public Vector3 direction;
    }
    
    private void UpdatePreviewCamera()
    {
        if (previewRender == null) return;
        
        if (currentViewMode == ViewMode.View2D)
        {
            previewRender.camera.orthographic = true;
            previewRender.camera.orthographicSize = camera2DSize;
            previewRender.camera.transform.position = camera2DPosition;
            previewRender.camera.transform.rotation = Quaternion.identity;
        }
        else
        {
            previewRender.camera.orthographic = false;
            previewRender.camera.fieldOfView = 60f;
            
            float radX = previewCameraAngle.x * Mathf.Deg2Rad;
            float radY = previewCameraAngle.y * Mathf.Deg2Rad;
            
            Vector3 offset = new Vector3(
                Mathf.Sin(radY) * Mathf.Cos(radX),
                Mathf.Sin(radX),
                Mathf.Cos(radY) * Mathf.Cos(radX)
            ) * previewCameraDistance;
            
            previewRender.camera.transform.position = offset;
            previewRender.camera.transform.LookAt(Vector3.zero);
        }
    }
    
    private void DrawGrid()
    {
        if (!showGrid) return;
        
        if (currentViewMode == ViewMode.View2D)
        {
            int gridSize = 20;
            float gridSpacing = 1f;
            Color gridColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            Color axisColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            
            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 start = new Vector3(i * gridSpacing, -gridSize * gridSpacing, 0);
                Vector3 end = new Vector3(i * gridSpacing, gridSize * gridSpacing, 0);
                Color lineColor = (i == 0) ? axisColor : gridColor;
                Debug.DrawLine(start, end, lineColor);
            }
            
            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 start = new Vector3(-gridSize * gridSpacing, i * gridSpacing, 0);
                Vector3 end = new Vector3(gridSize * gridSpacing, i * gridSpacing, 0);
                Color lineColor = (i == 0) ? axisColor : gridColor;
                Debug.DrawLine(start, end, lineColor);
            }
        }
        else
        {
            int gridSize = 10;
            float gridSpacing = 1f;
            Color gridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            Color axisColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            
            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 start = new Vector3(-gridSize * gridSpacing, 0, i * gridSpacing);
                Vector3 end = new Vector3(gridSize * gridSpacing, 0, i * gridSpacing);
                Color lineColor = (i == 0) ? axisColor : gridColor;
                Debug.DrawLine(start, end, lineColor);
            }
            
            for (int i = -gridSize; i <= gridSize; i++)
            {
                Vector3 start = new Vector3(i * gridSpacing, 0, -gridSize * gridSpacing);
                Vector3 end = new Vector3(i * gridSpacing, 0, gridSize * gridSpacing);
                Color lineColor = (i == 0) ? axisColor : gridColor;
                Debug.DrawLine(start, end, lineColor);
            }
        }
    }
    
    private void RefreshConfigList()
    {
        allConfigs.Clear();
        
        string[] guids = AssetDatabase.FindAssets("t:WeaponConfig");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponConfig config = AssetDatabase.LoadAssetAtPath<WeaponConfig>(path);
            if (config != null)
            {
                allConfigs.Add(config);
            }
        }
        
        allConfigs = allConfigs.OrderBy(c => string.IsNullOrEmpty(c.weaponName) ? c.name : c.weaponName).ToList();
        FilterConfigs();
    }
    
    private void FilterConfigs()
    {
        if (string.IsNullOrEmpty(searchQuery))
        {
            filteredConfigs = new List<WeaponConfig>(allConfigs);
        }
        else
        {
            filteredConfigs = allConfigs.Where(c => 
                (c.weaponName != null && c.weaponName.ToLower().Contains(searchQuery.ToLower())) ||
                (c.name != null && c.name.ToLower().Contains(searchQuery.ToLower())) ||
                AssetDatabase.GetAssetPath(c).ToLower().Contains(searchQuery.ToLower())
            ).ToList();
        }
    }
    
    private void LoadRecentConfigs()
    {
        string saved = EditorPrefs.GetString(RECENT_CONFIGS_KEY, "");
        if (!string.IsNullOrEmpty(saved))
        {
            recentConfigPaths = saved.Split('|').ToList();
            recentConfigPaths.RemoveAll(string.IsNullOrEmpty);
        }
    }
    
    private void SaveRecentConfigs()
    {
        EditorPrefs.SetString(RECENT_CONFIGS_KEY, string.Join("|", recentConfigPaths));
    }
    
    private void AddToRecentConfigs(WeaponConfig config)
    {
        if (config == null) return;
        
        string path = AssetDatabase.GetAssetPath(config);
        recentConfigPaths.Remove(path);
        recentConfigPaths.Insert(0, path);
        
        if (recentConfigPaths.Count > MAX_RECENT_CONFIGS)
        {
            recentConfigPaths.RemoveRange(MAX_RECENT_CONFIGS, recentConfigPaths.Count - MAX_RECENT_CONFIGS);
        }
        
        SaveRecentConfigs();
    }
    
    private void SelectConfig(WeaponConfig config)
    {
        if (isPreviewActive)
        {
            StopPreview();
        }
        
        selectedConfig = config;
        AddToRecentConfigs(config);
        Repaint();
    }
    
    private void InitializeStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(0, 0, 10, 10)
            };
        }
        
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };
        }
        
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                padding = new RectOffset(10, 10, 8, 8)
            };
        }
        
        if (configButtonStyle == null)
        {
            configButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(0, 0, 2, 2)
            };
        }
        
        if (selectedConfigButtonStyle == null)
        {
            selectedConfigButtonStyle = new GUIStyle(configButtonStyle);
            var selectedTex = new Texture2D(1, 1);
            selectedTex.SetPixel(0, 0, new Color(0.3f, 0.6f, 1f, 0.3f));
            selectedTex.Apply();
            selectedConfigButtonStyle.normal.background = selectedTex;
            selectedConfigButtonStyle.fontStyle = FontStyle.Bold;
        }
    }
    
    private void OnGUI()
    {
        InitializeStyles();
        
        DrawToolbar();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        DrawConfigSelectorPanel();
        
        EditorGUILayout.BeginVertical();
        
        if (selectedConfig == null)
        {
            DrawNoConfigSelected();
        }
        else
        {
            DrawSplitView();
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawSplitView()
    {
        float totalHeight = position.height - 35;
        float topHeight = totalHeight * splitPosition;
        float bottomHeight = totalHeight * (1 - splitPosition);
        
        EditorGUILayout.BeginVertical(GUILayout.Height(topHeight));
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        DrawTabs();
        EditorGUILayout.Space(10);
        DrawSelectedTab();
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.EndVertical();
        
        Rect splitterRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(3), GUILayout.ExpandWidth(true));
        DrawSplitter(splitterRect);
        HandleSplitterEvents(splitterRect);
        
        if (showPreviewPanel)
        {
            EditorGUILayout.BeginVertical(GUILayout.Height(bottomHeight - 3));
            DrawPreviewPanel();
            EditorGUILayout.EndVertical();
        }
    }
    
    private void DrawSplitter(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);
    }
    
    private void HandleSplitterEvents(Rect rect)
    {
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            isResizingSplit = true;
            e.Use();
        }
        
        if (isResizingSplit)
        {
            if (e.type == EventType.MouseDrag)
            {
                float totalHeight = position.height - 35;
                splitPosition = Mathf.Clamp(e.mousePosition.y / totalHeight, 0.2f, 0.8f);
                Repaint();
                e.Use();
            }
            
            if (e.type == EventType.MouseUp)
            {
                isResizingSplit = false;
                e.Use();
            }
        }
    }
    
    private void DrawPreviewPanel()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Live Preview", EditorStyles.boldLabel);
        
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = currentViewMode == ViewMode.View2D ? new Color(0.5f, 0.8f, 1f) : Color.white;
        if (GUILayout.Button("2D", EditorStyles.miniButtonLeft, GUILayout.Width(40)))
        {
            currentViewMode = ViewMode.View2D;
            UpdatePreviewCamera();
        }
        
        GUI.backgroundColor = currentViewMode == ViewMode.View3D ? new Color(0.5f, 0.8f, 1f) : Color.white;
        if (GUILayout.Button("3D", EditorStyles.miniButtonRight, GUILayout.Width(40)))
        {
            currentViewMode = ViewMode.View3D;
            UpdatePreviewCamera();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(10);
        
        if (!isPreviewActive)
        {
            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("▶ Start", GUILayout.Width(70), GUILayout.Height(25)))
            {
                StartPreview();
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("⬛ Stop", GUILayout.Width(70), GUILayout.Height(25)))
            {
                StopPreview();
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                foreach (var proj in previewProjectiles)
                {
                    if (proj.visualObject != null)
                    {
                        var renderer = proj.visualObject.GetComponent<Renderer>();
                        if (renderer != null && renderer.sharedMaterial != null)
                        {
                            DestroyImmediate(renderer.sharedMaterial);
                        }
                        
                        var trail = proj.visualObject.GetComponent<TrailRenderer>();
                        if (trail != null && trail.sharedMaterial != null)
                        {
                            DestroyImmediate(trail.sharedMaterial);
                        }
                        
                        DestroyImmediate(proj.visualObject);
                    }
                }
                previewProjectiles.Clear();
                // Regenerate homing targets when cleared
                GenerateRandomHomingTargets();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (isPreviewActive && previewRender != null)
        {
            EditorGUILayout.Space(5);
            
            Rect previewRect = GUILayoutUtility.GetRect(100, 200, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            UpdatePreviewCamera();
            
            if (showGrid)
            {
                DrawGrid();
            }
            
            previewRender.BeginPreview(previewRect, GUIStyle.none);
            
            if (previewWeaponObject != null)
            {
                previewRender.DrawMesh(
                    previewWeaponObject.GetComponent<MeshFilter>().sharedMesh,
                    previewWeaponObject.transform.localToWorldMatrix,
                    previewWeaponObject.GetComponent<Renderer>().sharedMaterial,
                    0
                );
            }
            
            foreach (var proj in previewProjectiles)
            {
                if (proj.visualObject != null)
                {
                    var meshFilter = proj.visualObject.GetComponent<MeshFilter>();
                    var renderer = proj.visualObject.GetComponent<Renderer>();
                    
                    if (meshFilter != null && renderer != null)
                    {
                        previewRender.DrawMesh(
                            meshFilter.sharedMesh,
                            proj.visualObject.transform.localToWorldMatrix,
                            renderer.sharedMaterial,
                            0
                        );
                    }
                }
            }
            
            previewRender.camera.Render();
            Texture resultRender = previewRender.EndPreview();
            GUI.DrawTexture(previewRect, resultRender, ScaleMode.StretchToFill, false);
            
            // Draw homing targets visualization (FIXED - moved after preview render)
            if (selectedConfig.projectileConfig.type == ProjectileType.Homing && homingPreviewTargets.Count > 0)
            {
                foreach (var pt in homingPreviewTargets)
                {
                    Vector3 screen = previewRender.camera.WorldToScreenPoint(pt);
                    if (screen.z > 0)
                    {
                        var pointRect = new Rect(screen.x + previewRect.x - 4, previewRect.y + (previewRect.height - screen.y) - 4, 8, 8);
                        EditorGUI.DrawRect(pointRect, new Color(1f, 0.5f, 1f, 0.9f));
                    }
                }
            }
            
            HandlePreviewInput(previewRect);
            
            GUI.color = new Color(1, 1, 1, 0.9f);
            string controlsText = currentViewMode == ViewMode.View2D ? "Drag: Pan | Scroll: Zoom" : "Drag: Rotate | Scroll: Zoom";
                
            GUI.Label(new Rect(previewRect.x + 5, previewRect.y + 5, 250, 60), 
                $"Time: {previewTime:F1}s | Projectiles: {previewProjectiles.Count}\n" +
                $"{controlsText} | Right-click: Fire", 
                EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            autoFire = EditorGUILayout.ToggleLeft("Auto Fire", autoFire, GUILayout.Width(90));
            
            if (!autoFire && GUILayout.Button("Fire", GUILayout.Width(50)))
            {
                SimulateFire();
            }
            
            EditorGUILayout.Space(60);
            GUILayout.Label("Speed:", GUILayout.Width(45));
            timeScale = EditorGUILayout.Slider(timeScale, 0.1f, 3f, GUILayout.Width(150));
            
            if (EditorGUI.EndChangeCheck())
            {
                if (previewRender != null)
                {
                    previewRender.camera.backgroundColor = backgroundColor;
                }
                UpdatePreviewCamera();
            }
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Preview inactive\nClick 'Start' to begin", EditorStyles.centeredGreyMiniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void HandlePreviewInput(Rect previewRect)
    {
        Event e = Event.current;
        
        if (previewRect.Contains(e.mousePosition))
        {
            if (currentViewMode == ViewMode.View3D)
            {
                if (e.type == EventType.MouseDrag && e.button == 0)
                {
                    Vector2 delta = e.mousePosition - lastMousePosition;
                    previewCameraAngle.y += delta.x * 0.5f;
                    previewCameraAngle.x -= delta.y * 0.5f;
                    previewCameraAngle.x = Mathf.Clamp(previewCameraAngle.x, -89f, 89f);
                    UpdatePreviewCamera();
                    e.Use();
                }
                
                if (e.type == EventType.ScrollWheel)
                {
                    previewCameraDistance += e.delta.y * 0.5f;
                    previewCameraDistance = Mathf.Clamp(previewCameraDistance, 2f, 15f);
                    UpdatePreviewCamera();
                    e.Use();
                }
            }
            else
            {
                if (e.type == EventType.MouseDrag && e.button == 0)
                {
                    Vector2 delta = e.mousePosition - lastMousePosition;
                    camera2DPosition.x -= delta.x * 0.01f * camera2DSize;
                    camera2DPosition.y += delta.y * 0.01f * camera2DSize;
                    UpdatePreviewCamera();
                    e.Use();
                }
                
                if (e.type == EventType.ScrollWheel)
                {
                    camera2DSize += e.delta.y * 0.2f;
                    camera2DSize = Mathf.Clamp(camera2DSize, 1f, 20f);
                    UpdatePreviewCamera();
                    e.Use();
                }
            }
            
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                SimulateFire();
                e.Use();
            }
        }
        
        lastMousePosition = e.mousePosition;
    }
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        GUILayout.Label("⚔️ Weapon Config Editor", EditorStyles.boldLabel);
        
        GUILayout.FlexibleSpace();
        
        if (selectedConfig != null)
        {
            GUILayout.Label($"Editing: {selectedConfig.weaponName}", EditorStyles.miniLabel);
            
            GUILayout.Space(10);
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("💾 Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                SaveConfig();
            }
            GUI.backgroundColor = Color.white;
        }
        
        if (isPreviewActive)
        {
            GUILayout.Label("🟢", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawConfigSelectorPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label("Weapon Configs", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        string newSearchQuery = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField);
        if (newSearchQuery != searchQuery)
        {
            searchQuery = newSearchQuery;
            FilterConfigs();
        }
        
        if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
        {
            searchQuery = "";
            FilterConfigs();
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", EditorStyles.miniButton))
        {
            RefreshConfigList();
        }
        
        if (GUILayout.Button("New", EditorStyles.miniButton))
        {
            CreateNewConfig();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(5);
        
        if (recentConfigPaths.Count > 0)
        {
            EditorGUILayout.BeginVertical(boxStyle);
            GUILayout.Label("Recent", EditorStyles.miniBoldLabel);
            
            List<string> pathsCopy = new List<string>(recentConfigPaths);
            List<string> validPaths = new List<string>();
            
            foreach (string path in pathsCopy)
            {
                WeaponConfig config = AssetDatabase.LoadAssetAtPath<WeaponConfig>(path);
                if (config != null)
                {
                    validPaths.Add(path);
                    DrawConfigButton(config, true);
                }
            }
            
            if (validPaths.Count != recentConfigPaths.Count)
            {
                recentConfigPaths = validPaths;
                SaveRecentConfigs();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label($"All Configs ({filteredConfigs.Count})", EditorStyles.miniBoldLabel);
        
        configListScrollPosition = EditorGUILayout.BeginScrollView(configListScrollPosition, GUILayout.ExpandHeight(true));
        
        if (filteredConfigs.Count == 0)
        {
            EditorGUILayout.HelpBox("No configs found.\nCreate one or click Refresh!", MessageType.Info);
        }
        else
        {
            List<WeaponConfig> configsCopy = new List<WeaponConfig>(filteredConfigs);
            foreach (var config in configsCopy)
            {
                DrawConfigButton(config, false);
            }
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawConfigButton(WeaponConfig config, bool isRecent)
    {
        bool isSelected = selectedConfig == config;
        GUIStyle style = isSelected ? selectedConfigButtonStyle : configButtonStyle;
        
        EditorGUILayout.BeginHorizontal();
        
        string displayName = string.IsNullOrEmpty(config.weaponName) ? config.name : config.weaponName;
        string buttonText = isRecent ? $"★ {displayName}" : displayName;
        
        if (GUILayout.Button(buttonText, style))
        {
            SelectConfig(config);
        }
        
        if (GUILayout.Button("•", EditorStyles.miniButton, GUILayout.Width(25)))
        {
            EditorGUIUtility.PingObject(config);
            Selection.activeObject = config;
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawNoConfigSelected()
    {
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.BeginVertical();
        
        GUILayout.Label("No Config Selected", headerStyle);
        EditorGUILayout.Space(10);
        
        GUILayout.Label("← Select a config from the left panel", EditorStyles.label);
        EditorGUILayout.Space(20);
        
        GUILayout.Label("Or create a new one:", EditorStyles.label);
        EditorGUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create New Config", GUILayout.Height(40), GUILayout.Width(200)))
        {
            CreateNewConfig();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        GUILayout.FlexibleSpace();
    }
    
    private void DrawTabs()
    {
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
    }
    
    private void DrawSelectedTab()
    {
        switch (selectedTab)
        {
            case 0: DrawGeneralTab(); break;
            case 1: DrawFireModeTab(); break;
            case 2: DrawMuzzlesTab(); break;
            case 3: DrawProjectileTab(); break;
            case 4: DrawAmmoTab(); break;
            case 5: DrawRecoilTab(); break;
            case 6: DrawAudioTab(); break;
            case 7: DrawAimTab(); break;
        }
    }
    
    private void DrawGeneralTab()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label("General Settings", headerStyle);
        
        selectedConfig.weaponName = EditorGUILayout.TextField("Weapon Name", selectedConfig.weaponName);
        selectedConfig.weaponIcon = (Sprite)EditorGUILayout.ObjectField("Weapon Icon", selectedConfig.weaponIcon, typeof(Sprite), false);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Asset Info", EditorStyles.boldLabel);
        string assetPath = AssetDatabase.GetAssetPath(selectedConfig);
        EditorGUILayout.LabelField("Path:", assetPath, EditorStyles.wordWrappedMiniLabel);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawFireModeTab()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label("Fire Mode Configuration", headerStyle);
        
        if (selectedConfig.fireModeConfig == null)
            selectedConfig.fireModeConfig = new FireModeConfig();
        
        var fireConfig = selectedConfig.fireModeConfig;
        
        fireConfig.mode = (FireMode)EditorGUILayout.EnumPopup("Fire Mode", fireConfig.mode);
        fireConfig.fireRate = EditorGUILayout.FloatField("Fire Rate", fireConfig.fireRate);
        fireConfig.autoShooting = EditorGUILayout.Toggle("Auto Shooting", fireConfig.autoShooting);
        
        EditorGUILayout.Space(10);
        
        switch (fireConfig.mode)
        {
            case FireMode.Burst:
                DrawBurstSettings(fireConfig);
                break;
            case FireMode.Spread:
                DrawSpreadSettings(fireConfig);
                break;
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawBurstSettings(FireModeConfig config)
    {
        EditorGUILayout.LabelField("Burst Settings", EditorStyles.boldLabel);
        config.burstCount = EditorGUILayout.IntField("Burst Count", Mathf.Max(1, config.burstCount));
        config.timeBetweenShots = EditorGUILayout.FloatField("Time Between Shots", config.timeBetweenShots);
        config.timeBetweenBursts = EditorGUILayout.FloatField("Time Between Bursts", config.timeBetweenBursts);
    }
    
    private void DrawSpreadSettings(FireModeConfig config)
    {
        EditorGUILayout.LabelField("Spread Settings", EditorStyles.boldLabel);
        config.spreadProjectileCount = EditorGUILayout.IntField("Projectile Count", Mathf.Max(1, config.spreadProjectileCount));
        config.randomSpread = EditorGUILayout.Toggle("Random Spread", config.randomSpread);
        config.minSpreadAngle = EditorGUILayout.FloatField("Min Spread Angle", config.minSpreadAngle);
        config.maxSpreadAngle = EditorGUILayout.FloatField("Max Spread Angle", config.maxSpreadAngle);
        config.spreadFactor = EditorGUILayout.Slider("Spread Factor", config.spreadFactor, 0f, 1f);
    }
    
    private void DrawMuzzlesTab()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label("Muzzle Configuration", headerStyle);
        
        if (selectedConfig.muzzleConfigs == null)
            selectedConfig.muzzleConfigs = new MuzzleConfig[0];
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Muzzles: {selectedConfig.muzzleConfigs.Length}", EditorStyles.boldLabel);
        
        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            ArrayUtility.Add(ref selectedConfig.muzzleConfigs, new MuzzleConfig());
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        for (int i = 0; i < selectedConfig.muzzleConfigs.Length; i++)
        {
            DrawMuzzleConfig(ref selectedConfig.muzzleConfigs[i], i);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawMuzzleConfig(ref MuzzleConfig config, int index)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Muzzle {index}", EditorStyles.boldLabel);
        
        if (GUILayout.Button("X", GUILayout.Width(30)))
        {
            ArrayUtility.RemoveAt(ref selectedConfig.muzzleConfigs, index);
            return;
        }
        
        EditorGUILayout.EndHorizontal();
        
        config.mode = (MuzzleMode)EditorGUILayout.EnumPopup("Mode", config.mode);
        config.pointsCount = EditorGUILayout.IntField("Points Count", Mathf.Max(1, config.pointsCount));
        
        if (config.mode == MuzzleMode.Line)
        {
            config.distanceBetweenPoints = EditorGUILayout.FloatField("Distance Between Points", Mathf.Max(0.0001f, config.distanceBetweenPoints));
        }
        else if (config.mode == MuzzleMode.Circle)
        {
            config.radius = EditorGUILayout.FloatField("Radius", Mathf.Max(0.0001f, config.radius));
            config.autoCalculateStepAngle = EditorGUILayout.Toggle("Auto Calculate Step Angle", config.autoCalculateStepAngle);
            if (!config.autoCalculateStepAngle)
            {
                config.stepAngle = EditorGUILayout.FloatField("Step Angle", Mathf.Max(0.0001f, config.stepAngle));
            }
        }
        
        config.fireAngleModifier = EditorGUILayout.Slider("Fire Angle Modifier", config.fireAngleModifier, 0f, 1f);
        config.localPosition = EditorGUILayout.Vector3Field("Local Position", config.localPosition);
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private void DrawProjectileTab()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label("Projectile Configuration", headerStyle);
        
        if (selectedConfig.projectileConfig == null)
            selectedConfig.projectileConfig = new ProjectileConfig();
        
        var projConfig = selectedConfig.projectileConfig;
        
        projConfig.projectilePrefab = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab", projConfig.projectilePrefab, typeof(GameObject), false);
        
        if (projConfig.projectilePrefab != null && projConfig.projectilePrefab.GetComponent<Projectile>() == null)
        {
            EditorGUILayout.HelpBox("Warning: Projectile prefab doesn't have Projectile component!", MessageType.Warning);
        }
        
        projConfig.type = (ProjectileType)EditorGUILayout.EnumPopup("Projectile Type", projConfig.type);
        projConfig.speed = EditorGUILayout.FloatField("Speed", projConfig.speed);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Lifetime Settings", EditorStyles.boldLabel);
        projConfig.unlimitedLifetime = EditorGUILayout.Toggle("Unlimited Lifetime", projConfig.unlimitedLifetime);
        if (!projConfig.unlimitedLifetime)
        {
            projConfig.lifetime = EditorGUILayout.FloatField("Lifetime", projConfig.lifetime);
        }
        
        projConfig.destroyOnDistanceTraveled = EditorGUILayout.Toggle("Destroy On Distance", projConfig.destroyOnDistanceTraveled);
        if (projConfig.destroyOnDistanceTraveled)
        {
            projConfig.maxDistance = EditorGUILayout.FloatField("Max Distance", projConfig.maxDistance);
        }
        
        EditorGUILayout.Space(10);
        
        switch (projConfig.type)
        {
            case ProjectileType.Ballistic:
                EditorGUILayout.LabelField("Ballistic Settings", EditorStyles.boldLabel);
                projConfig.gravityModifier = EditorGUILayout.FloatField("Gravity Modifier", projConfig.gravityModifier);
                break;
            case ProjectileType.Homing:
                EditorGUILayout.LabelField("Homing Settings", EditorStyles.boldLabel);
                projConfig.followStrength = EditorGUILayout.FloatField("Follow Strength", projConfig.followStrength);
                break;
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Damage Settings", EditorStyles.boldLabel);
        projConfig.damage = EditorGUILayout.IntField("Damage", projConfig.damage);
        projConfig.damageToStructure = EditorGUILayout.IntField("Damage To Structure", projConfig.damageToStructure);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
        projConfig.impactEffect = (GameObject)EditorGUILayout.ObjectField("Impact Effect", projConfig.impactEffect, typeof(GameObject), false);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawAmmoTab()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label("Ammo Configuration", headerStyle);
        
        if (selectedConfig.ammoConfig == null)
            selectedConfig.ammoConfig = new AmmoConfig();
        
        var ammoConfig = selectedConfig.ammoConfig;
        
        ammoConfig.unlimitedAmmo = EditorGUILayout.Toggle("Unlimited Ammo", ammoConfig.unlimitedAmmo);
        if (!ammoConfig.unlimitedAmmo)
        {
            ammoConfig.maxAmmo = EditorGUILayout.IntField("Max Ammo", ammoConfig.maxAmmo);
        }
        
        ammoConfig.unlimitedMagazineSize = EditorGUILayout.Toggle("Unlimited Magazine", ammoConfig.unlimitedMagazineSize);
        if (!ammoConfig.unlimitedMagazineSize)
        {
            ammoConfig.magazineSize = EditorGUILayout.IntField("Magazine Size", ammoConfig.magazineSize);
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Reload Settings", EditorStyles.boldLabel);
        ammoConfig.reloadTime = EditorGUILayout.FloatField("Reload Time", ammoConfig.reloadTime);
        ammoConfig.autoReload = EditorGUILayout.Toggle("Auto Reload", ammoConfig.autoReload);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Consumption Settings", EditorStyles.boldLabel);
        ammoConfig.partialAmmoFire = EditorGUILayout.Toggle("Partial Ammo Fire", ammoConfig.partialAmmoFire);
        ammoConfig.countEachProjectile = EditorGUILayout.Toggle("Count Each Projectile", ammoConfig.countEachProjectile);
        ammoConfig.ammoPerShot = EditorGUILayout.IntField("Ammo Per Shot", ammoConfig.ammoPerShot);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawRecoilTab()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label("Recoil Configuration", headerStyle);
        
        if (selectedConfig.recoilConfig == null)
            selectedConfig.recoilConfig = new RecoilConfig();
        
        var recoilConfig = selectedConfig.recoilConfig;
        
        recoilConfig.enabled = EditorGUILayout.Toggle("Enabled", recoilConfig.enabled);
        
        if (recoilConfig.enabled)
        {
            recoilConfig.force = EditorGUILayout.FloatField("Force", recoilConfig.force);
            recoilConfig.recoveryTime = EditorGUILayout.FloatField("Recovery Time", Mathf.Max(0, recoilConfig.recoveryTime));
            recoilConfig.recoilCurve = EditorGUILayout.CurveField("Recoil Curve", recoilConfig.recoilCurve);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawAudioTab()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label("Audio Configuration", headerStyle);
        
        if (selectedConfig.audioConfig == null)
            selectedConfig.audioConfig = new AudioConfig();
        
        var audioConfig = selectedConfig.audioConfig;
        
        audioConfig.shootSound = (AudioClip)EditorGUILayout.ObjectField("Shoot Sound", audioConfig.shootSound, typeof(AudioClip), false);
        audioConfig.shootVolume = EditorGUILayout.Slider("Shoot Volume", audioConfig.shootVolume, 0f, 1f);
        
        EditorGUILayout.Space(5);
        
        audioConfig.reloadSound = (AudioClip)EditorGUILayout.ObjectField("Reload Sound", audioConfig.reloadSound, typeof(AudioClip), false);
        audioConfig.reloadVolume = EditorGUILayout.Slider("Reload Volume", audioConfig.reloadVolume, 0f, 1f);
        
        EditorGUILayout.Space(5);
        
        audioConfig.noAmmoSound = (AudioClip)EditorGUILayout.ObjectField("No Ammo Sound", audioConfig.noAmmoSound, typeof(AudioClip), false);
        audioConfig.noAmmoVolume = EditorGUILayout.Slider("No Ammo Volume", audioConfig.noAmmoVolume, 0f, 1f);
        audioConfig.noAmmoSoundCooldown = EditorGUILayout.FloatField("No Ammo Cooldown", audioConfig.noAmmoSoundCooldown);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawAimTab()
    {
        EditorGUILayout.BeginVertical(boxStyle);
        GUILayout.Label("Aim Configuration", headerStyle);
        
        if (selectedConfig.aimConfig == null)
            selectedConfig.aimConfig = new AimConfig();
        
        var aimConfig = selectedConfig.aimConfig;
        
        aimConfig.enabled = EditorGUILayout.Toggle("Enabled", aimConfig.enabled);
        
        if (aimConfig.enabled)
        {
            aimConfig.aimAtCursor = EditorGUILayout.Toggle("Aim At Cursor", aimConfig.aimAtCursor);
            aimConfig.rotateSpeed = EditorGUILayout.FloatField("Rotate Speed", aimConfig.rotateSpeed);
            aimConfig.rotationRadius = EditorGUILayout.FloatField("Rotation Radius", aimConfig.rotationRadius);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void CreateNewConfig()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create Weapon Config", "NewWeaponConfig", "asset", "Enter a name for the new config");
        if (!string.IsNullOrEmpty(path))
        {
            selectedConfig = CreateInstance<WeaponConfig>();
            selectedConfig.weaponName = "New Weapon";
            
            selectedConfig.fireModeConfig = new FireModeConfig();
            selectedConfig.muzzleConfigs = new MuzzleConfig[] { new MuzzleConfig() };
            selectedConfig.projectileConfig = new ProjectileConfig();
            selectedConfig.ammoConfig = new AmmoConfig();
            selectedConfig.recoilConfig = new RecoilConfig();
            selectedConfig.audioConfig = new AudioConfig();
            selectedConfig.aimConfig = new AimConfig();
            
            AssetDatabase.CreateAsset(selectedConfig, path);
            AssetDatabase.SaveAssets();
            
            RefreshConfigList();
            AddToRecentConfigs(selectedConfig);
            
            EditorUtility.DisplayDialog("Success", "New config created successfully!", "OK");
        }
    }
    
    private void SaveConfig()
    {
        if (selectedConfig == null) return;
        
        EditorUtility.SetDirty(selectedConfig);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", $"'{selectedConfig.weaponName}' saved!", "OK");
    }
}
