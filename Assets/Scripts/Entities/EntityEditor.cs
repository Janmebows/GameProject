using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
//script to allow the editing of entities without too much interaction within the model/prefab
[CustomEditor(typeof(BaseEntity))]
public class EntityEditor : Editor
{
    BaseEntity thisTarget;
    bool adding = false;

    string path = "Assets/Settings/Appendages/CreatureSpecificDefaults";
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

        AutoAppendagesButton();

        //Regular expression to remove <space>(number) at the end of duplicates

        //Stuff from the target itself
        thisTarget.humanoid = EditorGUILayout.Toggle("Humanoid", thisTarget.humanoid);

        //appendage information

        ObtainAppendageColliders();

        //ADD APPENDAGE
        AddAppendageButton();
        DisplayAppendages();

    }





    void AutoAppendagesButton()
    {
        if (GUILayout.Button("Auto Add Appendages (this might take a second)"))
        {
            string regexPattern = "\\s\\(\\d.*\\)\\z";
            string folderName = Regex.Replace(target.name, regexPattern, "");
            if (!AssetDatabase.IsValidFolder(path +"/" + folderName))
            {
                string guid = AssetDatabase.CreateFolder(path, folderName);
                string folderPath = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log("Created: " + folderPath);
                AutoAddAppendages();
            }
            else if (thisTarget.appendages.Count == 0)
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(folderName);
                foreach (Object asset in assets)
                {
                    Appendage newAppendage = thisTarget.gameObject.AddComponent<Appendage>();
                    newAppendage.baseAppendageData = (AppendageData)asset;
                    int collindex = colliderNames.FindIndex(x => x == newAppendage.baseAppendageData.limbName);
                    newAppendage.collider = colliders[collindex];
                    thisTarget.appendages.Add(newAppendage);
                }
            }

        }
    }





    void AutoAddAppendages()
    {
        Debug.Log("You will have to manually put in appendage types");
        ObtainAppendageColliders();
        //if its a human
        if (thisTarget.humanoid)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                //type needs to be similar to the collider name
                AppendageData.AppendageType type = FindAppendageType(colliderNames[i]);
                //AppendageData.AppendageType.
                //add the appendage
                DealWithAppendage(colliderNames[i], type, i);
            }
        }
    }

    AppendageData.AppendageType FindAppendageType(string compare)
    {
        //clean the string to compare against


        //compare to appendage types


        return AppendageData.AppendageType.Other;
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
                DealWithAppendage(appendageName, appType,appendageColliderIndex);
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
    void DealWithAppendage(string appName, AppendageData.AppendageType type, int index)
    {
        Appendage newAppendage = thisTarget.gameObject.AddComponent<Appendage>();
        newAppendage.baseAppendageData = ScriptableObject.CreateInstance<AppendageData>();
        newAppendage.baseAppendageData.limbName = appName;
        newAppendage.baseAppendageData.appendageType = type;
        newAppendage.collider = colliders[index];
        thisTarget.appendages.Add(newAppendage);

        AssetDatabase.CreateAsset(newAppendage.baseAppendageData, path +"/" + target.name +"/"+ appName +".asset");
        AssetDatabase.SaveAssets();


    }


    void ObtainAppendageColliders()
    {
        if (colliders == null || colliderNames ==null)
        {
            colliders = thisTarget.gameObject.GetComponentsInChildren<Collider>();
            colliderNames = new List<string>();
            foreach (Collider collider in colliders)
            {
                colliderNames.Add(collider.name);
            }
        }
    }



}
