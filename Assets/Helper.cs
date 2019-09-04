using UnityEngine;
using System;
using System.Collections;
using System.IO;

public static class Helper
{
    private static AppData app = AppData.Instance;
    static System.Random random = new System.Random();
    public static void printApp()
    {
        Debug.Log("Here is the Value");
        Debug.Log(app.m.stretch_magn);
    }
    public static double get_random_double(double minimum, double maximum)
    {
        //random = new System.Random();
        return random.NextDouble() * (maximum - minimum) + minimum;
    }
    public static Matrix4x4 get_rot_X(double theta)
    {
        // Convert to radians
        theta *= (Math.PI / 180);

        Matrix4x4 ret = Matrix4x4.identity;

        ret[0, 0] = 1; ret[0, 1] = 0; ret[0, 2] = 0;
        ret[1, 0] = 0; ret[1, 1] = (float)Math.Cos(theta); ret[1, 2] = (float)-Math.Sin(theta);
        ret[2, 0] = 0; ret[2, 1] = (float)Math.Sin(theta); ret[2, 2] = (float)Math.Cos(theta);

        return ret;
    }

    public static Matrix4x4 get_rot_Y(double theta)
    {
        // Convert to radians
        theta *= (Math.PI / 180);

        Matrix4x4 ret = Matrix4x4.identity;

        ret[0, 0] = (float)Math.Cos(theta); ret[0, 1] = 0; ret[0, 2] = (float)Math.Sin(theta);
        ret[1, 0] = 0; ret[1, 1] = 1; ret[1, 2] = 0;
        ret[2, 0] = (float)-Math.Sin(theta); ret[2, 1] = 0; ret[2, 2] = (float)Math.Cos(theta);

        return ret;
    }

