using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PullGame.WeaponSystem.Demo
{
    /// <summary>
    /// Demo UI for displaying weapon ammo information
    /// Attach to a Canvas or GameObject with UI elements
    /// </summary>
    public class AmmoDisplayDemo : MonoBehaviour
    {
        [Header("Weapon Reference")]
        [SerializeField] private Weapon weapon;
        
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI magazineText;
        [SerializeField] private TextMeshProUGUI reserveText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Slider magazineSlider;
        [SerializeField] private Image reloadIndicator;
        
        [Header("Settings")]
        [SerializeField] private Color normalAmmoColor = Color.white;
        [SerializeField] private Color lowAmmoColor = Color.red;
        [SerializeField] private Color criticalAmmoColor = new Color(1f, 0f, 0f, 0.7f);
        [SerializeField] private float lowAmmoThreshold = 0.3f;
        [SerializeField] private float criticalAmmoThreshold = 0.1f;
        
        private IAmmoSystem _ammoSystem;
        
        private void Start()
        {
            if (weapon == null)
            {
                weapon = GetComponentInChildren<Weapon>();
            }
            
            if (weapon != null)
            {
                _ammoSystem = weapon.Ammo;
                _ammoSystem.OnAmmoChanged += OnAmmoChanged;
                _ammoSystem.OnReloadStart += OnReloadStart;
                _ammoSystem.OnReloadEnd += OnReloadEnd;
                
                // Initial update
                UpdateAmmoDisplay();
            }
        }
        
        private void OnDestroy()
        {
            if (_ammoSystem != null)
            {
                _ammoSystem.OnAmmoChanged -= OnAmmoChanged;
                _ammoSystem.OnReloadStart -= OnReloadStart;
                _ammoSystem.OnReloadEnd -= OnReloadEnd;
            }
        }
        
        private void OnAmmoChanged(int magazineAmmo, int totalAmmo)
        {
            UpdateAmmoDisplay();
        }
        
        private void OnReloadStart()
        {
            if (statusText != null)
            {
                statusText.text = "RELOADING...";
                statusText.color = Color.yellow;
            }
            
            if (reloadIndicator != null)
            {
                reloadIndicator.fillAmount = 1f;
            }
        }
        
        private void OnReloadEnd()
        {
            if (statusText != null)
            {
                statusText.text = "";
            }
            
            if (reloadIndicator != null)
            {
                reloadIndicator.fillAmount = 0f;
            }
        }
        
        private void Update()
        {
            UpdateReloadIndicator();
        }
        
        private void UpdateAmmoDisplay()
        {
            if (_ammoSystem == null) return;
            
            int magazine = _ammoSystem.CurrentMagazineAmmo;
            int magazineSize = _ammoSystem.MagazineSize;
            int reserve = _ammoSystem.CurrentTotalAmmo;
            
            // Update text displays
            if (magazineText != null)
            {
                magazineText.text = $"{magazine} / {magazineSize}";
                magazineText.color = GetAmmoColor(magazine, magazineSize);
            }
            
            if (reserveText != null)
            {
                reserveText.text = reserve.ToString();
            }
            
            // Update slider
            if (magazineSlider != null)
            {
                magazineSlider.maxValue = magazineSize;
                magazineSlider.value = magazine;
            }
        }
        
        private void UpdateReloadIndicator()
        {
            if (reloadIndicator != null && _ammoSystem?.IsReloading == true)
            {
                // Simple reload animation - in real implementation, track actual reload progress
                reloadIndicator.fillAmount -= Time.deltaTime * 0.5f;
            }
        }
        
        private Color GetAmmoColor(int current, int max)
        {
            float ratio = (float)current / max;
            
            if (ratio <= criticalAmmoThreshold)
                return criticalAmmoColor;
            if (ratio <= lowAmmoThreshold)
                return lowAmmoColor;
            
            return normalAmmoColor;
        }
        
        /// <summary>
        /// Call this to add ammo (e.g., from pickup)
        /// </summary>
        public void AddAmmo(int amount)
        {
            weapon?.AddAmmo(amount);
        }
        
        /// <summary>
        /// Force reload
        /// </summary>
        public void Reload()
        {
            weapon?.TryReload();
        }
    }
}
