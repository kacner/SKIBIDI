using UnityEngine;
using UnityEngine.UI;

public class GunUiManager : MonoBehaviour
{
    [System.Serializable]
    struct Icon
    {
        public Sprite gunIcon;
        public string GunName;
    }
    [SerializeField] private Icon[] gunIcons;
    [SerializeField] private Image[] gunSlots;
    private void Start()
    {
        for (int i = 0; i < gunSlots.Length - 1; i++)
        {
            gunSlots[i].sprite = null;
            gunSlots[i].enabled = false;
        }
    }
    public void UpdateSlot(int slot, string gunName)
    {
        for (int i = 0; i < gunIcons.Length; i++)
        {
            print(gunIcons[i].GunName + "  =  " + gunName);
            if (gunIcons[i].GunName == gunName)
            {
                gunSlots[slot].enabled = true;
                gunSlots[slot].sprite = gunIcons[i].gunIcon;
                print("Replaced gun icon");
                return;
            }
            Debug.LogWarning("No matching gun icon found for: " + gunName);
        }
    }
}
