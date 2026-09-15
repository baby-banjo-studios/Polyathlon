using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Video;
using System;
using UnityEngine.Experimental.Animations;

[ExecuteAlways]
[RequireComponent(typeof(Animator))]
public class StaticPose : MonoBehaviour
{
    public AnimationClip animationClip;
    [Range(0f, 1f)]
    public float normalizedTime = 0f;
    public bool mirror = false;
    
    private Animator animator;
    private PlayableGraph graph;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        ApplyPose();
    }

    private void OnDisable()
    {
        CleanupGraph();
    }

    private void OnValidate()
    {
        ApplyPose();
    }

    private void ApplyPose()
    {
        if (animationClip != null)
        {
            CleanupGraph();

            graph = PlayableGraph.Create(string.Format("{0}_pose", gameObject.name));
            AnimationPlayableOutput playableOutput = AnimationPlayableOutput.Create(graph, "output", animator);
            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(graph, animationClip);

            clipPlayable.SetTime(animationClip.length * normalizedTime);
            clipPlayable.Pause();

            playableOutput.SetSourcePlayable(clipPlayable);
            
            graph.Evaluate();
        }
    }

    private void CleanupGraph()
    {
        if (graph.IsValid())
        {
            graph.Destroy();
        }
    }
}