using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]

public class Unit : MonoBehaviour
{

    Outline outline;
    CapsuleCollider col;
    NavMeshAgent agent;

    // true if the user owns this object.
    public bool owned = false;

    Dictionary<string, AnimationClip> animations;

    // Movement & animation
    public bool flying = false, swimming =false, floored = true;

    public float flyingSpeed = 7.5f, swimmingSpeed = 3.5f;

    Animator anim;

    Vector3 currentDestination = new Vector3();

    void Start()
    {
        anim = this.gameObject.GetComponentInChildren<Animator>();
        anim.applyRootMotion=false;

        outline = this.gameObject.GetComponent<Outline>();
        agent = this.gameObject.GetComponent<NavMeshAgent>();
        col = this.gameObject.GetComponent<CapsuleCollider>();
        StartCoroutine(MovementUpdater());
    }

    IEnumerator MovementUpdater(){
        var dist = Vector3.Distance(transform.position, currentDestination);
        if(dist < 1){
            Idle();
        }

        yield return new WaitForSeconds(.5f);
        StartCoroutine(MovementUpdater());
    }

    #region movement & animation

    IEnumerator MoveToHovering(float time, Vector3 destination){
        Vector3 startingPos  = transform.position;
        float elapsedTime = 0;
        
        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(startingPos, destination, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void MoveTo(Vector3 destination){
        if(floored){
            anim.Play("move");
            currentDestination = destination;
            agent.SetDestination(destination);
        }else{
            currentDestination = destination;
            currentDestination.y = transform.position.y;
            this.StopCoroutine("MoveToHovering");
            var d = Vector3.Distance(gameObject.transform.position, currentDestination);

            if(swimming){
                anim.Play("swim");
                gameObject.transform.LookAt(currentDestination);
                StartCoroutine(MoveToHovering(d/swimmingSpeed, currentDestination));
            }

            if(flying){
                anim.Play("fly");
                gameObject.transform.LookAt(currentDestination);
                StartCoroutine(MoveToHovering(d/flyingSpeed, currentDestination));
            }
        }
    }

    public void TransitionState(string to){
        if(to == "floored"){
            if(!floored){
                reFloor();
                swimming = false;
                flying = false;
            }
        }else if (to == "swimming"){
            if(!swimming){
                unFloor();
                swimming = true;
                flying = false;
            }
        }else if (to == "flying"){
            if(!flying){
                unFloor();
                swimming = false;
                flying = true;
            }
        }
        Idle();
    }

    private void unFloor(){
        floored = false;
        agent.enabled = false;
    }

    private void reFloor(){
        floored = true;
        agent.enabled = true;
    }

    public void RaiseHeight(){
        var newPos = this.transform.position + new Vector3(0,1,0);
        if(newPos.y <= 98){
            this.transform.position += new Vector3(0,1,0);
        }
    }

    public void LowerHeight(){
        var newPos = this.transform.position + new Vector3(0,-1,0);
        if(newPos.y >= 2){
            this.transform.position += new Vector3(0,-1,0);
        }
    }

    public void Idle(){
        if(flying){
            anim.Play("flyidle");
        }else if(swimming){
            anim.Play("swimidle");
        }else if (floored){
            anim.Play("idle");
        }
    }
    #endregion
}
