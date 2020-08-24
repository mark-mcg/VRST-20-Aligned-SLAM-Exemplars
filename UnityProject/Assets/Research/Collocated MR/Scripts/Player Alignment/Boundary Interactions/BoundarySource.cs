using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoundarySource : MonoBehaviour
{
    public abstract List<Vector3> GetBoundary();
}