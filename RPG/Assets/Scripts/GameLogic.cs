using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class encapsulating the global game logic and subsystems.
 *
 * Examples:
 *  GameLogic.Instance
 *  GameLogic.Instance.GameTime
 */
public class GameLogic : MonoBehaviour {

    public static GameLogic Instance {
        get;
        private set;
    }

    public GameTime GameTime {
        get;
        private set;
    }

    void Awake() {
        Instance = this;

        // Start at midnight
        GameTime = new GameTime(0.0f);
    }

    void OnDestroy() {
        Instance = null;
    }

    void Start() { }

    void Update() {
        GameTime.Update(Time.deltaTime);
    }

}
