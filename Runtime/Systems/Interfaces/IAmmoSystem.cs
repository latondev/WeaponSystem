using System;

namespace PullGame
{
    public interface IAmmoSystem
    {
        int CurrentMagazineAmmo { get; }
        int CurrentTotalAmmo { get; }
        int MagazineSize { get; }
        bool IsReloading { get; }
        
        event Action OnReloadStart;
        event Action OnReloadEnd;
        event Action<int, int> OnAmmoChanged;
        
        bool CanFire(int projectilesNeeded);
        void ConsumeAmmo(int projectilesFired);
        bool TryReload();
        void AddAmmo(int amount);
    }
}
