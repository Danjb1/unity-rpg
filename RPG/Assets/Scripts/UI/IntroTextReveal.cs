using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroTextReveal : MonoBehaviour {

    private const float CHARACTER_REVEAL_DELAY = 0.025f;   // Seconds

    private TextMeshProUGUI textMesh;
    private string fullText;
    private int revealedChars;

    void Start() {
        textMesh = GetComponent<TextMeshProUGUI>();
        fullText = textMesh.text;
        textMesh.text = "";
        StartCoroutine("RevealCharacter");
    }

    void Update() {
        if (Input.GetButtonUp("Jump")) {
            if (revealedChars < fullText.Length) {
                RevealAllCharacters();
            } else {
                StartGame();
            }
        }
    }

    private void RevealAllCharacters() {
        revealedChars = fullText.Length;
        textMesh.text = fullText.Substring(0, revealedChars);
    }

    IEnumerator RevealCharacter() {
        while (revealedChars < fullText.Length) {
            revealedChars++;
            textMesh.text = fullText.Substring(0, revealedChars);
            yield return new WaitForSeconds(CHARACTER_REVEAL_DELAY);
        }
    }

    private void StartGame() {
        SceneManager.LoadScene("Game");
    }

}
