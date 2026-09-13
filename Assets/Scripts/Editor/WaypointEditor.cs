using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace BabyBanjo.Polyathlon.Race
{

    [CustomEditor(typeof(Waypoint))]
    public class WaypointEditor : Editor {
        private Waypoint waypoint;
        public SerializedProperty water;
        public SerializedProperty forceToGround;
        public SerializedProperty waypointFork;
        public SerializedProperty itemWaypointFork;
        public SerializedProperty jumpOffWaypointFork;
        public SerializedProperty ignoreMovementMode;

        void OnEnable()
        {
            water = serializedObject.FindProperty("water");
            forceToGround = serializedObject.FindProperty("forceToGround");
            waypointFork = serializedObject.FindProperty("waypointFork");
            itemWaypointFork = serializedObject.FindProperty("itemWaypointFork");
            jumpOffWaypointFork = serializedObject.FindProperty("jumpOffWaypointFork");
            ignoreMovementMode = serializedObject.FindProperty("ignoreMovementMode");
        }

        public override void OnInspectorGUI() {
            //DrawDefaultInspector();
            waypoint = ((Waypoint)this.target);
            serializedObject.Update();
            // Handle the fork array
            EditorGUI.BeginChangeCheck();
            //waypoint.water = EditorGUILayout.Toggle("Water Waypoint", waypoint.water);
            //waypoint.forceToGround = EditorGUILayout.Toggle("Force To Ground", waypoint.forceToGround);
            EditorGUILayout.PropertyField(water, new GUIContent("Water"));
            EditorGUILayout.PropertyField(forceToGround, new GUIContent("Force Height To Zero"));
            
            EditorGUILayout.PropertyField(waypointFork, true);
            EditorGUILayout.PropertyField(itemWaypointFork, true);
            EditorGUILayout.PropertyField(jumpOffWaypointFork, true);
            EditorGUILayout.PropertyField(ignoreMovementMode, true);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            
            waypoint.Seq = waypoint.transform.GetSiblingIndex();
            if (!waypoint.forceToGround)
            {
                Collider thisCollider = waypoint.transform.GetComponent<Collider>();
                if (thisCollider.GetType() == typeof(CapsuleCollider))
                {
                    waypoint.height = Mathf.Max(((CapsuleCollider)thisCollider).radius, ((CapsuleCollider)thisCollider).height / 2) / 2;
                } else if (thisCollider.GetType() == typeof(BoxCollider))
                {
                    waypoint.height = ((BoxCollider)thisCollider).size.y / 2;
                }
            }
            else
            {
                waypoint.height = 0;
            }
            EditorGUILayout.LabelField("Seqence Number", waypoint.Seq.ToString());
            EditorGUILayout.LabelField("Height", waypoint.height.ToString());
        }
    }
}