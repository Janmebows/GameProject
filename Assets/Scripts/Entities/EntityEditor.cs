using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//script to allow the editing of entities without too much interaction within the model/prefab
[CustomEditor(typeof(BaseEntity))]
public class EntityEditor : Editor
{
    BaseEntity thisTarget;
    bool adding = false;


    //Entity Info
    List<string> colliderNames;
    Collider[] colliders;
    //Appendage Info
    string appendageName = "";
    int appendageColliderIndex = 0;
    AppendageData.AppendageType appType = AppendageData.AppendageType.Other;

    public override void OnInspectorGUI()
    {
        //necessary to make shit work
        thisTarget = (BaseEntity)target;

        //Stuff from the target itself
        thisTarget.humanoid = EditorGUILayout.Toggle("Humanoid", thisTarget.humanoid);

        //appendage information

        ObtainAppendageColliders();
        DisplayAppendages();

        //ADD APPENDAGE
        AddAppendageButton();


    }















    //APPENDAGES
    void AddAppendageButton()
    {

        
        if (GUILayout.Button("Add a new appendage") || adding)
        {
            GUILayout.Label("Make sure the colliders are ready!");
            adding = true;


            //NAME
            GUILayout.BeginHorizontal(GUIStyle.none);
            GUILayout.Label("Appendage Name:");
            appendageName = GUILayout.TextField(appendageName);
            GUILayout.EndHorizontal();
            
            //TYPE
            appType = (AppendageData.AppendageType)EditorGUILayout.EnumPopup("Type of Appendage", appType);

            //COLLIDER
            appendageColliderIndex = EditorGUILayout.Popup(appendageColliderIndex, colliderNames.ToArray());
            


            //FINISH BUTTON
            if (GUILayout.Button("Finish appendage") && appendageName.Length > 0)
            {
                DealWithAppendage();
                adding = false;
            }
        }



    }

    //Displays appendage information
    void DisplayAppendages()
    {
        foreach (Appendage appendage in thisTarget.appendages)
        {
            //if its dodgy
            if (appendage == null || appendage.baseAppendageData == null)
            {

            }
            else
            {
                GUILayout.Label(appendage.baseAppendageData.limbName);
            }
        }
    }
 
    //fills in the appendage info given inputs
    void DealWithAppendage()
    {
        Appendage newAppendage = thisTarget.gameObject.AddComponent<Appendage>();
        newAppendage.baseAppendageData = CreateInstance<AppendageData>();
        newAppendage.baseAppendageData.limbName = appendageName;
        newAppendage.baseAppendageData.appendageType = appType;
        newAppendage.collider = colliders[appendageColliderIndex];
        thisTarget.appendages.Add(newAppendage);


    }


    void ObtainAppendageColliders()
    {
        if (colliders == null || colliderNames ==null)
        {
            colliders = thisTarget.gameObject.GetComponentsInChildren<Collider>();
            colliderNames = new List<string>();
            foreach (Collider collider in colliders)
            {
                colliderNames.Add(collider.ToString());
            }
        }
    }



}
