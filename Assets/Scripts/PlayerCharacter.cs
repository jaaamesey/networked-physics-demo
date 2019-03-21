// James Karlsson 13203260

using System;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Networking;

public class PlayerCharacter : NetworkBehaviour
{
    private Rigidbody _rb;
    private Camera _firstPersonCamera; // added for funsies (DO NOT NETWORK)
    private ParticleSystem _particleSystem;
    private Material _particleSprite;
    private MeshRenderer _characterMeshRenderer;
    private TrailRenderer _trailRenderer;
    private Light _light;
    private GameObject _eyebrows;
    private CapsuleCollider _kickHitBox;
    private playerPoints _playerPointsComponent;
    private PlayerCustomisationManager _playerCustomisationComponent;

    private CameraController _camera;

    [HideInInspector] public NetworkConnection PlayerNetworkConnection;
    [HideInInspector] public PlayerConnection PlayerConnection;
    [HideInInspector] public int Index;

    // @TODO: Make public for tuning different player characters
    public float MoveSpeed = 9.0f;
    public float TurnSpeed = 0.4f;
    public float GravitySpeed = 2.7f;
    public float JumpHeight = 1.5f;
    public bool PhysicsInducedRotationAllowed = true; // If true, player can be rotated by physics in Kinematic mode
    public bool DolphinJumps = true; // If true, the player does a flip when jumping (physically accurate)
    public Vector3 RespawnLocation = new Vector3(0, 4, 0);
    public MeshRenderer[] CosmeticMeshes;
    public bool Paused = false;

    private bool _kinematicMode = false;

    private Vector3 _inputVector;
    private Vector3 _movementVector;
    private Vector3 _offset;

    private Rigidbody _rigidbodyToMoveWith;
    private bool _riding = false;
    private bool _forcePhysics = false;

    private bool _floorRaycast;
    private RaycastHit _floorRaycastHit;
    
    // Object parented to target object that player object will "stick" to when riding
    private GameObject _ductTapeObject;

    // Server-side variables (CAN ONLY BE CHANGED BY SERVER OR THROUGH A COMMAND)
    [SyncVar] [HideInInspector] public Color SvrLightColor;
    [SyncVar] [HideInInspector] public bool SvrDead = false;
    [SyncVar] [HideInInspector] public bool SvrAttacking = false;
    [SyncVar] [HideInInspector] public float SvrKnockbackScale = 0.0f;


    private const float FixedUpdateTime = 1 / 60f; // Used when converting values in FixedUpdate() to seconds

    // @TODO: Make movement more "loose" (simulated lack of friction)

    // Use this for initialization

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _characterMeshRenderer = GetComponentInChildren<MeshRenderer>();
        _firstPersonCamera = GetComponent<Camera>();
        _particleSystem = GetComponent<ParticleSystem>();
        _particleSprite = _particleSystem.GetComponent<Renderer>().material;
        _trailRenderer = GetComponent<TrailRenderer>();
        _light = GetComponentInChildren<Light>();
        _eyebrows = transform.Find("GooglyEyes/Eyebrows").gameObject;
        _kickHitBox = transform.Find("KickHitBox").GetComponent<CapsuleCollider>();

        _characterMeshRenderer.material.color = 0.9f * SvrLightColor;
        _firstPersonCamera.enabled = false;
        _light.color = SvrLightColor;
        _particleSprite.color = 40f * SvrLightColor;
        _trailRenderer.endColor = 0.9f * SvrLightColor;
        _trailRenderer.startColor = 40f * SvrLightColor;

        _playerPointsComponent = GetComponent<playerPoints>();
        _playerPointsComponent.enabled = false;

        _playerCustomisationComponent = GetComponent<PlayerCustomisationManager>();
        _playerCustomisationComponent.enabled = false;

        _ductTapeObject = new GameObject();

