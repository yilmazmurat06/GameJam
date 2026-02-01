using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// StatusBar UI displaying player HP, Armor, and Energy.
/// Similar to SoulKnight's HUD.
/// </summary>
public class StatusBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController _player;
    
    [Header("Health Bar")]
    [SerializeField] private Image _healthFill;
    [SerializeField] private Image _healthBackground;
    [SerializeField] private Text _healthText;
    
    [Header("Armor Bar")]
    [SerializeField] private Image _armorFill;
    [SerializeField] private Image _armorBackground;
    [SerializeField] private Text _armorText;
    
    [Header("Energy Bar")]
    [SerializeField] private Image _energyFill;
    [SerializeField] private Image _energyBackground;
    [SerializeField] private Text _energyText;
    
    [Header("Weapon Display")]
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private Text _weaponName;
    [SerializeField] private Text _ammoText;
    
    [Header("Colors")]
    [SerializeField] private Color _healthColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color _armorColor = new Color(0.3f, 0.6f, 0.9f);
    [SerializeField] private Color _energyColor = new Color(0.2f, 0.8f, 0.4f);
    [SerializeField] private Color _lowHealthColor = new Color(1f, 0f, 0f);
    [SerializeField] private Color _lowEnergyColor = new Color(0.5f, 0.4f, 0f);
    
    private Health _health;
    private Energy _energy;
    
    private void Start()
    {
        FindPlayer();
        SetupColors();
    }
    
    private void FindPlayer()
    {
        if (_player == null)
        {
            _player = FindObjectOfType<PlayerController>();
        }
        
        if (_player != null)
        {
            _health = _player.Health;
            _energy = _player.Energy;
            
            // Subscribe to events
            if (_health != null)
            {
                _health.OnHealthChanged += UpdateHealthBar;
                _health.OnArmorChanged += UpdateArmorBar;
            }
            
            if (_energy != null)
            {
                _energy.OnEnergyChanged += UpdateEnergyBar;
            }
            
            // Initial update
            UpdateAllBars();
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe
        if (_health != null)
        {
            _health.OnHealthChanged -= UpdateHealthBar;
            _health.OnArmorChanged -= UpdateArmorBar;
        }
        
        if (_energy != null)
        {
            _energy.OnEnergyChanged -= UpdateEnergyBar;
        }
    }
    
    private void Update()
    {
        UpdateWeaponDisplay();
    }
    
    private void SetupColors()
    {
        if (_healthFill != null) _healthFill.color = _healthColor;
        if (_armorFill != null) _armorFill.color = _armorColor;
        if (_energyFill != null) _energyFill.color = _energyColor;
    }
    
    private void UpdateAllBars()
    {
        if (_health != null)
        {
            UpdateHealthBar(_health.CurrentHealth, _health.MaxHealth);
            UpdateArmorBar(_health.CurrentArmor, _health.MaxArmor);
        }
        
        if (_energy != null)
        {
            UpdateEnergyBar(_energy.CurrentEnergy, _energy.MaxEnergy);
        }
    }
    
    private void UpdateHealthBar(float current, float max)
    {
        if (_healthFill != null)
        {
            float percent = max > 0 ? current / max : 0;
            _healthFill.fillAmount = percent;
            
            // Flash low health warning
            _healthFill.color = percent < 0.25f ? _lowHealthColor : _healthColor;
        }
        
        if (_healthText != null)
        {
            _healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }
    
    private void UpdateArmorBar(float current, float max)
    {
        if (_armorFill != null)
        {
            float percent = max > 0 ? current / max : 0;
            _armorFill.fillAmount = percent;
        }
        
        if (_armorText != null)
        {
            _armorText.text = $"{Mathf.CeilToInt(current)}";
        }
    }
    
    private void UpdateEnergyBar(float current, float max)
    {
        if (_energyFill != null)
        {
            float percent = max > 0 ? current / max : 0;
            _energyFill.fillAmount = percent;
            
            // Flash low energy warning
            _energyFill.color = percent < 0.2f ? _lowEnergyColor : _energyColor;
        }
        
        if (_energyText != null)
        {
            _energyText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }
    
    private void UpdateWeaponDisplay()
    {
        if (_player == null || _player.CurrentWeapon == null) return;
        
        WeaponBase weapon = _player.CurrentWeapon;
        
        if (_weaponName != null)
        {
            _weaponName.text = weapon.WeaponName;
        }
        
        // Show ammo for ranged weapons
        RangedWeapon rangedWeapon = weapon as RangedWeapon;
        if (rangedWeapon != null && _ammoText != null)
        {
            if (rangedWeapon.CurrentAmmo >= 0)
            {
                _ammoText.text = $"{rangedWeapon.CurrentAmmo}";
            }
            else
            {
                _ammoText.text = "∞"; // Infinite ammo
            }
        }
    }
    
    /// <summary>
    /// Create a basic StatusBar UI programmatically.
    /// Call this from an editor script or at runtime.
    /// </summary>
    [ContextMenu("Create StatusBar UI")]
    public void CreateStatusBarUI()
    {
        // Create Canvas if not exists
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("StatusBarCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create StatusBar container
        GameObject container = new GameObject("StatusBar");
        container.transform.SetParent(canvas.transform, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -20);
        containerRect.sizeDelta = new Vector2(250, 100);
        
        // Create bars
        CreateBar(container.transform, "HealthBar", 0, _healthColor, out _healthFill, out _healthText);
        CreateBar(container.transform, "ArmorBar", 30, _armorColor, out _armorFill, out _armorText);
        CreateBar(container.transform, "EnergyBar", 60, _energyColor, out _energyFill, out _energyText);
        
        Debug.Log("[StatusBar] UI created!");
    }
    
    private void CreateBar(Transform parent, string name, float yOffset, Color color, out Image fill, out Text text)
    {
        // Background
        GameObject bgObj = new GameObject(name + "_BG");
        bgObj.transform.SetParent(parent, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(0, 1);
        bgRect.pivot = new Vector2(0, 1);
        bgRect.anchoredPosition = new Vector2(0, -yOffset);
        bgRect.sizeDelta = new Vector2(200, 20);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Fill
        GameObject fillObj = new GameObject(name + "_Fill");
        fillObj.transform.SetParent(bgObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
        fill = fillObj.AddComponent<Image>();
        fill.color = color;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        
        // Text
        GameObject textObj = new GameObject(name + "_Text");
        textObj.transform.SetParent(bgObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        text = textObj.AddComponent<Text>();
        text.text = "100/100";
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontSize = 14;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
