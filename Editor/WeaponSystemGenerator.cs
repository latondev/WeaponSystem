using UnityEngine;
using UnityEditor;
using System.IO;

namespace PullGame
{
	public class WeaponSystemGenerator : EditorWindow
	{
	    private string targetFolder = "Assets/Scripts/WeaponSystem";
	
	    [MenuItem("Tools/Generate Weapon System")]
	    public static void ShowWindow()
	    {
	        GetWindow<WeaponSystemGenerator>("Weapon System Generator");
	    }
	
	    private void OnGUI()
	    {
	        GUILayout.Label("Generate Complete Weapon System", EditorStyles.boldLabel);
	
	        EditorGUILayout.Space();
	        targetFolder = EditorGUILayout.TextField("Target Folder:", targetFolder);
	
	        EditorGUILayout.Space();
	        if (GUILayout.Button("Generate All Files", GUILayout.Height(40)))
	        {
	            GenerateAllFiles();
	        }
	
	        EditorGUILayout.Space();
	        EditorGUILayout.HelpBox(
	            "This will create all weapon system files in the specified folder.\n" +
	            "Total files: 22 C# scripts",
	            MessageType.Info
	        );
	    }
	
	    private void GenerateAllFiles()
	    {
	        if (!Directory.Exists(targetFolder))
	        {
	            Directory.CreateDirectory(targetFolder);
	        }
	
	        // Create subfolders
	        string configFolder = Path.Combine(targetFolder, "Configs");
	        string systemFolder = Path.Combine(targetFolder, "Systems");
	        string fireModeFolder = Path.Combine(targetFolder, "FireModes");
	        string projectileFolder = Path.Combine(targetFolder, "Projectiles");
	
	        Directory.CreateDirectory(configFolder);
	        Directory.CreateDirectory(systemFolder);
	        Directory.CreateDirectory(fireModeFolder);
	        Directory.CreateDirectory(projectileFolder);
	
	        // Generate all files
	        GenerateWeaponConfig(configFolder);
	        GenerateFireModeConfig(configFolder);
	        GenerateMuzzleConfig(configFolder);
	        GenerateProjectileConfig(configFolder);
	        GenerateAmmoConfig(configFolder);
	        GenerateRecoilConfig(configFolder);
	        GenerateAudioConfig(configFolder);
	        GenerateAimConfig(configFolder);
	
	        GenerateWeapon(targetFolder);
	        GenerateMuzzleSystem(systemFolder);
	        GenerateAmmoSystem(systemFolder);
	        GenerateRecoilSystem(systemFolder);
	        GenerateAimSystem(systemFolder);
	        GenerateAudioSystem(systemFolder);
	
	        GenerateIWeaponFireMode(fireModeFolder);
	        GenerateSingleFireMode(fireModeFolder);
	        GenerateAutoFireMode(fireModeFolder);
	        GenerateBurstFireMode(fireModeFolder);
	        GenerateSpreadFireMode(fireModeFolder);
	
	        GenerateProjectile(projectileFolder);
	        GenerateIDamageable(projectileFolder);
	
	        AssetDatabase.Refresh();
	        EditorUtility.DisplayDialog("Success", "All weapon system files generated successfully!", "OK");
	    }
	
	    private void GenerateWeaponConfig(string folder)
	    {
	        string content = @"using UnityEngine;
	
	[CreateAssetMenu(fileName = ""WeaponConfig"", menuName = ""Weapon System/Weapon Config"")]
	public class WeaponConfig : ScriptableObject
	{
	    [Header(""Identity"")]
	    public string weaponName;
	    public Sprite weaponIcon;
	    
	    [Header(""Fire Configuration"")]
	    public FireModeConfig fireModeConfig;
	    public MuzzleConfig[] muzzleConfigs;
	    
	    [Header(""Projectile Configuration"")]
	    public ProjectileConfig projectileConfig;
	    
	    [Header(""Ammo Configuration"")]
	    public AmmoConfig ammoConfig;
	    
	    [Header(""Recoil Configuration"")]
	    public RecoilConfig recoilConfig;
	    
	    [Header(""Audio Configuration"")]
	    public AudioConfig audioConfig;
	    
	    [Header(""Aim Configuration"")]
	    public AimConfig aimConfig;
	}";
	        File.WriteAllText(Path.Combine(folder, "WeaponConfig.cs"), content);
	    }
	
	    private void GenerateFireModeConfig(string folder)
	    {
	        string content = @"using System;
	using UnityEngine;
	
	[Serializable]
	public class FireModeConfig
	{
	    public FireMode mode;
	    public float fireRate = 0.1f;
	    
	    [Min(1)] public int burstCount = 3;
	    public float timeBetweenShots = 0.1f;
	    public float timeBetweenBursts = 0.5f;
	    
	    [Min(1)] public int spreadProjectileCount = 5;
	    public bool randomSpread = true;
	    public float minSpreadAngle = -30f;
	    public float maxSpreadAngle = 30f;
	    [Range(0, 1)] public float spreadFactor = 1f;
	    
	    public bool autoShooting = false;
	}
	
	public enum FireMode
	{
	    Single,
	    Auto,
	    Burst,
	    Spread
	}";
	        File.WriteAllText(Path.Combine(folder, "FireModeConfig.cs"), content);
	    }
	
