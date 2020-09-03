using System.Collections;
using System.Collections.Generic;

/**
 * Class responsible for keeping track of in-game time.
 */
public class GameTime {

    public const int HOURS_IN_DAY = 24;
    public const int DAY_LENGTH_MS = 10000; // TMP
    public const int HOUR_LENGTH_MS = DAY_LENGTH_MS / HOURS_IN_DAY;

    // Daytime is 06:00-20:00 (14 hours)
    // Night-time is 20:00-06:00 (10 hours)
    public const int DAYTIME_START_HOUR = 6;
    public const int NIGHT_START_HOUR = 20;
    public const int DAYTIME_START_MS = DAYTIME_START_HOUR * HOUR_LENGTH_MS;
    public const int NIGHT_START_MS = NIGHT_START_HOUR * HOUR_LENGTH_MS;
    public const int DAYTIME_LENGTH_MS = NIGHT_START_MS - DAYTIME_START_MS;
    public const int NIGHT_LENGTH_MS = DAY_LENGTH_MS - DAYTIME_LENGTH_MS;

    private float currentTimeMs;  // 0 - DAY_LENGTH_MS
    private float hourOfDay;      // 0 - HOURS_IN_DAY
    private bool night;

    public GameTime(float initialTimeMs) {
        SetTime(initialTimeMs);
    }

    // Update is called once per frame
    public void Update(float delta) {
        SetTime(currentTimeMs + delta * 1000);
    }

    public void SetTime(float newTimeMs) {
        currentTimeMs = newTimeMs % DAY_LENGTH_MS;
        hourOfDay = currentTimeMs / HOUR_LENGTH_MS;
        night = (hourOfDay < DAYTIME_START_HOUR
                || hourOfDay >= NIGHT_START_HOUR);
    }

    public float GetCurrentTimeMs() {
        return currentTimeMs;
    }

    public bool IsNight() {
        return night;
    }

}
