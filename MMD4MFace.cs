/**
 * MMD4MFace
 * author : udasan
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MMD4MFace : MonoBehaviour
{
	public string faceName;

	[System.Serializable]
	public class MorphParam {
		public string morphName;
		public float morphSpeed;
		public float morphWeight;
		//public bool overrideWeight; // true
	}
	public MorphParam[] morphParams;

	MMD4MFaceController faceController_;
	public MMD4MFaceController faceController {
		get { return faceController_ ? faceController_ : (faceController_ = this.GetComponent<MMD4MFaceController>()); }
	}

	Dictionary<string, MorphParam> morphParamDict_ = new Dictionary<string, MorphParam>();
	public MorphParam GetMorphParam (string morphName) {
		if (morphParamDict_.Count == 0 && morphParams.Length != 0) {
			foreach (var m in morphParams) {
				morphParamDict_.Add(m.morphName, m);
			}
		}

		MorphParam morphParam;
		morphParamDict_.TryGetValue(morphName, out morphParam);
		return morphParam;
	}

	void Update ()
	{
		// to show checkbox on inspector
	}

	void OnDisable ()
	{
		if (faceController.currentFace == this) {
			faceController.ResetFace();
		}
	}
}
