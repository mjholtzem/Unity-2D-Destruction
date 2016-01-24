using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(Explodable))]
public abstract class ExplodableAddon : MonoBehaviour {
    protected Explodable explodable;
	// Use this for initialization
	void Start () {
        explodable = GetComponent<Explodable>();
	}

    public abstract void OnFragmentsGenerated(List<GameObject> fragments);
}
