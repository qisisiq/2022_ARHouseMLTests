using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeap.Examples;
using UnityEngine;

public class BouncyBall : MonoBehaviour
{
    private AudioSource _audioSource;

    public ParticleSystem _particleSystemPrefab;
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
        var overallDistance = Vector3.Distance(BounceMeshing.Instance.PreviousBallPos, transform.position)
                              > BounceMeshing.Instance.MinDistanceBetweenBounce;
        if (overallDistance)
        {
            BounceMeshing.Instance.NumBounces++;
            BounceMeshing.Instance.UpdateText();
            _audioSource.PlayOneShot(_audioSource.clip);

            var ps = Instantiate(_particleSystemPrefab);
            ps.transform.position = other.contacts[0].point;
            ps.transform.localRotation = Quaternion.FromToRotation(Vector3.up, other.contacts[0].normal);
            ps.Play();

        }
        BounceMeshing.Instance.PreviousBallPos = transform.position;
    }
}
