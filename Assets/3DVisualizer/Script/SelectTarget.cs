using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
	public class SelectTarget : MonoBehaviour {

        public void onSelectTarget()
        {
            Debug.Log(this.transform.name + " is selected. ");
        }
	}
}
