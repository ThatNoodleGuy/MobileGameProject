using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TarodevController;
using UnityEngine;
using UnityEngine.UI;

public class Lap : MonoBehaviour {
    [SerializeField] private List<LapTrigger> _triggers;
    [SerializeField] private Text _bestText, _timesText;


    private bool _runningLap;
    private float _timeStarted;
    private readonly List<TimeSpan> _times = new List<TimeSpan>();
    private TimeSpan _best = TimeSpan.MaxValue;

    private const int TIMES_COUNT = 3;
    
    private void OnValidate() {
        _triggers = FindObjectsOfType<LapTrigger>().ToList();
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.TryGetComponent(out IPlayerController player)) {
            if (_runningLap) FinishLap();
            else StartLap();
        }
    }

    private void StartLap() {
        foreach (var trigger in _triggers) {
            trigger.Reset();
        }

        _timeStarted = Time.time;
        _runningLap = true;
    }

    private void FinishLap() {
        if (!_triggers.All(t => t.IsHit)) return;

        var time = TimeSpan.FromSeconds(Time.time - _timeStarted);

        _times.Add(time);
        if (_times.Count > TIMES_COUNT) _times.RemoveAt(0);

        var builder = new StringBuilder();
        for (var i = _times.Count - 1; i >= 0; i--) {
            var timeSpan = _times[i];
            builder.Append(i == _times.Count -1 ? $"<color=#75D4FF>{FormatTime(timeSpan)}</color>\n" : $"{FormatTime(timeSpan)}\n");
        }

        _timesText.text = builder.ToString();

        if (time < _best) _best = time;
        _bestText.text = FormatTime(_best);

        _runningLap = false;

        StartLap();
        
        string FormatTime(TimeSpan span) => $"{span:mm\\:ss\\:ff}";
    }
}