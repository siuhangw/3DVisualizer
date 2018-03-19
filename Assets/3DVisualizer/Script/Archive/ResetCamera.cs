using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace S3
{
	public class ResetCamera : MonoBehaviour, IPointerClickHandler
    {
        public GameObject gameCamera;

        private Transform _XForm_Camera;
        private Transform _XForm_Parent;

        private void Start()
        {
            _XForm_Camera = gameCamera.transform;
            _XForm_Parent = gameCamera.transform.parent;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Use this to tell when the user left-clicks on the Button
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _XForm_Parent.transform.localPosition = Vector3.zero;
                _XForm_Parent.transform.localRotation = Quaternion.identity;
                _XForm_Camera.transform.localPosition = new Vector3(0f, 0f, -10f);
                Debug.Log("Camera Reset");
            }
        }
    }
}