    public static Matrix4x4 get_rot_Z(double theta)
    {
        // Convert to radians
        theta *= (Math.PI / 180);

        Matrix4x4 ret = Matrix4x4.identity;

        ret[0, 0] = (float)Math.Cos(theta); ret[0, 1] = (float)-Math.Sin(theta); ret[0, 2] = 0;
        ret[1, 0] = (float)Math.Sin(theta); ret[1, 1] = (float)Math.Cos(theta); ret[1, 2] = 0;
        ret[2, 0] = 0; ret[2, 1] = 0; ret[2, 2] = 1;

        return ret;
    }
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }

    public static void DisplayLine(OffObject ofObj, int startAt, GameObject[] lineGameObjz)
    {
        for (int i = 0; i < ofObj.edges.Length; i++)
        {
            var line = lineGameObjz[i + startAt].GetComponent<LineRenderer>();
            Vector3 stPnt = ofObj.verts[(int)ofObj.edges[i].x];
            Vector3 endPnt = ofObj.verts[(int)ofObj.edges[i].y];

            stPnt += app.wire_frm_dst * (stPnt - ofObj.centre).normalized;// + ofObj.centre;
            endPnt += app.wire_frm_dst * (endPnt - ofObj.centre).normalized;// + ofObj.centre;
            // Number of vertices of the line, we need two vertices for drawing the edge.
            line.SetVertexCount(2);
            // Setting up the line and displaying it.
            line.SetPosition(0, stPnt);
            line.SetPosition(1, endPnt);
            line.SetWidth(app.line_width, app.line_width);
            line.material = new Material(Shader.Find("Particles/Additive"));
            line.SetColors(app.edge_clr, app.edge_clr);
            //line.useWorldSpace = true;
            line.useWorldSpace = false;
        }
    }
    public static void DisplayLine(Vector3[] vertz, GameObject[] lineGameObjz, Vector2[] edges, Vector3 centre)
    {
        for (int i = 0; i < edges.Length; i++)
        {
            var line = lineGameObjz[i].GetComponent<LineRenderer>();
            Vector3 stPnt = vertz[(int)edges[i].x];
            Vector3 endPnt = vertz[(int)edges[i].y];

            stPnt += app.wire_frm_dst * (stPnt - centre).normalized;// + ofObj.centre;
            endPnt += app.wire_frm_dst * (endPnt - centre).normalized;// + ofObj.centre;
            // Number of vertices of the line, we need two vertices for drawing the edge.
            line.SetVertexCount(2);
            // Setting up the line and displaying it.
            line.SetPosition(0, stPnt);
            line.SetPosition(1, endPnt);
            line.SetWidth(app.line_width, app.line_width);
            line.material = new Material(Shader.Find("Particles/Additive"));
            line.SetColors(app.edge_clr, app.edge_clr);
            //line.useWorldSpace = true;
        }
    }
    public static Vector3 GetCentroid(Vector3[] vertz)
    {
        Vector3 ret = new Vector3(0.0f, 0.0f, 0.0f);
        for (int i = 0; i < vertz.Length; i++)
        {
            ret += vertz[i];
        }
        return ret / vertz.Length;
    }
    public static int[] CreateTriangleArray(OffObject ofObj)
    {
        // Triangles
        // tri is an array required by the Mesh of MeshFilter.
        int tot_trgs = ofObj.faces.Length;
        int[] tri = new int[3 * tot_trgs];

        // Storing how each triangle will be drawn.
        for (int i = 0; i < tot_trgs; i++)
        {
            tri[3 * i] = (int)ofObj.faces[i].x;
            tri[3 * i + 1] = (int)ofObj.faces[i].y;
            tri[3 * i + 2] = (int)ofObj.faces[i].z;
        }
        return tri;
    }
    public static Vector3[] TransformVertices(Vector3[] vertz, Matrix4x4 tMat)
    {
        Vector3[] ret = new Vector3[vertz.Length];
        for(int i =0; i < vertz.Length; i++)
        {
            ret[i] = tMat.MultiplyPoint3x4(vertz[i]);
        }
        return ret;
    }

    // Compute barycentric coordinates (u, v, w) for
    // point p with respect to triangle (a, b, c)
    // Transcribed from Christer Ericson's Real-Time Collision Detection
    // http://gamedev.stackexchange.com/questions/23743/
    public static void barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float u, out float v, out float w)
    {
        Vector3 v0 = b - a, v1 = c - a, v2 = p - a;
        float d00 = Vector3.Dot(v0,v0);
        float d01 = Vector3.Dot(v0,v1);
        float d11 = Vector3.Dot(v1,v1);
        float d20 = Vector3.Dot(v2,v0);
        float d21 = Vector3.Dot(v2,v1);
        float denom = d00 * d11 - d01 * d01;
        v = (d11 * d20 - d01 * d21) / denom;
        w = (d00 * d21 - d01 * d20) / denom;
        u = 1.0f - v - w;
    }
    //Refer:https://www.cs.princeton.edu/courses/archive/fall00/cs426/lectures/raycast/sld017.htm
    //Ray: p = p0 + t*v, p represents a point on the ray, and t is a scalar
    //Plnane: n.p + d = 0, p represents a point on the plane
    public static Vector4 ray_plane_intersection(Vector4 pln, Vector3 v, Vector3 p0)
    {
        Vector3 n = new Vector3(pln[0], pln[1], pln[2] );
        float d = pln[3];
        float t = -(Vector3.Dot(n,p0) + d) / (Vector3.Dot(v,n));
        Vector3 intr = p0 + t * v;
        Vector4 ret = new Vector4(intr[0], intr[1], intr[2], t );
        return ret;
    }
    public static bool is_vertex_occluded(Vector3[] trg, Vector3 vc)
    {
        //find the plane representing the triangle
        Vector3 v1 = trg[1] - trg[0];
        Vector3 v2 = trg[2] - trg[0];
        Vector3 n = Vector3.Cross(v1, v2);
        n = n.normalized;
        float d = Vector3.Dot(n,trg[0]);
        Vector4 pln = new Vector4(n[0],n[1],n[2], -d );
        //find ray-plane intersection
        //Vector3 p0 = new Vector3(vc.x, vc.y, 0);
        Vector3 p0 = new Vector3(0, 0, 0);
        Vector3 v = vc.normalized;
        //Vector3 v = new Vector3(0, 0, -1 );
        Vector4 intr = ray_plane_intersection(pln, v, p0);
        Vector3 pnt = new Vector3(intr[0], intr[1], intr[2] );
        //see if the point on plane is further than the 3D point
        //If yes then that point is in front of the trg and is visible
        if ((vc - p0).sqrMagnitude < (pnt - p0).sqrMagnitude)
            return false;
        // see if the point is within the triangle, using barycentric coords
        float alp, bta, gma;
        barycentric(pnt, trg[0], trg[1], trg[2], out alp, out bta, out gma);

        float tol = 0.05f;
        if (alp >= (0 - tol) && alp <= (1 + tol) && bta >= (0 - tol) && bta <= (1 + tol) && gma >= (0 - tol) && gma <= (1 + tol))
            return true;
        else
            return false;
    }

    public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, false);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }

}
