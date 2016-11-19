using Prototype.NetworkLobby;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerControls : NetworkBehaviour
{
    [SerializeField] private Vector3 _offset;
    [SerializeField] private GameObject _shotEmitter;
    [SerializeField] private GameObject _bulletPrefab;

    [SyncVar(hook = "UpdateBulletSpeedText")] private int _bulletSpeed = 100;
    private Text _bulletSpeedText;
    private void UpdateBulletSpeedText(int bulletSpeed)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        _bulletSpeed = bulletSpeed;
        _bulletSpeedText.text = "Bullet speed: " + bulletSpeed;
    }

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
         _bulletSpeedText = GameObject.Find("BulletSpeedText").GetComponent<Text>();
        UpdateBulletSpeedText(_bulletSpeed);
         _hitCounterText = GameObject.Find("HitCounterText").GetComponent<Text>();
        UpdateHitCounterText(_hitCounter);
         _enemyHitCounterText = GameObject.Find("EnemyHitCounterText").GetComponent<Text>();
        UpdateEnemyHitCounterText(_enemyHitCounter);
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        var x = Input.GetAxis("Horizontal") * 0.1f;
        var z = Input.GetAxis("Vertical") * 0.1f;

        transform.Translate(x, 0, z);

        transform.Rotate(0, Input.GetAxis("Mouse X"), 0);

        FindObjectOfType<Camera>().transform.position = transform.position - (Quaternion.Euler(0, transform.eulerAngles.y, 0) * _offset);

        FindObjectOfType<Camera>().transform.LookAt(transform);

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            CmdIncreaseBulletSpeed(_bulletSpeed + 1);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            CmdIncreaseBulletSpeed(_bulletSpeed - 1);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMultiply))
        {
            CmdIncreaseBulletSpeed(_bulletSpeed * 2);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadDivide))
        {
            CmdIncreaseBulletSpeed(_bulletSpeed / 2);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdFire();
        }

        if (transform.position.y < -5)
        {
            CmdReturnToLobby();
        }
    }

    [Command]
    private void CmdIncreaseBulletSpeed(int newBulletSpeed)
    {
        _bulletSpeed = newBulletSpeed;
    }

    [Command]
    private void CmdFire()
    {
        var hits = Physics.RaycastAll(new Ray(_shotEmitter.transform.position, _shotEmitter.transform.forward));
        var destination = new RaycastHit();
        var hitSomething = false;
        foreach (var raycastHit in hits)
        {
            if (raycastHit.transform.tag.Equals("Bullet"))
            {
                continue;
            }
            if (raycastHit.transform.Equals(transform))
            {
                continue;
            }
            destination = raycastHit;
            hitSomething = true;
            break;
        }
        var bulletInstance = Instantiate(_bulletPrefab);
        bulletInstance.transform.position = _shotEmitter.transform.position;
        bulletInstance.GetComponent<Rigidbody>().velocity = transform.forward * _bulletSpeed;
        bulletInstance.GetComponent<Bullet>().SetDestination(hitSomething, destination);
        NetworkServer.Spawn(bulletInstance);
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