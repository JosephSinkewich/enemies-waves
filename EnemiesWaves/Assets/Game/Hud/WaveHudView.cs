using TMPro;
using UnityEngine;

namespace EnemiesWaves.UI
{
    public sealed class WaveHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _waveText;
        [SerializeField] private TMP_Text _enemiesText;
        [SerializeField] private string _waveTextFormat = "Волна: {0}";
        [SerializeField] private string _enemiesTextFormat = "Врагов: {0}";

        public void UpdateView(WaveHudState state)
        {
            _waveText.text = string.Format(_waveTextFormat, state.CurrentWave);
            _enemiesText.text = string.Format(_enemiesTextFormat, state.ActiveEnemies);
        }
    }
}
