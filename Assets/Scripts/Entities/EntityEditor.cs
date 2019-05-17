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
        FillAppendageListButton();
        //Regular expression to remove <space>(number) at the end of duplicates

        //Stuff from the target itself
        thisTarget.humanoid = EditorGUILayout.Toggle("Humanoid", thisTarget.humanoid);

        //appendage information

        ObtainAppendageColliders();

        //ADD APPENDAGE
        AddAppendageButton();
        DisplayAppendages();

    }


    void FillAppendageListButton()
    {
        if (GUILayout.Button("Fill appendage list"))
        {
            try
            {
                thisTarget.appendages = new List<Appendage>(thisTarget.gameObject.GetComponents<Appendage>());
            }
            catch
            {
                Debug.LogWarning("Try adding appendages first!");
            }
        }
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
        Debug.Log("You may have to manually put in appendage types");
        
        ObtainAppendageColliders();
        //if its a human
        if (thisTarget.humanoid)
        {
            for (int i = 0; i < colliders.Length; ++i)
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
        //First clean string and see if it matches any keywords
        //clean the string to compare against
        string newcompare = compare.ToLower();
        string regexPattern = "_.*";
        newcompare = Regex.Replace(newcompare, regexPattern, "");

        //compare to keywords
        switch (newcompare)
        {
            case "arml":
            case "lowerarm":
            case "elbow":
            case "armlower":
                return AppendageData.AppendageType.ArmL;
            case "armu":
            case "arm":
            case "upperarm":
            case "armupper":
                return AppendageData.AppendageType.ArmU;
            case "foot":
            case "ankle":
                return AppendageData.AppendageType.Foot;
            case "hand":
            case "wrist":
                return AppendageData.AppendageType.Hand;
            case "head":
            case "face":
                return AppendageData.AppendageType.Head;
            case "legl":
            case "lowerleg":
            case "knee":
            case "calf":
                return AppendageData.AppendageType.LegL;
            case "legu":
            case "upperleg":
            case "thigh":
            case "leg":
                return AppendageData.AppendageType.LegU;
            case "neck":
                return AppendageData.AppendageType.Neck;
            case "torsol":
            case "lowertorso":
                return AppendageData.AppendageType.TorsoL;
            case "torsou":
            case "uppertorso":
            case "upperbody":
                return AppendageData.AppendageType.TorsoU;
            case "waist":
            case "lowerbody":
                return AppendageData.AppendageType.Waist;
            default:
                break;
        }
        //compare to appendage types

        Debug.LogWarning("The type for appendage: " + compare + " was not automatically assigned");
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
                EditorGUILayout.Vector3Field(appendage.baseAppendageData.limbName + ": ", appendage.collider.transform.localPosition);
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