	    private void GenerateMuzzleConfig(string folder)
	    {
	        string content = @"using System;
	using UnityEngine;
	
	[Serializable]
	public class MuzzleConfig
	{
	    public MuzzleMode mode;
	    [Min(1)] public int pointsCount = 1;
	    
	    [Min(0.0001f)] public float distanceBetweenPoints = 0.5f;
	    
	    [Min(0.0001f)] public float radius = 1f;
	    public bool autoCalculateStepAngle = true;
	    [Min(0.0001f)] public float stepAngle = 45f;
	    
	    [Range(0, 1)] public float fireAngleModifier = 0f;
	    public Vector3 localPosition;
	    public Quaternion localRotation = Quaternion.identity;
	}
	
	public enum MuzzleMode
	{
	    Line,
	    Circle
	}";
	        File.WriteAllText(Path.Combine(folder, "MuzzleConfig.cs"), content);
	    }
	
	    private void GenerateProjectileConfig(string folder)
	    {
	        string content = @"using System;
	using UnityEngine;
	
	[Serializable]
	public class ProjectileConfig
	{
	    public GameObject projectilePrefab;
	    public ProjectileType type;
	    public float speed = 10f;
	    
	    public bool unlimitedLifetime = false;
	    public float lifetime = 5f;
	    
	    public bool destroyOnDistanceTraveled = false;
	    public float maxDistance = 100f;
	    
	    public float gravityModifier = 1f;
	    public float followStrength = 5f;
	    
	    public int damage = 10;
	    public int damageToStructure = 5;
	    
	    public GameObject impactEffect;
	}
	
	public enum ProjectileType
	{
	    Normal,
	    Ballistic,
	    Homing
	}";
	        File.WriteAllText(Path.Combine(folder, "ProjectileConfig.cs"), content);
	    }
	
	    private void GenerateAmmoConfig(string folder)
	    {
	        string content = @"using System;
	
	[Serializable]
	public class AmmoConfig
	{
	    public bool unlimitedAmmo = false;
	    public int maxAmmo = 300;
	    
	    public bool unlimitedMagazineSize = false;
	    public int magazineSize = 30;
	    
	    public float reloadTime = 2f;
	    public bool autoReload = true;
	    
	    public bool partialAmmoFire = false;
	    public bool countEachProjectile = true;
	    public int ammoPerShot = 1;
	}";
	        File.WriteAllText(Path.Combine(folder, "AmmoConfig.cs"), content);
	    }
	
	    private void GenerateRecoilConfig(string folder)
	    {
	        string content = @"using System;
	using UnityEngine;
	
	[Serializable]
	public class RecoilConfig
	{
	    public bool enabled = true;
	    public float force = 5f;
	    [Min(0)] public float recoveryTime = 0.5f;
	    public AnimationCurve recoilCurve = AnimationCurve.Linear(0, 0, 1, 1);
	}";
	        File.WriteAllText(Path.Combine(folder, "RecoilConfig.cs"), content);
	    }
	
	    private void GenerateAudioConfig(string folder)
	    {
	        string content = @"using System;
	using UnityEngine;
	
	[Serializable]
	public class AudioConfig
	{
	    public AudioClip shootSound;
	    public AudioClip reloadSound;
	    public AudioClip noAmmoSound;
	    
	    [Range(0, 1)] public float shootVolume = 1f;
	    [Range(0, 1)] public float reloadVolume = 1f;
	    [Range(0, 1)] public float noAmmoVolume = 1f;
	    
	    public float noAmmoSoundCooldown = 0.5f;
	}";
	        File.WriteAllText(Path.Combine(folder, "AudioConfig.cs"), content);
	    }
	
	    private void GenerateAimConfig(string folder)
	    {
	        string content = @"using System;
	using UnityEngine;
	
	[Serializable]
	public class AimConfig
	{
	    public bool enabled = false;
	    public bool aimAtCursor = true;
	    public float rotateSpeed = 10f;
	    public float rotationRadius = 1f;
	}";
	        File.WriteAllText(Path.Combine(folder, "AimConfig.cs"), content);
	    }
	
