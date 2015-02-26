/**
 * MMD4MFaceBlink
 * author : udasan
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MMD4MFaceBlink : MonoBehaviour
{
	[System.Serializable]
	public class BlinkParam {
		public string morphName;
		public const float minimumWeight = 0.0f;
		public const float maximumWeight = 1.0f;
	}
	public BlinkParam[] blinkParams;

	public float closingTime = 0.05f;
	public float closedTime = 0.1f;
	public float openingTime = 0.05f;

	public float minimumInterval = 0.5f;
	public float maximumInterval = 8.0f;

	public string[] preferredMorphs;

	float nextTime_;

	enum BlinkState {
		Opened,
		Closing,
		Closed,
		Opening
	}
	BlinkState blinkState_ = BlinkState.Opened;

	public bool isBlinking {
		get { return blinkState_ != BlinkState.Opened; }
	}

	MMD4MecanimModel model_;
	Dictionary<string, MMD4MecanimModel.Morph> preferredModelMorphDict_ = new Dictionary<string, MMD4MecanimModel.Morph>();
	bool isPreferdMorphEnabledCache_;

	Dictionary<string, MMD4MecanimMorphHelper> morphHelperDict_ = new Dictionary<string, MMD4MecanimMorphHelper>();
	public MMD4MecanimMorphHelper GetMorphHelper (string morphName)
	{
		MMD4MecanimMorphHelper morphHelper;
		morphHelperDict_.TryGetValue(morphName, out morphHelper);
		return morphHelper;
	}

	float GetNextInterval ()
	{
		return Random.Range(minimumInterval, maximumInterval);
	}

	void Awake ()
	{
		model_ = this.GetComponentInChildren<MMD4MecanimModel>();
		if (!model_) { return; }

		var originalMorphHelperDict = new Dictionary<string, MMD4MecanimMorphHelper>();
		foreach (var morphHelper in model_.GetComponents<MMD4MecanimMorphHelper>()) {
			originalMorphHelperDict.Add(morphHelper.morphName, morphHelper);
		}

		foreach (var blinkParam in blinkParams) {
			if (morphHelperDict_.ContainsKey(blinkParam.morphName)) { continue; }
			MMD4MecanimMorphHelper morphHelper;
			originalMorphHelperDict.TryGetValue(blinkParam.morphName, out morphHelper);
			if (!morphHelper) {
				morphHelper = model_.gameObject.AddComponent<MMD4MecanimMorphHelper>();
				morphHelper.morphName = blinkParam.morphName;
			}
			morphHelperDict_.Add(blinkParam.morphName, morphHelper);
		}
	}

	void Start ()
	{
		ResetBlinking();
		nextTime_ = Time.time + GetNextInterval();
	}

	void Update ()
	{
		UpdatePreferredMorphs();
		bool isPreferredMorphEnabled = IsPreferredMorphEnabled();
		if (isPreferredMorphEnabled) {
			if (isPreferredMorphEnabled != isPreferdMorphEnabledCache_) {
				ResetBlinking();
			}
			isPreferdMorphEnabledCache_ = isPreferredMorphEnabled;
			return;
		} else {
			if (isPreferredMorphEnabled != isPreferdMorphEnabledCache_) {
				nextTime_ = Time.time + GetNextInterval();
			}
			isPreferdMorphEnabledCache_ = isPreferredMorphEnabled;
		}

		switch (blinkState_) {
		case BlinkState.Opened : OnOpened(); break;
		case BlinkState.Closing : OnClosing(); break;
		case BlinkState.Closed : OnClosed(); break;
		case BlinkState.Opening : OnOpening(); break;
		default : break;
		}
	}

	void OnEnable ()
	{
		nextTime_ = Time.time + GetNextInterval();
	}

	void OnDisable ()
	{
		ResetBlinking();
	}

	void UpdatePreferredMorphs ()
	{
		foreach (string morphName in preferredMorphs) {
			if (preferredModelMorphDict_.ContainsKey(morphName)) { continue; } // already included

			MMD4MecanimModel.Morph modelMorph = model_.GetMorph(morphName);
			if (modelMorph == null) { continue; } // not found

			preferredModelMorphDict_.Add(morphName, modelMorph);
		}
	}

	bool IsPreferredMorphEnabled ()
	{
		foreach (var modelMorph in preferredModelMorphDict_.Values) {
			if (modelMorph == null) { continue; }
			if (modelMorph.weight != 0.0f) { return true; }
		}
		return false;
	}

	void OnOpened ()
	{
		if (Time.time < nextTime_) { return; }

		foreach (var blinkParam in blinkParams) {
			MMD4MecanimMorphHelper morphHelper = morphHelperDict_[blinkParam.morphName];
			morphHelper.morphSpeed = closingTime;
			morphHelper.morphWeight = BlinkParam.maximumWeight;
		}

		nextTime_ = Time.time + closingTime;
		blinkState_ = BlinkState.Closing;
	}

	void OnClosing ()
	{
		if (Time.time < nextTime_) { return; }

		foreach (var morphHelper in morphHelperDict_.Values) {
			if (morphHelper.isProcessing) { return; }
		}

		nextTime_ += closedTime;
		blinkState_ = BlinkState.Closed;
	}

	void OnClosed ()
	{
		if (Time.time < nextTime_) { return; }

		foreach (var blinkParam in blinkParams) {
			MMD4MecanimMorphHelper morphHelper = morphHelperDict_[blinkParam.morphName];
			morphHelper.morphSpeed = openingTime;
			morphHelper.morphWeight = BlinkParam.minimumWeight;
		}
		
		nextTime_ = Time.time + openingTime;
		blinkState_ = BlinkState.Opening;
	}

	void OnOpening ()
	{
		if (Time.time < nextTime_) { return; }

		foreach (var morphHelper in morphHelperDict_.Values) {
			if (morphHelper.isProcessing) { return; }
		}
		
		nextTime_ = Time.time + GetNextInterval();
		blinkState_ = BlinkState.Opened;
	}

	void ResetBlinking ()
	{
		foreach (var morphHelper in morphHelperDict_.Values) {
			morphHelper.morphSpeed = 0.0f;
			morphHelper.morphWeight = BlinkParam.minimumWeight;
		}

		blinkState_ = BlinkState.Opened;
	}
}
