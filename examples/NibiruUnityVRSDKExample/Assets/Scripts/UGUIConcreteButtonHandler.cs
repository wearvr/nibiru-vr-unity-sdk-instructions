using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UGUIConcreteButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, INvrButtonListener {
	private string originalText;
	private bool isGazedAt;
	private Text label;

	private void Awake () {
		label = GetComponentInChildren<Text> ();
		originalText = label.text;
	}

	private void SetTextIfGazedAt(string text) {
		if (isGazedAt) {
			label.text = text;
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
		SetTextIfGazedAt("Pressed Enter");
	}

	public void OnPressLeft() {
		SetTextIfGazedAt("Pressed Left");
	}

	public void OnPressRight() {
		SetTextIfGazedAt("Pressed Right");
	}

	public void OnPressUp() {
		SetTextIfGazedAt("Pressed Up");
	}

	public void OnPressDown() {
		SetTextIfGazedAt("Pressed Down");
	}

	public void OnPressBack() {
		SetTextIfGazedAt("Pressed Back");
	}

	public void OnPressVolumnUp() {
		SetTextIfGazedAt("Pressed Volume+");
	}

	public void OnPressVolumnDown() {
		SetTextIfGazedAt("Pressed Volume-");
	}
}