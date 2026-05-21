using UnityEngine;

[CreateAssetMenu(fileName = "NewRagdollProfile", menuName = "BabyBanjo/Ragdoll")]
public class RagdollProfile: ScriptableObject
{
    [Header("Mass")]
    [Tooltip("Total weight in lb")]
    public float totalMass_lb = 44.0925f;
    public float TotalMass_kg { get => totalMass_lb * UnitConversions.lb_to_kg; }

    public float pelvisWeight = 5f;
    public float spineWeight =  5f;
    public float headWeight =   2f;
    public float hipsWeight =   3f;
    public float kneeWeight =   3f;
    public float armWeight =    2f;
    public float elbowWeight =  2f;

    [Header("Joint limits")]
    public RagdollJointConstraints hipsConstraints =  new RagdollJointConstraints(JointAxis.Left,  false, -20f, 70f, JointAxis.Forward, false, 30f, 0f, 0.3f,  1.5f);
    public RagdollJointConstraints kneeConstraints =  new RagdollJointConstraints(JointAxis.Left,  false, -80f, 0f,  JointAxis.Forward, false, 0f,  0f, 0.25f, 1.5f);
    public RagdollJointConstraints spineConstraints = new RagdollJointConstraints(JointAxis.Right, false, -20f, 20f, JointAxis.Forward, false, 10f, 0f, 1f,    2.5f);
    public RagdollJointConstraints armConstraints =   new RagdollJointConstraints(JointAxis.Back,  false, -70f, 10f, JointAxis.Right,   true, 50f, 0f, 0.25f, 1f);
    public RagdollJointConstraints elbowConstraints = new RagdollJointConstraints(JointAxis.Right, true,  -90f, 0f,  JointAxis.Back,    false, 0f,  0f, 0.2f,  1f);
    public RagdollJointConstraints headConstraints =  new RagdollJointConstraints(JointAxis.Right, false, -40f, 25f, JointAxis.Forward, false, 25f, 0f, 1f,    1f);

    [Header("Non-Humanoid naming conventions")]
    public string pelvisName =      "mixamorig:Hips"; 
    public string leftHipshName =   "mixamorig:LeftUpLeg";
    public string leftKneeName =    "mixamorig:LeftLeg";
    public string leftFootName =    "mixamorig:leftFoot";
    public string rightHipsName =  "mixamorig:RightUpLeg";
    public string rightKneeName =   "mixamorig:RightLeg";
    public string rightFootName =   "mixamorig:RightFoot";
    public string leftArmName =     "mixamorig:LeftArm";
    public string leftElbowName =   "mixamorig:LeftForeArm";
    public string rightArmName =    "mixamorig:RightArm";
    public string rightElbowName =  "mixamorig:RightForeArm";
    public string middleSpineName = "mixamorig:Spine";
    public string headName =        "mixamorig:Head";

    public string GetNameOfBone(RagdollBones boneType)
    {
        switch (boneType)
        {
            case RagdollBones.Pelvis:
                return pelvisName;
            case RagdollBones.MiddleSpine:
                return middleSpineName;
            case RagdollBones.Head:
                return headName;
            case RagdollBones.LeftHips:
                return leftHipshName;
            case RagdollBones.LeftKnee:
                return leftKneeName;
            // case RagdollBones.LeftFoot:
            //     return leftFootName;
            case RagdollBones.RightHips:
                return rightHipsName;
            case RagdollBones.RightKnee:
                return rightKneeName;
            // case RagdollBones.RightFoot:
            //     return rightFootName;
            case RagdollBones.LeftArm:
                return leftArmName;
            case RagdollBones.LeftElbow:
                return leftElbowName;
            case RagdollBones.RightArm:
                return rightArmName;
            case RagdollBones.RightElbow:
                return rightElbowName;
        }
        return "Invalid";
    }
    
    public RagdollJointConstraints GetBoneJointConstraints(RagdollBones boneType)
    {
        switch (boneType)
        {
            case RagdollBones.MiddleSpine:
                return spineConstraints;
            case RagdollBones.Head:
                return headConstraints;
            case RagdollBones.LeftHips:
            case RagdollBones.RightHips:
                return hipsConstraints;
            case RagdollBones.LeftKnee:
            case RagdollBones.RightKnee:
                return kneeConstraints;
            case RagdollBones.LeftArm:
            case RagdollBones.RightArm:
                return armConstraints;
            case RagdollBones.LeftElbow:
            case RagdollBones.RightElbow:
                return elbowConstraints;
        }
        return null;
    }
    
    public float GetBoneWeight_Kg(RagdollBones boneType)
    {
        float totalWeightAmount = 0f;
        totalWeightAmount += pelvisWeight;
        totalWeightAmount += spineWeight;
        totalWeightAmount += headWeight;
        totalWeightAmount += hipsWeight * 2f;
        totalWeightAmount += kneeWeight * 2f;
        totalWeightAmount += armWeight * 2f;
        totalWeightAmount += elbowWeight * 2f;

        float weightAmount = 0f;
        switch (boneType)
        {
            case RagdollBones.Pelvis:
                weightAmount = pelvisWeight;
                break;
            case RagdollBones.MiddleSpine:
                weightAmount = spineWeight;
                break;
            case RagdollBones.Head:
                weightAmount = headWeight;
                break;
            case RagdollBones.LeftHips:
            case RagdollBones.RightHips:
                weightAmount = hipsWeight;
                break;
            case RagdollBones.LeftKnee:
            case RagdollBones.RightKnee:
                weightAmount = kneeWeight;
                break;
            case RagdollBones.LeftArm:
            case RagdollBones.RightArm:
                weightAmount = armWeight;
                break;
            case RagdollBones.LeftElbow:
            case RagdollBones.RightElbow:
                weightAmount = elbowWeight;
                break;
        }
        return weightAmount / totalWeightAmount * TotalMass_kg;
    }

}