﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    /**
     * Length of each intensity transition, relative to the length of the
     * day (for the sun) or night (for the moon).
     */
    private const float INTENSITY_TRANSITION_LENGTH = 0.15f;

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
        if (time.IsNight()) {
            float t = CalculateNightTimeProgress();
            UpdateNight(t);
        } else {
            float t = CalculateDaytimeProgress();
            UpdateDay(t);
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

    private void UpdateNight(float t) {

        // Move the moon across the sky
        float angle = 180.0f - Mathf.LerpAngle(0.0f, 180.0f, t);
        SetLightAngle(moon, angle);

        // Adjust the lighting over time
        float intensity;
        Color ambientLight;

        if (t < INTENSITY_TRANSITION_LENGTH) {
            // Dusk - Night (moon rising)
            float t2 = Mathf.InverseLerp(0.0f, INTENSITY_TRANSITION_LENGTH, t);
            intensity = Mathf.Lerp(0.0f, MOON_INTENSITY, t2);
            ambientLight =
                    Color.Lerp(AMBIENT_COLOR_DUSK, AMBIENT_COLOR_NIGHT, t2);

        } else if (t < 1 - INTENSITY_TRANSITION_LENGTH) {
            // Night (moon at full intensity)
            intensity = MOON_INTENSITY;
            ambientLight = AMBIENT_COLOR_NIGHT;

        } else {
            // Night - Dawn (moon setting)
            float t2 = Mathf.InverseLerp(
                    1 - INTENSITY_TRANSITION_LENGTH, 1.0f, t);
            intensity = Mathf.Lerp(MOON_INTENSITY, 0.0f, t2);
            ambientLight =
                    Color.Lerp(AMBIENT_COLOR_NIGHT, AMBIENT_COLOR_DAWN, t2);
        }

        moon.intensity = intensity;
        RenderSettings.ambientLight = ambientLight;

        // Ensure sun is always disabled during the day
        sun.intensity = 0.0f;
    }

    private void UpdateDay(float t) {

        // Move the sun across the sky
        float angle = 180.0f - Mathf.LerpAngle(0.0f, 180.0f, t);
        SetLightAngle(sun, angle);

        // Adjust the lighting over time
        float intensity;
        Color ambientLight;

        if (t < INTENSITY_TRANSITION_LENGTH) {
            // Dawn - Day (sun rising)
            float t2 = Mathf.InverseLerp(0.0f, INTENSITY_TRANSITION_LENGTH, t);
            intensity = Mathf.Lerp(0.0f, SUN_INTENSITY, t2);
            ambientLight =
                    Color.Lerp(AMBIENT_COLOR_DAWN, AMBIENT_COLOR_DAY, t2);

        } else if (t < 1 - INTENSITY_TRANSITION_LENGTH) {
            // Day (sun at full intensity)
            intensity = SUN_INTENSITY;
            ambientLight = AMBIENT_COLOR_DAY;

        } else {
            // Day - Dusk (sun setting)
            float t2 = Mathf.InverseLerp(
                    1 - INTENSITY_TRANSITION_LENGTH, 1.0f, t);
            intensity = Mathf.Lerp(SUN_INTENSITY, 0.0f, t2);
            ambientLight =
                    Color.Lerp(AMBIENT_COLOR_DAY, AMBIENT_COLOR_DUSK, t2);
        }

        sun.intensity = intensity;
        RenderSettings.ambientLight = ambientLight;

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
