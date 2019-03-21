// Mahinda AggaJoti Viryanata 99178648
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {

    public enum EnemyState { IDLE = 0, CHASE = 1};
    public EnemyState CurrentState = EnemyState.IDLE;
    private Transform ThisTransform;
    private Transform PlayerObject = null;

    private NavMeshAgent agent = null;

    //public Transform target;

    //Enemy Visibility Settings
    public bool CanSeePlayer = false;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ThisTransform = GetComponent<Transform>();
        //PlayerObject = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        PlayerObject = GameManager.GetCurrent().GetPlayerCharacter(0).GetComponent<Transform>();
        

    }





	// Use this for initialization
	void Start () {
        //agent = GetComponent<NavMeshAgent>();
        ChangeState(CurrentState);
 
    }

    //---

    public IEnumerator Idle()
    {
        //Get Random Point to walk around
        Vector3 Point = RandomMovement();
        float WaitTime = 2f;
        float ElapsedTime = 0f;

        //Idle State loop
        while (CurrentState == EnemyState.IDLE)
        {
            agent.SetDestination(Point);

            ElapsedTime += Time.deltaTime;

            if (ElapsedTime >= WaitTime)
            {
                ElapsedTime = 0f;
                Point = RandomMovement();
            }

            if (CanSeePlayer)
            {
                ChangeState(EnemyState.CHASE);
                yield break;
            }

            yield return null;
        }
    }

    //---

    public IEnumerator Chase()
    {

        var player0 = GameManager.GetCurrent().GetPlayerCharacter(0);
        // Do not continue if there is no player 0
        if (player0 == null)
            yield return 0;

        while (CurrentState == EnemyState.CHASE)
        {
            //agent.SetDestination(PlayerObject.position);



            //var targetObject = player0.transform.position;

            agent.SetDestination(PlayerObject.position);

            //var targetObject = PlayerObject.transform.position;
           // agent.SetDestination(targetObject);

            if (!CanSeePlayer)
            {
                yield return new WaitForSeconds(2f);

                if (!CanSeePlayer)
                {
                    ChangeState(EnemyState.IDLE);
                    yield break;
                }
            }

            yield return null;
        }
    }

    //---

    public void ChangeState(EnemyState NewState)
    {
        StopAllCoroutines();
        CurrentState = NewState;

        switch (NewState)
        {
            case EnemyState.IDLE:
                StartCoroutine(Idle());
                break;

            case EnemyState.CHASE:
                StartCoroutine(Chase());
                break;

        }
    }

    //---


    /*void OnTriggerEnter(Collider Col)
    {
        if (!Col.gameObject.CompareTag("Player"))

        //if (!Col.CompareTag("Player"))
        {
            //Debug.Log("Doesnt See");
            CanSeePlayer = false;
            return;

        }

        //Debug.Log("See Player1");
        CanSeePlayer = true;
    }*/

    void OnTriggerEnter(Collider Col)
    {
        if (!Col.gameObject.CompareTag("Player"))
            CanSeePlayer = true;
        //if (!Col.CompareTag("Player"))

        //Debug.Log("Doesnt See");
        //CanSeePlayer = false;
        //return;



        //Debug.Log("See Player1");
        //CanSeePlayer = true;
        
        /*
        if (!Col.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Doesnt See");
            return;
        }
        CanSeePlayer = false;*/


        //Debug.Log("See Player1");
        //CanSeePlayer = true;



        // If colliding with a physics object, take knockback from it - Jamie
        var rb = Col.GetComponent<Rigidbody>();
        if (rb != null && !rb.CompareTag("Player") && !rb.CompareTag("Enemy") && Col.isTrigger == false)
        {
            var direction = (transform.position - rb.transform.position).normalized;
            ApplyForce(5 * direction);
            
            // Disable navigation mesh agent to allow for falling off
            DisableAgent();
            EnableAgentAfterSeconds(2.3f);
        }
        return;
    }

    //---

    public Vector3 RandomMovement()
    {
        float Radius = 5f;
        Vector3 Point = ThisTransform.position + Random.insideUnitSphere * Radius;
        NavMeshHit NH;
        NavMesh.SamplePosition(Point, out NH, Radius, NavMesh.AllAreas);
        return NH.position;
    }

    //---

    void OnTriggerExit(Collider Col)
    {
        if (!Col.gameObject.CompareTag("Player"))
            return;

        CanSeePlayer = false;
    }

    //---




    // Update is called once per frame
    void Update ()
	{
	    // Kill zombie if it falls below kill plane
	    if (transform.position.y <= -1.0f)
	        Destroy(this);
        /*
      //  var player0 = GameManager.GetCurrent().GetPlayerCharacter(0);
		// Do not continue if there is no player 0
	//	if (player0 == null)
			return;
        /*
		var targetObject = player0.transform.position;
        agent.SetDestination(targetObject);

		if (Input.GetKeyDown(KeyCode.R))
		{
			agent.velocity = 20f * -agent.velocity.normalized;
		}
        */
    }

    void OnDestroy()
    {
        // Put any xp related stuff here
    }

    // Jamie's physics stuff
    public void ApplyForce(Vector3 velocity)
    {
        agent.velocity = velocity;
    }

    public void DisableAgent()
    {
        agent.updatePosition = false;
        //agent.enabled = false;
    }
    
    public void EnableAgent()
    {
        agent.nextPosition = transform.position;
        agent.updatePosition = true;
        //agent.enabled = true;
    }
    
    public void EnableAgentAfterSeconds(float seconds)
    {
        Invoke("EnableAgent", seconds);
    }

}
