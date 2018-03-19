using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
	public class Loader : MonoBehaviour {

        public GameObject gameManager;
        
		void Awake ()
		{
            if (GameManager.instance == null)
                Instantiate(gameManager);
		}
	}
}