        _camera = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
    }


    // Update is called once per frame
    private void Update()
    {
        // Enable any local-only components
        if (hasAuthority && _playerPointsComponent.enabled == false)
        {
            _playerPointsComponent.enabled = true;
            _playerCustomisationComponent.enabled = true;
            GameManager.GetCurrent().CurrentPlayer = this;
        }

        // DEBUG: Force pause toggle
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F6))
            Paused = !Paused;

        if (Paused)
        {
            _inputVector = Vector3.zero;
            MovePlayerAsPhysicsObject();
            return;
        }


        transform.localScale = Vector3.one;  // Force consistent scale

        // Make line trail colour get progressively lighter the more damage the player has taken
        _trailRenderer.startColor = 40f * SvrLightColor + SvrKnockbackScale * Color.white;

        if (hasAuthority)
            ProcessInput(); // Input is run here to avoid input bugs

        // Determine if dead or not
        if (_rb.position.y <= -2.0f) // Kill plane
            CmdSetDead(true);

        // DEBUG: Respawn when F5 key is pressed
        if (Input.GetKeyDown(KeyCode.F5))
            Respawn();

        // DEBUG: Return to lobby on escape key
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.F4))
        {
            RespawnLocation = new Vector3(0, 4, 0);
            Respawn();

            // Also reset the camera
            _camera.Target = _camera.InitialTarget;
        }

        // Client-side code for representations of this player
        // Do not run code past this point if player is owned by local connection
        if (hasAuthority)
            return;

        _kickHitBox.gameObject.SetActive(SvrAttacking);

        _eyebrows.SetActive(SvrAttacking);
    }

    // @TODO: Move some stuff from FixedUpdate to Update or find a way to interpolate between physics and render frames
    private void FixedUpdate()
    {

        if (!hasAuthority || _forcePhysics)
        {
            // Run gravity code and all that then just exit out
            MovePlayerAsPhysicsObject(); 
            return;
        }
        
        // Do nothing if dead
        if (SvrDead)
            return;

        // Code from here on out is run entirely on the owning player's client

        // Handle raycasts
        var ray = new Ray(_rb.position, Vector3.down);
        _floorRaycast = Physics.Raycast(ray, out _floorRaycastHit, 1.12f);

        // Clear riding mechanic stuff
        var hitRigidbody = _floorRaycastHit.rigidbody;
        transform.SetParent(null);
        _riding = false;

        // Riding mechanic
        if (_floorRaycast && hitRigidbody != null) // If the player is standing on a rigidbody...
        {
            // TODO: Maybe only do this in certain circumstances?
            // Move with it (WITH DUCT TAPE)
            _ductTapeObject.transform.position = hitRigidbody.position + 1.0f * Vector3.up;

            transform.SetParent(_ductTapeObject.transform, true);
            
            if (_kinematicMode && IsRigidbodyControllable(hitRigidbody))
            {
                // If on something relatively small, literally share its position.
                transform.position = Vector3.Lerp(transform.position, _ductTapeObject.transform.position, 0.2f);

                // Prevent gravity
                var rbVelocity = _rb.velocity;
                rbVelocity.y = 0;
                _rb.velocity = rbVelocity;

                // Signal that an object is being ridden
                _riding = true;

                CmdObtainAuthority(hitRigidbody.gameObject);
            }
        }

        if (_kinematicMode)
            MoveCharacterKinematically();
        else
            MovePlayerAsPhysicsObject();
    }

    private void OnDestroy()
    {
        // Kill duct tape (not a child)
        Destroy(_ductTapeObject);
    }


    private void ProcessInput()
    {
        // Return if not the local player
        if (!hasAuthority)
            return;

        var attacking = Input.GetButton("Attack");
        _eyebrows.SetActive(
            Input.GetButton("Attack")); // Show it on player side before submitting to server to look smoother
        _kickHitBox.gameObject.SetActive(attacking);

        if (attacking != SvrAttacking) // If value is inconsistent between player and server, change it
            CmdSetEyebrowVisibility(attacking);

        // Get main camera
        var mainCamera = Camera.main;
        if (_firstPersonCamera.enabled)
            mainCamera = _firstPersonCamera;
        // Return if camera is unavailable (prevents errors at frame 0)
        if (mainCamera == null) return;

        // Get input vector
        _inputVector.x = Input.GetAxis("Horizontal");
        _inputVector.z = Input.GetAxis("Vertical");
        // Normalise input vector
        var inputMagnitude = Mathf.Clamp01(_inputVector.magnitude);
        _inputVector = _inputVector.normalized;


        var facing = mainCamera.transform.eulerAngles.y;

        _movementVector = Quaternion.Euler(0, facing, 0) * _inputVector * inputMagnitude;

        // Jumping
        if (Input.GetButtonDown("Jump") && _kinematicMode) // Not using isGrounded() kinda feels better in gameplay
        {
            _kinematicMode = false;
            if (!DolphinJumps) // Freeze rotation on initial jump impulse if not set to flip
                _rb.freezeRotation = true;
            _rb.velocity += 8f * Vector3.up; // Already start with a bit of initial velocity
            _rb.AddForce(JumpHeight * 1000f * Vector3.up);
            _particleSystem.Play();

            // Tell server to play particle animation on all other clients
            CmdPlayJumpParticles();
        }


        if (Input.GetButtonUp("Jump") || !Input.GetButton("Jump"))
        {
            if (IsGrounded())
                _kinematicMode = true;
        }

        // Toggle first person view when f is pressed
        if (Input.GetButtonDown("FirstPersonToggle"))
            _firstPersonCamera.enabled = !_firstPersonCamera.enabled;
    }


    // Full control over the character - zero physics influence apart from collisions
    private void MoveCharacterKinematically()
    {
        /* (IMPORTANT - DO NOT ENABLE RIGIDBODY'S BUILT-IN KINEMATIC MODE: 
         THIS BREAKS COLLISIONS) */

        Vector3 movementVelocity;
        float vRotSpd;

        if (_riding)
        {
            movementVelocity = _movementVector * MoveSpeed;
            vRotSpd = 0.2f;
            // Disallow physics-induced rotations
            _rb.freezeRotation = true;
        }
        else
        {
            movementVelocity = _movementVector.magnitude * transform.forward * MoveSpeed;
            vRotSpd = 0.2f;
            // Allow physics-induced rotations if permitted by character
            if (PhysicsInducedRotationAllowed)
                _rb.freezeRotation = false;
        }

        _rb.velocity = Vector3.zero;
        _rb.velocity += movementVelocity;

        _rb.velocity += GravitySpeed * Vector3.down;

        // Smoothly lerp to up rotation
        var newRot = _rb.rotation.eulerAngles;
        newRot.x = 0f;
        newRot.z = 0f;

        // Lerp to standing up straight
        if (_movementVector != Vector3.zero || Mathf.Abs(_rb.rotation.eulerAngles.x) >= 0.1f)
            _rb.rotation = Quaternion.Lerp(_rb.rotation, Quaternion.Euler(newRot), vRotSpd);
        else
            // Freeze rotation if not moving
            _rb.freezeRotation = true;


        // Smoothly lerp to desired rotation (uses Quaternion lerp instead of Euler value lerp
        // to prevent weird over-rotating issues)
        // @TODO: Somehow prevent the player from getting stuck if there's no room to rotate

        if (_movementVector != Vector3.zero)
        {
            Quaternion targetRot;

            if (_riding)
            {
                targetRot = Quaternion.LookRotation(_inputVector, Vector3.up);
                _rb.rotation = Quaternion.Lerp(_rb.rotation, targetRot, _inputVector.magnitude * TurnSpeed);
            }
            else
            {
                targetRot = Quaternion.LookRotation(_movementVector, Vector3.up);
                _rb.rotation = Quaternion.Lerp(_rb.rotation, targetRot, _inputVector.magnitude * TurnSpeed);
            }
        }

        // Workaround for weird rotation issue when riding
        if (_riding && _movementVector == Vector3.zero)
            _rb.rotation = Quaternion.Lerp(_rb.rotation, Quaternion.Euler(newRot), vRotSpd);
    }

    private void MovePlayerAsPhysicsObject()
    {
        _rb.freezeRotation = false;

        // Add player-induced forces
        _rb.AddForce(50f * _movementVector);

        // Apply gravity
        /* Yes - even "real physics" uses fake gravity. */
        _rb.velocity += GravitySpeed * Vector3.down;
    }


    private void OnCollisionEnter(Collision other)
    {
        // Return if not the controlling player
        if (!hasAuthority)
            return;

        if (IsRigidbodyControllable(other.rigidbody) && !other.gameObject.CompareTag("Player"))
        {
            CmdObtainAuthority(other.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // HANDLE TELEPORTERS
        // If player comes into contact with a teleporter... 
        var teleporter = other.gameObject.GetComponent<PlayerTeleporterTrigger>();
        if (teleporter != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.position = teleporter.TeleporterExit.position;

            if (teleporter.SetAsNewSpawnPoint)
                RespawnLocation = teleporter.TeleporterExit.position;
            return;
        }

        // HANDLE CONTACT WITH HITBOXES
        var hitbox = other.gameObject.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            var direction = (_rb.position - hitbox.transform.position).normalized;
            SvrKnockbackScale += hitbox.Damage * 0.01f;
            _forcePhysics = true;
            Invoke("DisableForcePhysics", 0.5f);
            var force = direction * 1f * hitbox.Damage * SvrKnockbackScale;
            force += Vector3.up * 10f;
            _rb.AddForce(force, ForceMode.Impulse);
            // Run this force on all other players to appear smoother
            CmdApplyFakeClientSideImpulse(force);
        }
    }

    private void DisableForcePhysics()
    {
        _forcePhysics = false;
    }

    private void Respawn()
    {
        SvrDead = false;
        SvrKnockbackScale = 0.0f;
        _rb.velocity = Vector3.zero;
        _rb.position = RespawnLocation;
        _playerPointsComponent.Reset();
    }

    private bool IsGrounded()
    {
        return _floorRaycast;
    }

    private static bool IsRigidbodyControllable(Rigidbody hitRigidbody, bool allowPlayer = true,
        bool allowEnemy = false)
    {
        if (hitRigidbody == null)
            return false;

        if (!allowEnemy && hitRigidbody.CompareTag("Enemy"))
            return false;

        if (!allowPlayer && hitRigidbody.CompareTag("Player"))
            return false;

        if (hitRigidbody.CompareTag("MovingPlatform"))
            return false;

        // Final check: is object physically small enough to ride?
        var answer = hitRigidbody.GetComponent<Collider>().bounds.size.magnitude <= 6f;
        return answer;
    }

    // COMMANDS (**SERVER ONLY** FUNCTIONS)
    [Command]
    public void CmdApplyFakeClientSideImpulse(Vector3 force)
    {
        // Call RPC on all players
        RpcApplyFakeClientSideImpulse(force);
    }

    [Command]
    private void CmdPlayJumpParticles()
    {
        RpcPlayJumpParticles();
    }

    [Command]
    private void CmdSetEyebrowVisibility(bool boolean)
    {
        SvrAttacking = boolean;
    }

    [Command]
    private void CmdSetDead(bool boolean)
    {
        SvrDead = boolean;
    }

/*    [Command] // For use with syncing hats over the network when/if implemented
    public void CmdSetTransformParent(Transform childTransform, Transform parentTransform,
        bool worldPositionStays = false)
    {
        RpcSetTransformParent(childTransform, parentTransform, worldPositionStays);
    }*/

    [Command]
    public void CmdObtainAuthority(GameObject gameObjectToOwn)
    {
        var networkConnection = NetworkServer.connections[Index];

        // If an object has a network identity, try to own it.
        // DO NOT DO THIS FOR PLAYERS OR MOVING PLATFORMS THOUGH. 
        if (gameObjectToOwn.CompareTag("Player") ||
            gameObjectToOwn.CompareTag("MovingPlatform"))
            return;

        var networkIdentity = gameObjectToOwn.GetComponent<NetworkIdentity>();
        var rigidbodyToOwn = gameObjectToOwn.GetComponent<Rigidbody>();

        // Don't try to obtain authority if the object has no network identity or rigidbody component
        if (networkIdentity == null || rigidbodyToOwn == null || networkConnection == null)
            return;

        // Don't try to obtain authority if it is already had
        if (networkIdentity.clientAuthorityOwner == networkConnection)
            return;

        networkIdentity.localPlayerAuthority = true;
        if (networkIdentity.clientAuthorityOwner != null)
            networkIdentity.RemoveClientAuthority(networkIdentity.clientAuthorityOwner);
        networkIdentity.AssignClientAuthority(networkConnection);
    }


    // RPC CALLS (** FUNCTIONS CALLED ON ALL CLIENTS BUT NOT SERVER **)

    // Force particles to play on all other clients
    [ClientRpc]
    private void RpcPlayJumpParticles()
    {
        // Return if own player (particles have already spawned instantaneously)
        if (hasAuthority)
            return;
        _particleSystem.Play();
    }

    // Apply a fake impulse to the client-side representation of this player on other clients
    [ClientRpc]
    private void RpcApplyFakeClientSideImpulse(Vector3 force)
    {
        // Return if own player (force has already occurred)
        if (hasAuthority)
            return;
        _rb.AddForce(force, ForceMode.Impulse);
    }
}