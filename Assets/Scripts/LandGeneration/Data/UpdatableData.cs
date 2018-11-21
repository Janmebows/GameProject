using UnityEngine;
using System.Collections;

public class UpdatableData : ScriptableObject
{

    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

#if UNITY_EDITOR 
    //this is only needed if in the unity editor, doesn't compile otherwise
    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            UnityEditor.EditorApplication.update+=NotifyOfUpdatedValues;
        }
    }

    public void NotifyOfUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }
#endif
}