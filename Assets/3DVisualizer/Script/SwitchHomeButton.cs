using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
	public class SwitchHomeButton : MonoBehaviour {

        public GameObject homeButton;
        private bool isEnable;

        public void SwitchButton()
        {
            if (!isEnable)
            {
                homeButton.SetActive(true);
                isEnable = true;
            }
            else
            {
                homeButton.SetActive(false);
                isEnable = false;
            }
        }
	}
}
