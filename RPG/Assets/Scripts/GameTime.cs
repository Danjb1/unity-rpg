using System.Collections;
using System.Collections.Generic;

/**
 * Governs the passing of in-game time.
 */
public class GameTime {

    public enum DayPhase {
        DAWN,
        DAY,
        DUSK,
        NIGHT
    }

    // Game time definition
    public const int HOURS_IN_DAY = 24;
    public const int DAY_LENGTH_MS = 10000;//10 * 60 * 1000;  // 10 minutes
    public const int HOUR_LENGTH_MS = DAY_LENGTH_MS / HOURS_IN_DAY;

    // Start times of each period, in hours:
    //  Dawn:    06:00-07:00 (1 hour)
    //  Day:     07:00-19:00 (12 hours)
    //  Dusk:    19:00-20:00 (1 hour)
    //  Night:   20:00-06:00 (10 hours)
    public const int DAWN_START_HOUR    = 6;
    public const int DAYTIME_START_HOUR = 7;
    public const int DUSK_START_HOUR    = 19;
    public const int NIGHT_START_HOUR   = 20;

    // Start times of each period, in ms
    public const int DAWN_START_MS = DAWN_START_HOUR * HOUR_LENGTH_MS;
    public const int DAYTIME_START_MS = DAYTIME_START_HOUR * HOUR_LENGTH_MS;
    public const int DUSK_START_MS = DUSK_START_HOUR * HOUR_LENGTH_MS;
    public const int NIGHT_START_MS = NIGHT_START_HOUR * HOUR_LENGTH_MS;

    // Length of each period, in ms
    public const int DAWN_LENGTH_MS = DAYTIME_START_MS - DAWN_START_MS;
    public const int DAYTIME_LENGTH_MS = DUSK_START_MS - DAYTIME_START_MS;
    public const int DUSK_LENGTH_MS = NIGHT_START_MS - DUSK_START_MS;
    public const int NIGHT_LENGTH_MS =
            (DAWN_START_MS + DAY_LENGTH_MS) - NIGHT_START_MS;

    // Sunrise / sunset
    public const int SUNRISE_TIME_MS = DAWN_START_MS + DAWN_LENGTH_MS / 2;
    public const int SUNSET_TIME_MS = DUSK_START_MS + DUSK_LENGTH_MS / 2;

    private float currentTimeMs;  // Ranges from 0 - (DAY_LENGTH_MS - 1)
    private float hourOfDay;      // Ranges from 0 - (HOURS_IN_DAY - 1)
    private DayPhase phase;

    public GameTime(float initialTimeMs) {
        SetTime(initialTimeMs);
    }

    public void Update(float delta) {
        SetTime(currentTimeMs + delta * 1000);
    }

    public void SetTime(float newTimeMs) {
        currentTimeMs = newTimeMs % DAY_LENGTH_MS;
        hourOfDay = currentTimeMs / HOUR_LENGTH_MS;
        phase = CalculatePhase();
    }

    private DayPhase CalculatePhase() {
        if (hourOfDay < DAWN_START_HOUR) {
            return DayPhase.NIGHT;
        } else if (hourOfDay < DAYTIME_START_HOUR) {
            return DayPhase.DAWN;
        } else if (hourOfDay < DUSK_START_HOUR) {
            return DayPhase.DAY;
        } else if (hourOfDay < NIGHT_START_HOUR) {
            return DayPhase.DUSK;
        } else {
            return DayPhase.NIGHT;
        }
    }

    public float GetCurrentTimeMs() {
        return currentTimeMs;
    }

    public DayPhase GetPhase() {
        return phase;
    }

}