	    private void GenerateWeapon(string folder)
	    {
	        string content = @"using UnityEngine;
	using UnityEngine.Events;
	
	public class Weapon : MonoBehaviour
	{
	    [Header(""Configuration"")]
	    [SerializeField] private WeaponConfig config;
	    
	    [Header(""Input"")]
	    [SerializeField] private bool usePlayerInput = true;
	    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
	    [SerializeField] private KeyCode reloadKey = KeyCode.R;
	    
	    [Header(""Events"")]
	    public UnityEvent onShoot;
	    public UnityEvent onReloadStart;
	    public UnityEvent onReloadEnd;
	    
	    private IWeaponFireMode fireMode;
	    private MuzzleSystem muzzleSystem;
	    private AmmoSystem ammoSystem;
	    private RecoilSystem recoilSystem;
	    private AimSystem aimSystem;
	    private AudioSystem audioSystem;
	    
	    private bool fireInputHold;
	    private bool fireInputDown;
	    private Transform projectileTarget;
	    
	    public WeaponConfig Config => config;
	    public AmmoSystem Ammo => ammoSystem;
	    public bool IsReloading => ammoSystem.IsReloading;
	    
	    private void Awake()
	    {
	        InitializeSystems();
	    }
	    
	    private void Start()
	    {
	        if (config == null)
	        {
	            Debug.LogError(""Weapon config is not assigned!"", this);
	            enabled = false;
	            return;
	        }
	        
	        muzzleSystem.Initialize();
	    }
	    
	    private void Update()
	    {
	        HandleInput();
	        UpdateSystems();
	    }
	    
	    private void InitializeSystems()
	    {
	        muzzleSystem = new MuzzleSystem(transform, config);
	        ammoSystem = new AmmoSystem(config.ammoConfig);
	        recoilSystem = new RecoilSystem(config.recoilConfig);
	        aimSystem = new AimSystem(transform, config.aimConfig);
	        audioSystem = new AudioSystem(gameObject, config.audioConfig);
	        
	        fireMode = CreateFireMode();
	        
	        ammoSystem.OnReloadStart += () => {
	            onReloadStart?.Invoke();
	            StartCoroutine(ammoSystem.PerformReload(this));
	        };
	        ammoSystem.OnReloadEnd += () => onReloadEnd?.Invoke();
	    }
	    
	    private IWeaponFireMode CreateFireMode()
	    {
	        return config.fireModeConfig.mode switch
	        {
	            FireMode.Single => new SingleFireMode(config.fireModeConfig),
	            FireMode.Auto => new AutoFireMode(config.fireModeConfig),
	            FireMode.Burst => new BurstFireMode(config.fireModeConfig, this),
	            FireMode.Spread => new SpreadFireMode(config.fireModeConfig),
	            _ => new SingleFireMode(config.fireModeConfig)
	        };
	    }
	    
	    private void HandleInput()
	    {
	        if (!usePlayerInput) return;
	        
	        fireInputHold = Input.GetKey(fireKey);
	        fireInputDown = Input.GetKeyDown(fireKey);
	        
	        if (Input.GetKeyDown(reloadKey))
	        {
	            TryReload();
	        }
	    }
	    
	    private void UpdateSystems()
	    {
	        fireMode.Update(Time.deltaTime);
	        recoilSystem.Update(Time.deltaTime);
	        aimSystem.Update(Time.deltaTime);
	        audioSystem.Update(Time.deltaTime);
	        
	        if (ShouldAttemptFire() && fireMode.CanFire())
	        {
	            TryFire();
	        }
	    }
	    
	    private bool ShouldAttemptFire()
	    {
	        if (usePlayerInput)
	        {
	            return config.fireModeConfig.autoShooting ? fireInputHold : fireInputDown;
	        }
	        return false;
	    }
	    
	    public void TryFire()
	    {
	        if (ammoSystem.IsReloading) return;
	        
	        var firePoints = muzzleSystem.GetFirePoints();
	        int projectilesNeeded = CalculateProjectilesNeeded(firePoints.Length);
	        
	        if (!ammoSystem.CanFire(projectilesNeeded))
	        {
	            HandleNoAmmo();
	            return;
	        }
	        
	        Fire(firePoints, projectilesNeeded);
	    }
	    
	    private void Fire(FirePoint[] firePoints, int projectilesNeeded)
	    {
	        fireMode.OnFireStarted();
	        
	        var shootContext = new ShootContext
	        {
	            firePoints = firePoints,
	            projectilesNeeded = projectilesNeeded,
	            recoilAngle = recoilSystem.GetRecoilAngle(),
	            spreadAngles = GenerateSpreadAngles()
	        };
	        
	        int projectilesFired = fireMode.Fire(shootContext, SpawnProjectile);
	        
	        ammoSystem.ConsumeAmmo(projectilesFired);
	        recoilSystem.ApplyRecoil();
	        audioSystem.PlayShootSound();
	        
	        onShoot?.Invoke();
	    }
	    
	    private void SpawnProjectile(Vector3 position, Quaternion rotation)
	    {
	        var projectileObj = Instantiate(config.projectileConfig.projectilePrefab, position, rotation);
	        var projectile = projectileObj.GetComponent<Projectile>();
	        
	        if (projectile != null)
	        {
	            projectile.Initialize(config.projectileConfig, projectileTarget);
	        }
	    }
	    
	    private int CalculateProjectilesNeeded(int firePointCount)
	    {
	        int multiplier = config.fireModeConfig.mode switch
	        {
	            FireMode.Burst => config.fireModeConfig.burstCount,
	            FireMode.Spread => config.fireModeConfig.spreadProjectileCount,
	            _ => 1
	        };
	        
	        return firePointCount * multiplier;
	    }
	    
	    private float[] GenerateSpreadAngles()
	    {
	        if (config.fireModeConfig.mode != FireMode.Spread)
	            return null;
	            
	        int count = config.fireModeConfig.spreadProjectileCount;
	        float[] angles = new float[count];
	        
	        if (config.fireModeConfig.randomSpread)
	        {
	            for (int i = 0; i < count; i++)
	            {
	                angles[i] = Random.Range(
	                    config.fireModeConfig.minSpreadAngle, 
	                    config.fireModeConfig.maxSpreadAngle
	                ) * config.fireModeConfig.spreadFactor;
	            }
	        }
	        else
	        {
	            float step = (config.fireModeConfig.maxSpreadAngle - config.fireModeConfig.minSpreadAngle) / Mathf.Max(count - 1, 1);
	            for (int i = 0; i < count; i++)
	            {
	                angles[i] = (config.fireModeConfig.minSpreadAngle + step * i) * config.fireModeConfig.spreadFactor;
	            }
	        }
	        
	        return angles;
	    }
	    
	    private void HandleNoAmmo()
	    {
	        if (config.ammoConfig.autoReload)
	        {
	            TryReload();
	        }
	        else
	        {
	            audioSystem.PlayNoAmmoSound();
	        }
	    }
	    
	    public void TryReload()
	    {
	        ammoSystem.TryReload();
	    }
	    
	    public void SetProjectileTarget(Transform target)
	    {
	        projectileTarget = target;
	    }
	    
	    public void AddAmmo(int amount)
	    {
	        ammoSystem.AddAmmo(amount);
	    }
	    
	    public void SetFireInput(bool hold, bool down)
	    {
	        fireInputHold = hold;
	        fireInputDown = down;
	    }
	    
	    private void OnDrawGizmos()
	    {
	        if (muzzleSystem != null)
	        {
	            muzzleSystem.DrawGizmos();
	        }
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "Weapon.cs"), content);
	    }
	
