using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class Model
{
    public Vector3[] verts;
    public Vector4 sym;
    //Params for creating modified object
    public Matrix4x4 md_rot_mat;
    public float stretch_magn;
    // Matrix to generate random view, orthographic projection
    // of this random view is shown to the user
    public Quaternion rv_rot_mat;
    //Matrix that the user modifies
    public Matrix4x4 usr_mat;
    //Position of Stationary stretched object
    public Vector3 pos;
    //Position of Rotating object
    public Vector3 delta_pos;
    public float scale;
    public float fct;
    // When vary_cmpct_dir = 0, the objects is tretched by vary_cmpct_fctr along x-axis; and along y-axis when its 1.
    public int vary_cmpct_dir;
    // The factor by which we stretch the object to vary compactness.
    public float vary_cmpct_fctr;

    public Model()
    {
        
        scale = 0.000165f;
        delta_pos = new Vector3(0.35f, 0, 0.0f);
        pos = new Vector3(-0.20f, 0, 0.60f);
        fct = 0.75f;

        usr_mat = Matrix4x4.identity;
        md_rot_mat = Matrix4x4.identity;
        rv_rot_mat = Quaternion.identity;
        stretch_magn = 3.50f;
        verts = new Vector3[8];
        sym = new Vector4(1, 0, 0, 0);
        vary_cmpct_dir = 0;
        vary_cmpct_fctr = 1;
    }
    float r(int a, int b)
    {
        // Swap a and b
        if (b < a)
        {
            int temp = a;
            a = b;
            b = temp;
        }
        return a + UnityEngine.Random.Range(0, int.MaxValue) % (b - a);
    }

    public void regenerate()
    {
        /*
         *    0 -- 1 -- 2 -- 3            // edge to base-plane orthogonal to sym
         *    |    |    |    |            // 3 faces on side of object
         *    4 -- 5 -- 6 -- 7            // edge to 3 faces on top of object
        */

        while (true)
        {
            // The first quad (becomes vertices 0-3)
            Vector3 p1, p4, p5, p6;
            p1.x = r(-110, -10); p1.y = r(0, 100); p1.z = r(-150, -50);
            p4.x = r(-110, -10); p4.y = r(-210, -110); p4.z = r(-150, -50);
            p6.x = r(-110, -10); p6.y = r(0, 100); p6.z = r(-40, 60);

            Vector3 a2n = Vector3.Cross(p4 - p1, p6 - p1);
            float a2Width = -Vector3.Dot(a2n, p1);
            Vector4 a2 = new Vector4(a2n.x, a2n.y, a2n.z, a2Width);

            p5.y = p4.y;
            p5.z = r(-40, 60);
            p5.x = -(a2.w + a2.y * p5.y + a2.z * p5.z) / a2.x;

            // Need to check the reflection.
            Vector3 reflectionNormals = new Vector3(sym.x, sym.y, sym.z);

            //Vector3 v33 = new Vector3(0,1,0);
            //Vector3 pnt1 = new Vector3(-1,1,-1);
            //Vector3 p22 = Vector3.Reflect (pnt1, v33);
            //Debug.Log (p22);

            Vector3 p2 = Vector3.Reflect(p1, reflectionNormals);
            Vector3 p3 = Vector3.Reflect(p4, reflectionNormals);
            Vector3 p7 = Vector3.Reflect(p6, reflectionNormals);
            Vector3 p8 = Vector3.Reflect(p5, reflectionNormals);

            // FATAL CHECK IMPLEMENTATION NEEDS TO GO HERE.

            // Sanity checks on the roughness of the shape
            if (p4.y > p1.y) continue;
            if (p1.x > p2.x) continue;
            if (p4.x > p3.x) continue;
            if (p5.y > p6.y) continue;
            if (p5.z < p4.z) continue;
            if (p6.x > p7.x) continue;
            if (p5.x > p8.x) continue;
            if (p7.z < p2.z) continue;
            if (p8.z < p3.z) continue;
            if (p5.x > 400 || p5.x < -400) continue;

            Vector3 p9, p10, p13, p14;

            p9.x = r(-210, -110);
            p9.y = p5.y;
            p9.z = r(60, 160);

            Vector3 a7n = Vector3.Cross(p6 - p5, p9 - p5);
            float a7Width = -Vector3.Dot(a7n, p5);
            Vector4 a7 = new Vector4(a7n.x, a7n.y, a7n.z, a7Width);

            p10.x = r(-210, -110);
            p10.y = r(110, 210);
            p10.z = -(a7.w + a7.y * p10.y + a7.x * p10.x) / a7.z;

            p14.x = r(-210, -110);
            p14.y = r(110, 210);
            p14.z = r(200, 300);

            Vector3 a11n = Vector3.Cross(p14 - p10, p9 - p10);
            float a11Width = -Vector3.Dot(a11n, p10);
            Vector4 a11 = new Vector4(a11n.x, a11n.y, a11n.z, a11Width);

            p13.z = r(300, 400);
            p13.y = p9.y;
            p13.x = -(a11.w + a11.y * p13.y + a11.z * p13.z) / a11.x;

            Vector3 p11 = Vector3.Reflect(p10, reflectionNormals);
            Vector3 p12 = Vector3.Reflect(p9, reflectionNormals);
            Vector3 p15 = Vector3.Reflect(p14, reflectionNormals);
            Vector3 p16 = Vector3.Reflect(p13, reflectionNormals);

            if (p9.y > p10.y) continue;
            if (p10.x > p11.x) continue;
            if (p9.x > p12.x) continue;
            if (p13.y > p14.y) continue;
            if (p13.z < p9.z) continue;
            if (p14.x > p15.x) continue;
            if (p13.x > p16.x) continue;
            if (p15.z < p11.z) continue;
            if (p16.z < p12.z) continue;
            if (p10.z > 400 || p10.z < -400) continue;
            if (p13.x > 400 || p13.x < -400) continue;

            if (p5.x < p9.x) continue;

            // We've got a good object
            /*
			*    0 -- 1 -- 2 -- 3          // edge to base-plane orthogonal to sym
			*    |    |    |    |          // 3 faces on side of object
			*    4 -- 5 -- 6 -- 7          // edge to 3 faces on top of object
			*/

            uint pos = 0;

            verts[pos++] = new Vector3(p4.x, p4.y, p4.z);
            verts[pos++] = new Vector3(p5.x, p5.y, p5.z);
            verts[pos++] = new Vector3(p9.x, p9.y, p9.z);
            verts[pos++] = new Vector3(p13.x, p13.y, p13.z);
            verts[pos++] = new Vector3(p1.x, p1.y, p1.z);
            verts[pos++] = new Vector3(p6.x, p6.y, p6.z);
            verts[pos++] = new Vector3(p10.x, p10.y, p10.z);
            verts[pos++] = new Vector3(p14.x, p14.y, p14.z);

            break;
        }
    }

    public OffObject generate_off_obj()
    {
        OffObject obj = new OffObject();

        obj.InitializeVertices(verts, sym);
        obj.InitializeFaces();
        obj.InitializeEdges();
        //calling this would automatically compute face norms and norms and centre
        obj.recalc_centre_norms_and_face_norms();
        //we want the object to have origin as centroid at the beginning
        obj.translate_object(obj.centre);
        ////stretch to vary compactness
        //AppData app = AppData.Instance;
        ////If it is a new session we always vary compactness
        //if (!app.is_saved_session)
        //{
        //    vary_cmpct_dir = UnityEngine.Random.Range(0, 2);
        //    vary_cmpct_fctr = (float)Helper.get_random_double(0.2f, 5.0f);
        //    Debug.Log("Current variance along: " + vary_cmpct_dir.ToString() + " By Fct: " + vary_cmpct_fctr.ToString());
        //    //Matrix4x4 tmp = Matrix4x4.identity;
        //    if (vary_cmpct_dir == 1)
        //    {
        //        Matrix4x4 tmp = Helper.get_rot_Y(90);
        //        obj.modify_object(tmp, vary_cmpct_fctr);
        //        tmp = Matrix4x4.identity;
        //        obj.modify_object(tmp, (1 / vary_cmpct_fctr));
        //    }
        //    else
        //    {
        //        Matrix4x4 tmp = Matrix4x4.identity;
        //        obj.modify_object(tmp, vary_cmpct_fctr);
        //        tmp = Helper.get_rot_Y(90);
        //        obj.modify_object(tmp, (1 / vary_cmpct_fctr));
        //    }
        //}
        //If it is a saved session, there are 2 possibilities:
        //1. The old model files are used which does not vary compactness. In this case,
        // The constructor sets the stretch factor to 1 and so no stretch happens.
        //2. The new model files which vary compactness is used. In this case, the load
        // function would load values for vary_cmpct_dir and vary_cmpct_fctr and the obj
        // would be stretched.

        return obj;
    }

    public void clear()
    {
        verts.Populate(new Vector3(0.0f, 0.0f, 0.0f));
        usr_mat = Matrix4x4.identity;
        rv_rot_mat = Quaternion.identity;
        md_rot_mat = Matrix4x4.identity;
        stretch_magn = 0.0f;
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
            sw.WriteLine(md_rot_mat[0, 0].ToString() + "\t" + md_rot_mat[0, 1].ToString() + "\t" + md_rot_mat[0, 2].ToString());
            sw.WriteLine(md_rot_mat[1, 0].ToString() + "\t" + md_rot_mat[1, 1].ToString() + "\t" + md_rot_mat[1, 2].ToString());
            sw.WriteLine(md_rot_mat[2, 0].ToString() + "\t" + md_rot_mat[2, 1].ToString() + "\t" + md_rot_mat[2, 2].ToString());

            sw.WriteLine(rv_rot_mat.x.ToString() + "\t" + rv_rot_mat.y.ToString() + "\t" + rv_rot_mat.z.ToString() + "\t" + rv_rot_mat.w.ToString());

            sw.WriteLine(usr_mat[0, 0].ToString() + "\t" + usr_mat[0, 1].ToString() + "\t" + usr_mat[0, 2].ToString());
            sw.WriteLine(usr_mat[1, 0].ToString() + "\t" + usr_mat[1, 1].ToString() + "\t" + usr_mat[1, 2].ToString());
            sw.WriteLine(usr_mat[2, 0].ToString() + "\t" + usr_mat[2, 1].ToString() + "\t" + usr_mat[2, 2].ToString());

            sw.WriteLine(stretch_magn.ToString());
            sw.WriteLine(pos.x.ToString() + "\t" + pos.y.ToString() + "\t" + pos.z.ToString());
            sw.WriteLine(delta_pos.x.ToString() + "\t" + delta_pos.y.ToString() + "\t" + delta_pos.z.ToString());
            sw.WriteLine(scale.ToString());
            sw.WriteLine(fct.ToString());
            sw.WriteLine("isMonocular\t=\t" + app.isMonocular.ToString());
            sw.WriteLine(vary_cmpct_dir.ToString() + "\t" + vary_cmpct_fctr.ToString());

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
                if (cntr < 8)
                {
                    verts[cntr] = new Vector3(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]));
                }
                if (cntr >= 8 && cntr < 11)
                    md_rot_mat.SetRow(cntr - 8, new Vector4(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]), 0));
                if (cntr == 11)
                    rv_rot_mat = new Quaternion(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]), Convert.ToSingle(items[3]));
                if (cntr >= 12 && cntr < 15)
                    usr_mat.SetRow(cntr - 12, new Vector4(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]), 0));
                if (cntr == 15)
                    stretch_magn = Convert.ToSingle(items[0]);
                if (cntr == 16)
                    pos = new Vector3(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]));
                if (cntr == 17)
                    delta_pos = new Vector3(Convert.ToSingle(items[0]), Convert.ToSingle(items[1]), Convert.ToSingle(items[2]));
                if (cntr == 18)
                    scale = Convert.ToSingle(items[0]);
                if (cntr == 19)
                    fct = Convert.ToSingle(items[0]);
                if (cntr == 20)
                    app.isMonocular = Convert.ToBoolean(items[2]);
                if (cntr == 21)
                {
                    vary_cmpct_dir = Convert.ToInt32(items[0]);
                    vary_cmpct_fctr = Convert.ToSingle(items[1]);
                }
                cntr++;
            }
        }

    }

    public void save_mvls(string file_name)
    {
        using (StreamWriter sw = new StreamWriter(file_name))
        {
            sw.WriteLine(usr_mat[2, 0].ToString() + "\t" + usr_mat[2, 1].ToString() + "\t" + usr_mat[2, 2].ToString());
        }
    }
}
