using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace S3
{
	public class CameraContolScript : MonoBehaviour
    {
        // Camera Objects
        private Transform _XForm_Camera;
        private Transform _XForm_Parent;

        // Camera Parameters
        private Vector3 _LocalRotation;
        private float _CameraDistance = -10f;

        // Mouse States
        private Vector3 mouseOrigin;
        private bool isOrbitting = false;
        private bool isZooming = false;
        private bool isPanning = false;

        // Touch State
        //private bool touchMoving = false;
        private int TapCount = 0;

        // Adjustment Parameters
        public float MouseSensitivity = 2f;
        public float ScrollSensitivity = 2f;
        public float OrbitSpeed = 10f;
        public float ScrollSpeed = 10;

        // Selection
        public GameObject selectedObject;
        
        private void Start()
        {
            _XForm_Camera = this.transform;
            _XForm_Parent = this.transform.parent;
        }

        private void Update()
        {
            //#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_WEBGL
            //if (!EventSystem.current.IsPointerOverGameObject())
            //{
            //    Debug.Log("No UI Detected.");
            //    // Keyboard Mouse Input Method
            //    if (Input.GetKeyDown(KeyCode.H))
            //    {
            //        print("Reset Camera");
            //        this._XForm_Parent.transform.localPosition = Vector3.zero;
            //        this._XForm_Parent.transform.localRotation = Quaternion.identity;
            //        this._XForm_Camera.transform.localPosition = new Vector3(0f, 0f, -10f);
            //    }

            //    // Mouse Input Check --- Yes
            //    if (Input.GetMouseButton(0))
            //    {
            //        _LocalRotation.x += Input.GetAxis("Mouse X") * MouseSensitivity;
            //        _LocalRotation.y -= Input.GetAxis("Mouse Y") * MouseSensitivity;
            //        _LocalRotation.y = Mathf.Clamp(_LocalRotation.y, -90f, 90f);
            //        isOrbitting = true;
            //    }

            //    if (Input.GetAxis("Mouse ScrollWheel") != 0)
            //    {
            //        float ScrollAmount = Input.GetAxis("Mouse ScrollWheel") * ScrollSensitivity;
            //        ScrollAmount *= this._CameraDistance * 0.3f;
            //        this._CameraDistance += ScrollAmount * -1f;
            //        isZooming = true;
            //    }

            //    if (Input.GetMouseButtonDown(2))
            //    {
            //        mouseOrigin = Input.mousePosition;
            //        isPanning = true;
            //    }

            //    // Mouse Input Check --- No
            //    if (/*!Input.GetMouseButton(0) && */Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0) isOrbitting = false;
            //    if (Input.GetAxis("Mouse ScrollWheel") == 0) isZooming = false;
            //    if (!Input.GetMouseButton(2)) isPanning = false;
            //}

//#elif UNITY_IOS || UNITY_ANDROID

            if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Ended && touch.tapCount == 1)
                    {
                        //Debug.Log("*");
                    }
                    if (touch.phase == TouchPhase.Ended && touch.tapCount == 2)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, 100))
                        {
                            Debug.DrawLine(ray.origin, hit.point);
                            GameObject hitObject = hit.transform.gameObject;
                            SelectObject(hitObject);
                        }
                        else
                        {
                            ClearSelectObject();
                        }
                        //Debug.Log("**");
                    }
                    if (touch.phase == TouchPhase.Ended && touch.tapCount == 3)
                    {
                        ResetCamera();
                        //Debug.Log("***");
                    }
                }
            }

            // Double touches for Panning & Zooming
            if (Input.touchCount == 2)
            {
                // Store both touches.
                Touch touchA = Input.GetTouch(0);
                Touch touchB = Input.GetTouch(1);

                // Find the position in the previous frame of each touch.
                Vector2 touchAPrevPos = touchA.position - touchA.deltaPosition;
                Vector2 touchBPrevPos = touchB.position - touchB.deltaPosition;
                Vector2 touchAB = (touchBPrevPos + touchAPrevPos)*0.5f;
                //Debug.Log("touchA: " + touchA.position);
                //Debug.Log("touchB: " + touchB.position);
                //Debug.Log("touchAB: " + touchAB);

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchAPrevPos - touchBPrevPos).magnitude;
                float touchDeltaMag = (touchA.position - touchB.position).magnitude;

                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                
                if(touchA.phase == TouchPhase.Moved && touchB.phase == TouchPhase.Moved && !EventSystem.current.IsPointerOverGameObject(touchA.fingerId))
                {
                    _CameraDistance -= deltaMagnitudeDiff * 0.01f;
                    mouseOrigin = touchAB;

                    isZooming = true;
                    isPanning = true;
                }
                else if (touchA.phase == TouchPhase.Ended || touchB.phase == TouchPhase.Ended)
                {
                    isZooming = false;
                    isPanning = false;
                }
            }

            // Single touch for Orbitting
            if (Input.touchCount == 1)
            {
                //Touch touchZero = Input.GetTouch(0);
                Touch touchZero = Input.touches[0];

                if (touchZero.phase == TouchPhase.Moved && !EventSystem.current.IsPointerOverGameObject(touchZero.fingerId))
                {
                    _LocalRotation.x += touchZero.deltaPosition.x * 0.5f;
                    _LocalRotation.y -= touchZero.deltaPosition.y * 0.5f;
                    _LocalRotation.y = Mathf.Clamp(_LocalRotation.y, -90f, 90f);

                    isOrbitting = true;
                }
                else if (touchZero.phase == TouchPhase.Ended)
                {
                    isOrbitting = false;
                }
            }

            // Single touch for Panning
            //if (Input.touchCount == 1)
            //{
            //    //Touch touchZero = Input.GetTouch(0);
            //    Touch touchZero = Input.touches[0];

            //    if (touchZero.phase == TouchPhase.Moved && !EventSystem.current.IsPointerOverGameObject(touchZero.fingerId))
            //    {
            //        mouseOrigin = Input.GetTouch(0).deltaPosition;

            //        isPanning = true;
            //    }
            //    else if (touchZero.phase == TouchPhase.Ended)
            //    {
            //        isPanning = false;
            //    }
            //}
