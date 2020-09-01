using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroTextReveal : MonoBehaviour {

    private const float CHARACTER_REVEAL_DELAY = 0.02f;   // Seconds

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
        // TODO: If space is pressed, reveal all characters
        // TODO: If space is pressed and all characters are revealed,
        //        load the game!
    }

    IEnumerator RevealCharacter() {
        while (revealedChars < fullText.Length) {
            revealedChars++;
            textMesh.text = fullText.Substring(0, revealedChars);
            yield return new WaitForSeconds(CHARACTER_REVEAL_DELAY);
        }
    }

}
