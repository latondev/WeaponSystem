using UnityEngine;
using UnityEngine.UI;

namespace PullGame.WeaponSystem.Demo
{
    /// <summary>
    /// Simple ammo display using standard Unity UI (Text instead of TextMeshPro)
    /// Use this if TextMeshPro is not available in your project
    /// </summary>
    public class SimpleAmmoDisplay : MonoBehaviour
    {
        [Header("Weapon Reference")]
        [SerializeField] private Weapon weapon;
        
        [Header("UI Elements - Assign in Inspector")]
        [SerializeField] private Text magazineText;      // Standard Unity UI Text
        [SerializeField] private Text reserveText;       // Standard Unity UI Text  
        [SerializeField] private Slider magazineSlider;
        [SerializeField] private Image reloadCircle;
        
        [Header("Warning Thresholds")]
        [SerializeField, Range(0f, 1f)] private float warningPercent = 0.25f;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        
        private IAmmoSystem _ammoSystem;
        
        private void Start()
        {
            if (weapon == null)
                weapon = FindObjectOfType<Weapon>();
            
            if (weapon != null)
            {
                _ammoSystem = weapon.Ammo;
                _ammoSystem.OnAmmoChanged += UpdateDisplay;
                _ammoSystem.OnReloadStart += OnReloadStart;
                _ammoSystem.OnReloadEnd += OnReloadEnd;
                
                UpdateDisplay(_ammoSystem.CurrentMagazineAmmo, _ammoSystem.CurrentTotalAmmo);
            }
        }
        
        private void OnDestroy()
        {
            if (_ammoSystem != null)
            {
                _ammoSystem.OnAmmoChanged -= UpdateDisplay;
                _ammoSystem.OnReloadStart -= OnReloadStart;
                _ammoSystem.OnReloadEnd -= OnReloadEnd;
            }
        }
        
        private void UpdateDisplay(int magazine, int reserve)
        {
            if (_ammoSystem == null) return;
            
            // Update magazine text
            if (magazineText != null)
            {
                magazineText.text = $"{magazine} / {_ammoSystem.MagazineSize}";
                
                float percent = (float)magazine / _ammoSystem.MagazineSize;
                if (percent <= warningPercent * 0.5f)
                    magazineText.color = criticalColor;
                else if (percent <= warningPercent)
                    magazineText.color = warningColor;
                else
                    magazineText.color = normalColor;
            }
            
            // Update reserve text
            if (reserveText != null)
            {
                reserveText.text = reserve.ToString();
            }
            
            // Update slider
            if (magazineSlider != null)
            {
                magazineSlider.maxValue = _ammoSystem.MagazineSize;
                magazineSlider.value = magazine;
            }
        }
        
        private void OnReloadStart()
        {
            Debug.Log("[SimpleAmmoDisplay] Reloading...");
        }
        
        private void OnReloadEnd()
        {
            Debug.Log("[SimpleAmmoDisplay] Reload complete!");
        }
        
        private void Update()
        {
            // Animate reload circle if reloading
            if (reloadCircle != null && _ammoSystem?.IsReloading == true)
            {
                reloadCircle.fillAmount -= Time.deltaTime;
                if (reloadCircle.fillAmount <= 0)
                    reloadCircle.fillAmount = 1f;
            }
        }
    }
}
