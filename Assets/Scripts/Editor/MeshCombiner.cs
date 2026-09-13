using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace BabyBanjo.Core.Utility
{

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MeshCombiner : MonoBehaviour
    {

        /*  combines all child meshes of an object into 1 mesh */
        public void CombineMeshes()
        {

            // I'm really lazy and don't wanna check if the game object is a prefab or not
            try
            {
                PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            DeleteMesh();

            Vector3 oldPos = transform.position;
            Quaternion oldRot = transform.rotation;

            transform.rotation = Quaternion.identity;
            transform.position = Vector3.zero;

            MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
            Mesh finalMesh = new Mesh();
            CombineInstance[] combiners = new CombineInstance[filters.Length];

            for (int i = 0; i < filters.Length; i++)
            {
                if (filters[i].transform != transform)
                {
                    combiners[i].subMeshIndex = 0;
                    combiners[i].mesh = filters[i].sharedMesh;
                    combiners[i].transform = filters[i].transform.localToWorldMatrix;
                }
            }

            finalMesh.CombineMeshes(combiners);
            GetComponent<MeshFilter>().sharedMesh = finalMesh;

            transform.rotation = oldRot;
            transform.position = oldPos;

            
        }

        /*  deletes all child objects */
        public void DeleteChildren()
        {

            if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to delete " + transform.childCount + " objects?", "Delete", "Do Not Delete"))
            {
                for (int i = 0; i < transform.childCount + i; i++)
                {
                    UnityEngine.Object.DestroyImmediate(transform.GetChild(0).gameObject);
                    //transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        public void DeleteMesh()
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(mesh, true);
        }
    }
}