using System;

namespace PullGame
{
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
	}
}