using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Linq;
using System;


/// <summary>
/// Inspired by https://stackoverflow.com/questions/22159897/how-to-compare-two-shapes
/// </summary>
public class XRStoredCalibration : MonoBehaviour
{
    public class StoredBoundaryTransformation
    {
        public List<Vector3> points;
        public TransformationToReality BoundaryTransformation, DeviceTransformation;
    }

    public BoundarySource boundarySource;

    public void Update()
    {
        DoOnce();
    }

    public List<Vector3> TestDataA = new List<Vector3>()
    {
        new Vector3(0,1,1),
        new Vector3(3,2,3),
        new Vector3(5,2,4),
        new Vector3(23,3,65),
        new Vector3(0,0.5f,0.8f)
    };

    public List<Vector3> TestDataB = new List<Vector3>()
    {
        new Vector3(23,3,65),
        new Vector3(0,0.5f,0.8f),
        new Vector3(0,1,1),
        new Vector3(3,2,3),
        new Vector3(5,2,4)
    };

    bool ran = false;
    // Start is called before the first frame update
    void DoOnce()
    {
        if (!ran)
        {
            ran = true;
            List<Vector3> boundary = boundarySource.GetBoundary();
            List<Vector3> previousCalibratedBoundary = new List<Vector3>();

            // Load the previous calibrated boundary here 

            // and add functions so that when the alignment manager performs a calibration, it can save the calibration state with the boundary (and the boundarys orientation/position)
            // while no alignment, periodically check the stored boundary against current boundary


            Debug.Log("XRStoredCalibration got boundary length " + boundary.Count);
            if (boundary.Count == 0)
            {
                Debug.Log("Failed to get any boundary data, using debug data instead");
                boundary = TestDataA;
                previousCalibratedBoundary = TestDataB;
            }

            Debug.Log("Boundary is:");
            boundary.ForEach(x => Debug.Log(x));

            LinkedList<Difference> currentBoundary = GetDifference(boundary);

            Debug.Log("Aligned difference is:");
            AlignedDifferenceLists comparison = new AlignedDifferenceLists(currentBoundary, GetDifference( previousCalibratedBoundary));
        }
    }

    public LinkedList<Difference> GetDifference(List<Vector3> points)
    {
        LinkedList<Vector3> linkedPoints = new LinkedList<Vector3>(points);
        LinkedListNode<Vector3> current = linkedPoints.First;
        LinkedList<Difference> differenceList = new LinkedList<Difference>();

        int ind = 0;
        while (current != null && current.Next != null && current.Next.Value != null)
        {
            differenceList.AddLast(new Difference(current.Value, current.Next.Value, ind));
            current = current.Next;
            ind++;
        }

        return differenceList;
    }

    public class AlignedDifferenceLists
    {
        public LinkedList<Difference> a, b;
        public int alignedStartIndexB;

        public AlignedDifferenceLists(LinkedList<Difference> a, LinkedList<Difference> b)
        {
            this.a = a;
            this.b = b;

            // First the closest match to our start point in terms of the angle between the points and the distance
            Difference bStart = b.Aggregate((min, x) => x.Similarity(a.First.Value) < min.Similarity(a.First.Value) ? x : min);

            Debug.Log("Closest point in b to start of a ("+ a.First.Value + ") is " + bStart);

            Debug.Log("Full output of similarity scores as follows...");

            // For debugging, lets see what the scores look like
            foreach (Difference diff in b)
            {
                diff.calculatedSimilarity = diff.Similarity(a.First.Value);
                Debug.Log(diff);
            }


            if (a.Count != b.Count)
            {
                Debug.LogError("Different collections of points, can't compare, counts of " + a.Count + " and " + b.Count);

            }
            else
            {
                /*
                 * for each possible b start position from 1..b.Count, we want to calculate the total similarity score
                 * to each point in a.
                 * 
                 * We then find the shift that most closely matches a and b from the index with the lowest score
                 * 
                 */
                float[] similarityScores = new float[a.Count];

                for (int bStartPos = 0; bStartPos < b.Count; bStartPos++)
                {
                    int currentIndex = 0;
                    foreach (Difference aDiff in a)
                    {
                        similarityScores[bStartPos] += aDiff.Similarity(b.ElementAt((bStartPos + currentIndex) % b.Count));
                        currentIndex++;
                    }
                }

                Debug.Log("Similarity scores for each b start position are..." + String.Join(",", similarityScores));

                int minIndex = Array.IndexOf(similarityScores, similarityScores.Min());

                Debug.Log("Best match for b start position aligning to A is " + minIndex);

            }
        }

        public LinkedListNode<Difference> LoopingNext(LinkedListNode<Difference> current) {
            if (current.Next != null)
                return current.Next;
            else
                return current.List.First;
        }
    }

    public class Difference
    {
        public float length;
        public float angle;
        public Vector3 point, nextPoint;
        public int index;
        public float calculatedSimilarity;

        public Difference(Vector3 point, Vector3 next, int index)
        {
            this.point = point;
            this.nextPoint = next;
            this.length = Vector3.Distance(point, nextPoint);
            this.angle = Vector3.Angle(point, next);
            this.index = index;
        }

        public float Similarity(Difference b)
        {
            return Mathf.Abs(length - b.length) + Mathf.Abs(angle - b.angle);
        }

        public override string ToString()
        {
            return "index " + index + " point " + point + " similarity score to a.first " + calculatedSimilarity + " length " + length + " angle " + angle;
        }
    }
}
