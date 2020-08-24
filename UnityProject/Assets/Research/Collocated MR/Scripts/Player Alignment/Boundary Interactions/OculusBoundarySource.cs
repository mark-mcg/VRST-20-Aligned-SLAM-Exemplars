using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusBoundarySource : BoundarySource
{
    public override List<Vector3> GetBoundary()
    {
        List<Vector3> boundary = new List<Vector3>();

        //Check if the boundary is configured
        bool configured = OVRManager.boundary.GetConfigured() || true; 
        if (configured)
        {
            //Grab all the boundary points. Setting BoundaryType to OuterBoundary is necessary
            Vector3[] boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
            boundary.AddRange(boundaryPoints);
            Debug.Log("Got boundary of length " + boundary.Count);
        }

        return boundary;
    }
}
