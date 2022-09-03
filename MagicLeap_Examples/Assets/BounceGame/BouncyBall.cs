using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeap.Examples;
using UnityEngine;

public class BouncyBall : MonoBehaviour
{
    private AudioSource _audioSource;

    public ParticleSystem _particleSystem;
    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (Vector3.Distance(BounceMeshing.Instance.PreviousBallPos, transform.position)
            > BounceMeshing.Instance.MinDistanceBetweenBounce)
        {
            BounceMeshing.Instance.NumBounces++;
            BounceMeshing.Instance.UpdateText();
            _audioSource.PlayOneShot(_audioSource.clip);
            _particleSystem.Play();

        }
        BounceMeshing.Instance.PreviousBallPos = transform.position;
    }
}
