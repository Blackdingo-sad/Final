using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(TMP_Text))]
public class WorldTimeDisplay : MonoBehaviour
{
    [SerializeField]
    private WorldTime _worldTime;
    private TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _worldTime.WorldTimeChanged += OnWorldTimeChanged;
    }

    private void OnDestroy()
    {
        _worldTime.WorldTimeChanged -= OnWorldTimeChanged;
    }

    private void OnWorldTimeChanged(object sender, TimeSpan newTime)
    {
        _text.text = newTime.ToString(@"hh\:mm");
    }
}
