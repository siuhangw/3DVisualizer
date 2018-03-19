using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

namespace S3
{
	public class SwitchMode : MonoBehaviour {
        
        private IEnumerator coroutine;

        //public void Update()
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        TogglVR();
        //    }
        //}

        public void TogglVR()
        {
            if (VRSettings.loadedDeviceName == "cardboard")
            {
                StartCoroutine(SwitchToVR(""));
                Debug.Log("Toggel Normal");
            }
            else
            {
                StartCoroutine(SwitchToVR("cardboard"));
                Debug.Log("Toggel VR");
            }
        }
        
        IEnumerator SwitchToVR(string desiredDevice)
        {
            VRSettings.LoadDeviceByName(desiredDevice);
            
            yield return null;
            
            VRSettings.enabled = true;
        }
    }
}
