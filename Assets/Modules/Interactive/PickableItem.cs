using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyDoggy.Interactive;
using NaughtyDoggy.Helper;
using UnityEngine;

public class PickableItem : InteractiveItemBase
{
    [SerializeField] private GameObject _handle;
    [SerializeField] private GameObject _holder;
    private Transform _dynamicObjParent;
    private Rigidbody _rigidbody;
    
    public bool BeHeld = false;
    protected override void Start()
    {
        base.Start();
        _handle = transform.Find("Handle").gameObject;
        _rigidbody = GetComponent<Rigidbody>();
        _dynamicObjParent = transform.parent;
    }

    private void FixedUpdate()
    {
        
    }

    public override void HandleInteraction()
    {
        base.HandleInteraction();
        if (!_holder)
        {
            _holder = GameObject.FindWithTag("Player").transform.Find("Holder").gameObject;
            if (!_holder)
            {
                Debug.LogError("Player object should be tagged with \'Player\'");
            }
        }

        if (!BeHeld)
        {
            BeHeld = true;
            transform.SetParent(_holder.transform, false);
            transform.localPosition =  - MathHelper.Vec3Mul(_handle.transform.localPosition, transform.localScale);
            transform.rotation = _holder.transform.rotation;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
        }
        else
        {
            BeHeld = false;
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
            transform.SetParent(_dynamicObjParent, true);
        }
            
    }
}
