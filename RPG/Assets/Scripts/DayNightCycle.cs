using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DayPhase = GameTime.DayPhase;

/**
 * Class that governs the passing of in-game time.
 *
 * There are 24 hours in a day, which are divided into day and night. During the
 * day, the sun rises in the east and sets in the west. During the night, the
 * same is true of the moon.
 *
 * The positive z-axis is considered north.
 *
 * Day and night each have their own ambient light settings, as do the
 * transitions between them (dusk and dawn).
 *
 * The environment lighting should have its Sun Source set to None, to ensure
 * that it correctly alternates between the sun and moon.
 */
public class DayNightCycle : MonoBehaviour {

    private const float SUN_INTENSITY = 1.0f;
    private const float MOON_INTENSITY = 0.2f;

    private static readonly Color AMBIENT_COLOR_DAWN
            = new Color(0.5f, 0.4f, 0.35f);
    private static readonly Color AMBIENT_COLOR_DAY
            = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color AMBIENT_COLOR_DUSK
            = new Color(0.5f, 0.35f, 0.5f);
    private static readonly Color AMBIENT_COLOR_NIGHT
            = new Color(0.25f, 0.35f, 0.5f);

    private static readonly Vector3 LIGHT_TILT = new Vector3(-50, 0, 0);

    private GameTime time;
    private Light sun;
    private Light moon;

    void Start() {
        sun = transform.Find("Sun").GetComponent<Light>();
        moon = transform.Find("Moon").GetComponent<Light>();

        time = GameLogic.Instance.GameTime;
    }

    void Update() {
        UpdateSunAndMoon();
        UpdateDayPhase();
    }

    private void UpdateSunAndMoon() {
        float currentTimeMs = time.GetCurrentTimeMs();
        float t = currentTimeMs / GameTime.DAY_LENGTH_MS;
        float angle = 180.0f - Mathf.LerpAngle(0.0f, 180.0f, t);
        if (IsSunPresent()) {
            SetLightAngle(sun, angle);
            moon.intensity = 0.0f;
        } else {
            SetLightAngle(moon, angle);
            sun.intensity = 0.0f;
        }
    }

    private bool IsSunPresent() {
        float currentTimeMs = time.GetCurrentTimeMs();
        return currentTimeMs > GameTime.SUNRISE_TIME_MS
                && currentTimeMs < GameTime.SUNSET_TIME_MS;
    }

    private void SetLightAngle(Light light, float angle) {

        // Rotate the light along the east-west axis
        light.transform.eulerAngles = new Vector3(angle, 90, 0);

        // Tilt the light to give us a more interesting angle
        light.transform.Rotate(LIGHT_TILT, Space.World);
    }

    private void UpdateDayPhase() {
        DayPhase phase = time.GetPhase();
        switch (phase) {
            case DayPhase.DAWN:
                UpdateDawn();
                break;
            case DayPhase.DAY:
                UpdateDay();
                break;
            case DayPhase.DUSK:
                UpdateDusk();
                break;
            case DayPhase.NIGHT:
                UpdateNight();
                break;
        }
    }

    private float CalculateDaytimeProgress() {
        float dayTimePassed =
                time.GetCurrentTimeMs() - GameTime.DAYTIME_START_MS;
        return dayTimePassed / GameTime.DAYTIME_LENGTH_MS;
    }

    private float CalculateNightTimeProgress() {
        float nightTimePassed;
        if (time.GetCurrentTimeMs() >= GameTime.NIGHT_START_MS) {
            nightTimePassed =
                    time.GetCurrentTimeMs() - GameTime.NIGHT_START_MS;
        } else {
            // Early morning - be sure to account for the night hours that
            // elapsed before midnight!
            nightTimePassed = time.GetCurrentTimeMs()
                    + (GameTime.DAY_LENGTH_MS - GameTime.NIGHT_START_MS);
        }
        return nightTimePassed / GameTime.NIGHT_LENGTH_MS;
    }

    private void UpdateDawn() {
        // Dawn is further divided into 2 phases: moon setting and sun rising.
        // We need to fully disable one light source before we enable another,
        // otherwise the change in the skybox is jarring.
        if (!IsSunPresent()) {
            // Moon is setting
            UpdateMoonSet();
        } else {
            // Sun is rising
            UpdateSunrise();
        }
    }

    private void UpdateMoonSet() {
        // Calculate progress through the phase, in the range 0-1
        float t = Mathf.InverseLerp(
                GameTime.DAWN_START_MS,
                GameTime.DAWN_START_MS + GameTime.DAWN_LENGTH_MS / 2.0f,
                time.GetCurrentTimeMs());
        moon.intensity = Mathf.Lerp(MOON_INTENSITY, 0.0f, t);
        RenderSettings.ambientLight =
                Color.Lerp(AMBIENT_COLOR_NIGHT, AMBIENT_COLOR_DAWN, t);
    }

    private void UpdateSunrise() {
        // Calculate progress through the phase, in the range 0-1
        float t = Mathf.InverseLerp(
                GameTime.DAWN_START_MS + GameTime.DAWN_LENGTH_MS / 2.0f,
                GameTime.DAYTIME_START_MS,
                time.GetCurrentTimeMs());
        sun.intensity = Mathf.Lerp(0.0f, SUN_INTENSITY, t * 2);
        RenderSettings.ambientLight =
                Color.Lerp(AMBIENT_COLOR_DAWN, AMBIENT_COLOR_DAY, t);
    }

    private void UpdateDay() {
        sun.intensity = SUN_INTENSITY;
        RenderSettings.ambientLight = AMBIENT_COLOR_DAY;
    }

    private void UpdateDusk() {
        // Dusk is further divided into 2 phases: sun setting and moon rising.
        // We need to fully disable one light source before we enable another,
        // otherwise the change in the skybox is jarring.
        if (IsSunPresent()) {
            // Sun is setting
            UpdateSunset();
        } else {
            // Moon is rising
            UpdateMoonRise();
        }
    }

    private void UpdateSunset() {
        // Calculate progress through the phase, in the range 0-1
        float t = Mathf.InverseLerp(
                GameTime.DUSK_START_MS,
                GameTime.DUSK_START_MS + GameTime.DUSK_LENGTH_MS / 2.0f,
                time.GetCurrentTimeMs());
        sun.intensity = Mathf.Lerp(SUN_INTENSITY, 0.0f, t);
        RenderSettings.ambientLight =
                Color.Lerp(AMBIENT_COLOR_DAY, AMBIENT_COLOR_DUSK, t);
    }

    private void UpdateMoonRise() {
        // Calculate progress through the phase, in the range 0-1
        float t = Mathf.InverseLerp(
                GameTime.DUSK_START_MS + GameTime.DUSK_LENGTH_MS / 2.0f,
                GameTime.NIGHT_START_MS,
                time.GetCurrentTimeMs());
        moon.intensity = Mathf.Lerp(0.0f, MOON_INTENSITY, t * 2);
        RenderSettings.ambientLight =
                Color.Lerp(AMBIENT_COLOR_DUSK, AMBIENT_COLOR_NIGHT, t);
    }

    private void UpdateNight() {
        moon.intensity = MOON_INTENSITY;
        RenderSettings.ambientLight = AMBIENT_COLOR_NIGHT;
    }

}
