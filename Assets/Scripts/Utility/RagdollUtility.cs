using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RagdollJointConstraints
{
    public JointAxis twistAxis;
    public bool mirrorTwistAxisOnRightSide;
    public float lowTwistLimit;
    public float highTwistLimit;
    public JointAxis swingAxis;
    public bool mirrorSwingAxisOnRightSide;
    public float swing1Limit;
    public float swing2Limit;
    public float radiusScale;
    public float density;
    
    public RagdollJointConstraints(JointAxis twistAxis, bool mirrorTwistAxisOnRightSide, float lowTwistLimit, float highTwistLimit,
                                   JointAxis swingAxis, bool mirrorSwingAxisOnRightSide, float swing1Limit, float swing2Limit, float radiusScale, float density)
    {
        this.twistAxis = twistAxis;
        this.mirrorTwistAxisOnRightSide = mirrorTwistAxisOnRightSide;
        this.lowTwistLimit = lowTwistLimit;
        this.highTwistLimit = highTwistLimit;
        this.swingAxis = swingAxis;
        this.mirrorSwingAxisOnRightSide = mirrorSwingAxisOnRightSide;
        this.swing1Limit = swing1Limit;
        this.swing2Limit = swing2Limit;
        this.radiusScale = radiusScale;
        this.density = density;
    }
}

public enum RagdollBones
{
    Pelvis,
    MiddleSpine,
    Head,
    LeftHips,
    LeftKnee,
    //LeftFoot,
    RightHips,
    RightKnee,
    //RightFoot,
    LeftArm,
    LeftElbow,
    RightArm,
    RightElbow,
    NumberOfBones   // must be last
}

public enum JointAxis
{
    Right,
    Left,
    Up,
    Down,
    Forward,
    Back,
}

public class RagdollUtility
{
    public class StaticBoneInfo
    {
        public HumanBodyBones BoneType { get; private set; }
        public RagdollBones? Parent { get; private set; }
        public Type ColliderType { get; private set; }

        public StaticBoneInfo(HumanBodyBones boneType,
                              RagdollBones? parent,
                              Type colliderType)
        {
            BoneType = boneType;
            Parent = parent;
            ColliderType = colliderType;
        }
    }

    public class DynamicBoneInfo
    {
        public HumanBodyBones boneType;
        public Transform transform;
        public RagdollBones? parentBoneType;
        public DynamicBoneInfo parent;
        public List<DynamicBoneInfo> children;
        public Rigidbody rb;
        public Type colliderType;
        public Collider collider;
        public CharacterJoint joint;
        public float mass;

        public DynamicBoneInfo(StaticBoneInfo staticInfo)
        {
            boneType = staticInfo.BoneType;
            transform = null;
            parentBoneType = staticInfo.Parent;
            parent = null;
            children = new List<DynamicBoneInfo>();
            rb = null;
            colliderType = staticInfo.ColliderType;
            collider = null;
            joint = null;
        }
    }

    private static StaticBoneInfo[] staticBones = new StaticBoneInfo[]
    {
        new StaticBoneInfo(HumanBodyBones.Hips,             null,                       typeof(BoxCollider))    ,
        new StaticBoneInfo(HumanBodyBones.Spine,            RagdollBones.Pelvis,        typeof(BoxCollider))    ,
        new StaticBoneInfo(HumanBodyBones.Head,             RagdollBones.MiddleSpine,   typeof(SphereCollider)) ,
        new StaticBoneInfo(HumanBodyBones.LeftUpperLeg,     RagdollBones.Pelvis,        typeof(CapsuleCollider)),
        new StaticBoneInfo(HumanBodyBones.LeftLowerLeg,     RagdollBones.LeftHips,      typeof(CapsuleCollider)),
        //new StaticBoneInfo(HumanBodyBones.LeftFoot,         RagdollBones.LeftKnee,      null),
        new StaticBoneInfo(HumanBodyBones.RightUpperLeg,    RagdollBones.Pelvis,        typeof(CapsuleCollider)),
        new StaticBoneInfo(HumanBodyBones.RightLowerLeg,    RagdollBones.RightHips,     typeof(CapsuleCollider)),
        //new StaticBoneInfo(HumanBodyBones.RightFoot,        RagdollBones.RightKnee,     null),
        new StaticBoneInfo(HumanBodyBones.LeftUpperArm,     RagdollBones.MiddleSpine,   typeof(CapsuleCollider)),
        new StaticBoneInfo(HumanBodyBones.LeftLowerArm,     RagdollBones.LeftArm,       typeof(CapsuleCollider)),
        new StaticBoneInfo(HumanBodyBones.RightUpperArm,    RagdollBones.MiddleSpine,   typeof(CapsuleCollider)),
        new StaticBoneInfo(HumanBodyBones.RightLowerArm,    RagdollBones.RightArm,      typeof(CapsuleCollider)),
    };

