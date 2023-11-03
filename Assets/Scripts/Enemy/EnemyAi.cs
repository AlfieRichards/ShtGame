using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    private NavMeshAgent _agent;

    [SerializeField]private float _attackRange = 5f;

    [SerializeField]private float _decisionTime = 2f;
    [SerializeField]private float _forgetTime = 8f;

    [SerializeField]private float _patrolRange = 10f; //radius of sphere

    private Transform _centrePoint; //centre of the area the _agent wants to move around in
    private LevelManager levelManager; //thing that handles score

    public int health = 100;
    public int damage = 10;
    public float shotDelay = 5;
    public LayerMask enemy;
    public AnimationManager anim;
    private AudioController audioController;

    private bool _lostSight = true;
    private bool _newDest = true;
    bool canAttack = true;
    private Transform _lastSeen;

    private bool walkAnimPlaying = false;
    private bool shootAnimPlaying = false;
    private bool idleAnimPlaying = false;

    private playerMovement player;
    private GameObject playerObj;

    //0 patrolling, 1 chasing, 2 attacking, 3 fleeing
    private int _state = 0;
    private bool _hasAnim = false;




    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        audioController = GetComponent<AudioController>();
        playerObj = GameObject.Find("Player");
        player = playerObj.GetComponent<playerMovement>();
        _centrePoint = this.transform.parent;
        levelManager = FindObjectOfType<LevelManager>();

        if(anim == null){_hasAnim = false;}
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0)
        {
            Die();
        }
        //Debug.Log(_state);

        switch (_state)
        {
            case 0:
            {
                Patrolling();
                break;
            }
            case 1:
            {
                Chasing();
                break;
            }
            case 2:
            {
                Attacking();
                break;
            }
            default:
                break;
        }

        //idle animation
        if(!idleAnimPlaying && _agent.velocity == new Vector3(0,0,0) && _hasAnim)
        {
            AnimHandler("idle");
            idleAnimPlaying = true;
        }

        if(!walkAnimPlaying && _agent.velocity != new Vector3(0,0,0) && _hasAnim)
        {
            AnimHandler("walk");
            SoundManager("step");
            walkAnimPlaying = true;
        }

        if(_state == 2 && !shootAnimPlaying && _hasAnim)
        {
            AnimHandler("shoot");
        }

        if(_hasAnim && !anim.animSource.isPlaying)
        {
            walkAnimPlaying = false;
            idleAnimPlaying = false;
        }
    }

    void Die()
    {
        //death anim
        //wait a bit
        //destroy
        Destroy(gameObject);
        levelManager.EnemyKilled();
    }

    void Patrolling()
    {
        if(_newDest)
        {
            Vector3 point;
            if (RandomPoint(_centrePoint.position, _patrolRange, out point)) //pass in our centre point and radius of area
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
                _agent.SetDestination(point);
                _newDest = false;

                Invoke("DestReset", _decisionTime);
            }
        }
    }

    void DestReset()
    {
        _newDest = true;
    }

    void Chasing()
    {
        if(_lastSeen != null)
        {
            if(_lostSight)
            {
                StartCoroutine(StartWait());

                //transform.LookAt(_lastSeen.position);
                _agent.SetDestination(_lastSeen.position);
            }
            else
            {
                //transform.LookAt(_lastSeen.position);
                _agent.SetDestination(_lastSeen.position);

                float dist = Vector3.Distance(_lastSeen.position, transform.position);

                if(dist < _attackRange)
                {
                    _state = 2;
                }
            }
        }
    }

    void Attacking()
    {
        int num = Random.Range(0, 100);
        Debug.Log(num);
        if(num < 90)
        {
            if (GameObject.Find("Player") != null)
            {
                RaycastHit info;
                if(Physics.Linecast(transform.position, playerObj.transform.position, out info, enemy))
                {
                    if(info.collider.gameObject.name == "Player")
                    {
                        if(canAttack)
                        {
                            SoundManager("shoot");
                            player._health -= damage;
                            canAttack = false;
                            Invoke("ResetAttack", shotDelay);
                        }
                    }
                }
                //Debug.Log(info.collider.gameObject.name);
                _state = 1;
            }
        }
        else
        {
            if(canAttack)
            {
                SoundManager("shoot");
                canAttack = false;
                Invoke("ResetAttack", shotDelay);
                _state = 1;
            }
        }
    }

    void ResetAttack()
    {
        canAttack = true;
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            RaycastHit info;
            if(Physics.Linecast(transform.position, playerObj.transform.position, out info, enemy))
            {
                if(info.collider.gameObject.name == "Player")
                {
                    _lostSight = false;
                    _lastSeen = other.gameObject.transform;

                    if(_state != 2)
                    {
                        _state = 1;
                    }
                }
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            RaycastHit info;
            if(Physics.Linecast(transform.position, playerObj.transform.position, out info, enemy))
            {
                //Debug.Log(info.collider.gameObject.name);
                if(info.collider.gameObject.name == "Player")
                {
                    _lostSight = false;
                    _lastSeen = other.gameObject.transform;

                    if(_state != 2)
                    {
                        _state = 1;
                    }
                }  
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        _lostSight = true;
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;

        for (int i = 0; i < 30; i++)
        {
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            { 
                //the 1.0f is the max distance from the random point to a point on the navmesh
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    IEnumerator StartWait()
    {
        yield return new WaitForSeconds(_forgetTime);
        if(_lostSight)
        {
            _state = 0;
            _lastSeen = null;
        }
        else
        {
            _state = 1;
        }
    }

    void AnimHandler(string animReq)
    {
        switch (animReq)
        {
            case "idle":
                //audioController.PlayOneShotSound("equip");
                anim.PlayAnim("Idle");
                break;

            case "walk":
                //audioController.PlayOneShotSound("equip");
                anim.PlayAnim("Walk");
                break;

            case "shoot":
                //audioController.PlayOneShotSound("equip");
                anim.PlayAnim("Shoot");
                break;
            default:
                // If the input string does not match any animation name, print an error message
                Debug.LogError("Animation not found: " + animReq);
                break;
        }
    }

    void SoundManager(string sound)
    {
        switch (sound)
        {
            case "step":
                int step = Random.Range(1, 10);
                audioController.PlayOneShotSound("step" + step);
                break;
            case "shoot":
                audioController.PlayOneShotSound("shoot");
                break;
            default:
                // If the input string does not match any animation name, print an error message
                Debug.LogError("Sound not found: " + sound);
                break;
        }
    }
}
