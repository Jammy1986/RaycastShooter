using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float _maxBulletDistance;
    [SerializeField] private float _maxLifeTime;
    private float _startTime;

    [SyncVar] private Vector3 _origin;
    [SyncVar] private Vector3 _destination;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        _startTime = Time.time;
        _lineRenderer.SetPositions(new[] { _origin, _destination });
    }

    private void Update()
    {
        if (!isServer)
        {
            return;
        }
        if (_startTime + _maxLifeTime < Time.time)
        {
            Destroy(gameObject);
        }
    }

    public void Setup(Vector3 position, Vector3 direction)
    {
        var hits = Physics.RaycastAll(new Ray(position, direction));
        var destination = new RaycastHit();
        var hasDestination = false;
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
            hasDestination = true;

            if (raycastHit.transform.tag.Equals("Player"))
            {
                var player = raycastHit.transform.gameObject.GetComponent<PlayerControls>();
                player.TakeDamage();
            }

            break;
        }

        _origin = position;
        if (hasDestination)
        {
            _destination = destination.point;
        }
        else
        {
            _destination = _origin + direction * _maxBulletDistance;
        }
    }
}