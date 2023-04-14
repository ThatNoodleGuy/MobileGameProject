using TarodevController;
using UnityEngine;

public class LadderTop : MonoBehaviour {
    [SerializeField] private float _disableTimeOnInput = 0.3f;
    
    private bool _disabled;
    private Collider2D _col;

    private void Awake() => _col = GetComponent<Collider2D>();

    private void OnCollisionStay2D(Collision2D collision) {
        if (_disabled || !collision.transform.TryGetComponent(out IPlayerController player)) return;

        if (player.Input.y < 0) {
            _disabled = true;
            _col.enabled = false;
            Invoke(nameof(ReActivate), _disableTimeOnInput);
        }
    }

    private void ReActivate() {
        _col.enabled = true;
        _disabled = false;
    }
}