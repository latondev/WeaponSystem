using UnityEngine;

namespace PullGame
{
	[CreateAssetMenu(fileName = "WeaponConfig", menuName = "Weapon System/Weapon Config")]
	public class WeaponConfig : ScriptableObject
	{
	    [Header("Identity")]
	    public string weaponName;
	    public Sprite weaponIcon;
	    
	    [Header("Fire Configuration")]
	    public FireModeConfig fireModeConfig;
	    public MuzzleConfig[] muzzleConfigs;
	    
	    [Header("Projectile Configuration")]
	    public ProjectileConfig projectileConfig;
	    
	    [Header("Ammo Configuration")]
	    public AmmoConfig ammoConfig;
	    
	    [Header("Recoil Configuration")]
	    public RecoilConfig recoilConfig;
	    
	    [Header("Audio Configuration")]
	    public AudioConfig audioConfig;
	    
	    [Header("Aim Configuration")]
	    public AimConfig aimConfig;
	}
}