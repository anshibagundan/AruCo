using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPerfomance : MonoBehaviour
{
    public AudioClip sound1;
    AudioSource audioSource;
    void Start()
    {
        //Component���擾
        audioSource = GetComponent<AudioSource>();
    }

    public void GoalPerformance()
    {
        //��(sound1)��炷
        audioSource.PlayOneShot(sound1);


    }
}
