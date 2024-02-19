using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    bool gameHasEnded = false;

    public float restartDelay = 1f;

    public GameObject completeLevelUI;
    public GameObject failLevelUI;
    public AudioSource source;
    public AudioClip music1;
    public AudioClip music2;
    public AudioClip music3;
    public AudioSource dogSource;
    public AudioClip dogGrowl;

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
        int playIndex = SceneManager.GetActiveScene().buildIndex;
        if(playIndex == 0) {
            //play first track
            source.PlayOneShot(music1);
        }
        else if (playIndex == 1) {
            source.PlayOneShot(music2);
        }
        else {
            source.PlayOneShot(music3);
        }
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

}
