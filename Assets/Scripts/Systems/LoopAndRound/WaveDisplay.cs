using UnityEngine;
using TMPro;

public class WaveDisplay : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI waveText;

    private void Update()
    {
        if (waveText == null) return;
        if (WaveManager.Instance == null) return;

        waveText.text = "" + WaveManager.Instance.currentWave;
    }
}