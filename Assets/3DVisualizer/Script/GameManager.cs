using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
	public class GameManager : MonoBehaviour
    {
        public static GameManager instance = null;
        private string shotFileName;
        private int screenShotCount;
        private bool shotTaken = false;

        public object ScreenCapture { get; private set; }

        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != null)
                Destroy(gameObject);

            DontDestroyOnLoad(transform.gameObject);
        }

        public void Quit()
        {
            Application.Quit();
            Debug.Log("Quit");
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                screenShotCount++;
                shotFileName = "Screenshot__" + screenShotCount + System.DateTime.Now.ToString("__yyyy-MM-dd") + ".png";
                Application.CaptureScreenshot(shotFileName);
                shotTaken = true;
                Debug.Log("Screen Captured!");
            }

            if (shotTaken == true)
            {
                string pathA = "D:/Projects/2017/02.GameExperiment/GE007.3DVisualizer/3DVisualizer/" + shotFileName;
                string pathB = "D:/Projects/2017/02.GameExperiment/GE007.3DVisualizer/3DVisualizer/tmp/" + shotFileName;
                Debug.Log("pathA" + pathA);
                Debug.Log("pathB" + pathB);
                if (System.IO.File.Exists(pathA))
                {
                    System.IO.File.Move(pathA, pathB);
                    shotTaken = false;
                }
            }
        }
    }
}
