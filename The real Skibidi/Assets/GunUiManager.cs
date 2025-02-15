using System.Linq;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class WeaponIcon
{
    public string weaponName;
    public Sprite iconSprite;
}
public class GunUiManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image primarySlot;
    [SerializeField] private Image secondarySlot;
    [SerializeField] private Image meleeSlot;
    [SerializeField] private string defaultMeleeWeapon = "Karambit";
    [Header("Weapon Icons")]
    [SerializeField] private WeaponIcon[] weaponIcons;

    [Header("UI Feedback")]
    [SerializeField] private Color selectedSlotColor = Color.yellow;
    [SerializeField] private Color unselectedSlotColor = Color.white;

    private void Start()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        // Clear gun slots
        primarySlot.sprite = null;
        secondarySlot.sprite = null;
        primarySlot.enabled = false;
        secondarySlot.enabled = false;

        // Initialize melee slot with default weapon
        SetupMeleeSlot();
    }
    private void SetupMeleeSlot()
    {
        // Find and set the default melee weapon icon
        Sprite meleeSprite = FindWeaponSprite(defaultMeleeWeapon);
        if (meleeSprite != null)
        {
            meleeSlot.sprite = meleeSprite;
            meleeSlot.enabled = true;
            meleeSlot.color = unselectedSlotColor;
        }
        else
        {
            Debug.LogError($"Default melee weapon icon not found: {defaultMeleeWeapon}");
        }
    }
    public void UpdateWeaponSlot(GunInventoryType type, string weaponName)
    {

        Image targetSlot = GetSlotByType(type);

        if (type == GunInventoryType.Melee)
        {

            if (meleeSlot.sprite == null || weaponName != defaultMeleeWeapon)
            {
                SetupMeleeSlot();
            }
            return;
        }

        if (string.IsNullOrEmpty(weaponName))
        {
            ClearSlot(targetSlot);
            return;
        }

        Sprite weaponSprite = FindWeaponSprite(weaponName.Trim());
        if (weaponSprite != null)
        {
            targetSlot.sprite = weaponSprite;
            targetSlot.enabled = true;
        }
        else
        {
            Debug.LogWarning($"No icon found for weapon: {weaponName}");
            ClearSlot(targetSlot);
        }
    }

    public void HighlightSelectedWeapon(GunInventoryType type)
    {
        // Reset all slots to unselected color
        primarySlot.color = unselectedSlotColor;
        secondarySlot.color = unselectedSlotColor;
        meleeSlot.color = unselectedSlotColor;

        // Highlight the selected slot
        Image selectedSlot = GetSlotByType(type);
        if (selectedSlot != null)
        {
            selectedSlot.color = selectedSlotColor;
        }
    }

    public Image GetSlotByType(GunInventoryType type)
    {
        return type switch
        {
            GunInventoryType.Primary => primarySlot,
            GunInventoryType.Secondary => secondarySlot,
            GunInventoryType.Melee => meleeSlot,
            _ => null
        };
    }

    public void ClearSlot(Image slot)
    {
        if (slot != null && slot != meleeSlot)
        {
            slot.sprite = null;
            slot.enabled = false;
        }
    }

    private Sprite FindWeaponSprite(string weaponName)
    {

        string trimmedWeaponName = weaponName.Trim();

        foreach (var icon in weaponIcons)
        {

            if (icon.weaponName.Trim() == trimmedWeaponName)
            {
                return icon.iconSprite;
            }
        }
        Debug.LogWarning($"No icon found for weapon: {trimmedWeaponName}");
        return null;
    }
}
