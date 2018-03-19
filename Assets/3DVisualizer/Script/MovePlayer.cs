using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
	public class MovePlayer : MonoBehaviour {

        private RayCastSelection t;
        private Camera c;
        private bool onMove = false;
        public float speed;
        public float safeDist;

        private void Start()
        {
            t = GameObject.Find("Main Camera").GetComponent<RayCastSelection>();
            c = GetComponentInChildren<Camera>();
        }

        void Update ()
		{
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
#elif UNITY_ANDROID
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
            {
                Debug.Log(t.GetTarget());
                onMove = true;
            }
            if (onMove)
            {
                transform.position = Vector3.MoveTowards(transform.position, t.target.position - new Vector3(0, 0, safeDist), Time.deltaTime * speed);
                //transform.LookAt(t.target.localPosition);
                //c.transform.LookAt(t.target.localPosition);
            }
        }
	}
}
