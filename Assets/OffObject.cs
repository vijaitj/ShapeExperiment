using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class OffObject
{
    public Vector3[] norms;
    public Vector3[] face_norms;
    public Vector3 centre;

    //Temporary, to calculate the projected area
    public List<int> polygon;
    public List<Vector2> all_verts;

    // Drawing the object
    public double scale;
    public Vector3 translate;
    public Quaternion rotation;

    public Vector3[] verts;
    public Vector3[] faces;
    public Vector2[] edges;
    int ttt = 1;


    public OffObject()
    {
        scale = 1.0f;
        translate = new Vector3(0.0f, 0.0f, 0.0f);
        rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        List<Vector2> all_verts = new List<Vector2>();
        List<int> polygon = new List<int>();
        verts = new Vector3[16];
        faces = new Vector3[28];
        face_norms = new Vector3[28];
        norms = new Vector3[28];
        edges = new Vector2[28];
        clear();
    }

    public void InitializeVertices(Vector3[] halfVertices, Vector3 reflectionNormals)
    {
        for (uint i = 0; i < 8; ++i)
        {
            verts[i] = new Vector3(halfVertices[i].x, halfVertices[i].y, halfVertices[i].z); // 0-7
                                                                                             // 8-15 are reflections
            verts[i + 8] = Vector3.Reflect(verts[i], reflectionNormals);
        }
    }

    public void InitializeFaces()
    {
        uint pos = 0;
        // Faces on the side 0-5: 0---1---2---3
        //                        |   |   |   |
        //                        4---5---6---7
        faces[pos++] = new Vector3(0, 1, 4);
        faces[pos++] = new Vector3(1, 5, 4);
        faces[pos++] = new Vector3(1, 2, 5);
        faces[pos++] = new Vector3(2, 6, 5);
        faces[pos++] = new Vector3(2, 3, 6);
        faces[pos++] = new Vector3(3, 7, 6);

        // Reflected faces:  8---9--10--11
        //                   |   |   |   |
        //                  12--13--14--15
        faces[pos++] = new Vector3(12, 9, 8);
        faces[pos++] = new Vector3(13, 9, 12);
        faces[pos++] = new Vector3(13, 10, 9);
        faces[pos++] = new Vector3(14, 10, 13);
        faces[pos++] = new Vector3(14, 11, 10);
        faces[pos++] = new Vector3(15, 11, 14);
        //old, wrong direction
        //        for(uint i = 0; i < 6; ++i)
        //            faces[pos++] = faces[i] + Point3(8, 8, 8);

        // Faces on the base 12-17: 0--1---2---3
        //                          |  |   |   |
        //                          8--9--10--11
        faces[pos++] = new Vector3(8, 1, 0);
        faces[pos++] = new Vector3(9, 1, 8);
        faces[pos++] = new Vector3(9, 2, 1);
        faces[pos++] = new Vector3(10, 2, 9);
        faces[pos++] = new Vector3(10, 3, 2);
        faces[pos++] = new Vector3(11, 3, 10);

        // Faces on the top 18-23:  4---5---6---7
        //                          |   |   |   |
        //                         12--13--14--15
        //for(uint i = 0; i < 6; ++i)
        faces[pos++] = new Vector3(4, 5, 12);
        faces[pos++] = new Vector3(5, 13, 12);
        faces[pos++] = new Vector3(5, 6, 13);
        faces[pos++] = new Vector3(6, 14, 13);
        faces[pos++] = new Vector3(6, 7, 14);
        faces[pos++] = new Vector3(7, 15, 14);

        // Faces on the top-quad: 0 -- 8
        //                        |    |
        //                        4 --12
        faces[pos++] = new Vector3(4, 8, 0);
        faces[pos++] = new Vector3(12, 8, 4);

        // Faces on the top-quad: 11 --3
        //                        |    |
        //                       15 -- 7
        faces[pos++] = new Vector3(15, 3, 11);
        faces[pos++] = new Vector3(7, 3, 15);
    }

    public void InitializeEdges()
    {
        uint pos = 0;
        // Edges on the side 0-5: 0---1---2---3
        //                        |   |   |   |
        //                        4---5---6---7
        edges[pos++] = new Vector2(0, 1); edges[pos++] = new Vector2(4, 5);
        edges[pos++] = new Vector2(1, 2); edges[pos++] = new Vector2(5, 6);
        edges[pos++] = new Vector2(2, 3); edges[pos++] = new Vector2(6, 7);
        edges[pos++] = new Vector2(0, 4);
        edges[pos++] = new Vector2(1, 5);
        edges[pos++] = new Vector2(2, 6);
        edges[pos++] = new Vector2(3, 7);

        // Reflected edges:  8---9--10--11
        //                   |   |   |   |
        //                  12--13--14--15
        for (uint i = 0; i < 10; ++i)
            edges[pos++] = edges[i] + new Vector2(8, 8);

        // Joining  0--1---2---3
        //          |  |   |   |
        //          8--9--10--11
        for (uint i = 0; i < 4; ++i)
            edges[pos++] = new Vector2(i, i + 8);

        // Joining  4---5---6---7
        //          |   |   |   |
        //         12--13--14--15
        for (uint i = 0; i < 4; ++i)
            edges[pos++] = new Vector2(i + 4, i + 12);
    }

    public void clear()
    {
        verts.Populate(new Vector3(0.0f, 0.0f, 0.0f));
        norms.Populate(new Vector3(0.0f, 0.0f, 0.0f));
        face_norms.Populate(new Vector3(0.0f, 0.0f, 0.0f));
        faces.Populate(new Vector3(0.0f, 0.0f, 0.0f));
        edges.Populate(new Vector3(0.0f, 0.0f, 0.0f));
        scale = 0.5f;
        translate = new Vector3(0.0f, 0.0f, 0.0f);
        rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
    }

    public void recalc_centre_norms_and_face_norms()
    {
        // -- * -- face-normals -- * -- //
        for (int i = 0; i < faces.Length; ++i)
        {
            Vector3 v1 = verts[(int)faces[i].x];
            Vector3 v2 = verts[(int)faces[i].y];
            Vector3 v3 = verts[(int)faces[i].z];
            face_norms[i] = Vector3.Cross(v2 - v1, v3 - v2);
            face_norms[i].Normalize();
        }

        // -- * -- norms -- * -- //
        // Make sure all data is defined in 'norms'
        norms.Populate(new Vector3(0, 0, 0));

        for (int i = 0; i < faces.Length; ++i)
            for (int j = 0; j < 3; ++j)
                norms[(int)faces[i][j]] += face_norms[i];

        foreach (Vector3 n in norms)
            n.Normalize();

        // -- * -- Get Object Centre -- * -- //
        centre = new Vector3(0, 0, 0);
        foreach (Vector3 v in verts)
            centre += v;
        centre /= Convert.ToSingle(verts.Length);
    }

    public void scale_object(float scl)
    {
        translate_object(-centre);
        for (int i = 0; i < verts.Length; ++i)
        {
            verts[i] = scl * verts[i]; ;
        }
        translate_object(centre);
        recalc_centre_norms_and_face_norms();
    }

    public void translate_object(Vector3 vc)
    {
        for (int i = 0; i < verts.Length; ++i)
        {
            verts[i] = vc + verts[i]; ;
        }
        recalc_centre_norms_and_face_norms();
    }

    public float avg_dist_from_centre()
    {
        float dst = 0.0f;
        for (int i = 0; i < verts.Length; ++i)
        {
            dst += (verts[i] - centre).magnitude;
        }
        return dst/ verts.Length;
    }
    public float max_dist_from_centre()
    {
        float dst = 0.0f;
        for (int i = 0; i < verts.Length; ++i)
        {
            float tmp = (verts[i] - centre).magnitude;
            if (tmp > dst)
                dst = tmp;
        }
        return dst;
    }
    public float max_dist_btw_verts()
    {
        float dst = 0.0f;
        for (int i = 0; i < verts.Length; i++)
        {
            for (int j = i+1; j < verts.Length; j++)
            {
                float tmp = (verts[i] - verts[j]).magnitude;
                if (tmp > dst)
                    dst = tmp;
            }
        }
        return dst;
    }
    //public float max_image_span()
    //{
    //    float dst = 0.0f;
    //    for (int i = 0; i < verts.Length; ++i)
    //    {
    //        Vector2 tmpi = new Vector2(verts[i].x, verts[i].y);
    //        for (int j = i + 1; j < verts.Length; ++j)
    //        {
    //            Vector2 tmpj = new Vector2(verts[j].x, verts[j].y);
    //            float tmp = (tmpj - tmpi).magnitude;
    //            if (tmp > dst)
    //                dst = tmp;
    //        }
    //    }
    //    return dst;
    //}
    public double max_angular_span()
    {
        double ang = 0.0f;
        for (int i = 0; i < verts.Length; ++i)
        {
            for (int j = i + 1; j < verts.Length; ++j)
            {
                Vector3 tmpi = new Vector3(verts[i].x, verts[i].y, -verts[i].z);
                Vector3 tmpj = new Vector3(verts[j].x, verts[j].y, -verts[j].z);

                double  tmp = (tmpi.x * tmpj.x + tmpi.y * tmpj.y  + tmpi.z * tmpj.z)/(tmpi.magnitude * tmpj.magnitude);
                tmp = Math.Acos(tmp);
                if (tmp > ang)
                    ang = tmp;
            }
        }
        return (180*ang)/Math.PI;
    }
    public void modify_object(Matrix4x4 m, float magn)
    {
        translate_object(-centre);
        for (int i = 0; i < verts.Length; ++i)
        {
            //Vector3 ret = mat * to_vector3d(x);
            Vector3 tmp = m.MultiplyVector(verts[i]);
            tmp[0] *= magn;
            verts[i] = m.transpose.MultiplyVector(tmp);

        }
        translate_object(centre);
        recalc_centre_norms_and_face_norms();
        //we want the object to have origin as centroid at the beginning
        translate_object(centre);
    }

    public void rotate_object(Matrix4x4 m)
    {
        translate_object(-centre);
        for (int i = 0; i < verts.Length; ++i)
        {
            verts[i] = m.MultiplyVector(verts[i]);
        }
        translate_object(centre);
        recalc_centre_norms_and_face_norms();
    }

    public void rotate_object(Quaternion q)
    {
        translate_object(-centre);
        // Extract the vector part of the quaternion
        Vector3 u = new Vector3(q.x, q.y, q.z);

        // Extract the scalar part of the quaternion
        float s = q.w;

        for (int i = 0; i < verts.Length; ++i)
        {
            Vector3 v = verts[i];
            Vector3 vprime = 2.0f * Vector3.Dot(u, v) * u + (s * s - Vector3.Dot(u, u)) * v + 2.0f * s * Vector3.Cross(u, v);
            verts[i] = vprime;
        }
        translate_object(centre);
        recalc_centre_norms_and_face_norms();
    }

    public void copy_properties_from(OffObject orig)
    {
        //Copy vertices
        int ln = orig.verts.Length;
        verts = new Vector3[ln];
        for (int i = 0; i < ln; i++)
        {
            verts[i] = new Vector3(orig.verts[i].x, orig.verts[i].y, orig.verts[i].z);
        }
        //Copy faces (mapping)
        ln = orig.faces.Length;
        faces = new Vector3[ln];
        for (int i = 0; i < ln; i++)
        {
            faces[i] = new Vector3(orig.faces[i].x, orig.faces[i].y, orig.faces[i].z);
        }
        //Copy edges
        ln = orig.edges.Length;
        edges = new Vector2[ln];
        for (int i = 0; i < ln; i++)
        {
            edges[i] = new Vector2(orig.edges[i].x, orig.edges[i].y);
        }
        //Copy norms
        ln = orig.norms.Length;
        norms = new Vector3[ln];
        for (int i = 0; i < ln; i++)
        {
            norms[i] = new Vector3(orig.norms[i].x, orig.norms[i].y, orig.norms[i].z);
        }
        //Copy face-norms
        ln = orig.face_norms.Length;
        face_norms = new Vector3[ln];
        for (int i = 0; i < ln; i++)
        {
            face_norms[i] = new Vector3(orig.face_norms[i].x, orig.face_norms[i].y, orig.face_norms[i].z);
        }
        centre = new Vector3(orig.centre.x, orig.centre.y, orig.centre.z);
        scale = orig.scale;
        rotation = new Quaternion(orig.rotation.x, orig.rotation.y, orig.rotation.z, orig.rotation.w);
        translate = new Vector3(orig.translate.x, orig.translate.y, orig.translate.z);
    }

    List<int> find_attached_edges(int edg_num)
    {
        List<int> ret = new List<int>();
        int tot_edges = edges.Length;
        for (int i = 0; i < tot_edges; ++i)
        {
            Vector2 e = edges[i];
            if (e[0] == edg_num || e[1] == edg_num)
                ret.Add(i);
        }
        return ret;
    }

    double edge_angle_dif(int i, int p, int q)
    {
        Vector3 p1, p2, p3, p4, v1, v2;
        //The current vertex being considered
        Vector3 base1 = verts[i];
        Vector3 base2 = verts[(i + 8) % 16];

        if (edges[p][0] != i)
        {
            p1 = verts[(int)edges[p][0]];
            int tmp = ((int)edges[p][0] + 8) % 16;
            p3 = verts[tmp];
        }
        else
        {
            p1 = verts[(int)edges[p][1]];
            int tmp = ((int)edges[p][1] + 8) % 16;
            p3 = verts[tmp];
        }

        if (edges[q][0] != i)
        {
            p2 = verts[(int)edges[q][0]];
            int tmp = ((int)edges[q][0] + 8) % 16;
            p4 = verts[tmp];
        }
        else
        {
            p2 = verts[(int)edges[q][1]];
            int tmp = ((int)edges[q][1] + 8) % 16;
            p4 = verts[tmp];
        }

        v1 = (p1 - base1).normalized;
        v2 = (p2 - base1).normalized;
        double cos_tht1 = Vector3.Dot(v1, v2);
        double tht1 = Math.Acos(cos_tht1);

        v1 = (p3 - base2).normalized;
        v2 = (p4 - base2).normalized;
        double cos_tht2 = Vector3.Dot(v1, v2);
        double tht2 = Math.Acos(cos_tht2);

        return Math.Abs((tht1 - tht2));
    }

    double edge_angle_dif(int i, int p, int q, OffObject obj2)
    {
        Vector3 p11, p12, p21, p22, v1, v2;
        //The current vertex being considered
        Vector3 base1 = verts[i];
        Vector3 base2 = obj2.verts[i];

        if (edges[p][0] != i)
        {
            p11 = verts[(int)edges[p][0]];
            p21 = obj2.verts[(int)edges[p][0]];
        }
        else
        {
            p11 = verts[(int)edges[p][1]];
            p21 = obj2.verts[(int)edges[p][1]];
        }

        if (edges[q][0] != i)
        {
            p12 = verts[(int)edges[q][0]];
            p22 = obj2.verts[(int)edges[q][0]];
        }
        else
        {
            p12 = verts[(int)edges[q][1]];
            p22 = obj2.verts[(int)edges[q][1]];
        }

        v1 = (p11 - base1).normalized;
        v2 = (p12 - base1).normalized;
        double cos_tht1 = Vector3.Dot(v1, v2);
        double tht1 = Math.Acos(cos_tht1);

        v1 = (p21 - base2).normalized;
        v2 = (p22 - base2).normalized;
        double cos_tht2 = Vector3.Dot(v1, v2);
        double tht2 = Math.Acos(cos_tht2);

        return Math.Abs((tht1 - tht2));
    }

    public double degree_of_asymmetry()
    {
        double ang_df = 0;
        int cntr = 0;
        for (int i = 0; i < 8; i++)
        {
            List<int> edg_lst = find_attached_edges(i);
            //for each pair of edges connected to vertex base
            for (int p = 0; p < edg_lst.Count; p++)
                for (int q = p + 1; q < edg_lst.Count; q++)
                {
                    ang_df += edge_angle_dif(i, edg_lst[p], edg_lst[q]);
                    cntr++;
                }
        }
        return ang_df / (cntr * Math.PI);
    }

    public double degree_of_asymmetry(OffObject obj2)
    {
        double ang_df = 0;
        int cntr = 0;
        for (int i = 0; i < 16; i++)
        {
            List<int> edg_lst = find_attached_edges(i);
            //for each pair of edges connected to vertex base
            for (int p = 0; p < edg_lst.Count; p++)
                for (int q = p + 1; q < edg_lst.Count; q++)
                {
                    ang_df += edge_angle_dif(i, edg_lst[p], edg_lst[q],obj2);
                    cntr++;
                }
        }
        return ang_df / (cntr * Math.PI);
    }

    double signed_volume_triangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    public double volume()
    {
        int face_num = faces.Length;
        double vlm = 0.0;
        for (int i = 0; i < face_num; ++i)
        {
            Vector3 v1 = verts[(int)faces[i].x];
            Vector3 v2 = verts[(int)faces[i].y];
            Vector3 v3 = verts[(int)faces[i].z];
            vlm += signed_volume_triangle(v1, v2, v3);
        }
        return Math.Abs(vlm);
    }

    public double area()
    {
        int face_num = faces.Length;
        double ar = 0.0;
        for (int i = 0; i < face_num; ++i)
        {
            Vector3 v1 = verts[(int)faces[i].x];
            Vector3 v2 = verts[(int)faces[i].y];
            Vector3 v3 = verts[(int)faces[i].z];
            Vector3 vd1 = v1 - v2;
            Vector3 vd2 = v3 - v2;
            ar += (Vector3.Cross(vd1, vd2).magnitude) / 2.0;
        }
        return ar;
    }

    public double compactess()
    {
       return 36 * Math.PI * Math.Pow(volume(), 2) / Math.Pow(area(), 3);
    }

    float two_d_cross(Vector2 v, Vector2 w)
    {
        return v.x * w.y - v.y * w.x;
    }

    public float projected_area()
    {
        /******* Find additional edges formed by intersection of 2 existing edges *****/
        // http://stackoverflow.com/questions/563198/
        // "Intersection of two lines in three-space" by Ronald Goldman,
        // published in Graphics Gems, page 304

        List<Vector2> rmv_verts = new List<Vector2>();
        int tmpCntr;
        for (int i = 0; i < 16; i++)
        {
            tmpCntr = 0;
            Vector3 cv = verts[i];
            for (int j = 0; j < faces.Length; j++)
            {
                //if the trg contains this vertex
                if (faces[j].x == i || faces[j].y == i || faces[j].z == i)
                {
                    tmpCntr++;
                    continue;
                }
                Vector3[] trg = { verts[(int)faces[j].x], verts[(int)faces[j].y], verts[(int)faces[j].z] };
                if (Helper.is_vertex_occluded(trg, cv))
                {
                    break;
                }
                tmpCntr++;
            }
            // if none of the faces block it
            if (tmpCntr++ >= faces.Length)
                rmv_verts.Add(new Vector2(verts[i].x, verts[i].y));
        }
        for (int i = 0; i < verts.Length; i++)
        {
            all_verts.Add(new Vector2(verts[i].x, verts[i].y));
        }
        List<Vector2> all_edges = new List<Vector2>(edges);


        for (int i = 0; i < edges.Length; i++)
        {
            Vector2 ei = edges[i];
            Vector2 p = all_verts[(int)ei[0]];
            Vector2 r = all_verts[(int)ei[1]] - p;
            for (int j = 0; j < edges.Length; j++)
            {
                Vector2 ej = edges[j];
                Vector2 q = all_verts[(int)ej[0]];
                Vector2 s = all_verts[(int)ej[1]] - q;
                float numer = two_d_cross(q - p, s);
                float denom = two_d_cross(r, s);
                // if parallel or collinear
                if (denom == 0)
                    continue;
                float t = numer / denom;
                numer = two_d_cross(q - p, r);
                float u = numer / denom;
                // if the segments intersect
                if (t > 0 && t < 1 && u > 0 && u < 1)
                {
                    // add the intersection point to the vertices list
                    all_verts.Add(p + t * r);
                    // add the new edges info
                    all_edges.Add(new Vector2(ei[0], all_verts.Count - 1));
                    all_edges.Add(new Vector2(ei[1], all_verts.Count - 1));
                    all_edges.Add(new Vector2(ej[0], all_verts.Count - 1));
                    all_edges.Add(new Vector2(ej[1], all_verts.Count - 1));
                }
            }
        }
        /******* create the adjacency matrix ******/
        int[,] adj = new int[all_verts.Count, all_verts.Count];
        for (int i = 0; i < all_edges.Count; i++)
        {
            Vector2 ei = all_edges[i];
            adj[(int)ei[0], (int)ei[1]] = 1;
            adj[(int)ei[1], (int)ei[0]] = 1;
        }
        /******* find the rightmost vertex ******/
        Vector2 rt = all_verts[0];
        int init = 0;
        for (int i = 1; i < all_verts.Count; i++)
        {
            if (rt.x < all_verts[i].x)
            {
                rt = all_verts[i];
                init = i;
            }
        }
        /******* trace the polygon starting from the right most ******/
        int crnt = init;
        int prev = init;
        int cnt = 0;
        //std::vector<int> polygon;
        polygon.Clear();
        polygon.Add(init);
        do
        {
            Vector2 cv = all_verts[crnt];
            Vector2 ref_dir;
            //if its the first vertex the angle is calculated wrt x-axis
            if (cnt == 0)
            {
                ref_dir = new Vector2(1, 0);
            }
            //else wrt to the direction of the path being traced
            else
            {
                ref_dir = all_verts[prev] - all_verts[crnt];
            }
            ref_dir.Normalize();
            // traverse the row
            int mn = 0;
            double mn_ang = 361, mn_dst = 1e7f;
            for (int i = 0; i < all_verts.Count; i++)
            {
                if (adj[crnt, i] == 1)
                {
                    Vector2 tmpv = all_verts[i];
                    Vector2 dir = tmpv - cv;
                    float dst = dir.magnitude;
                    //normalize
                    dir.Normalize();
                    double dt = Vector2.Dot(dir, ref_dir);
                    double cr = dir.x * ref_dir.y - dir.y * ref_dir.x;
                    double ang = Math.Atan2(cr, dt);
                    if (ang < 0)
                        ang = Math.PI + (Math.PI + ang);
                    //
                    ang = 2 * Math.PI - ang;
                    //no going back please
                    if (Math.Abs(ang) < 1e-1)
                        ang = 2.0 * Math.PI;
                    if (Math.Abs(ang - mn_ang) < 1e-1)
                    {
                        if (dst < mn_dst)
                        {
                            mn_ang = ang;
                            mn = i;
                            mn_dst = dst;
                        }
                    }
                    else if (ang < mn_ang)
                    {
                        mn_ang = ang;
                        mn = i;
                        mn_dst = dst;
                    }

                }
            }
            polygon.Add(mn);
            prev = crnt;
            crnt = mn;
            Console.WriteLine("Angle: " + (mn_ang * (180 / Math.PI)).ToString());
            cnt++;
        } while (crnt != init && cnt < ttt);
        if (cnt >= 1000)
            Console.WriteLine("Exited without completing polygon");
        return 0.0f;
    }

    public float proj_area()
    {
        int face_num = faces.Length;
        float ar = 0.0f;
        float fct = 1e6f;
        for (int i = 0; i < face_num; ++i)
        {
            Vector3 v1 = verts[(int)faces[i].x];
            Vector3 v2 = verts[(int)faces[i].y];
            Vector3 v3 = verts[(int)faces[i].z];
            v1.z = v1.z / fct;
            v2.z = v2.z / fct;
            v3.z = v3.z / fct;
            Vector3 vd1 = v1 - v2;
            Vector3 vd2 = v3 - v2;
            ar += Vector3.Cross(vd1, vd2).magnitude / 2.0f;
        }
        return ar;
    }

    public int write_metrics_to_file(string file_name)
    {
        using (StreamWriter sw = new StreamWriter(file_name, true))
        {
            sw.WriteLine(degree_of_asymmetry().ToString() + "," + volume().ToString() + "," + area().ToString());
        }
        return 0;
    }
    public int write_metrics_to_file(string file_name, bool append)
    {
        using (StreamWriter sw = new StreamWriter(file_name, append))
        {
            sw.WriteLine(degree_of_asymmetry().ToString() + "," + volume().ToString() + "," + area().ToString());
        }
        return 0;
    }
    public int write_metrics_to_file(string file_name, int ctg, double stretch_ang, double view_ang)
    {
        using (StreamWriter sw = new StreamWriter(file_name, true))
        {
            sw.WriteLine(degree_of_asymmetry().ToString() + "," + volume().ToString() + "," + area().ToString() + "," + ctg.ToString() + "," + stretch_ang.ToString() + "," + view_ang.ToString());
        }
        return 0;
    }

    public void write_tranform_params(string file_name)
    {
        using (StreamWriter sw = new StreamWriter(file_name, true))
        {
            sw.WriteLine(rotation.x.ToString() + "," + rotation.y.ToString() + "," + rotation.z.ToString() + "," + rotation.w.ToString());
            sw.WriteLine(translate.x.ToString() + "," + translate.y.ToString() + "," + translate.z.ToString());
            sw.WriteLine(scale.ToString());
        }
    }

    public void set_trfm_prms_from_file(string file_name)
    {
        string line = "";
        using (StreamReader sr = new StreamReader(file_name))
        {
            int cntr = 0;
            while ((line = sr.ReadLine()) != null)
            {
                string[] items = line.Split('\t');
                if (cntr == 0)
                {
                    rotation = new Quaternion(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]), Convert.ToSingle(items[3]));
                }
                else if(cntr == 1)
                {
                    translate = new Vector3(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]));
                }
                else if(cntr == 2)
                {
                    scale = Convert.ToSingle(items[0]);
                }
                cntr++;
            }
        }
    }

    public void save(string file_name)
    {
        AppData app = AppData.Instance;
        using (StreamWriter sw = new StreamWriter(file_name))
        {
            foreach (Vector3 v in verts)
            {
                sw.WriteLine(v.x.ToString() + "\t" + v.y.ToString() + "\t" + v.z.ToString());
            }
            sw.WriteLine(rotation.x.ToString() + "\t" + rotation.y.ToString() + "\t" + rotation.z.ToString() + "\t" + rotation.w.ToString());
            sw.WriteLine(translate.x.ToString() + "\t" + translate.y.ToString() + "\t" + translate.z.ToString());
            sw.WriteLine(scale.ToString());

        }
    }
    public void load(string file_name)
    {
        AppData app = AppData.Instance;
        string line = "";
        using (StreamReader sr = new StreamReader(file_name))
        {
            int cntr = 0;

            while ((line = sr.ReadLine()) != null)
            {
                string[] items = line.Split('\t');
                if (cntr < 16)
                {
                    verts[cntr] = new Vector3(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]));
                }
                if (cntr == 16)
                {
                    rotation = new Quaternion(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]), Convert.ToSingle(items[3]));
                }
                else if (cntr == 17)
                {
                    translate = new Vector3(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]));
                }
                else if (cntr == 18)
                {
                    scale = Convert.ToSingle(items[0]);
                }
                cntr++;
            }
        }

    }

}