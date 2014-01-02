using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(ShadowMakerHelper))]
public class ShadowMaker : Editor {

    [MenuItem("PlanetCute/Remove Shadows %&r")]
    public static void RemoveShadows() {
        // remove all existing shadows
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Shadow")) {
            DestroyImmediate(obj);
        }
    }

    [MenuItem("PlanetCute/Make Shadows %&m")]
    public static void MakeShadows() {
        GameObject helperObj = GameObject.Find("ShadowMakerHelper");
        ShadowMakerHelper helper = helperObj.GetComponent<ShadowMakerHelper>();
        if(helperObj == null) {
            Debug.LogError("Please place the ShadowMakerHelper in the scene");
            return;
        }
        RemoveShadows();
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Ground")) {
            MakeShadow(obj, helper);
        }
    }

    public static void MakeShadow(GameObject ground, ShadowMakerHelper helper) {
        Vector3 basePos = ground.transform.position, pos;
        
        // figure out yay/nay if the other 8 spots above this are empty
        bool[,] filled = new bool[3, 3];
        for(int i = 0; i < 3; i++) {
            for(int j = 0; j < 3; j++) {
                pos = new Vector3(basePos.x - 1 + i, basePos.y + 1, basePos.z - 1 + j);
                filled[i, j] = IsGroundInPosition(pos);
            }
        }

        //Debug.Log(ground.name + " filled is: ["
        //    + (filled[0, 0] ? "1" : "0")
        //    + (filled[1, 0] ? "1" : "0")
        //    + (filled[2, 0] ? "1" : "0")
        //    + "],["
        //    + (filled[0, 1] ? "1" : "0")
        //    + (filled[1, 1] ? "1" : "0")
        //    + (filled[2, 1] ? "1" : "0")
        //    + "],["
        //    + (filled[0, 2] ? "1" : "0")
        //    + (filled[1, 2] ? "1" : "0")
        //    + (filled[2, 2] ? "1" : "0")
        //    + "]");
        HashSet<GameObject> toAdd = new HashSet<GameObject>();
        // only look for shadows on top of us if there isn't something
        // directly above us
        pos = new Vector3(basePos.x, basePos.y + 1, basePos.z);
        if(!IsGroundInPosition(pos)) {
            if(filled[1, 2]) {
                toAdd.Add(helper.shadowNorth);
            } else {
                if(!filled[0, 1] && filled[0, 2]) {
                    toAdd.Add(helper.shadowNorthWest);
                }
                if(!filled[2, 1] && filled[2, 2]) {
                    toAdd.Add(helper.shadowNorthEast);
                }
            }
            if(filled[0, 1]) {
                toAdd.Add(helper.shadowWest);
            }
            if(filled[2, 1]) {
                toAdd.Add(helper.shadowEast);
            }
            if(filled[1, 0]) {
                toAdd.Add(helper.shadowSouth);
            } else {
                if(!filled[0, 1] && filled[0, 0]) {
                    toAdd.Add(helper.shadowSouthWest);
                }
                if(!filled[2, 1] && filled[2, 0]) {
                    toAdd.Add(helper.shadowSouthEast);
                }
            }
        }
        // if there ISN'T a block south of us but there IS a block southWest,
        // do ShadowSideWest to get a shadow on our front face
        bool isSouthWest = IsGroundInPosition(
            new Vector3(basePos.x - 1, basePos.y, basePos.z - 1));
        bool isSouth = IsGroundInPosition(
            new Vector3(basePos.x, basePos.y, basePos.z - 1));
        if(isSouthWest && !isSouth) {
            toAdd.Add(helper.shadowSideWest);
        }
        foreach(GameObject prefab in toAdd) {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.parent = ground.transform;
            obj.transform.position = basePos;
        }
    }
    private static bool IsGroundInPosition(Vector3 position) {
        Collider[] objs = Physics.OverlapSphere(position, .2f);
        foreach(Collider col in objs) {
            if(col.tag == "Ground") {
                return true;
            }
        }
        return false;
    }
}
