using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour 
{
	public GameObject explosion;		// Prefab of explosion effect.


	void Start () 
	{
		// Destroy the rocket after 2 seconds if it doesn't get destroyed before then.
		Destroy(gameObject, 2);
	}


	void OnExplode()
	{
		// Create a quaternion with a random rotation in the z-axis.
		Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

		// Instantiate the explosion where the rocket is with the random rotation.
		Instantiate(explosion, transform.position, randomRotation);
	}
	
	void OnTriggerEnter2D (Collider2D col) 
	{
		// Otherwise if the player manages to shoot himself...
		if(col.gameObject.tag != "Player")
		{
            //calls explode on all Explodable gameobjects using sendMessage
			Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position,3);
			foreach(Collider2D coll in colliders){
				if(coll.GetComponent<Rigidbody2D>()&&coll.name!="hero"){
					coll.gameObject.SendMessage("explode",SendMessageOptions.DontRequireReceiver);
				}
			}
            //create an explosion force at teh location of the rocket
			GameObject.Find("ExplosionForce").GetComponent<ExplosionForce>().doExplosion(transform.position);

			// Instantiate the explosion effect and destroy the rocket.
			OnExplode();
			Destroy (gameObject);
		}
	}
}
