using System;
using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class LapTrigger : MonoBehaviour {
    public bool IsHit { get; private set; }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.TryGetComponent(out IPlayerController player)) IsHit = true;
    }

    public void Reset() => IsHit = false;
}