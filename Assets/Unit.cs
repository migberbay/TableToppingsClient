using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(CapsuleCollider))]
public class Unit : MonoBehaviour
{
    Animator anim;
    Outline outline;
    CapsuleCollider col;

    public bool owned = false;

    void Start()
    {
        anim = this.gameObject.GetComponent<Animator>();
        outline = this.gameObject.GetComponent<Outline>();
        col = this.gameObject.GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
