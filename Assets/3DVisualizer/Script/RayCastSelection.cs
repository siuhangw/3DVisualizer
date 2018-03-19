using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace S3
{
	public class RayCastSelection : MonoBehaviour {

        public Transform target;

        void Update ()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(ray, out hit, 100))
                {
                    target = hit.transform; ;
                    //Debug.Log("Target: " + target.name);
                }
            }

            //Debug.DrawRay(transform.position, transform.forward, Color.cyan, 0.1f);
        }

        public Transform GetTarget()
        {
            Transform t = target;

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(ray, out hit, 20))
                {
                    target = hit.transform; ;
                    Debug.Log("Target: " + target.name);
                }
            }

            return t;
        }
    }
}