	    private void GenerateMuzzleSystem(string folder)
	    {
	        string content = @"using System.Collections.Generic;
	using UnityEngine;
	
	public class MuzzleSystem
	{
	    private readonly Transform weaponTransform;
	    private readonly WeaponConfig config;
	    private readonly List<Transform> muzzleTransforms = new();
	    private readonly List<FirePoint> firePoints = new();
	    
	    public MuzzleSystem(Transform weaponTransform, WeaponConfig config)
	    {
	        this.weaponTransform = weaponTransform;
	        this.config = config;
	    }
	    
	    public void Initialize()
	    {
	        CreateMuzzleTransforms();
	        GenerateFirePoints();
	    }
	    
	    private void CreateMuzzleTransforms()
	    {
	        foreach (Transform child in muzzleTransforms)
	        {
	            if (child != null && Application.isPlaying)
	            {
	                Object.Destroy(child.gameObject);
	            }
	        }
	        muzzleTransforms.Clear();
	        
	        for (int i = 0; i < config.muzzleConfigs.Length; i++)
	        {
	            var muzzleConfig = config.muzzleConfigs[i];
	            var muzzleObj = new GameObject($""Muzzle_{i}"");
	            muzzleObj.transform.SetParent(weaponTransform);
	            muzzleObj.transform.localPosition = muzzleConfig.localPosition;
	            muzzleObj.transform.localRotation = muzzleConfig.localRotation;
	            
	            muzzleTransforms.Add(muzzleObj.transform);
	        }
	    }
	    
	    private void GenerateFirePoints()
	    {
	        firePoints.Clear();
	        
	        for (int i = 0; i < config.muzzleConfigs.Length; i++)
	        {
	            var muzzleConfig = config.muzzleConfigs[i];
	            var muzzleTransform = muzzleTransforms[i];
	            
	            var points = muzzleConfig.mode switch
	            {
	                MuzzleMode.Line => GenerateLinePoints(muzzleConfig, muzzleTransform),
	                MuzzleMode.Circle => GenerateCirclePoints(muzzleConfig, muzzleTransform),
	                _ => new List<FirePoint>()
	            };
	            
	            firePoints.AddRange(points);
	        }
	    }
	    
	    private List<FirePoint> GenerateLinePoints(MuzzleConfig config, Transform muzzle)
	    {
	        var points = new List<FirePoint>();
	        float halfDistance = (config.pointsCount - 1) * config.distanceBetweenPoints / 2f;
	        Vector2 startPos = Vector2.up * halfDistance;
	        
	        for (int i = 0; i < config.pointsCount; i++)
	        {
	            Vector2 localPos = startPos - Vector2.up * (i * config.distanceBetweenPoints);
	            Vector3 direction = Vector2.Lerp(Vector2.right, localPos, config.fireAngleModifier);
	            
	            points.Add(new FirePoint(localPos, direction, muzzle));
	        }
	        
	        return points;
	    }
	    
	    private List<FirePoint> GenerateCirclePoints(MuzzleConfig config, Transform muzzle)
	    {
	        var points = new List<FirePoint>();
	        float stepAngle = config.autoCalculateStepAngle 
	            ? 360f / config.pointsCount 
	            : config.stepAngle;
	            
	        float minAngle = -((config.pointsCount - 1) * stepAngle) / 2f;
	        
	        for (int i = 0; i < config.pointsCount; i++)
	        {
	            float angleRad = Mathf.Deg2Rad * (minAngle + i * stepAngle);
	            Vector2 localPos = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * config.radius;
	            Vector3 direction = Vector2.Lerp(localPos.normalized, Vector2.right, config.fireAngleModifier);
	            
	            points.Add(new FirePoint(localPos, direction, muzzle));
	        }
	        
	        return points;
	    }
	    
	    public FirePoint[] GetFirePoints() => firePoints.ToArray();
	    
	    public void DrawGizmos()
	    {
	        if (firePoints == null || firePoints.Count == 0) return;
	        
	        Gizmos.color = Color.yellow;
	        foreach (var point in firePoints)
	        {
	            Vector3 pos = point.GetWorldPosition();
	            Vector3 dir = point.GetWorldDirection();
	            
	            Gizmos.DrawRay(pos, dir * 0.5f);
	            DrawArrow(pos, dir * 0.5f);
	        }
	    }
	    
	    private void DrawArrow(Vector3 pos, Vector3 direction, float arrowSize = 0.1f, float angle = 20f)
	    {
	        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(180 + angle, 0, 0) * Vector3.forward;
	        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(180 - angle, 0, 0) * Vector3.forward;
	        
	        Gizmos.DrawRay(pos + direction, right * arrowSize);
	        Gizmos.DrawRay(pos + direction, left * arrowSize);
	    }
	}
	
	public class FirePoint
	{
	    private readonly Vector3 localPosition;
	    private readonly Vector3 localDirection;
	    private readonly Transform muzzle;
	    
	    public FirePoint(Vector3 localPosition, Vector3 localDirection, Transform muzzle)
	    {
	        this.localPosition = localPosition;
	        this.localDirection = localDirection;
	        this.muzzle = muzzle;
	    }
	    
	    public Vector3 GetWorldPosition()
	    {
	        return muzzle.TransformPoint(localPosition);
	    }
	    
	    public Vector3 GetWorldDirection()
	    {
	        return muzzle.TransformDirection(localDirection).normalized;
	    }
	    
	    public Quaternion GetWorldRotation()
	    {
	        Vector3 dir = GetWorldDirection();
	        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
	        return Quaternion.Euler(0, 0, angle);
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "MuzzleSystem.cs"), content);
	    }
	
