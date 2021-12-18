using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class Fourier3D : MonoBehaviour
{
    [Serializable]
    public struct Wave
    {
        public float amplitude;
        public float frequency;
        public float phase;
    }

    public float viewScale= .1f;
    public List<Vector3> plotted = new List<Vector3>();

    public List<Wave> points;

    float f(Wave point, float t)
    {
        return point.amplitude * Mathf.Cos(Mathf.PI * point.frequency * t + point.phase);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Vector3[] arrowPositions = new Vector3[points.Count];
            for (var i = 0; i < arrowPositions.Length; i++)
            {
                //y = A sin(Time.time * f)
                var y = i == 0 ? 0 : points[i-1].amplitude * Mathf.Sin(Time.time * points[i-1].frequency + points[i-1].phase);
                var x = i == 0 ? 0 : points[i-1].amplitude * Mathf.Cos(Time.time * points[i-1].frequency + points[i-1].phase);
                
                arrowPositions[i] = i == 0 ? Vector3.zero : arrowPositions[i - 1] + new Vector3(x,y,0);
            }
            
            for (var i = 0; i < points.Count; i++)
            {
                var toVector = new Vector3(Mathf.Cos(Time.time * points[i].frequency),
                    Mathf.Sin(Time.time * points[i].frequency), 0);
                
                var rotation = Quaternion.FromToRotation(Vector3.right,toVector);
                
                Handles.ArrowHandleCap(0, arrowPositions[i], rotation * Quaternion.Euler(0,90,0), points[i].amplitude, EventType.Repaint);
            }

            var lastPoint = points[points.Count - 1];
            var lastArrow = arrowPositions.Last();
            
            var yf = lastPoint.amplitude * Mathf.Sin(Time.time * lastPoint.frequency + lastPoint.phase);
            var xf = lastPoint.amplitude * Mathf.Cos(Time.time * lastPoint.frequency + lastPoint.phase);
            
            plotted.Add(new Vector3(0,lastArrow.y,0) + new Vector3(0, yf, 0));
            
            if(plotted.Count > 500)
                plotted.RemoveAt(0);

            for (var i = 0; i < plotted.Count; i++)
            {
                var vector3 = plotted[i];
                vector3.x += viewScale;
                plotted[i] = vector3;
                
                Gizmos.DrawLine(plotted[i] + Vector3.right, (i == 0 ? plotted[i] : plotted[i-1]) + Vector3.right);
                
                // Handles.SphereHandleCap(0, vector3 + Vector3.right * 2, Quaternion.identity, .1f, EventType.Repaint);
            }
            
            foreach (var point in plotted)
            {
                // point.x += viewScale;
            }
            

        }
        else{
            Gizmos.color = Color.yellow;
            List<Vector3> pointsSummed = new List<Vector3>();

            for (float t = -2; t < 2; t += .01f)
            {
                float amplitudeSum = 0;

                foreach (var point in points)
                {
                    amplitudeSum +=
                        f(new Wave { amplitude = point.amplitude, frequency = point.frequency, phase = point.phase },
                            t);
                }

                pointsSummed.Add(new Vector3(t, amplitudeSum + 3, .5f));
            }

            foreach (var vector3 in pointsSummed)
            {
                Gizmos.DrawSphere(vector3, .1f);
            }


            points.ForEach(p =>
            {
                var pos = new Vector3(p.frequency, p.amplitude, p.phase);
                Handles.zTest = CompareFunction.Always;
                Handles.color = Color.green;
                Gizmos.color = new Color(0, 1, 0, .25f);
                Handles.SphereHandleCap(0, pos, Quaternion.identity, .1f, EventType.Repaint);
                Handles.Label(pos + Vector3.up * .15f, $"F = {p.frequency} - A = {p.amplitude}");
                Gizmos.DrawLine(pos, new Vector3(pos.x, 0, pos.z));
                Gizmos.DrawLine(pos, new Vector3(0, pos.y, pos.z));
            });
        }
    }
}