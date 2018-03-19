using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
	public class SpawnRandomObject : MonoBehaviour {

        public GameObject[] obj;
        public int amount;

        void Start ()
		{
            for (int i = 0; i < amount; i++)
            {
                Vector3 position = new Vector3(Random.Range(-20f, 20f), Random.Range(1f, 10f), Random.Range(-20f, 20f));
                Instantiate(obj[Random.Range(0, obj.Length)], position, Quaternion.identity);
            }
        }
	}
}
