using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimator : MonoBehaviour
{
    public InputActionProperty triggerAction;
    public InputActionProperty gripAction;
    float _triggerValue;
    float _gripValue;
    Animator animator;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _triggerValue = triggerAction.action.ReadValue<float>();
        _gripValue = gripAction.action.ReadValue<float>();

        animator.SetFloat("Trigger", _triggerValue);
        animator.SetFloat("Grip", _gripValue);
    }
}
