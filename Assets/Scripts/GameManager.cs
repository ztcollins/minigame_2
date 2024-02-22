using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    bool gameHasEnded = false;

    public float restartDelay = 1f;

    public GameObject completeLevelUI;
    public GameObject failLevelUI;
    public GameObject normalDog;
    public GameObject indicatorDog;
    public GameObject beastDog;
    public AudioSource source;
    public AudioClip music1;
    public AudioClip music2;
    public AudioClip music3;
    public AudioSource dogSource;
    public AudioClip dogGrowl;

    private AudioClip[] clips;

    void Start()
    {
        clips = new AudioClip[] {music1, music2, music3};
    }

    public void CompleteLevel () {
        completeLevelUI.SetActive(true);
    }

    public void EndGame () {

        if(gameHasEnded == false) {
            gameHasEnded = true;
            failLevelUI.SetActive(true);
            Invoke("Restart", restartDelay);
        }
    }

    void Restart () {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void playMusic () {
        int sceneIdx = SceneManager.GetActiveScene().buildIndex;
        int shouldBePlayingTrack = 0;
        if (sceneIdx >= 0 && sceneIdx <= 2) shouldBePlayingTrack = 0;
        else if (sceneIdx >= 3 && sceneIdx <= 5) shouldBePlayingTrack = 1;
        else shouldBePlayingTrack = 2;

        source.PlayOneShot(clips[shouldBePlayingTrack]);
    }

    public void playGrowls () {
        Debug.Log("playing growl");
        dogSource.clip = dogGrowl;
        dogSource.loop = true;
        dogSource.Play();
    }

    public void pauseGrowls () {
        dogSource.Pause();
    }

    public void setNormal() {
        if(beastDog.activeInHierarchy) {
            beastDog.SetActive(false);
        }
        if(indicatorDog.activeInHierarchy) {
            indicatorDog.SetActive(false);
        }

        if(!normalDog.activeInHierarchy) {
            normalDog.SetActive(true);
        }
    }

    public void setIndicator() {
        if(beastDog.activeInHierarchy) {
            beastDog.SetActive(false);
        }
        if(normalDog.activeInHierarchy) {
            normalDog.SetActive(false);
        }

        if(!indicatorDog.activeInHierarchy) {
            indicatorDog.SetActive(true);
        }
    }

    public void setBeast() {
        if(indicatorDog.activeInHierarchy) {
            indicatorDog.SetActive(false);
        }
        if(normalDog.activeInHierarchy) {
            normalDog.SetActive(false);
        }
        Debug.Log("SETTING BEAST");
        if(!beastDog.activeInHierarchy) {
            beastDog.SetActive(true);
        }

    }
}
