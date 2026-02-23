using UnityEngine;
using UnityEngine.Events;

namespace PullGame
{
	public class Weapon : MonoBehaviour
	{
	    [Header("Configuration")]
	    [SerializeField] private WeaponConfig config;
	    
	    [Header("Input")]
	    [SerializeField] private bool usePlayerInput = true;
	    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
	    [SerializeField] private KeyCode reloadKey = KeyCode.R;
	    
	    [Header("Events")]
	    public UnityEvent onShoot;
	    public UnityEvent onReloadStart;
	    public UnityEvent onReloadEnd;
	    
	    [Header("Targeting")]
	    [SerializeField] private Transform defaultProjectileTarget;

	    
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
	            Debug.LogError("Weapon config is not assigned!", this);
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
	        Debug.Log($"<b><color=green>_Log Fire</color></b> :");

	        int projectilesFired = fireMode.Fire(shootContext, SpawnProjectile);
	        
	        ammoSystem.ConsumeAmmo(projectilesFired);
	        recoilSystem.ApplyRecoil();
	        audioSystem.PlayShootSound();
	        
	        onShoot?.Invoke();
	    }

	    public GameObject t;
	    
	    private void SpawnProjectile(Vector3 position, Quaternion rotation)
	    {
	        var projectileObj = Instantiate(config.projectileConfig.projectilePrefab, position, rotation);
	        var projectile = projectileObj.GetComponent<Projectile>();
	        t = projectileObj;
	        if (projectile != null)
	        {
		       // Debug.Log($"<b><color=green>_Log SpawnProjectile</color></b> :" + $" Pos:{position} Rot:{rotation.eulerAngles}");

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
	}
}