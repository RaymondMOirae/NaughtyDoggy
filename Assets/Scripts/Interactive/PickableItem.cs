using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyDoggy.Interactive;
using UnityEngine;

public class PickableItem : InteractiveItemBase
{
    [SerializeField] private GameObject _handle;
    [SerializeField] private GameObject _holder;
    [SerializeField] private bool _beHeld = false;
    private Rigidbody _rigidbody;
    protected override void Start()
    {
        base.Start();
        _handle = transform.Find("Handle").gameObject;
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_beHeld)
        {
            transform.position = _holder.transform.position + _handle.transform.localPosition;
        }

    }

    public override void HandleInteraction()
    {
        base.HandleInteraction();
        if (_holder)
        {
            _holder = GameObject.FindWithTag("Player");
            if (_holder)
            {
                Debug.LogError("Player object should be tagged with \'Player\'");
            }
        }

        if (!_beHeld)
        {
            _beHeld = true;
            _rigidbody.useGravity = false;
        }
        else
        {
            _beHeld = false;
            _rigidbody.useGravity = true;
        }
            
    }
}