    public static bool CreateRagdoll(GameObject model, RagdollProfile profile)
    {
        bool success = true;

        // if using humaniod avatar, can directly use avatar bone assignments instead of mixamo implementations
        Animator animator = model.GetComponent<Animator>();
        bool isHumanoid = animator.isHuman && animator != null;

        // create dynamic bones
        DynamicBoneInfo[] dynamicBones = new DynamicBoneInfo[(int)RagdollBones.NumberOfBones];
        for (int i = 0; i < (int)RagdollBones.NumberOfBones; i++)
        {
            dynamicBones[i] = new DynamicBoneInfo(staticBones[i]);
        }

        // assign dynamic bone transforms, rigidbodies, and parents - must be done before colliders and joints
        for (int i = 0; i < (int)RagdollBones.NumberOfBones; i++)
        {
            DynamicBoneInfo bone = dynamicBones[i];
            // get transform
            if (isHumanoid)
            {
                bone.transform = animator.GetBoneTransform(bone.boneType);
            }
            else
            {
                bone.transform = FindChildRecursive(model.transform, profile.GetNameOfBone((RagdollBones)i));
            }
            if (bone.transform == null)
            {
                Debug.LogError(String.Format("Failed to find bone {0}", ((RagdollBones)i).ToString()));
                success = false;
            }

            // add rigidbody
            Rigidbody rb = bone.transform.gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = bone.transform.gameObject.AddComponent<Rigidbody>();
            }
            rb.mass = profile.GetBoneWeight_Kg((RagdollBones)i);
            bone.rb = rb;
            
            // link up parent and children
            if (staticBones[i].Parent.HasValue)
            {
                DynamicBoneInfo parentBone = dynamicBones[(int)staticBones[i].Parent.Value];
                bone.parent = parentBone;
                parentBone.children.Add(bone);

            }
        }        

        if (!success)
        {
            Debug.LogError("Failed to initialize dynamic bones");
            return false;
        }

        // for quick reference
        DynamicBoneInfo pelvisBone =        dynamicBones[(int)RagdollBones.Pelvis];
        DynamicBoneInfo spineBone =         dynamicBones[(int)RagdollBones.MiddleSpine];
        DynamicBoneInfo headBone =          dynamicBones[(int)RagdollBones.Head];
        DynamicBoneInfo leftHipsBone =      dynamicBones[(int)RagdollBones.LeftHips];
        DynamicBoneInfo rightHipsBone =     dynamicBones[(int)RagdollBones.RightHips];
        DynamicBoneInfo leftArmBone =       dynamicBones[(int)RagdollBones.LeftArm];
        DynamicBoneInfo rightArmBone =      dynamicBones[(int)RagdollBones.RightArm];
        DynamicBoneInfo rightElbowBone =    dynamicBones[(int)RagdollBones.RightElbow];

        Vector3[] localAxes = new Vector3[6];
        localAxes[(int)JointAxis.Right] =   pelvisBone.transform.TransformDirection(Vector3.right);
        localAxes[(int)JointAxis.Left] =    pelvisBone.transform.TransformDirection(Vector3.left);
        localAxes[(int)JointAxis.Up] =      pelvisBone.transform.TransformDirection(Vector3.up);
        localAxes[(int)JointAxis.Down] =    pelvisBone.transform.TransformDirection(Vector3.down);
        localAxes[(int)JointAxis.Forward] = pelvisBone.transform.TransformDirection(Vector3.forward);
        localAxes[(int)JointAxis.Back] =    pelvisBone.transform.TransformDirection(Vector3.back);

