using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]

public class Unit : MonoBehaviour
{
    Animator anim;
    Outline outline;
    CapsuleCollider col;
    CharacterController controller;
    NavMeshAgent agent;

    public bool owned = false;

    Dictionary<string, AnimationClip> animations;

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        outline = this.gameObject.GetComponent<Outline>();
        col = this.gameObject.GetComponent<CapsuleCollider>();
        controller = this.gameObject.GetComponent<CharacterController>();
        agent =this.gameObject.GetComponent<NavMeshAgent>();
        animations = new Dictionary<string, AnimationClip>();
        foreach(AnimationClip ac in anim.runtimeAnimatorController.animationClips){
            Debug.Log("Unit has animation: " + ac.name);
            animations[ac.name] = ac;
        }

    }

    public void MoveTo(Vector3 position){
        agent.SetDestination(position);
        anim.Play("move");
    }
}