	    private void GenerateAmmoSystem(string folder)
	    {
	        string content = @"using System;
	using System.Collections;
	using UnityEngine;
	
	public class AmmoSystem
	{
	    private readonly AmmoConfig config;
	    
	    private int currentMagazineAmmo;
	    private int currentTotalAmmo;
	    private bool isReloading;
	    
	    public event Action OnReloadStart;
	    public event Action OnReloadEnd;
	    public event Action<int, int> OnAmmoChanged;
	    
	    public int CurrentMagazineAmmo => currentMagazineAmmo;
	    public int CurrentTotalAmmo => currentTotalAmmo;
	    public int MagazineSize => config.magazineSize;
	    public bool IsReloading => isReloading;
	    
	    public AmmoSystem(AmmoConfig config)
	    {
	        this.config = config;
	        
	        currentMagazineAmmo = config.unlimitedMagazineSize ? int.MaxValue : config.magazineSize;
	        currentTotalAmmo = config.unlimitedAmmo ? int.MaxValue : config.maxAmmo;
	    }
	    
	    public bool CanFire(int projectilesNeeded)
	    {
	        if (config.unlimitedAmmo && config.unlimitedMagazineSize)
	            return true;
	            
	        int ammoRequired = CalculateAmmoRequired(projectilesNeeded);
	        
	        if (!config.unlimitedMagazineSize)
	        {
	            if (config.partialAmmoFire)
	                return currentMagazineAmmo > 0;
	            else
	                return currentMagazineAmmo >= ammoRequired;
	        }
	        else if (!config.unlimitedAmmo)
	        {
	            if (config.partialAmmoFire)
	                return currentTotalAmmo > 0;
	            else
	                return currentTotalAmmo >= ammoRequired;
	        }
	        
	        return true;
	    }
	    
	    public void ConsumeAmmo(int projectilesFired)
	    {
	        if (config.unlimitedAmmo && config.unlimitedMagazineSize)
	            return;
	            
	        int ammoToConsume = config.countEachProjectile ? projectilesFired : config.ammoPerShot;
	        
	        if (!config.unlimitedMagazineSize)
	        {
	            currentMagazineAmmo = Mathf.Max(0, currentMagazineAmmo - ammoToConsume);
	        }
	        else if (!config.unlimitedAmmo)
	        {
	            currentTotalAmmo = Mathf.Max(0, currentTotalAmmo - ammoToConsume);
	        }
	        
	        OnAmmoChanged?.Invoke(currentMagazineAmmo, currentTotalAmmo);
	    }
	    
	    private int CalculateAmmoRequired(int projectilesNeeded)
	    {
	        return config.countEachProjectile ? projectilesNeeded : config.ammoPerShot;
	    }
	    
	    public bool TryReload()
	    {
	        if (isReloading) return false;
	        if (config.unlimitedMagazineSize) return false;
	        if (currentMagazineAmmo >= config.magazineSize) return false;
	        if (!config.unlimitedAmmo && currentTotalAmmo <= 0) return false;
	        
	        OnReloadStart?.Invoke();
	        return true;
	    }
	    
	    public IEnumerator PerformReload(MonoBehaviour coroutineRunner)
	    {
	        isReloading = true;
	        
	        yield return new WaitForSeconds(config.reloadTime);
	        
	        if (config.unlimitedAmmo)
	        {
	            currentMagazineAmmo = config.magazineSize;
	        }
	        else
	        {
	            int neededAmmo = config.magazineSize - currentMagazineAmmo;
	            int ammoToLoad = Mathf.Min(neededAmmo, currentTotalAmmo);
	            
	            currentMagazineAmmo += ammoToLoad;
	            currentTotalAmmo -= ammoToLoad;
	        }
	        
	        isReloading = false;
	        OnReloadEnd?.Invoke();
	        OnAmmoChanged?.Invoke(currentMagazineAmmo, currentTotalAmmo);
	    }
	    
	    public void AddAmmo(int amount)
	    {
	        if (config.unlimitedAmmo) return;
	        
	        currentTotalAmmo = Mathf.Min(currentTotalAmmo + amount, config.maxAmmo);
	        OnAmmoChanged?.Invoke(currentMagazineAmmo, currentTotalAmmo);
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "AmmoSystem.cs"), content);
	    }
	
