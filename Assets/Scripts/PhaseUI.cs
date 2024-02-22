using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhaseUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    public void setPhase(string dogPhase)
    {
        scoreText.text = "Dog Phase: " + dogPhase;
        if(dogPhase == "BEAST!!!") {
            scoreText.color = new Color(255,0,0);
        }
        else if(dogPhase == "playful!") {
            scoreText.color = new Color(0,255,0);
        }
        else if(dogPhase == "CAREFUL!") {
            scoreText.color = new Color(255,255,153);
        }
        else {
            scoreText.color = new Color(0,0,0);
        }
        
    }
}