using UnityEngine;
using System;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class MainObject : MonoBehaviour
{
    public GameObject[] lnzGameObj1;
    public GameObject[] lnzGameObj2;

    public GameObject sldGameObj1;
    public GameObject sldGameObj2;

    AppData app = AppData.Instance;

    Vector3 trans1;// = new Vector3(580, 0, 1600);
    Vector3 trans2;// = new Vector3(-1080, 0, 1600);

    bool isStrtDone = false;

    // Use this for initialization
    void Start()
    {

        if (app.create_objs)
        {
            CreateSaveModels();
        }
        else if (app.crreate_sets)
        {
            CreateSets();
        }
        else if (app.is_clc_cst_stat)
        {
            CalculateStats();
        }
        else
        {
            Initialize();
            isStrtDone = true;
        }

    }
    //void Awake()
    //{
    //    Application.targetFrameRate = 200;
    //}
    public void Update()
    {
        GameObject myCam = GameObject.Find("Main Camera");
        Vector3 cr_cam_pos = myCam.transform.position;

        if (!app.is_clc_cst_stat)
        {
            if (isStrtDone)
            {
                ////Debug.Log(Time.deltaTime);
                double theta;
                // -- We're going to rotate the model --
                if (app.pause_rot)
                {
                    theta = app.cur_rot_ang;
                }
                else
                {
                    // How many seconds have elapsed since application-start?
                    double elapsed_seconds = (DateTime.Now - app.rot_start_time).TotalSeconds;
                    double period = 1.0 / app.rot_hz;
                    double phase = (elapsed_seconds % period) / period;
                    // phase is between [0..1]
                    theta = phase * 2.0 * Math.PI;
                    app.cur_rot_ang = theta;
                }
                //UpdateVertz();
                Quaternion rot = Quaternion.Euler(45, (float)(theta * (180.0f / Math.PI)), 0.0f);
                sldGameObj1.transform.rotation = rot;
                //
                sldGameObj1.transform.position = trans1 + cr_cam_pos;
                sldGameObj2.transform.position = trans2 + cr_cam_pos;
                for (int i = 0; i < (app.modified_obj.edges.Length); i++)
                {
                    lnzGameObj1[i].transform.rotation = rot;
                    lnzGameObj1[i].transform.position = trans1 + cr_cam_pos;
                    lnzGameObj2[i].transform.position = trans2 + cr_cam_pos;

                }

                if (Input.GetButtonDown("Submit"))
                {
                    app.click_Count = 1;
                }
                //Debug.Log(Input.GetAxis("Jump"));
                if (Input.GetButton("m1Down"))
                {
                    if (app.m.usr_mat[2, 0] > app.m1rng[0])
                    {
                        app.m.usr_mat[2, 0] -= app.m1step;
                        UpdateVertz();
                    }

                }
                if (Input.GetButton("m1Up"))
                {
                    if (app.m.usr_mat[2, 0] < app.m1rng[1])
                    {
                        app.m.usr_mat[2, 0] += app.m1step;
                        UpdateVertz();
                    }

                }
                // Got to Next object
                if (Input.GetKeyDown(KeyCode.N) || (Input.GetButtonDown("SubmitConfirm") && app.click_Count == 1))
                {
                    app.click_Count = 0;
                    //if (EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to save this and move on to the next object", "Yes", "No"))
                    //{
                    if (app.save_files)
                    {
                        // Model class params are written to file
                        string tmp = app.cur_fldr + "/" + app.current_trial.ToString();
                        if (!Directory.Exists(tmp))
                            Directory.CreateDirectory(tmp);
                        app.m.save(tmp + "/model.txt");
                        //rotate offobject back to original
                        Quaternion tq = new Quaternion(-app.modified_obj.rotation.x, -app.modified_obj.rotation.y, -app.modified_obj.rotation.z, app.modified_obj.rotation.w);
                        app.modified_obj.rotate_object(tq);
                        app.modified_obj.save(tmp + "/" + "offObj.txt");
                        app.m.save_mvls(tmp + "/mvls.txt");
                        //Write metrics of modified stationary objet to file.
                        app.modified_obj.write_metrics_to_file(app.real_obj_file);
                        //Compute the user object now
                        app.modified_obj.rotate_object(app.m.usr_mat);
                        //Write metrics of user objet to file.
                        app.modified_obj.write_metrics_to_file(app.user_obj_file, app.rand_indices[app.current_trial - 1], app.angles[app.row], app.angles[app.col]);

                    }
                    app.current_trial++;
                    if (app.current_trial > app.tot_trials)
                    {
                        UnityEditor.EditorApplication.isPlaying = false;
//#if UNITY_EDITOR
//                        //if (EditorUtility.DisplayDialog("Quit Confirmation", "Are you sure you want to Quit", "Yes", "No"))
//                        UnityEditor.EditorApplication.isPlaying = false;
//#else
//                        Application.Quit();
//#endif
                    }
                    else
                    {

                        while (!app.initialize())
                            app.initialize();
                        Mesh mesh1 = sldGameObj1.GetComponent<MeshFilter>().mesh;
                        Mesh mesh2 = sldGameObj2.GetComponent<MeshFilter>().mesh;
                        AssignNewMesh(mesh1, mesh2);
                        UpdateVertz();
                    }
                    //}

                }

                // Deformation based on user input
                if (Input.GetKey(KeyCode.A) || Input.GetButton("m1Down"))
                {
                    if (app.m.usr_mat[2, 0] > app.m1rng[0])
                    {
                        app.m.usr_mat[2, 0] -= app.m1step;
                        UpdateVertz();
                    }
                }
                if (Input.GetKey(KeyCode.S) || Input.GetButton("m1Up"))
                {
                    if (app.m.usr_mat[2, 0] < app.m1rng[1])
                    {
                        app.m.usr_mat[2, 0] += app.m1step;
                        UpdateVertz();
                    }

                }
                if (Input.GetKey(KeyCode.F) || Input.GetButton("m2Down"))
                {
                    if (app.m.usr_mat[2, 1] > app.m2rng[0])
                    {
                        app.m.usr_mat[2, 1] -= app.m2step;
                        UpdateVertz();
                    }
                }
                if (Input.GetKey(KeyCode.D) || Input.GetButton("m2Up"))
                {
                    if (app.m.usr_mat[2, 1] < app.m2rng[1])
                    {
                        app.m.usr_mat[2, 1] += app.m2step;
                        UpdateVertz();
                    }
                }
                if (Input.GetKey(KeyCode.Q) || Input.GetAxis("m3") < 0)
                {
                    if (app.m.usr_mat[2, 2] > app.m3rng[0])
                    {
                        app.m.usr_mat[2, 2] -= app.m3step;
                        UpdateVertz();
                    }
                }
                if (Input.GetKey(KeyCode.W) || Input.GetAxis("m3") > 0)
                {
                    if (app.m.usr_mat[2, 2] < app.m3rng[1])
                    {
                        app.m.usr_mat[2, 2] += app.m3step;
                        UpdateVertz();
                    }
                }
                if (Input.GetAxis("SpeedChange") > 0)
                {
                    app.pause_rot = true;
                    if (app.rot_hz < 0.9)
                        app.rot_hz += 1e-3;
                    app.pause_rot = false;
                }
                if (Input.GetAxis("SpeedChange") < 0)
                {
                    app.pause_rot = true;
                    if (app.rot_hz > 0.09)
                        app.rot_hz -= 1e-3;
                    app.pause_rot = false;
                }
                if (Input.GetKeyDown("escape"))
                {
#if UNITY_EDITOR
                    //if (EditorUtility.DisplayDialog("Quit Confirmation", "Are you sure you want to Quit", "Yes", "No"))
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            }
        }

    }

    void AssignNewMesh(Mesh mesh1, Mesh mesh2)
    {
        app.modified_obj.rotate_object(app.modified_obj.rotation);
        mesh1.Clear();
        mesh2.Clear();

        int[] trgAr = Helper.CreateTriangleArray(app.modified_obj);
        mesh1.vertices = app.modified_obj.verts;
        mesh1.triangles = trgAr;
        mesh2.vertices = app.modified_obj.verts;
        mesh2.triangles = trgAr;

        Helper.DisplayLine(app.modified_obj, 0, lnzGameObj1);
        Helper.DisplayLine(app.modified_obj, 0, lnzGameObj2);
        Debug.Log("Current Trial #: " + app.current_trial.ToString());

        trans1 = app.modified_obj.translate + app.m.delta_pos;
        trans2 = app.modified_obj.translate;
        //New Scale
        for (int i = 0; i < (app.modified_obj.edges.Length); i++)
        {
            lnzGameObj1[i].transform.position = trans1;
            lnzGameObj1[i].transform.localScale = (float)app.modified_obj.scale * app.m.fct * Vector3.one;
        }
        for (int i = 0; i < (app.modified_obj.edges.Length); i++)
        {
            lnzGameObj2[i].transform.position = trans2;
            lnzGameObj2[i].transform.localScale = (float)app.modified_obj.scale * Vector3.one;
        }

        sldGameObj1.transform.localScale = (float)app.modified_obj.scale * app.m.fct * Vector3.one;
        sldGameObj2.transform.localScale = (float)app.modified_obj.scale * Vector3.one;
        sldGameObj1.transform.position = trans1;
        sldGameObj2.transform.position = trans2;
    }

    void UpdateVertz()
    {
        //Debug.Log("("+app.m.usr_mat[2, 0].ToString()+","+ app.m.usr_mat[2, 1].ToString() + ","+ app.m.usr_mat[2, 2].ToString() + ")");
        //Create the roating user object transformation matrix
        // Modify mesh of user object
        Mesh mesh1 = sldGameObj1.GetComponent<MeshFilter>().mesh;
        //Mesh mesh2 = sldGameObj2.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = new Vector3[app.modified_obj.verts.Length];
        int i = 0;
        while (i < vertices.Length)
        {
            //note, you are fliiping z-coord as unity is left-handed.
            vertices[i] = new Vector3(app.modified_obj.verts[i].x, app.modified_obj.verts[i].y, -app.modified_obj.verts[i].z);

            vertices[i] = app.m.usr_mat.MultiplyVector(vertices[i]);

            //unflip now
            vertices[i].z *= -1;
            i++;
        }
        mesh1.vertices = vertices;
        Helper.DisplayLine(vertices, lnzGameObj1, app.modified_obj.edges, Helper.GetCentroid(vertices));
        //Helper.DisplayLine(vertices,lnzGameObj2,app.modified_obj.edges,Helper.GetCentroid(vertices));
    }

    void Initialize()
    {
        sldGameObj1 = new GameObject();
        sldGameObj2 = new GameObject();

        while (!app.initialize())
            app.initialize();
        if (app.save_files)
        {
            if (!app.create_folders())
            {
                Debug.Log("Folder Creation failed, Quitting");
                return;
            }
        }
        GameObject myCamObj = GameObject.Find("Main Camera");
        myCamObj.transform.position = Vector3.zero;

        if (app.isMonocular)
        {
            myCamObj.GetComponent<Camera>().nearClipPlane = 20.0f;
            myCamObj.GetComponent<Camera>().farClipPlane = 1800.0f;
        }
        else
        {
            myCamObj.GetComponent<Camera>().nearClipPlane = 0.15f;
            myCamObj.GetComponent<Camera>().farClipPlane = 20.0f;
        }

        //MESH
        sldGameObj1.AddComponent<MeshFilter>();
        sldGameObj1.AddComponent<MeshRenderer>();
        sldGameObj1.GetComponent<Renderer>().material.color = app.face_clr;

        sldGameObj2.AddComponent<MeshFilter>();
        sldGameObj2.AddComponent<MeshRenderer>();
        sldGameObj2.GetComponent<Renderer>().material.color = app.face_clr;

        Mesh mesh1 = sldGameObj1.GetComponent<MeshFilter>().mesh;
        //mesh1.Optimize();

        Mesh mesh2 = sldGameObj2.GetComponent<MeshFilter>().mesh;
        //mesh2.Optimize();

        // Lines
        lnzGameObj1 = new GameObject[app.modified_obj.edges.Length];
        lnzGameObj2 = new GameObject[app.modified_obj.edges.Length];

        //Add line components
        for (int i = 0; i < (app.modified_obj.edges.Length); i++)
        {
            lnzGameObj1[i] = new GameObject("LineObject");
            var line = lnzGameObj1[i].AddComponent<LineRenderer>();
        }
        for (int i = 0; i < (app.modified_obj.edges.Length); i++)
        {
            lnzGameObj2[i] = new GameObject("LineObject");
            var line = lnzGameObj2[i].AddComponent<LineRenderer>();
        }
        // Draw Meshes
        AssignNewMesh(mesh1, mesh2);
        UpdateVertz();
    }

    void CalculateStats()
    {
        //        for (int q = app.stat_st; q <= app.stat_nd; q++)
        //        {
        //            float mx_m1 = 0, mx_m2 = 0, mx_m3 = 1, mx_cmpct = 0.0f;
        //            bool onlyUserDtPnt = false;
        //            app.m.load(app.stat_mn_fldr + q.ToString() + "/model.txt");
        //            app.modified_obj = new OffObject();
        //            app.modified_obj = app.m.generate_off_obj();
        //            //create modified object
        //            app.modified_obj.modify_object(app.m.md_rot_mat, app.m.stretch_magn);
        //            app.modified_obj.rotation = Helper.QuaternionFromMatrix(app.m.rv_rot_mat);
        //            app.modified_obj.translate = app.m.pos;
        //            app.modified_obj.scale = app.m.scale;

        //            ////Rotate
        //            //app.modified_obj.rotate_object(app.m.rv_rot_mat);
        //            ////scale
        //            //app.modified_obj.scale_object((float)app.modified_obj.scale);
        //            ////translate
        //            //app.modified_obj.translate_object(app.modified_obj.translate);

        //            OffObject tmp_obj = new OffObject();

        //            tmp_obj.copy_properties_from(app.modified_obj);
        //            Matrix4x4 tmp_usr_mat = app.m.usr_mat;
        //            // Rotate
        //            Matrix4x4 cur_mat = app.m.usr_mat * app.m.rv_rot_mat;
        //            tmp_obj.rotate_object(cur_mat);
        //            //scale
        //            tmp_obj.scale_object((float)tmp_obj.scale);
        //            //translate
        //            tmp_obj.translate_object(tmp_obj.translate);
        //            //Compute compactness
        //            double cmpct = 36 * Math.PI * Math.Pow(tmp_obj.volume(), 2) / Math.Pow(tmp_obj.area(), 3);
        //            // projected area 
        //            float projarea = tmp_obj.proj_area();
        //            using (StreamWriter sw = new StreamWriter((app.stat_mn_fldr + q.ToString() + "/searchNew.txt")))
        //            {
        //                sw.WriteLine(app.m.usr_mat[2, 0].ToString() + "," + app.m.usr_mat[2, 1].ToString() + "," + app.m.usr_mat[2, 2].ToString() + "," + -tmp_obj.degree_of_asymmetry() + "," + cmpct.ToString() + "," + projarea / tmp_obj.area());
        //            }
        //            if (onlyUserDtPnt)
        //            {
        //                tmp_obj.copy_properties_from(app.modified_obj);
        //                tmp_usr_mat[2, 0] = 0;
        //                tmp_usr_mat[2, 1] = 0;
        //                tmp_usr_mat[2, 2] = 1;
        //                // Rotate
        //                cur_mat = tmp_usr_mat * app.m.rv_rot_mat;
        //                tmp_obj.rotate_object(cur_mat);
        //                //scale
        //                tmp_obj.scale_object((float)tmp_obj.scale);
        //                //translate
        //                tmp_obj.translate_object(tmp_obj.translate);
        //                //Compute compactness
        //                cmpct = 36 * Math.PI * Math.Pow(tmp_obj.volume(), 2) / Math.Pow(tmp_obj.area(), 3);
        //                // projected area 
        //                projarea = tmp_obj.proj_area();
        //                using (StreamWriter sw = new StreamWriter((app.stat_mn_fldr + q.ToString() + "/searchNew.txt"), true))
        //                {
        //                    sw.WriteLine(tmp_usr_mat[2, 0].ToString() + "," + tmp_usr_mat[2, 1].ToString() + "," + tmp_usr_mat[2, 2].ToString() + "," + -tmp_obj.degree_of_asymmetry() + "," + cmpct.ToString() + "," + projarea / tmp_obj.area());
        //                }
        //#if UNITY_EDITOR
        //                //if (EditorUtility.DisplayDialog("Quit Confirmation", "Are you sure you want to Quit", "Yes", "No"))
        //                UnityEditor.EditorApplication.isPlaying = false;
        //#else
        //                        Application.Quit();
        //#endif
        //                continue;
        //            }
        //            float m1 = app.m.usr_mat[2, 0];
        //            float m2 = app.m.usr_mat[2, 1];
        //            float m3 = app.m.usr_mat[2, 2];

        //            float[] rng = { -2, 2 };
        //            float intr = (rng[1] - rng[0]) / 40;



        //            for (float i = m1 + rng[0]; i <= m1 + rng[1]; i += intr)
        //            {
        //                for (float j = m2 + rng[0]; j <= m2 + rng[1]; j += intr)
        //                {
        //                    for (float k = m3 + rng[0]; k <= m3 + rng[1]; k += intr)
        //                    {
        //                        if (m3 > 0)
        //                        {
        //                            if (k < 0.3)
        //                                continue;
        //                        }
        //                        else
        //                        {
        //                            if (k > 0.3)
        //                                continue;
        //                        }
        //                        tmp_obj.copy_properties_from(app.modified_obj);
        //                        tmp_usr_mat[2, 0] = i;
        //                        tmp_usr_mat[2, 1] = j;
        //                        tmp_usr_mat[2, 2] = k;
        //                        // Rotate
        //                        cur_mat = tmp_usr_mat * app.m.rv_rot_mat;
        //                        tmp_obj.rotate_object(cur_mat);
        //                        //scale
        //                        tmp_obj.scale_object((float)tmp_obj.scale);
        //                        //translate
        //                        tmp_obj.translate_object(tmp_obj.translate);
        //                        //Compute compactness
        //                        cmpct = 36 * Math.PI * Math.Pow(tmp_obj.volume(), 2) / Math.Pow(tmp_obj.area(), 3);
        //                        if (cmpct > mx_cmpct)
        //                        {
        //                            mx_cmpct = (float)cmpct;
        //                            mx_m1 = i;
        //                            mx_m2 = j;
        //                            mx_m3 = k;
        //                        }
        //                        // projected area 
        //                        projarea = tmp_obj.proj_area();
        //                        using (StreamWriter sw = new StreamWriter((app.stat_mn_fldr + q.ToString() + "/searchNew.txt"), true))
        //                        {
        //                            sw.WriteLine(tmp_usr_mat[2, 0].ToString() + "," + tmp_usr_mat[2, 1].ToString() + "," + tmp_usr_mat[2, 2].ToString() + "," + -tmp_obj.degree_of_asymmetry() + "," + cmpct.ToString() + "," + projarea / tmp_obj.area());
        //                        }
        //                    }
        //                }
        //            }
        //            //Calculate the metrics for the max compact one and also shape dissimilarity between the max compact shape and the user obj and with the real obj.
        //            // tmp_obj = max_cmpct obj
        //            tmp_obj.copy_properties_from(app.modified_obj);
        //            tmp_usr_mat[2, 0] = mx_m1;
        //            tmp_usr_mat[2, 1] = mx_m2;
        //            tmp_usr_mat[2, 2] = mx_m3;
        //            // Rotate
        //            cur_mat = tmp_usr_mat * app.m.rv_rot_mat;
        //            tmp_obj.rotate_object(cur_mat);
        //            //scale
        //            tmp_obj.scale_object((float)tmp_obj.scale);
        //            //translate
        //            tmp_obj.translate_object(tmp_obj.translate);

        //            // rl_obj = Real obj shown to user
        //            OffObject rl_obj = new OffObject();
        //            rl_obj.copy_properties_from(app.modified_obj);
        //            // Rotate
        //            cur_mat = app.m.rv_rot_mat;
        //            rl_obj.rotate_object(cur_mat);
        //            //scale
        //            rl_obj.scale_object((float)tmp_obj.scale);
        //            //translate
        //            rl_obj.translate_object(tmp_obj.translate);

        //            // usr_obj = the user chosen shape
        //            OffObject usr_obj = new OffObject();
        //            usr_obj.copy_properties_from(app.modified_obj);
        //            tmp_usr_mat = app.m.usr_mat;
        //            // Rotate
        //            cur_mat = tmp_usr_mat * app.m.rv_rot_mat;
        //            usr_obj.rotate_object(cur_mat);
        //            //scale
        //            usr_obj.scale_object((float)tmp_obj.scale);
        //            //translate
        //            usr_obj.translate_object(tmp_obj.translate);

        //            // Write metrics to file
        //            bool appnd = true;
        //            if (q == app.stat_st)
        //                appnd = false;

        //            tmp_obj.write_metrics_to_file((app.stat_mn_fldr + "max_metrics.csv"), appnd);
        //            usr_obj.write_metrics_to_file((app.stat_mn_fldr + "usr_metrics.csv"), appnd);
        //            rl_obj.write_metrics_to_file((app.stat_mn_fldr + "rl_metrics.csv"), appnd);
        //            // Compare Shapes and write to file.
        //            using (StreamWriter sw = new StreamWriter((app.stat_mn_fldr + "usr_max.txt"), appnd))
        //            {
        //                sw.WriteLine(tmp_obj.degree_of_asymmetry(usr_obj));
        //            }
        //            using (StreamWriter sw = new StreamWriter((app.stat_mn_fldr + "rl_max.txt"), appnd))
        //            {
        //                sw.WriteLine(tmp_obj.degree_of_asymmetry(rl_obj));
        //            }
        //            using (StreamWriter sw = new StreamWriter((app.stat_mn_fldr + "rl_usr.txt"), appnd))
        //            {
        //                sw.WriteLine(rl_obj.degree_of_asymmetry(usr_obj));
        //            }


        //#if UNITY_EDITOR
        //            //if (EditorUtility.DisplayDialog("Quit Confirmation", "Are you sure you want to Quit", "Yes", "No"))
        //            UnityEditor.EditorApplication.isPlaying = false;
        //#else
        //                        Application.Quit();
        //#endif
        //        }

    }

    void CreateSaveModels()
    {
        List<float> angles = new List<float>() { 20, 45, 70 };
        int num_objs = 20;
        string home_dir = app.home_fldr + "/" + DateTime.Now.ToString("MM_dd_yy_hh_mm_ss");
        // for each angle
        for (int i = 0; i < angles.Count; i++)
        {
            // for each object type
            for (int j = 0; j < 9; j++)
            {
                for (int k = 1; k <= num_objs; k++)
                {
                    string cur_fldr = home_dir + "/" + "cse_" + (j + 1).ToString() + "_ang_" + angles[i].ToString() + "/" + k.ToString();

                    bool is_success = false;
                    int cntr = 0, cntrLmt = 100;
                    int sym_num = ((8 - j) / 3) - 1;
                    int cmp_num = ((8 - j) % 3) - 1;
                    while (cntr < cntrLmt && !is_success)
                    {
                        if (sym_num == 1 && cmp_num == 1)
                        {
                            is_success = app.create_object_set_params(sym_num, cmp_num, 0.05f, 0.18f, 0.18f, 0.45f, angles[i]);
                        }
                        else
                        {
                            is_success = app.create_object_set_params(sym_num, cmp_num, 0.08f, 0.18f, 0.18f, 0.38f, angles[i]);
                        }
                        cntr++;
                    }
                    if (is_success)
                    {
                        if (!Directory.Exists(cur_fldr))
                            Directory.CreateDirectory(cur_fldr);
                        app.m.save(cur_fldr + "/" + "model.txt");
                        app.modified_obj.save(cur_fldr + "/" + "offObj.txt");
                    }
                }

            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
    }

    void CreateSets()
    {
        for (int p = 1; p <= 20; p++)
        {
            app.chosen_obj = p.ToString();
            //Obtain random ordering of cases
            List<int> rand_ind = new List<int>();

            for (int k = 0; k < 18; k++)
            {
                rand_ind.Add(k);
            }
            rand_ind.Shuffle();
            //Create the folder for the set and copy objects from DB in the random order created above
            string set_dir = app.db_path + "_Set_" + app.chosen_obj;
            if (!Directory.Exists(set_dir))
                Directory.CreateDirectory(set_dir);
            int[] lst_vw = new int[18];
            int[] lst_cse = new int[18];
            for (int k = 0; k < 18; k++)
            {
                string dst_dir = set_dir + "/" + (k + 1).ToString();
                int vw = rand_ind[k] / 9;
                int cse = (rand_ind[k] % 9) + 1;

                lst_cse[k] = cse;
                string source_dir;
                if (vw == 0)
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        source_dir = app.db_path + "/cse_" + cse.ToString() + "_ang_20/" + app.chosen_obj;
                        lst_vw[k] = 20;
                    }
                    else
                    {
                        source_dir = app.db_path + "/cse_" + cse.ToString() + "_ang_70/" + app.chosen_obj;
                        lst_vw[k] = 70;
                    }
                else
                {
                    source_dir = app.db_path + "/cse_" + cse.ToString() + "_ang_45/" + app.chosen_obj;
                    lst_vw[k] = 45;
                }

                Helper.DirectoryCopy(source_dir, dst_dir, false);
            }
            using (StreamWriter sw = new StreamWriter(set_dir + "/" + "cse_vw.txt"))
            {
                for (int k = 0; k < 18; k++)
                {
                    sw.WriteLine(lst_cse[k].ToString() + "\t" + lst_vw[k].ToString());
                }
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif

    }
}
