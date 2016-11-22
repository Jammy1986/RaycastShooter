using System;
using Prototype.NetworkLobby;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerControls : NetworkBehaviour
{
    [SerializeField] private Vector3 _offset;
    [SerializeField] private GameObject _shotEmitter;
    [SerializeField] private GameObject _laserPrefab;
    private bool _joystickFired;
    
    [SyncVar(hook = "UpdateHitCounterText")] private int _hitCounter;
    private Text _hitCounterText;
    private void UpdateHitCounterText(int hitCounter)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        _hitCounter = hitCounter;
        _hitCounterText.text = "Times hit: " + hitCounter;
    }


    [SyncVar(hook = "UpdateEnemyHitCounterText")]
    private int _enemyHitCounter;
    private Text _enemyHitCounterText;
    private void UpdateEnemyHitCounterText(int enemyHitCounter)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        _enemyHitCounter = enemyHitCounter;
        _enemyHitCounterText.text = "Times hit enemy: " + enemyHitCounter;
    }

    private void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        _hitCounterText = GameObject.Find("HitCounterText").GetComponent<Text>();
        UpdateHitCounterText(_hitCounter);
         _enemyHitCounterText = GameObject.Find("EnemyHitCounterText").GetComponent<Text>();
        UpdateEnemyHitCounterText(_enemyHitCounter);
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        FindObjectOfType<Camera>().transform.position = transform.position - (Quaternion.Euler(0, transform.eulerAngles.y, 0) * _offset);

        FindObjectOfType<Camera>().transform.LookAt(transform);

        if (Math.Abs(Input.GetAxis("Fire1")) < 0.0001f)
        {
            _joystickFired = false;
        }

        if (Input.GetButtonDown("Fire1") || (!_joystickFired && Input.GetAxis("Fire1") > 0))
        {
            CmdFire(GetComponent<Rigidbody>().velocity);
            _joystickFired = true;
        }

        if (transform.position.y < -5)
        {
            CmdReturnToLobby();
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddRelativeForce(new Vector3(Input.GetAxis("Horizontal") * 500, 0, Input.GetAxis("Vertical") * 500));
        rigidbody.rotation = Quaternion.Euler(rigidbody.rotation.eulerAngles + new Vector3(0f, 5 * Input.GetAxis("Mouse X"), 0f));
    }

    [Command]
    private void CmdFire(Vector3 velocityOffsetForShot)
    {
        var laserInstance = Instantiate(_laserPrefab);
        laserInstance.GetComponent<Laser>().Setup(_shotEmitter.transform.position, _shotEmitter.transform.forward, velocityOffsetForShot);
        NetworkServer.Spawn(laserInstance);
    }

    [Command]
    private void CmdReturnToLobby()
    {
        LobbyManager.s_Singleton.ServerReturnToLobby();
    }

    public void TakeDamage()
    {
        _hitCounter++;
        var players = FindObjectsOfType<PlayerControls>();
        foreach (var player in players)
        {
            if (player.Equals(this))
            {
                continue;
            }
            player._enemyHitCounter++;
        }
    }
}