        // now create colliders, joints for each bone
        for (int i = 0; i < (int)RagdollBones.NumberOfBones; i++)
        {
            DynamicBoneInfo bone = dynamicBones[i];
            RagdollJointConstraints constraints = profile.GetBoneJointConstraints((RagdollBones)i);

            // colliders
            if (bone.colliderType == typeof(BoxCollider))
            {
                BoxCollider col = bone.transform.gameObject.GetComponent<BoxCollider>();
                if (col == null)
                {
                    col = bone.transform.gameObject.AddComponent<BoxCollider>();
                }
                Bounds breastBounds = GetBreastBounds(bone.transform, 
                                                      leftArmBone.transform,
                                                      leftHipsBone.transform,
                                                      rightArmBone.transform,
                                                      rightHipsBone.transform);
                bool isSpine = bone == spineBone;
                Bounds bounds = Clip(breastBounds, bone.transform, spineBone.transform, below: isSpine);                                                      

                col.center = bounds.center;
                col.size = bounds.size;
                bone.collider = col;
            }
            else if (bone.colliderType == typeof(SphereCollider))
            {
                SphereCollider col = bone.transform.gameObject.GetComponent<SphereCollider>();
                if (col == null)
                {
                    col = bone.transform.gameObject.AddComponent<SphereCollider>();
                }
                float radius = Vector3.Distance(leftArmBone.transform.position, rightArmBone.transform.position) / 4f;
                col.radius = radius;
                Vector3 center = Vector3.zero;
                CalculateDirection(bone.transform.InverseTransformPoint(pelvisBone.transform.position), out int direction, out float distance);
                if (distance > 0)
                {
                    center[direction] = -radius;
                }            
                else
                {
                    center[direction] = radius;
                }
                col.center = center;
                bone.collider = col;
            }
            else if (bone.colliderType == typeof(CapsuleCollider))
            {
                CapsuleCollider col = bone.transform.gameObject.GetComponent<CapsuleCollider>();
                if (col == null)
                {
                    col = bone.transform.gameObject.AddComponent<CapsuleCollider>();
                }

                int direction;
                float distance;
                if (bone.children.Count == 1)
                {
                    Vector3 position = bone.children[0].transform.position;
                    CalculateDirection(bone.transform.InverseTransformPoint(position), out direction, out distance);
                }
                else
                {
                    if (bone.parent != null)
                    {
                        Vector3 position = bone.transform.position - bone.parent.transform.position + bone.transform.position;  // IS THIS NOT THE SAME AS bone.parent.transform.position? AM I TRIPPING?
                        CalculateDirection(bone.transform.InverseTransformPoint(position), out direction, out distance);
                        if (bone.children.Count > 1)
                        {
                            Bounds bounds = default(Bounds);
                            for (int j = 0; j < bone.children.Count; j++)
                            {
                                bounds.Encapsulate(bone.transform.InverseTransformPoint(bone.children[i].transform.position));
                            }
                            distance = distance <= 0f ? bounds.min[direction] : bounds.max[direction];
                        }
                    }
                    else
                    {
                        // HOW? HOW? HOW?
                        Debug.LogError(String.Format("Somehow bone {0} has no children AND no parents", ((RagdollBones)i).ToString()));
                        direction = 1;
                        distance = 0;
                        success = false;
                    }
                }

                col.direction = direction;
                Vector3 center = Vector3.zero;
                center[direction] = distance * 0.5f;
                col.center = center;
                col.height = Mathf.Abs(distance);
                if (constraints != null)
                {
                    col.radius = Mathf.Abs(distance * constraints.radiusScale);
                }
                else
                {
                    // should not be possible
                    col.radius = Mathf.Abs(distance);
                }
                bone.collider = col;
            }
            else if (bone.colliderType == null)
            {
                // do not make a collider
            }
            else
            {
                // ruh oh
                Debug.LogError(String.Format("Invalid collider type {0}", bone.colliderType.ToString()));
                success = false;
            }

            // joints
            if (bone.parent != null && constraints != null)
            {
                Vector3 worldTwistDir = localAxes[(int)constraints.twistAxis];
                Vector3 worldSwingDir = localAxes[(int)constraints.swingAxis];

                Vector3 localTwist = bone.transform.InverseTransformDirection(worldTwistDir);
                Vector3 localSwing = bone.transform.InverseTransformDirection(worldSwingDir);

                if ((RagdollBones)i == RagdollBones.RightArm || (RagdollBones)i == RagdollBones.RightElbow || (RagdollBones)i == RagdollBones.RightHips || (RagdollBones)i == RagdollBones.RightKnee)
                {
                    if (constraints.mirrorTwistAxisOnRightSide)
                    {
                        worldTwistDir *= -1;
                    }
                    if (constraints.mirrorSwingAxisOnRightSide)
                    {
                        worldSwingDir *= -1;
                    }
                }

                CharacterJoint joint = bone.transform.gameObject.AddComponent<CharacterJoint>();
                // joint.axis = CalculateDirectionAxis(localTwist);
                // joint.swingAxis = CalculateDirectionAxis(localSwing);
                joint.axis = CalculateDirectionAxis(worldTwistDir);
                joint.swingAxis = CalculateDirectionAxis(worldSwingDir);
                joint.anchor = Vector3.zero;
                joint.connectedBody = bone.parent.rb;
                joint.enablePreprocessing = false;
                joint.lowTwistLimit =   new SoftJointLimit() { contactDistance = 0, limit = constraints.lowTwistLimit };
                joint.highTwistLimit =  new SoftJointLimit() { contactDistance = 0, limit = constraints.highTwistLimit };
                joint.swing1Limit =     new SoftJointLimit() { contactDistance = 0, limit = constraints.swing1Limit };
                joint.swing2Limit =     new SoftJointLimit() { contactDistance = 0, limit = constraints.swing2Limit };
                bone.joint = joint;
            }
        }
        
