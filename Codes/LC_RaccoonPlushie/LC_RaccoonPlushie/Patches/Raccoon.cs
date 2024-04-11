using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raccoon : GrabbableObject
{
    private Animator animator;
    private static AudioClip[] sounds;
    private AudioSource audio;

    public override void Start()
    {
        base.Start();
        audio = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    public override void Update()
    {
        base.Update();
        animator.SetBool("IsHeld", isHeld);
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        animator.SetTrigger("Squish");
        audio.PlayOneShot(sounds[Random.Range(0, sounds.Length)]);
        RoundManager.Instance.PlayAudibleNoise(base.transform.position, 5f, 0.4f, 0, isInElevator && StartOfRound.Instance.hangarDoorsClosed);
    }


    public static void SetAudio(AudioClip[] audio)
    {
        sounds = audio;
    }

}
