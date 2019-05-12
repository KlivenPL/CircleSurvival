using UnityEngine;

public static class Utils {

    public static Bounds GetCameraBounds(Camera camera) {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = camera.orthographicSize * 2;
        Bounds bounds = new Bounds(camera.transform.position, new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
        return bounds;
    }

    public static Vector3 DefaultAccelometerPos { get; set; }

    public static void UpdateAccelometerDefPos() {
        DefaultAccelometerPos = Input.acceleration;
    }

    public static string NumberDecimalSeparator {
        get {
            return System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        }
    }
    public static float GetHighscore() {
        return PlayerPrefs.GetFloat("Highscore", 0f);
    }

    public static void SetHighscore(float newScore) {
        PlayerPrefs.SetFloat("Highscore", newScore);
    }

    public static float GetThemedCircles() {
        return PlayerPrefs.GetInt("ThemedCircles", 1);
    }

    public static void SetThemedCircles(bool themed) {
        PlayerPrefs.SetInt("ThemedCircles", themed ? 1 : 0);
    }
}
