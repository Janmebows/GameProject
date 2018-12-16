using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeaponAssembly))]
public class WeaponPartCreatorEditor : Editor {

    public GameObject weaponPiece=null;
    public WeaponPieceData.pieceType pieceType = WeaponPieceData.pieceType.guard;
    
    SerializedProperty propConnect;

    string filePath = "Assets/Settings/WeaponPieces/Defaults";


    public override void OnInspectorGUI()
    {
        WeaponAssembly weaponAssembly = (WeaponAssembly)target;
        EditorGUILayout.LabelField("Data for creating a new weapon piece:");
        weaponPiece = (GameObject)EditorGUILayout.ObjectField("Weapon piece prefab", weaponPiece, typeof(GameObject), true);
        pieceType = (WeaponPieceData.pieceType)EditorGUILayout.EnumPopup("Type of piece", pieceType);



        EditorGUILayout.PropertyField(propConnect, true);



        if (GUILayout.Button("Add Weapon Piece"))
        {
            if (weaponPiece != null)
            {
                WeaponPieceData newData = ScriptableObject.CreateInstance<WeaponPieceData>();
                newData.prefab =weaponPiece;
                newData.pieceName = weaponPiece.name;
                newData.typeOfPiece = pieceType;
                newData.connections = weaponAssembly.connectPoints;


                AssetDatabase.CreateAsset(newData, filePath + "/default" + newData.pieceName + ".asset");

                weaponPiece = null;
                propConnect.ClearArray();
            }
        }
        if (GUILayout.Button("Fill piece array"))
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(filePath);
            foreach (Object asset in assets)
            {
                WeaponPieceData pieceData = (WeaponPieceData)asset;
                weaponAssembly.weaponPieces.Add(pieceData);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
    void OnEnable()
    {
        propConnect = serializedObject.FindProperty("connectPoints");

    }



}