	    private void GenerateRecoilSystem(string folder)
	    {
	        string content = @"using UnityEngine;
	
	public class RecoilSystem
	{
	    private readonly RecoilConfig config;
	    
	    private float recoilFactor;
	    private float timeSinceLastShot;
	    
	    public RecoilSystem(RecoilConfig config)
	    {
	        this.config = config;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        timeSinceLastShot += deltaTime;
	        
	        if (timeSinceLastShot >= config.recoveryTime)
	        {
	            recoilFactor = 0f;
	        }
	    }
	    
	    public void ApplyRecoil()
	    {
	        if (!config.enabled) return;
	        
	        recoilFactor += Time.deltaTime;
	        timeSinceLastShot = 0f;
	    }
	    
	    public float GetRecoilAngle()
	    {
	        if (!config.enabled) return 0f;
	        
	        float curveValue = config.recoilCurve.Evaluate(Mathf.Clamp01(recoilFactor / config.recoveryTime));
	        return Random.Range(-config.force, config.force) * curveValue;
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "RecoilSystem.cs"), content);
	    }
	
	    private void GenerateAimSystem(string folder)
	    {
	        string content = @"using UnityEngine;
	
	public class AimSystem
	{
	    private readonly Transform weaponTransform;
	    private readonly AimConfig config;
	    private Transform customTarget;
	    
	    public AimSystem(Transform weaponTransform, AimConfig config)
	    {
	        this.weaponTransform = weaponTransform;
	        this.config = config;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        if (!config.enabled) return;
	        
	        Vector3 targetScreenPos = GetTargetScreenPosition();
	        Vector3 weaponScreenPos = Camera.main.WorldToScreenPoint(weaponTransform.position);
	        Vector3 holderScreenPos = GetHolderScreenPosition();
	        
	        if (weaponTransform.parent != null)
	        {
	            Vector2 directionToTarget = new Vector2(
	                targetScreenPos.x - holderScreenPos.x,
	                targetScreenPos.y - holderScreenPos.y
	            ).normalized;
	            
	            weaponTransform.localPosition = directionToTarget * config.rotationRadius;
	        }
	        
	        Vector2 aimDirection = new Vector2(
	            targetScreenPos.x - weaponScreenPos.x,
	            targetScreenPos.y - weaponScreenPos.y
	        );
	        
	        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
	        Quaternion targetRotation;
	        
	        if (targetScreenPos.x < weaponScreenPos.x)
	        {
	            targetRotation = Quaternion.Euler(180f, 0f, -angle);
	        }
	        else
	        {
	            targetRotation = Quaternion.Euler(0f, 0f, angle);
	        }
	        
	        weaponTransform.rotation = Quaternion.Lerp(
	            weaponTransform.rotation,
	            targetRotation,
	            deltaTime * config.rotateSpeed
	        );
	    }
	    
	    private Vector3 GetTargetScreenPosition()
	    {
	        if (config.aimAtCursor)
	        {
	            return Input.mousePosition;
	        }
	        else if (customTarget != null)
	        {
	            return Camera.main.WorldToScreenPoint(customTarget.position);
	        }
	        
	        return weaponTransform.position + weaponTransform.right;
	    }
	    
	    private Vector3 GetHolderScreenPosition()
	    {
	        if (weaponTransform.parent != null)
	        {
	            return Camera.main.WorldToScreenPoint(weaponTransform.parent.position);
	        }
	        return Camera.main.WorldToScreenPoint(weaponTransform.position);
	    }
	    
	    public void SetCustomTarget(Transform target)
	    {
	        customTarget = target;
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "AimSystem.cs"), content);
	    }
	
	    private void GenerateAudioSystem(string folder)
	    {
	        string content = @"using UnityEngine;
	
	public class AudioSystem
	{
	    private readonly GameObject owner;
	    private readonly AudioConfig config;
	    
	    private AudioSource shootSource;
	    private AudioSource reloadSource;
	    private AudioSource noAmmoSource;
	    
	    private float noAmmoSoundTimer;
	    
	    public AudioSystem(GameObject owner, AudioConfig config)
	    {
	        this.owner = owner;
	        this.config = config;
	        
	        InitializeAudioSources();
	    }
	    
	    private void InitializeAudioSources()
	    {
	        if (config.shootSound != null)
	        {
	            shootSource = CreateAudioSource(""ShootAudio"", config.shootSound, config.shootVolume);
	        }
	        
	        if (config.reloadSound != null)
	        {
	            reloadSource = CreateAudioSource(""ReloadAudio"", config.reloadSound, config.reloadVolume);
	        }
	        
	        if (config.noAmmoSound != null)
	        {
	            noAmmoSource = CreateAudioSource(""NoAmmoAudio"", config.noAmmoSound, config.noAmmoVolume);
	        }
	    }
	    
	    private AudioSource CreateAudioSource(string name, AudioClip clip, float volume)
	    {
	        var sourceObj = new GameObject(name);
	        sourceObj.transform.SetParent(owner.transform);
	        sourceObj.transform.localPosition = Vector3.zero;
	        
	        var source = sourceObj.AddComponent<AudioSource>();
	        source.clip = clip;
	        source.volume = volume;
	        source.playOnAwake = false;
	        source.loop = false;
	        
	        return source;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        noAmmoSoundTimer -= deltaTime;
	    }
	    
	    public void PlayShootSound()
	    {
	        shootSource?.Play();
	    }
	    
	    public void PlayReloadSound()
	    {
	        reloadSource?.Play();
	    }
	    
	    public void PlayNoAmmoSound()
	    {
	        if (noAmmoSoundTimer <= 0f && noAmmoSource != null)
	        {
	            noAmmoSource.Play();
	            noAmmoSoundTimer = config.noAmmoSoundCooldown;
	        }
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "AudioSystem.cs"), content);
	    }
	
