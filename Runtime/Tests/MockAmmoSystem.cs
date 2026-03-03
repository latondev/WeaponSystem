using System;
using System.Collections.Generic;

namespace PullGame.WeaponSystem.Tests
{
    /// <summary>
    /// Mock implementation of IAmmoSystem for testing
    /// </summary>
    public class MockAmmoSystem : IAmmoSystem
    {
        private int _magazineAmmo;
        private int _totalAmmo;
        private int _magazineSize;
        private bool _isReloading;
        
        public int CurrentMagazineAmmo => _magazineAmmo;
        public int CurrentTotalAmmo => _totalAmmo;
        public int MagazineSize => _magazineSize;
        public bool IsReloading => _isReloading;
        
        public event Action OnReloadStart;
        public event Action OnReloadEnd;
        public event Action<int, int> OnAmmoChanged;
        
        // Configurable behavior
        public bool CanFireResult { get; set; } = true;
        public Action OnConsumeAmmo { get; set; }
        public Func<bool> OnTryReload { get; set; }
        
        public void Configure(int magazineAmmo, int totalAmmo, int magazineSize)
        {
            _magazineAmmo = magazineAmmo;
            _totalAmmo = totalAmmo;
            _magazineSize = magazineSize;
        }
        
        public bool CanFire(int projectilesNeeded)
        {
            return CanFireResult;
        }
        
        public void ConsumeAmmo(int projectilesFired)
        {
            _magazineAmmo = Math.Max(0, _magazineAmmo - projectilesFired);
            OnConsumeAmmo?.Invoke();
            OnAmmoChanged?.Invoke(_magazineAmmo, _totalAmmo);
        }
        
        public bool TryReload()
        {
            if (OnTryReload != null)
                return OnTryReload.Invoke();
            
            _isReloading = true;
            OnReloadStart?.Invoke();
            return true;
        }
        
        public void AddAmmo(int amount)
        {
            _totalAmmo += amount;
            OnAmmoChanged?.Invoke(_magazineAmmo, _totalAmmo);
        }
        
        // Helper to simulate reload completion
        public void CompleteReload()
        {
            _isReloading = false;
            _magazineAmmo = _magazineSize;
            OnReloadEnd?.Invoke();
            OnAmmoChanged?.Invoke(_magazineAmmo, _totalAmmo);
        }
        
        // Helper to simulate reload failure
        public void FailReload()
        {
            _isReloading = false;
            OnReloadEnd?.Invoke();
        }
    }
}
