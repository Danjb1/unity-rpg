using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DayPhase = GameTime.DayPhase;

/**
 * Adjusts the sky and lighting based on the current time.
 *
 * During the day, the sun rises in the east and sets in the west. During the
 * night, the same is true of the moon. The positive z-axis is considered north.
 *
 * Day and night each have their own ambient light settings, as do the
 * transitions between them (dusk and dawn).
 *
 * The environment lighting should have its Sun Source set to None, to ensure
 * that it correctly alternates between the sun and moon.
 */
public class DayNightCycle : MonoBehaviour {

    /**
     * Angle of the sun / moon's orbit from vertical.
     *
     * A higher value will cause the sun / moon to stay closer to the southern
     * horizon.
     */
    public float lightTilt;

    // Sun / Moon intensity
    private const float SUN_INTENSITY = 1.0f;
    private const float MOON_INTENSITY = 0.3f;

    // Ambient light colour
    private static readonly Color AMBIENT_COLOR_DAWN
            = new Color(0.5f, 0.4f, 0.35f);
    private static readonly Color AMBIENT_COLOR_DAY
            = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color AMBIENT_COLOR_DUSK
            = new Color(0.4f, 0.35f, 0.4f);
    private static readonly Color AMBIENT_COLOR_NIGHT
            = new Color(0.25f, 0.35f, 0.5f);

    // Sky tint
    private const string SKYBOX_COLOUR_PARAM = "_SkyTint";
    private static readonly Color SKY_TINT_DAWN = new Color(1.0f, 0.5f, 0.0f);
    private static readonly Color SKY_TINT_DAY = Color.white;
    private static readonly Color SKY_TINT_DUSK = new Color(0.5f, 0.25f, 0.5f);
    private static readonly Color SKY_TINT_NIGHT = new Color(0.1f, 0.1f, 0.1f);

    // Math constants
    private const float SUN_ROTATION_PER_MS = 360.0f / GameTime.DAY_LENGTH_MS;

    private GameTime time;
    private Transform sunMoonTransform;
    private Transform skyboxCameraTransform;
    private Light sun;
    private Light moon;

    void Start() {
        sunMoonTransform = transform.Find("Celestial Bodies");
        skyboxCameraTransform = transform.Find("Skybox Camera");
        sun = sunMoonTransform.Find("Sun").GetComponent<Light>();
        moon = sunMoonTransform.Find("Moon").GetComponent<Light>();

        time = GameLogic.Instance.GameTime;
    }

    void OnDestroy() {
        // Revert the sky tint
        SetSkyColor(Color.white);
    }

    void Update() {
        DisableInactiveLightSource();
        UpdateSunAndMoonOrbit();
        UpdateSunAndMoonAngle();
        UpdateLighting();
    }

    /**
     * Updates the positions of the sun and moon in the sky.
     */
    private void UpdateSunAndMoonOrbit() {
        // We want to rotate the sunMoonTransform precisely based on game time.
        // Since the sun and moon are both positioned relative to this
        // transform, this effectively causes them to orbit its centre.
        // The y-angle used here ensures that the sun moves from east to west.
        float rotation = GetSunMoonRotation();
        sunMoonTransform.eulerAngles = new Vector3(rotation, 90, 0);

        // Finally, we apply a tilt to give the light to give us a more
        // interesting angle. That is, the sun will not pass directly overhead,
        // but rather its orbit will lean to one side.
        sunMoonTransform.transform.Rotate(Vector3.left, lightTilt, Space.World);
    }

    /**
     * Calculates the required angle of the sunMoonTransform based on game time.
     */
    private float GetSunMoonRotation() {
        // At sunrise, rotation should be 90
        // At midday, rotation should be 0
        // At sunset, rotation should be -90
        // At midnight, rotation should be -180
        return 90 - (GetMsSinceSunrise() * SUN_ROTATION_PER_MS);
    }

    /**
     * Gets the number of milliseconds since the last sunrise.
     */
    private float GetMsSinceSunrise() {
        return time.GetCurrentTimeMs() - GameTime.SUNRISE_TIME_MS;
    }

    /**
     * Updates the angle of the the sun and moon.
     */
    private void UpdateSunAndMoonAngle() {
        // We need to rotate the actual sun / moon objects to ensure that
        // their light is pointing in the right direction
        sun.transform.LookAt(skyboxCameraTransform);
        moon.transform.LookAt(skyboxCameraTransform);

        // The moon faces the wrong way by default, so we need to flip it!
        moon.transform.Rotate(0, 180, 0);
    }

    /**
     * Disables the sun at night, and the moon during the day.
     */
    private void DisableInactiveLightSource() {
        if (IsSunPresent()) {
            moon.intensity = 0.0f;
        } else {
            sun.intensity = 0.0f;
        }
    }

