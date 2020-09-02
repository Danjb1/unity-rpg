using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class that governs the passing of in-game time.
 *
 * There are 24 hours in a day, which are divided into day and night:
 *     NNNNNNDDDDDDDDDDDDDDNNNN
 *
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

    private const int DAYTIME_START_HOUR = 6;
    private const int NIGHT_START_HOUR = 20;
    private const int DAYTIME_START_MS = DAYTIME_START_HOUR * HOUR_LENGTH_MS;
    private const int NIGHT_START_MS = NIGHT_START_HOUR * HOUR_LENGTH_MS;
    private const int DAYTIME_LENGTH_MS = NIGHT_START_MS - DAYTIME_START_MS;
    private const int NIGHT_LENGTH_MS = DAY_LENGTH_MS - DAYTIME_LENGTH_MS;

    private const float SUN_INTENSITY_MIN = 0.5f;
    private const float SUN_INTENSITY_MAX = 1.0f;
    private const float MOON_INTENSITY_MIN = 0.25f;
    private const float MOON_INTENSITY_MAX = 0.5f;

    private static readonly Vector3 SUN_TILT = new Vector3(-50, 0, 0);

    private static readonly Color SUN_COLOR = Color.white;
    private static readonly Color MOON_COLOR = new Color(0.75f, 0.75f, 1.0f);

    private float currentTimeMs;  // 0 - DAY_LENGTH_MS
    private float hourOfDay;      // 0 - HOURS_IN_DAY
    private bool night;

    private Light sun;
    private Light moon;

    void Start() {
        sun = transform.Find("Sun").GetComponent<Light>();
        moon = transform.Find("Moon").GetComponent<Light>();

        SetTime(0);
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
            nightTimePassed = currentTimeMs + (DAY_LENGTH_MS - NIGHT_START_MS);
        }
        return nightTimePassed / NIGHT_LENGTH_MS;
    }

    private void UpdateNight(float t) {
        float angle = Mathf.LerpAngle(0, 180, t);
        SetLightAngle(moon, angle);

        if (t < 0.5) {
            // Moon rising, sun fading out
            moon.intensity = Mathf.Lerp(
                    MOON_INTENSITY_MIN, MOON_INTENSITY_MAX, t);
            sun.intensity = Mathf.Lerp(SUN_INTENSITY_MIN, 0, t);
        } else {
            // Moon setting, sun fading in
            moon.intensity = Mathf.Lerp(
                    MOON_INTENSITY_MAX, MOON_INTENSITY_MIN, t);
            sun.intensity = Mathf.Lerp(SUN_INTENSITY_MIN, 0, t);
        }
    }

    private void UpdateDay(float t) {
        float angle = Mathf.LerpAngle(0, 180, t);
        SetLightAngle(sun, angle);

        if (t < 0.5) {
            // Sun rising, moon fading out
            sun.intensity = Mathf.Lerp(SUN_INTENSITY_MIN, SUN_INTENSITY_MAX, t);
            moon.intensity = Mathf.Lerp(MOON_INTENSITY_MIN, 0, t);
        } else {
            // Sun setting, moon fading in
            sun.intensity = Mathf.Lerp(SUN_INTENSITY_MAX, SUN_INTENSITY_MIN, t);
            moon.intensity = Mathf.Lerp(0, MOON_INTENSITY_MIN, t);
        }
    }

    private void SetLightAngle(Light light, float angle) {

        // Rotate the light along the east-west axis
        light.transform.eulerAngles = new Vector3(angle, 90, 0);

        // Tilt the light to give us a more interesting angle
        light.transform.Rotate(SUN_TILT, Space.World);
    }

}