//#endif
        }

        private void LateUpdate()
        {
            // Orbiting
            if (isOrbitting)
            {
                Quaternion QT = Quaternion.Euler(_LocalRotation.y, _LocalRotation.x, 0);
                this._XForm_Parent.localRotation = Quaternion.Lerp(this._XForm_Parent.rotation, QT, Time.deltaTime * OrbitSpeed);
            }

            // Panning
            if (isPanning)
            {
                //Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);
                Vector3 pos = Camera.main.ScreenToViewportPoint((Vector3)Input.GetTouch(0).deltaPosition);
                Vector3 move = new Vector3(pos.x * -1f, pos.y * -1f, 0);
                this._XForm_Parent.transform.Translate(move, Space.Self);
            }

            // Zooming
            if (isZooming)
            {
                this._XForm_Camera.localPosition = new Vector3(0f, 0f, Mathf.Lerp(this._XForm_Camera.localPosition.z, this._CameraDistance, Time.deltaTime * ScrollSpeed));
            }
        }

        public void ResetCamera()
        {
            //if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            //{
            //    _XForm_Parent.transform.localPosition = new Vector3(0f, 0f, 0f);
            //    _XForm_Parent.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            //    _XForm_Camera.transform.localPosition = new Vector3(0f, 0f, -10f);
            //    Debug.Log("Camera Reset");
            //}

            _XForm_Parent.transform.localPosition = new Vector3(0f, 0f, 0f);
            _XForm_Parent.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            _XForm_Camera.transform.localPosition = new Vector3(0f, 0f, -10f);
            Debug.Log("Camera Reset");
        }

        private void SelectObject(GameObject obj)
        {
            if(selectedObject != null)
            {
                if (obj == selectedObject)
                    return;

                ClearSelectObject();
            }
            selectedObject = obj;
            Renderer r = selectedObject.GetComponent<Renderer>();
            Material m = r.material;
            //m.color = Color.red;
            m.color = new Color(1f,0.5f,0.5f);
            print(obj + " is selected.");
        }

        private void ClearSelectObject()
        {
            if (selectedObject == null)
                return;

            Renderer r = selectedObject.GetComponent<Renderer>();
            Material m = r.material;
            m.color = Color.white;
            print("Selection clear.");
        }

        void OnGUI()
        {
            GUI.Button(new Rect(10, 10, 150, 100), selectedObject.name);
        }
    }
}