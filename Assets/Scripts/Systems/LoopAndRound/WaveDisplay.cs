using UnityEngine;
using TMPro; // for TextMeshPro, or use UnityEngine.UI for legacy Text

public class WaveDisplay : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI waveText; // drag UI element here

    private void Update()
    {
        waveText.text = "Round # " + WaveManager.Instance.currentWave;
    }
}