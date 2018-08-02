using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MeshRenderer))]
public class GameObjectConcreteButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, INvrButtonListener {
	private Color originalColor;
	private bool isGazedAt;
	private MeshRenderer meshRenderer;

	private void Awake() {
		meshRenderer = GetComponent<MeshRenderer> ();
		originalColor = meshRenderer.material.color;
	}

	private void SetColorIfGazedAt(Color color) {
		if (isGazedAt) {
			meshRenderer.material.color = color;	
		}
	}

	/* Pointer Events */

	public void OnPointerEnter(PointerEventData eventData) {
		isGazedAt = true;
	}

	public void OnPointerExit(PointerEventData eventData) {
		isGazedAt = false;
	}

	/* Nibiru Button Events */

	public void OnPressEnter(bool isKeyUp) {
		SetColorIfGazedAt(Color.grey);
	}

	public void OnPressLeft() {
		SetColorIfGazedAt(Color.red);
	}

	public void OnPressRight() {
		SetColorIfGazedAt(Color.magenta);
	}

	public void OnPressUp() {
		SetColorIfGazedAt(Color.yellow);
	}

	public void OnPressDown() {
		SetColorIfGazedAt(Color.blue);
	}

	public void OnPressBack() {
		Application.Quit();
	}

	public void OnPressVolumnUp() {
		SetColorIfGazedAt(Color.black);
	}

	public void OnPressVolumnDown() {
		SetColorIfGazedAt(Color.white);
	}
}