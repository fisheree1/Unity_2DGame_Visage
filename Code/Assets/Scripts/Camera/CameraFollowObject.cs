using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CameraFollowObject : MonoBehaviour
{

    [SerializeField] private Transform _playerTransfrom;

    [SerializeField] private float _flipYRotationTime = 0.5f;

    private Coroutine _turnCoroutine;

    private HeroMovement _heroMovement;

    private bool _isFacingRight;

    private void Awake()
    {
        _heroMovement = _playerTransfrom.gameObject.GetComponent<HeroMovement>();
        _isFacingRight = _heroMovement.isFacingRight;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = _playerTransfrom.position;
    }

    public void CallTurn() {
        // _turnCoroutine = StartCoroutine(FlipYLerp());
        LeanTween.rotateY(gameObject,DetermineEndRotation(), _flipYRotationTime).setEaseInOutSine();
    }

    private float DetermineEndRotation() {
        _isFacingRight = !_isFacingRight;

        if (_isFacingRight)return 180f;
        else return 0f;
    }
}
