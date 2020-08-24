using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Tracks and enacts Player alignment to a given calibration point.
/// </summary>
public class AlignedPointManager : MonoBehaviour
{
    public List<TransformationToReality> Anchors;
    public TransformationToReality CurrentAnchor;

    void Start()
    {
        RefreshAnchors();
    }

    public void RefreshAnchors()
    {
        Anchors = FindObjectsOfType<TransformationToReality>().ToList();
    }

    public void AlignTo(int index)
    {
        SetCurrentAndAlignTo( Anchors[index]);
    }

    public void AlignTo(string GameObjectName)
    {
        SetCurrentAndAlignTo(Anchors.Single(x => x.gameObject.name.Equals(GameObjectName)));
    }

    public void AlignTo(TransformationToReality anchor)
    {
        Debug.Log("Aligning to " + anchor);
        SetCurrentAndAlignTo(anchor);
    }

    private void SetCurrentAndAlignTo(TransformationToReality anchor)
    {
        CurrentAnchor = anchor;
        AlignToCurrent();
    }

    private void AlignToCurrent()
    {
        Debug.Log("Aligning to current anchor " + CurrentAnchor);
        if (CurrentAnchor != null)
            CurrentAnchor.TryEnactTransformation();
    }
}