	    private void GenerateIWeaponFireMode(string folder)
	    {
	        string content = @"using System;
	
	public interface IWeaponFireMode
	{
	    void Update(float deltaTime);
	    bool CanFire();
	    void OnFireStarted();
	    int Fire(ShootContext context, Action<Vector3, Quaternion> spawnProjectile);
	}
	
	public struct ShootContext
	{
	    public FirePoint[] firePoints;
	    public int projectilesNeeded;
	    public float recoilAngle;
	    public float[] spreadAngles;
	}";
	        File.WriteAllText(Path.Combine(folder, "IWeaponFireMode.cs"), content);
	    }
	
	    private void GenerateSingleFireMode(string folder)
	    {
	        string content = @"using System;
	using UnityEngine;
	
	public class SingleFireMode : IWeaponFireMode
	{
	    private readonly FireModeConfig config;
	    private float cooldownTimer;
	    
	    public SingleFireMode(FireModeConfig config)
	    {
	        this.config = config;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        cooldownTimer -= deltaTime;
	    }
	    
	    public bool CanFire()
	    {
	        return cooldownTimer <= 0f;
	    }
	    
	    public void OnFireStarted()
	    {
	        cooldownTimer = config.fireRate;
	    }
	    
	    public int Fire(ShootContext context, Action<Vector3, Quaternion> spawnProjectile)
	    {
	        int projectilesFired = 0;
	        
	        foreach (var firePoint in context.firePoints)
	        {
	            Vector3 position = firePoint.GetWorldPosition();
	            Quaternion rotation = firePoint.GetWorldRotation();
	            
	            rotation *= Quaternion.Euler(0, 0, context.recoilAngle);
	            
	            spawnProjectile(position, rotation);
	            projectilesFired++;
	        }
	        
	        return projectilesFired;
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "SingleFireMode.cs"), content);
	    }
	
	    private void GenerateAutoFireMode(string folder)
	    {
	        string content = @"using System;
	using UnityEngine;
	
	public class AutoFireMode : IWeaponFireMode
	{
	    private readonly FireModeConfig config;
	    private float cooldownTimer;
	    
	    public AutoFireMode(FireModeConfig config)
	    {
	        this.config = config;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        cooldownTimer -= deltaTime;
	    }
	    
	    public bool CanFire()
	    {
	        return cooldownTimer <= 0f;
	    }
	    
	    public void OnFireStarted()
	    {
	        cooldownTimer = config.fireRate;
	    }
	    
	    public int Fire(ShootContext context, Action<Vector3, Quaternion> spawnProjectile)
	    {
	        int projectilesFired = 0;
	        
	        foreach (var firePoint in context.firePoints)
	        {
	            Vector3 position = firePoint.GetWorldPosition();
	            Quaternion rotation = firePoint.GetWorldRotation();
	            
	            rotation *= Quaternion.Euler(0, 0, context.recoilAngle);
	            
	            spawnProjectile(position, rotation);
	            projectilesFired++;
	        }
	        
	        return projectilesFired;
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "AutoFireMode.cs"), content);
	    }
	
	    private void GenerateBurstFireMode(string folder)
	    {
	        string content = @"using System;
	using System.Collections;
	using UnityEngine;
	
	public class BurstFireMode : IWeaponFireMode
	{
	    private readonly FireModeConfig config;
	    private readonly MonoBehaviour coroutineRunner;
	    
	    private float cooldownTimer;
	    private bool isBursting;
	    
	    public BurstFireMode(FireModeConfig config, MonoBehaviour coroutineRunner)
	    {
	        this.config = config;
	        this.coroutineRunner = coroutineRunner;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        if (!isBursting)
	        {
	            cooldownTimer -= deltaTime;
	        }
	    }
	    
	    public bool CanFire()
	    {
	        return cooldownTimer <= 0f && !isBursting;
	    }
	    
	    public void OnFireStarted()
	    {
	        isBursting = true;
	        coroutineRunner.StartCoroutine(BurstCoroutine());
	    }
	    
	    public int Fire(ShootContext context, Action<Vector3, Quaternion> spawnProjectile)
	    {
	        int projectilesFired = 0;
	        
	        foreach (var firePoint in context.firePoints)
	        {
	            Vector3 position = firePoint.GetWorldPosition();
	            Quaternion rotation = firePoint.GetWorldRotation();
	            
	            rotation *= Quaternion.Euler(0, 0, context.recoilAngle);
	            
	            spawnProjectile(position, rotation);
	            projectilesFired++;
	        }
	        
	        return projectilesFired;
	    }
	    
	    private IEnumerator BurstCoroutine()
	    {
	        for (int i = 0; i < config.burstCount; i++)
	        {
	            yield return new WaitForSeconds(config.timeBetweenShots);
	        }
	        
	        cooldownTimer = config.timeBetweenBursts;
	        isBursting = false;
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "BurstFireMode.cs"), content);
	    }
	