    /**
     * Determines if the sun's light should be active.
     */
    private bool IsSunPresent() {
        float currentTimeMs = time.GetCurrentTimeMs();
        return currentTimeMs > GameTime.SUNRISE_TIME_MS
                && currentTimeMs < GameTime.SUNSET_TIME_MS;
    }

    /**
     * Updates the lighting based on the time of day.
     */
    private void UpdateLighting() {
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

    /**
     * Determines the progress through the Daytime phase, in the range 0-1.
     */
    private float CalculateDaytimeProgress() {
        float dayTimePassed =
                time.GetCurrentTimeMs() - GameTime.DAYTIME_START_MS;
        return dayTimePassed / GameTime.DAYTIME_LENGTH_MS;
    }

    /**
     * Determines the progress through the Night phase, in the range 0-1.
     */
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

    /**
     * Updates the lighting when in the Dawn phase.
     */
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

    /**
     * Updates the lighting when in the first half of the Dawn phase.
     */
    private void UpdateMoonSet() {
        // Calculate progress through the phase, in the range 0-1
        float t = Mathf.InverseLerp(
                GameTime.DAWN_START_MS,
                GameTime.DAWN_START_MS + GameTime.DAWN_LENGTH_MS / 2.0f,
                time.GetCurrentTimeMs());
        moon.intensity = Mathf.Lerp(MOON_INTENSITY, 0.0f, t);
        RenderSettings.ambientLight =
                Color.Lerp(AMBIENT_COLOR_NIGHT, AMBIENT_COLOR_DAWN, t);
        SetSkyColor(Color.Lerp(SKY_TINT_NIGHT, SKY_TINT_DAWN, t));
    }

    /**
     * Updates the lighting when in the second half of the Dawn phase.
     */
    private void UpdateSunrise() {
        // Calculate progress through the phase, in the range 0-1
        float t = Mathf.InverseLerp(
                GameTime.DAWN_START_MS + GameTime.DAWN_LENGTH_MS / 2.0f,
                GameTime.DAYTIME_START_MS,
                time.GetCurrentTimeMs());
        sun.intensity = Mathf.Lerp(0.0f, SUN_INTENSITY, t * 2);
        RenderSettings.ambientLight =
                Color.Lerp(AMBIENT_COLOR_DAWN, AMBIENT_COLOR_DAY, t);
        SetSkyColor(Color.Lerp(SKY_TINT_DAWN, SKY_TINT_DAY, t));
    }

    /**
     * Updates the lighting when in the Daytime phase.
     */
    private void UpdateDay() {
        sun.intensity = SUN_INTENSITY;
        RenderSettings.ambientLight = AMBIENT_COLOR_DAY;
        SetSkyColor(SKY_TINT_DAY);
    }

    /**
     * Updates the lighting when in the Dusk phase.
     */
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

    /**
     * Updates the lighting when in the first half of the Dusk phase.
     */
    private void UpdateSunset() {
        // Calculate progress through the phase, in the range 0-1
        float t = Mathf.InverseLerp(
                GameTime.DUSK_START_MS,
                GameTime.DUSK_START_MS + GameTime.DUSK_LENGTH_MS / 2.0f,
                time.GetCurrentTimeMs());
        sun.intensity = Mathf.Lerp(SUN_INTENSITY, 0.0f, t);
        RenderSettings.ambientLight =
                Color.Lerp(AMBIENT_COLOR_DAY, AMBIENT_COLOR_DUSK, t);
        SetSkyColor(Color.Lerp(SKY_TINT_DAY, SKY_TINT_DUSK, t));
    }

    /**
     * Updates the lighting when in the second half of the Dusk phase.
     */
    private void UpdateMoonRise() {
        // Calculate progress through the phase, in the range 0-1
        float t = Mathf.InverseLerp(
                GameTime.DUSK_START_MS + GameTime.DUSK_LENGTH_MS / 2.0f,
                GameTime.NIGHT_START_MS,
                time.GetCurrentTimeMs());
        moon.intensity = Mathf.Lerp(0.0f, MOON_INTENSITY, t * 2);
        RenderSettings.ambientLight =
                Color.Lerp(AMBIENT_COLOR_DUSK, AMBIENT_COLOR_NIGHT, t);
        SetSkyColor(Color.Lerp(SKY_TINT_DUSK, SKY_TINT_NIGHT, t));
    }

    /**
     * Updates the lighting when in the Night phase.
     */
    private void UpdateNight() {
        moon.intensity = MOON_INTENSITY;
        RenderSettings.ambientLight = AMBIENT_COLOR_NIGHT;
        SetSkyColor(SKY_TINT_NIGHT);
    }

    /**
     * Changes the colour of the sky.
     */
    private void SetSkyColor(Color color) {
        RenderSettings.skybox.SetColor(SKYBOX_COLOUR_PARAM, color);
    }

}
