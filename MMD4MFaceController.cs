/**
 * MMD4MFaceController
 * author : udasan
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MMD4MFaceController : MonoBehaviour
{
	public string defaultFaceName;

	public float defaultMorphSpeed = 0.1f;
	public bool defaultOverrideWeight = true;

	const float defaultMorphWeight = 0.0f;

	MMD4MFace currentFace_;
	public MMD4MFace currentFace {
		get {
			return currentFace_;
		}
		private set {
			currentFace_ = value;
		}
	}

	public string currentFaceName {
		get {
			return currentFace ? currentFace.faceName : "";
		}
		private set {
			currentFace = this[value];
		}
	}

	Dictionary<string, MMD4MFace> faceDict_ = new Dictionary<string, MMD4MFace>();
	public MMD4MFace this[string faceName] {
		get {
			MMD4MFace face;
			faceDict_.TryGetValue(faceName, out face);
			return face;
		}
	}
	
	Dictionary<string, MMD4MecanimMorphHelper> morphHelperDict_ = new Dictionary<string, MMD4MecanimMorphHelper>();
	public MMD4MecanimMorphHelper GetMorphHelper (string morphName)
	{
		MMD4MecanimMorphHelper morphHelper;
		morphHelperDict_.TryGetValue(morphName, out morphHelper);
		return morphHelper;
	}

	public bool IsFace (MMD4MFace face)
	{
		return (face && (face == currentFace));
	}

	public bool IsFace (string faceName)
	{
		return IsFace(this[faceName]);
	}

	public bool HasFace (MMD4MFace face)
	{
		return face ? HasFace(face.faceName) : false;
	}

	public bool HasFace (string faceName)
	{
		return faceDict_.ContainsKey(faceName);
	}

	public bool isProcessing {
		get {
			foreach (var morphHelper in morphHelperDict_.Values) {
				if (morphHelper.isProcessing) { return true; }
			}
			return false;
		}
	}

	public bool isAnimating {
		get {
			foreach (var morphHelper in morphHelperDict_.Values) {
				if (morphHelper.isAnimating) { return true; }
			}
			return false;
		}
	}

	void Awake ()
	{
		var model = this.GetComponentInChildren<MMD4MecanimModel>();
		if (!model) { return; }

		var originalMorphHelperDict = new Dictionary<string, MMD4MecanimMorphHelper>();
		foreach (var morphHelper in model.GetComponents<MMD4MecanimMorphHelper>()) {
			originalMorphHelperDict.Add(morphHelper.morphName, morphHelper);
		}

		foreach (var face in this.GetComponents<MMD4MFace>()) {
			faceDict_.Add(face.faceName, face);
			foreach (var morphParam in face.morphParams) {
				if (morphHelperDict_.ContainsKey(morphParam.morphName)) { continue; }
				MMD4MecanimMorphHelper morphHelper;
				originalMorphHelperDict.TryGetValue(morphParam.morphName, out morphHelper);
				if (!morphHelper) {
					morphHelper = model.gameObject.AddComponent<MMD4MecanimMorphHelper>();
					morphHelper.morphName = morphParam.morphName;
				}
				morphHelperDict_.Add(morphParam.morphName, morphHelper);
			}
		}
	}

	void Start ()
	{
		SetFace(defaultFaceName);
	}

	void Update ()
	{
		// to show checkbox on inspector
	}

	void OnDisable ()
	{
		ResetFace();
	}

	public void SetFace (MMD4MFace face)
	{
		if (!this.enabled) { return; }
		if (!face || !face.enabled) { ResetFace (); return; }

		foreach (var morphHelper in morphHelperDict_.Values) {
			var morphParam = face.GetMorphParam(morphHelper.morphName);
			if (morphParam != null) {
				morphHelper.morphSpeed = morphParam.morphSpeed;
				morphHelper.morphWeight = morphParam.morphWeight;
			} else {
				morphHelper.morphSpeed = defaultMorphSpeed;
				morphHelper.morphWeight = defaultMorphWeight;
			}
			morphHelper.overrideWeight = defaultOverrideWeight;
		}

		currentFace = face;
	}

	public void SetFace (string faceName)
	{
		SetFace (this[faceName]);
	}

	public void ResetFace ()
	{
		foreach (var morphHelper in morphHelperDict_.Values) {
			morphHelper.morphSpeed = defaultMorphSpeed;
			morphHelper.morphWeight = defaultMorphWeight;
			morphHelper.overrideWeight = defaultOverrideWeight;
		}

		currentFace = null;
	}
}
