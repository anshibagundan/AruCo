using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPerfomance : MonoBehaviour
{
    public AudioClip sound1;
    AudioSource audioSource;
    void Start()
    {
        //Component‚ðŽæ“¾
        audioSource = GetComponent<AudioSource>();
    }

    public void GoalPerformance()
    {
        //‰¹(sound1)‚ð–Â‚ç‚·
        audioSource.PlayOneShot(sound1);


    }
}
