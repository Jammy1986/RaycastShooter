using UnityEngine;
using UnityEngine.Networking;

public class Laser : NetworkBehaviour
{
    [SerializeField] private float _maxBulletDistance;
    [SerializeField] private float _maxLifeTime;
    [SerializeField] private Color _startColour;
    [SerializeField] private Color _endColour;

    [SerializeField] private GameObject _laserSplashPrefab;
    private LineRenderer _lineRenderer;

    private float _startTime;

    [SyncVar] private bool _hasDestination;
    [SyncVar] private Vector3 _origin;
    [SyncVar] private Vector3 _destination;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        _startTime = Time.time;
        _lineRenderer.SetPositions(new[] { _origin, _destination });

        if (_hasDestination)
        {
            var laserSplash = (GameObject) Instantiate(_laserSplashPrefab, _destination, Quaternion.LookRotation(_origin - _destination));
            laserSplash.transform.parent = transform;
        }
    }

    private void Update()
    {
        _lineRenderer.SetColors(Color.Lerp(_startColour, _endColour, (Time.time - _startTime) / _maxLifeTime), Color.Lerp(_startColour, _endColour, (Time.time - _startTime) / _maxLifeTime));
        if (!isServer)
        {
            return;
        }
        if (_startTime + _maxLifeTime < Time.time)
        {
            Destroy(gameObject);
        }
    }

    public void Setup(Vector3 position, Vector3 direction, Vector3 velocityForOffset)
    {
        var hits = Physics.RaycastAll(new Ray(position, direction));
        var destination = new RaycastHit();
        foreach (var raycastHit in hits)
        {
            if (raycastHit.transform.tag.Equals("Laser"))
            {
                continue;
            }
            if (raycastHit.transform.Equals(transform))
            {
                continue;
            }

            destination = raycastHit;
            _hasDestination = true;

            if (raycastHit.transform.tag.Equals("Player"))
            {
                var player = raycastHit.transform.gameObject.GetComponent<PlayerControls>();
                player.TakeDamage();
            }

            break;
        }

        var projection = Vector3.Project(velocityForOffset * _maxLifeTime * 1.5f, direction);
        var imaginaryDestination = direction * 10000;
        _origin = Vector3.Distance(position, imaginaryDestination) < Vector3.Distance(position + projection, imaginaryDestination) ? position : position + projection;

        if (_hasDestination)
        {
            _destination = destination.point;
        }
        else
        {
            _destination = _origin + direction * _maxBulletDistance;
        }
    }
}