        return success;
    }

#region mirrored from UnityEditor.RagdollBuilder
    private static Bounds Clip(Bounds bounds, Transform relativeTo, Transform clipTransform, bool below)
    {
        int index = LargestComponent(bounds.size);
        if (Vector3.Dot(Vector3.up, relativeTo.TransformPoint(bounds.max)) > Vector3.Dot(Vector3.up, relativeTo.TransformPoint(bounds.min)) == below)
        {
            Vector3 min = bounds.min;
            min[index] = relativeTo.InverseTransformPoint(clipTransform.position)[index];
            bounds.min = min;
        }
        else
        {
            Vector3 max = bounds.max;
            max[index] = relativeTo.InverseTransformPoint(clipTransform.position)[index];
            bounds.max = max;
        }

        return bounds;
    }

    private static Bounds GetBreastBounds(Transform relativeTo, Transform leftArm, Transform leftLeg, Transform rightArm, Transform rightLeg)
    {
        Bounds result = default(Bounds);
        result.Encapsulate(relativeTo.InverseTransformPoint(leftLeg.position));
        result.Encapsulate(relativeTo.InverseTransformPoint(rightLeg.position));
        result.Encapsulate(relativeTo.InverseTransformPoint(leftArm.position));
        result.Encapsulate(relativeTo.InverseTransformPoint(rightArm.position));
        Vector3 size = result.size;
        size[SmallestComponent(result.size)] = size[LargestComponent(result.size)] / 2f;
        result.size = size;
        return result;
    }

    private static int SmallestComponent(Vector3 point)
    {
        int num = 0;
        if (Mathf.Abs(point[1]) < Mathf.Abs(point[0]))
        {
            num = 1;
        }

        if (Mathf.Abs(point[2]) < Mathf.Abs(point[num]))
        {
            num = 2;
        }

        return num;
    }

    private static int LargestComponent(Vector3 point)
    {
        int num = 0;
        if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
        {
            num = 1;
        }

        if (Mathf.Abs(point[2]) > Mathf.Abs(point[num]))
        {
            num = 2;
        }

        return num;
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name.Contains(name)) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private static void CalculateDirection(Vector3 point, out int direction, out float distance)
    {
        direction = 0;
        if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
        {
            direction = 1;
        }

        if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
        {
            direction = 2;
        }

        distance = point[direction];
    }

    private static Vector3 CalculateDirectionAxis(Vector3 point)
    {
        int direction = 0;
        CalculateDirection(point, out direction, out var distance);
        Vector3 zero = Vector3.zero;
        if (distance > 0f)
        {
            zero[direction] = 1f;
        }
        else
        {
            zero[direction] = -1f;
        }

        return zero;
    }
#endregion
}