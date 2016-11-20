using System;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float _maxLifeTime;
    private float _startTime;
    [SyncVar] private bool _hasDestination;
    [SyncVar] private Plane _destination;
    private bool _willPassPlane;

    public void SetDestination(bool haveDestination, RaycastHit hitInfo)
    {
        _hasDestination = haveDestination;
        if (_hasDestination)
        {
            _destination = new Plane(hitInfo.normal, hitInfo.transform.position);
        }
    }

    private void Start()
    {
        _startTime = Time.time;
        //Debug.Log("New bullet " + Time.fixedTime);
    }

    private void Update()
    {
        if (!isServer)
        {
            return;
        }
        if (_startTime + _maxLifeTime < Time.time)
        {
            //Debug.Log("Bullet life time ended " + Time.fixedTime);
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (!_hasDestination)
        {
            return;
        }
        if (GetComponent<Rigidbody>().IsSleeping())
        {
            //Debug.Log("Has paused " + Time.fixedTime);
            return;
        }
        if (_willPassPlane)
        {
            //Debug.Log("Sleeping rigidbody current position: " + transform.position + ". " + Time.fixedTime);
            GetComponent<Rigidbody>().Sleep();
            return;
        }
        var rigidbody = GetComponent<Rigidbody>();
        var projectedPosition = transform.position + Time.fixedDeltaTime * rigidbody.velocity;
        if (_destination.GetSide(projectedPosition))
        {
            return;
        }
        float distanceToPoint;
        _destination.Raycast(new Ray(transform.position, rigidbody.velocity), out distanceToPoint);
        //var oldVelocity = rigidbody.velocity;
        rigidbody.velocity = distanceToPoint * rigidbody.velocity.normalized / Time.fixedDeltaTime;
        //var newProjectedPosition = transform.position + Time.fixedDeltaTime * rigidbody.velocity;
        _willPassPlane = true;
        //Debug.Log("Will pass plane. Distance: " + distanceToPoint + ". Current position: " + transform.position + ". Projected position: " + projectedPosition + ". New position: " + newProjectedPosition + ". Old velocitty: " + oldVelocity + ". Velocity: " + rigidbody.velocity + ". " + Time.fixedTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Trigger enter " + other.transform.tag + " " + Time.fixedTime);

        if (!isServer)
        {
            return;
        }
        switch (other.transform.tag)
        {
            case "Bullet":
                break;
            case "Player":
                var player = other.gameObject.GetComponent<PlayerControls>();
                player.TakeDamage();
                break;
            case "Wall":
                break;
            default:
                throw new NotImplementedException("Bullet collided with an unknown object" + other.transform.tag);
        }
    }
}