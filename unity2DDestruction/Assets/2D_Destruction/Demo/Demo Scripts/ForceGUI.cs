using UnityEngine;
using System.Collections;

public class ForceGUI : MonoBehaviour {
	private bool forceOn = true;
	private ExplosionForce force;
    private string btnText = "Turn Explosion Force Off";
	// Use this for initialization
	void Start () {
		force = GameObject.Find("ExplosionForce").GetComponent<ExplosionForce>();
	}
	void OnGUI(){
		if(GUI.Button(new Rect(10,10,200,50),btnText)){
			if(forceOn){
				forceOn = false;
				btnText = "Turn Explosion Force On";
				force.force = 0;
			}
			else{
				forceOn = true;
				btnText = "Turn Explosion Force Off";
				force.force = 1000;
			}
		}
	}
}