	    private void GenerateSpreadFireMode(string folder)
	    {
	        string content = @"using System;
	using UnityEngine;
	
	public class SpreadFireMode : IWeaponFireMode
	{
	    private readonly FireModeConfig config;
	    private float cooldownTimer;
	    
	    public SpreadFireMode(FireModeConfig config)
	    {
	        this.config = config;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        cooldownTimer -= deltaTime;
	    }
	    
	    public bool CanFire()
	    {
	        return cooldownTimer <= 0f;
	    }
	    
	    public void OnFireStarted()
	    {
	        cooldownTimer = config.fireRate;
	    }
	    
	    public int Fire(ShootContext context, Action<Vector3, Quaternion> spawnProjectile)
	    {
	        int projectilesFired = 0;
	        
	        foreach (var firePoint in context.firePoints)
	        {
	            Vector3 position = firePoint.GetWorldPosition();
	            Quaternion baseRotation = firePoint.GetWorldRotation();
	            
	            for (int i = 0; i < config.spreadProjectileCount; i++)
	            {
	                float spreadAngle = context.spreadAngles != null && i < context.spreadAngles.Length 
	                    ? context.spreadAngles[i] 
	                    : 0f;
	                
	                Quaternion rotation = baseRotation * Quaternion.Euler(0, 0, spreadAngle);
	                
	                spawnProjectile(position, rotation);
	                projectilesFired++;
	            }
	        }
	        
	        return projectilesFired;
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "SpreadFireMode.cs"), content);
	    }
	
	    private void GenerateProjectile(string folder)
	    {
	        string content = @"using UnityEngine;
	
	public class Projectile : MonoBehaviour
	{
	    private ProjectileConfig config;
	    private Transform target;
	    
	    private Vector3 velocity;
	    private Vector3 startPosition;
	    private float lifetime;
	    private float traveledDistance;
	    
	    public void Initialize(ProjectileConfig config, Transform target = null)
	    {
	        this.config = config;
	        this.target = target;
	        
	        velocity = transform.right * config.speed;
	        startPosition = transform.position;
	        lifetime = config.lifetime;
	    }
	    
	    private void Update()
	    {
	        float deltaTime = Time.deltaTime;
	        
	        UpdateMovement(deltaTime);
	        UpdateLifetime(deltaTime);
	        CheckDistance();
	    }
	    
	    private void UpdateMovement(float deltaTime)
	    {
	        switch (config.type)
	        {
	            case ProjectileType.Normal:
	                transform.position += velocity * deltaTime;
	                break;
	                
	            case ProjectileType.Ballistic:
	                velocity += Physics2D.gravity * (config.gravityModifier * deltaTime);
	                transform.position += velocity * deltaTime;
	                
	                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
	                transform.rotation = Quaternion.Euler(0, 0, angle);
	                break;
	                
	            case ProjectileType.Homing:
	                if (target != null)
	                {
	                    Vector3 directionToTarget = (target.position - transform.position).normalized;
	                    velocity = Vector3.Lerp(velocity, directionToTarget * config.speed, config.followStrength * deltaTime);
	                    
	                    float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
	                    transform.rotation = Quaternion.Euler(0, 0, targetAngle);
	                }
	                
	                transform.position += velocity * deltaTime;
	                break;
	        }
	        
	        traveledDistance += velocity.magnitude * deltaTime;
	    }
	    
	    private void UpdateLifetime(float deltaTime)
	    {
	        if (config.unlimitedLifetime) return;
	        
	        lifetime -= deltaTime;
	        if (lifetime <= 0f)
	        {
	            DestroyProjectile();
	        }
	    }
	    
	    private void CheckDistance()
	    {
	        if (!config.destroyOnDistanceTraveled) return;
	        
	        if (traveledDistance >= config.maxDistance)
	        {
	            DestroyProjectile();
	        }
	    }
	    
	    private void OnTriggerEnter2D(Collider2D collision)
	    {
	        var damageable = collision.GetComponent<IDamageable>();
	        if (damageable != null)
	        {
	            damageable.TakeDamage(config.damage);
	        }
	        
	        var structure = collision.GetComponent<IStructure>();
	        if (structure != null)
	        {
	            structure.TakeDamage(config.damageToStructure);
	        }
	        
	        DestroyProjectile();
	    }
	    
	    private void DestroyProjectile()
	    {
	        if (config.impactEffect != null)
	        {
	            Instantiate(config.impactEffect, transform.position, Quaternion.identity);
	        }
	        
	        Destroy(gameObject);
	    }
	}";
	        File.WriteAllText(Path.Combine(folder, "Projectile.cs"), content);
	    }
	
	    private void GenerateIDamageable(string folder)
	    {
	        string content = @"public interface IDamageable
	{
	    void TakeDamage(int damage);
	}
	
	public interface IStructure
	{
	    void TakeDamage(int damage);
	}";
	        File.WriteAllText(Path.Combine(folder, "IDamageable.cs"), content);
	        File.WriteAllText(Path.Combine("Assets/Editor", "WeaponSystemGenerator.cs"), content);
	    }
	}
}