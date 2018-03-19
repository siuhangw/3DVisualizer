using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
	public class RotationContraintToCamera : MonoBehaviour {

        public GameObject mainCamera;

		void Update ()
		{
            transform.rotation = Quaternion.Euler(0, mainCamera.transform.rotation.y, 0);
		}
	}
}
