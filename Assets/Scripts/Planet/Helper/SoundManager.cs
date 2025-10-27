using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSourcePrefab;

    private List<AudioSource> sources = new List<AudioSource>();

    public static SoundManager Instance;
    private void Awake() {
        Instance = this;
    }

    public void PlaySound(AudioClip clip, float volume, float pitch, Vector3 position, float minDistance, float maxDistance) {
        if (sources.Count >= 32) return;

        AudioSource source = Instantiate(audioSourcePrefab);
        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.Play();
        sources.Add(source);

        StartCoroutine(KillSound(source));
    }
    private IEnumerator KillSound(AudioSource source) {
        float p = source.pitch;
        yield return new WaitForSeconds(source.clip.length / (p == 0f ? 0.01f : p));

        sources.Remove(source);
        Destroy(source.gameObject);
    }
}
