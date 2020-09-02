using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class that governs the passing of in-game time.
 *
 * There are 24 hours in a day, which are divided into day and night.
 * During the day, the sun rises in the east and sets in the west.
 * During the night, the same is true of the moon.
 *
 * The positive z-axis is considered north.
 *
 * The environment lighting should have its Sun Source set to None, to ensure
 * that it correctly alternates between the sun and moon.
 */
public class DayNightCycle : MonoBehaviour {

    private const int HOURS_IN_DAY = 24;
    private const int DAY_LENGTH_MS = 10000; // TMP
    private const int HOUR_LENGTH_MS = DAY_LENGTH_MS / HOURS_IN_DAY;

    // Daytime is 06:00-20:00 (14 hours)
    // Night-time is 20:00-06:00 (10 hours)
    private const int DAYTIME_START_HOUR = 6;
    private const int NIGHT_START_HOUR = 20;
    private const int DAYTIME_START_MS = DAYTIME_START_HOUR * HOUR_LENGTH_MS;
    private const int NIGHT_START_MS = NIGHT_START_HOUR * HOUR_LENGTH_MS;
    private const int DAYTIME_LENGTH_MS = NIGHT_START_MS - DAYTIME_START_MS;
    private const int NIGHT_LENGTH_MS = DAY_LENGTH_MS - DAYTIME_LENGTH_MS;

    private const float SUN_INTENSITY = 1.0f;
    private const float MOON_INTENSITY = 0.2f;

    /**
     * Length of each intensity transition, relative to the length of the
     * day (for the sun) or night (for the moon).
     */
    private const float INTENSITY_TRANSITION_LENGTH = 0.15f;

    private static readonly Vector3 LIGHT_TILT = new Vector3(-50, 0, 0);

    private float currentTimeMs;  // 0 - DAY_LENGTH_MS
    private float hourOfDay;      // 0 - HOURS_IN_DAY
    private bool night;

    private Light sun;
    private Light moon;

    void Start() {
        sun = transform.Find("Sun").GetComponent<Light>();
        moon = transform.Find("Moon").GetComponent<Light>();

        // Midnight
        SetTime(0.0f);
    }

    void Update() {
        SetTime(currentTimeMs + Time.deltaTime * 1000);
    }

    private void SetTime(float newTimeMs) {
        currentTimeMs = newTimeMs % DAY_LENGTH_MS;
        hourOfDay = currentTimeMs / HOUR_LENGTH_MS;
        night = (hourOfDay < DAYTIME_START_HOUR
                || hourOfDay >= NIGHT_START_HOUR);
        Refresh();
    }

    private void Refresh() {
        if (night) {
            float t = CalculateNightTimeProgress();
            UpdateNight(t);
        } else {
            float t = CalculateDaytimeProgress();
            UpdateDay(t);
        }
    }

    private float CalculateDaytimeProgress() {
        float dayTimePassed = currentTimeMs - DAYTIME_START_MS;
        return dayTimePassed / DAYTIME_LENGTH_MS;
    }

    private float CalculateNightTimeProgress() {
        float nightTimePassed;
        if (currentTimeMs >= NIGHT_START_MS) {
            nightTimePassed = currentTimeMs - NIGHT_START_MS;
        } else {
            // Early morning - be sure to account for the night hours that
            // elapsed before midnight!
            nightTimePassed = currentTimeMs + (DAY_LENGTH_MS - NIGHT_START_MS);
        }
        return nightTimePassed / NIGHT_LENGTH_MS;
    }

    private void UpdateNight(float t) {

        // Move the moon across the sky
        float angle = 180.0f - Mathf.LerpAngle(0.0f, 180.0f, t);
        SetLightAngle(moon, angle);

        // Change the brightness over time
        if (t < INTENSITY_TRANSITION_LENGTH) {
            // Moon rising
            float t2 = Mathf.InverseLerp(0.0f, INTENSITY_TRANSITION_LENGTH, t);
            moon.intensity = Mathf.Lerp(0.0f, MOON_INTENSITY, t2);
        } else if (t > 1 - INTENSITY_TRANSITION_LENGTH) {
            // Moon setting
            float t2 = Mathf.InverseLerp(1 - INTENSITY_TRANSITION_LENGTH, 1.0f, t);
            moon.intensity = Mathf.Lerp(MOON_INTENSITY, 0.0f, t2);
        }

        // Ensure sun is always disabled during the day
        sun.intensity = 0.0f;
    }

    private void UpdateDay(float t) {

        // Move the sun across the sky
        float angle = 180.0f - Mathf.LerpAngle(0.0f, 180.0f, t);
        SetLightAngle(sun, angle);

        // Change the brightness over time
        if (t < INTENSITY_TRANSITION_LENGTH) {
            // Sun rising
            float t2 = Mathf.InverseLerp(0.0f, INTENSITY_TRANSITION_LENGTH, t);
            sun.intensity = Mathf.Lerp(0.0f, SUN_INTENSITY, t2);
        } else if (t > 1 - INTENSITY_TRANSITION_LENGTH) {
            // Sun setting
            float t2 = Mathf.InverseLerp(1 - INTENSITY_TRANSITION_LENGTH, 1.0f, t);
            sun.intensity = Mathf.Lerp(SUN_INTENSITY, 0.0f, t2);
        }

        // Ensure moon is always disabled during the day
        moon.intensity = 0.0f;
    }

    private void SetLightAngle(Light light, float angle) {

        // Rotate the light along the east-west axis
        light.transform.eulerAngles = new Vector3(angle, 90, 0);

        // Tilt the light to give us a more interesting angle
        light.transform.Rotate(LIGHT_TILT, Space.World);
    }

}
