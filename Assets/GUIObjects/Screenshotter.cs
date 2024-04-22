using System.Collections;
using UnityEngine;

public class Screenshotter : MonoBehaviour
{
    public GameObject canvas;

    // referenced from UI
    public void Screenshot() {
        // Disables the UI and takes a screenshot 
        StartCoroutine(TakeScreenshotAndSave());
    }

    private IEnumerator TakeScreenshotAndSave() {
        canvas.SetActive(false);
        yield return new WaitForEndOfFrame();
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();
            string base64 = System.Convert.ToBase64String(bytes);

            string jsCode = "var link = document.createElement('a'); " +
                            "link.href = 'data:image/png;base64," + base64 + "'; " +
                            "link.download = 'CCG_Screenshot.png'; " +
                            "link.click();";
            Application.ExternalEval(jsCode);
            Destroy(texture);
        #else
            // Code for Unity Editor and standalone builds
            ScreenCapture.CaptureScreenshot("CCG_Screenshot.png", 2);
        #endif

        yield return null;
        canvas.SetActive(true);
    }
}
