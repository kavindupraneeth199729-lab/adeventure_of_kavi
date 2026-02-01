using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class IntroManager : MonoBehaviour
{
    public TextMeshProUGUI introText; // Reference to the TextMeshPro text
    public Button skipButton;        // Reference to the Skip Button
    public string[] introductionLines; // Array of lines to display
    public float displayTime = 2f;   // Time to display each line

    private int currentLineIndex = 0;
    private bool isSkipping = false;

    private void Start()
    {
        skipButton.onClick.AddListener(SkipIntro);
        StartCoroutine(DisplayIntro());
    }

    private IEnumerator DisplayIntro()
    {
        while (currentLineIndex < introductionLines.Length && !isSkipping)
        {
            introText.text = introductionLines[currentLineIndex];
            currentLineIndex++;
            yield return new WaitForSeconds(displayTime);
        }

        EndIntro();
    }

    private void SkipIntro()
    {
        isSkipping = true;
        EndIntro();
    }

    private void EndIntro()
    {
        introText.text = "";
        skipButton.gameObject.SetActive(false);
        // Load the main game or continue gameplay
    }
}
