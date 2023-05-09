using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SEManager : ScriptableObject
{
	public AudioSource audioSource { get; set; }

	public void PlayOneShot(AudioClip clip)
	{
		if (audioSource != null)
		{
			audioSource.PlayOneShot(clip);
		}
	}
}
