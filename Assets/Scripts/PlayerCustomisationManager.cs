using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomisationManager : MonoBehaviour {
	
	private GameObject playerUI;
	private PlayerCharacter playerCharacter;
	[SerializeField] private Transform playerHeadTransform;
	private GameObject hats;
	private Transform currentHat = null;
	
	// Use this for initialization
	void Start () {
		playerUI = GameObject.FindWithTag("PlayerUI");
		playerCharacter = gameObject.GetComponent<PlayerCharacter>();
		hats = GameObject.FindWithTag("Hats");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.H))
		{
			if (currentHat != null) 
				RemoveHat();
			print(GetAvailableHats()[1].name); 
			currentHat = GetAvailableHats()[1]; // First hat
			AddHat();
		}
			
	}

	Transform[] GetAvailableHats()
	{
		// IMPORTANT NOTE: This array also includes the Hats gameobject itself at [0] and any subchildren and subsubchildren, etc.
		return hats.GetComponentsInChildren<Transform>();
	}
	
	// Jamie's mini networking code hellhole

	void AddHat()
	{
		currentHat.SetParent(playerHeadTransform, false);
		//playerCharacter.CmdSetTransformParent(currentHat, playerHeadTransform, false);
	}
	
	
	void RemoveHat()
	{
		currentHat.SetParent(hats.transform, false);
		//playerCharacter.CmdSetTransformParent(currentHat, hats.transform, false);
	}

}
