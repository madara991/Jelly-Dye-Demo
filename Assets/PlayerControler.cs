using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerControler : MonoBehaviour
{
	public float offSetZ;
	public Transform needleTip;

	public Color colorEquiped;
	public GameObject fluidNeedle;
	public float speedFluidLevel;
	public bool isReloading;

	[HideInInspector]
	public Vector3 hitPositionAsScreenPoint;
	
	void Update()
    {
		ReloadNeedleAndColor();
		if (Input.GetMouseButton(0))
        {
			Vector3 dirToJelly = (needleTip.transform.position - Camera.main.transform.position).normalized;

			RaycastHit hit;
			if (Physics.Raycast(needleTip.transform.position,dirToJelly, out hit, 100f))
			{
				hitPositionAsScreenPoint = Camera.main.WorldToScreenPoint(hit.point);
				hitPositionAsScreenPoint.z = 0f;
				if (hit.collider.CompareTag("jelly"))
				{
					if (isReloading)
						return;
					Debug.Log("hit jelly");
					// injection texture rederer;
					FuildNeedleLevel();

				}
				return;
			}
		}
		
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = offSetZ;
		Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
		transform.position = worldPos;
	}

	float t = 1f;
	void FuildNeedleLevel()
	{
		float scaleY = fluidNeedle.transform.localScale.y;
		if (scaleY > 0)
		{
			t -= speedFluidLevel * Time.deltaTime;
			scaleY = Mathf.Lerp(0f, 1f, t);
			fluidNeedle.transform.localScale = new Vector3(fluidNeedle.transform.localScale.x, scaleY, fluidNeedle.transform.localScale.z);
		}
		else
			isReloading = true;
	}

	public void ReloadNeedleAndColor() // by scale object
	{
		if (!isReloading)
			return;
		Debug.Log("Reloading...");

		float scaleY = fluidNeedle.transform.localScale.y;
		if (scaleY == 1f) // for reload agine from start point scale 
			t = 0;

		t += speedFluidLevel * Time.deltaTime;
		scaleY = Mathf.Lerp(0f, 1f, t);
		fluidNeedle.transform.localScale = new Vector3(fluidNeedle.transform.localScale.x, scaleY, fluidNeedle.transform.localScale.z);

		fluidNeedle.transform.GetComponentInChildren<MeshRenderer>().material.color = colorEquiped;
		
		if (scaleY < 1f)
			return;
		GameState.rationComplete++;
		Debug.Log(GameState.rationComplete);
		isReloading = false;
	}
}
