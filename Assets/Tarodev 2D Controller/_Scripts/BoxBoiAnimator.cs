using TarodevController;
using UnityEngine;

/// <summary>
/// I have added the old box boi animator back. This was a rush job and is by no means prod ready. 
/// </summary>
public class BoxBoiAnimator : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Animator _anim;
    [SerializeField] private SpriteRenderer _sprite;

    [Header("Settings")]
    [SerializeField, Range(1f, 3f)] private float _maxIdleSpeed = 2;
    [SerializeField] private Vector2 _crouchScaleModifier = new(1, 0.5f);
    [SerializeField] private float _maxTilt = 5; // In degrees around the Z axis
    [SerializeField] private float _tiltSpeed = 20;
    
    [Header("Particles")]
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _launchParticles;
    [SerializeField] private ParticleSystem _moveParticles;
    [SerializeField] private ParticleSystem _landParticles;
    [SerializeField] private ParticleSystem _doubleJumpParticles;
    [SerializeField] private ParticleSystem _dashParticles;
    [SerializeField] private ParticleSystem _dashRingParticles;
    [SerializeField] private Transform _dashRingTransform;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip _doubleJumpClip;
    [SerializeField] private AudioClip _dashClip;
    [SerializeField] private AudioClip[] _footsteps;
    [SerializeField] private AudioClip[] _slideClips;

    private AudioSource _source;
    private IPlayerController _player;
    private Vector2 _defaultSpriteSize;

    private void Awake() {
        _source = GetComponent<AudioSource>();
        _player = GetComponentInParent<IPlayerController>();
        _defaultSpriteSize = _sprite.size;
    }

    private void OnEnable() {
        _player.Jumped += OnJumped;
        _player.AirJumped += OnDoubleJumped;
        _player.GroundedChanged += OnGroundedChanged;
        _player.DashingChanged += OnDashChanged;

        _moveParticles.Play();
    }

    private void OnDisable() {
        _player.Jumped -= OnJumped;
        _player.AirJumped -= OnDoubleJumped;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.DashingChanged -= OnDashChanged;

        _moveParticles.Stop();
    }

    private void Update() {
        if (_player == null) return;

        var xInput = _player.Input.x;
        
        DetectGroundColor();

        HandleSpriteFlip(xInput);

        HandleIdleSpeed(xInput);

        HandleCharacterTilt(xInput);

        HandleCrouching();
    }

    // Face the direction of your last input
    private void HandleSpriteFlip(float xInput) {
        if (_player.Input.x != 0) _sprite.flipX = xInput < 0; // _player.Input.x > 0 ? 1 : -1, 1, 1);
    }

    // Speed up idle while running
    private void HandleIdleSpeed(float xInput) {
        var inputStrength = Mathf.Abs(xInput);
        _anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));
        _moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale,
            Vector3.one * inputStrength, 2 * Time.deltaTime);
    }
  
    private void HandleCharacterTilt(float xInput) {
        var runningTilt = _grounded ? Quaternion.Euler(0, 0, _maxTilt * xInput) : Quaternion.identity;
        var targetRot = _grounded && _player.GroundNormal != Vector2.up ? runningTilt * _player.GroundNormal : runningTilt * Vector2.up;

        _anim.transform.up = Vector3.RotateTowards(_anim.transform.up, targetRot, _tiltSpeed * Time.deltaTime, 0f);
    }

    private bool _crouching;
    private void HandleCrouching() {
        if (!_crouching && _player.Crouching) {
            _sprite.size = _defaultSpriteSize * _crouchScaleModifier;
            _source.PlayOneShot(_slideClips[Random.Range(0, _slideClips.Length)], Mathf.InverseLerp(0, 5, Mathf.Abs(_player.Speed.x)));
            _crouching = true;
        }
        else if (_crouching && !_player.Crouching && _crouching) {
            _sprite.size = _defaultSpriteSize;
            _crouching = false;
        }
    }

    #region Event Callbacks

    private void OnJumped(bool wallJumped) {
        _anim.SetTrigger(JumpKey);
        _anim.ResetTrigger(GroundedKey);

        // Only play particles when grounded (avoid coyote)
        if (_grounded) {
            SetColor(_jumpParticles);
            SetColor(_launchParticles);
            _jumpParticles.Play();
        }
    }

    private void OnDoubleJumped() {
        _source.PlayOneShot(_doubleJumpClip);
        _doubleJumpParticles.Play();
    }

    private bool _grounded;
    private void OnGroundedChanged(bool grounded, float impact) {
        _grounded = grounded;
        if (grounded) {
            _anim.SetTrigger(GroundedKey);
            _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
            _moveParticles.Play();

            _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            SetColor(_landParticles);
            _landParticles.Play();
        }
        else {
            _moveParticles.Stop();
        }
    }

    private void OnDashChanged(bool dashing, Vector2 dir) {
        if (dashing) {
            _dashParticles.Play();
            _dashRingTransform.up = dir;
            _dashRingParticles.Play();
            _source.PlayOneShot(_dashClip);
        }
        else {
            _dashParticles.Stop();
        }
    }

    #endregion

    #region Helper Methods
    
    private ParticleSystem.MinMaxGradient _currentGradient;

    private void DetectGroundColor() {
        // Detect ground color. Little bit of garbage allocation, but faster computationally. Change to NonAlloc if you'd prefer
        var groundHits = Physics2D.RaycastAll(transform.position, Vector3.down, 2);
        foreach (var hit in groundHits) {
            if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) continue;
            _currentGradient = new ParticleSystem.MinMaxGradient(r.color * 0.9f, r.color * 1.2f);
            SetColor(_moveParticles);
            return;
        }
    }

    private void SetColor(ParticleSystem ps) {
        var main = ps.main;
        main.startColor = _currentGradient;
    }

    #endregion

    #region Animation Keys

    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
    private static readonly int JumpKey = Animator.StringToHash("Jump");

    #endregion
}