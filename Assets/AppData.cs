using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public sealed class AppData
{
    public bool isMonocular = false;
    //public Vector4 gl_background_colour = new Vector4(0.25f, 0.25f, 0.25f, 1.0f);

    public Color face_clr = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    public Color edge_clr = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    public float line_width = 0.00300f;
    public float wire_frm_dst = 7.10f;
    public double inter_ocu = 6.0;     // interoccular distance
    public double sim_dist = 600.0;    // distance to stimulus
    public int click_Count = 0;
    //Folder structure related
    public string rel_fld = "Documents/ShapePerceptionExperiment/Store";
    public string home_fldr = @"C:/Users/Darwin/Desktop/Store";
    public string cur_fldr;
    public string real_obj_file;
    public string user_obj_file;

    //If you need to generate stats instead of running experiment, set is_clc_cst_stat =  true
    // You cant run the experiment unless is_clc_cst_stat = false
    public string stat_mn_fldr = @"C:/Users/Darwin/Desktop/Store/02_13_17_01_01_48_Zyg_Bino/";
    public int stat_st = 1;
    public int stat_nd = 36;
    public bool is_clc_cst_stat = false;

    //Saved session
    public string session_path = @"../DataStore/03_15_17_03_44_21_Set_8_GD_EP_Mono/";
    public bool is_saved_session = true;
    //If you want to just choose the model from the saved session, set just_model = true
    // Note, when you mention a stored experiment folder to display the same objects again, that particular experiment session could have been mono or bino
    // You might want to run it exactly as the stored session, then set just_model = false. 
    // Or you may choose to keep just model. I.e., You might want to run the mono version for a session which was run as bino, then just_model = true
    public bool just_model = false;

    public bool create_objs = false;

    // Save results to disk
    public bool save_files = false;
    public string user_name = "ZG";

    //Create object sets from DB
    public bool crreate_sets = false;
    public string db_path = @"C:/Users/Darwin/Desktop/Store/03_15_17_03_44_21";
    public string chosen_obj = "3";

    //Slider controls
    public double[] m1rng = { -3.5f, 3.50f };
    public double[] m2rng = { -3.5f, 3.50f };
    public double[] m3rng = { 0.10f, 3.50f };
    public double[] m1shft = { -2, 2 };
    public double[] m2shft = { -2, 2 };
    public double[] m3shft = { -2, 2 };
    public float m1step = 0.01f;
    public float m2step = 0.01f;
    public float m3step = 0.01f;

    public int cntr = 0;
    public int attLmt = 5000;

    //Trials
    public int current_trial = 1;
    public bool pause_rot = false;
    //Variables deciding number/values of viewing/stretching directins
    public List<float> angles = new List<float>() { 20, 45, 70 };
    public int repetitions = 2;
    //int tot_trials = angles.Count*angles.Count*repetitions;
    public int tot_trials = 18;
    public List<int> rand_indices = new List<int>();
    // angles[row] decides stretch direction, angles[col] decides viewing direction
    public int row, col;

    public DateTime start_time;
    public DateTime rot_start_time;
    public DateTime pause_time;
    public double rot_hz = 0.18;
    public double[] rot_speed_rng = { 0.035, 0.25 };
    public float rot_step = 0.75f;
    public double cur_rot_ang = 0.0;
    // Application data
    public Model m = new Model();
    public Model n = new Model();
    public OffObject modified_obj, obj;


    /// <summary>
    ///  Private constructor, initialization outside class impossible.
    /// </summary>
    static readonly AppData _instance = new AppData();

    #region Functions

    /// <summary>
    /// To enable access to the singleton object.
    /// </summary>
    public static AppData Instance
    {
        get { return _instance; }
    }
    private AppData()
    {

        //Fill index vector
        for (int i = 0; i < repetitions; i++)
            for (int k = 0; k < (angles.Count * angles.Count); k++)
                rand_indices.Add(k);
        int nmb_sym_trials = (int)(tot_trials - angles.Count * angles.Count * repetitions);
        // Its assumed that # symmetric trials will be a multiple of angles.Count
        for (int i = 0; i < nmb_sym_trials / angles.Count; i++)
            for (int k = -3; k < 0; k++)
                rand_indices.Add(k);
        //Show contetnts before shuffle
        rand_indices.ForEach(i => Console.Write("{0}\t", i));
        //Randomize order
        rand_indices.Shuffle();
        //Show contetnts after shuffle
        rand_indices.ForEach(i => Console.Write("{0}\t", i));

        if (home_fldr == "")
        {
            string home_dir = @"C:\Users\Darwin\Desktop\ShapeExperiment";
            rel_fld = "";
            home_fldr = home_dir + @"\" + rel_fld;
        }
        //        if(cse != 2)
        //            tot_trials = angles.Count*angles.Count*repetitions;
    }
    public bool create_folders()
    {
        try
        {
            if (is_saved_session)
            {
                if (isMonocular)
                    cur_fldr = session_path.Substring(0, session_path.Length - 1) + "_" + user_name + "_Mono";
                else
                    cur_fldr = session_path.Substring(0, session_path.Length - 1) + "_" + user_name + "_Bino";
                if (Directory.Exists(cur_fldr))
                    if (isMonocular)
                        cur_fldr = session_path.Substring(0, session_path.Length - 1)+ "_" + DateTime.Now.ToString("MM_dd_yy_hh_mm_ss") + "_" + user_name + "_Mono";
                    else
                        cur_fldr = session_path.Substring(0, session_path.Length - 1) + "_" + DateTime.Now.ToString("MM_dd_yy_hh_mm_ss") + "_" + user_name + "_Bino";
            }
            else
                cur_fldr = home_fldr + "/" + DateTime.Now.ToString("MM_dd_yy_hh_mm_ss");
            real_obj_file = cur_fldr + "/real.csv";
            user_obj_file = cur_fldr + "/user.csv";

            if (!Directory.Exists(home_fldr))
                Directory.CreateDirectory(home_fldr);
            if (!Directory.Exists(cur_fldr))
                Directory.CreateDirectory(cur_fldr);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("The directory creation process failed: {0}", e.ToString());
            return false;
        }
    }

    bool is_view_acceptable()
    {
        obj = new OffObject();
        obj.copy_properties_from(modified_obj);

        //Rotate
        obj.rotate_object(m.rv_rot_mat);
        //scale
        obj.scale_object((float)m.scale);
        //translate
        //m.pos.z *= -1;
        obj.translate_object(m.pos);
        //Unity is left-handed
        for (int k = 0; k < 16; k++)
        {
            obj.verts[k].z *= -1;
        }
        obj.recalc_centre_norms_and_face_norms();
        //
        int[] baze = { 0, 1, 2, 3, 8, 9, 10, 11 };
        int[] non_baze = { 4, 5, 6, 7, 12, 13, 14, 15 };
        //Among the non-baze, how many are vsible?
        int vis_num = 0;
        int tmpCntr;
        for (int i = 0; i < 8; i++)
        {
            tmpCntr = 0;
            Vector3 cv = obj.verts[non_baze[i]];
            for (int j = 0; j < obj.faces.Length; j++)
            {
                //if the trg contains this vertex
                if (obj.faces[j].x == non_baze[i] || obj.faces[j].y == non_baze[i] || obj.faces[j].z == non_baze[i])
                {
                    tmpCntr++;
                    continue;
                }

                Vector3[] trg = { obj.verts[(int)obj.faces[j].x], obj.verts[(int)obj.faces[j].y], obj.verts[(int)obj.faces[j].z] };
                if (Helper.is_vertex_occluded(trg, cv))
                {
                    break;
                }
                tmpCntr++;
            }
            // if none of the faces block it
            if (tmpCntr++ >= obj.faces.Length)
                vis_num++;
        }
        //Debug.Log("vis_num:" + vis_num.ToString());
        //if atleast 5 of them are not visible, return failed
        if (vis_num < 6)
        {
            //if (cntr == (attLmt - 1))
            //{
            //    Debug.Log("Non-base vertex count fail");
            //}
            return false;
        }
        //Now we need to check if a total of 10 vertices are visible
        for (int i = 0; i < 8; i++)
        {
            Vector3 cv = obj.verts[baze[i]];
            tmpCntr = 0;
            for (int j = 0; j < obj.faces.Length; j++)
            {
                //if the trg contains this vertex
                if (obj.faces[j].x == baze[i] || obj.faces[j].y == baze[i] || obj.faces[j].z == baze[i])
                {
                    tmpCntr++;
                    continue;
                }
                Vector3[] trg = { obj.verts[(int)obj.faces[j].x], obj.verts[(int)obj.faces[j].y], obj.verts[(int)obj.faces[j].z] };
                if (Helper.is_vertex_occluded(trg, cv))
                {
                    break;
                }
                tmpCntr++;
            }
            if (tmpCntr >= obj.faces.Length)
                vis_num++;

            if (vis_num >= 11)
                return true;
        }
        //if (cntr == (attLmt - 1))
        //{
        //    Debug.Log("All vertex count fail");
        //}
        return false;
    }

    public bool initialize()
    {
        start_time = DateTime.Now;
        rot_start_time = start_time;
        pause_time = start_time;
        m = new Model();

        string tmp = session_path + "/" + current_trial.ToString() + "/model.txt";
        // we are just saving isMonocular becasue when model is loaded it will be overwritten
        bool isMono = isMonocular;
        m.load(tmp);
        //create modified object
        modified_obj = new OffObject();
        modified_obj.clear();

        tmp = session_path + "/" + current_trial.ToString() + "/offObj.txt";
        modified_obj.load(tmp);

        modified_obj.InitializeFaces();
        modified_obj.InitializeEdges();
        //for (int k = 0; k < 16; k++)
        //{
        //    modified_obj.verts[k].z *= -1;
        //}
        modified_obj.recalc_centre_norms_and_face_norms();

        // Read the notes at declaration of just_model to undrstand whats going on.
        if (just_model)
        {
            isMonocular = isMono;
            set_display_params();
        }
        else if(isMonocular)
        {
            line_width = 6.30f;
            wire_frm_dst = 5.50f;
        }
        else if(!isMonocular)
        {
            line_width = 0.006240f;
            wire_frm_dst = 3.80f;
        }
        //{
        //    modified_obj.translate = new Vector3(0.00f, 0, 5.0f);
        //    m.delta_pos = new Vector3(0.60f, 0.0f, 0.0f);
        //    line_width = 0.013240f;
        //    wire_frm_dst = 3.80f;
        //}
        //double asm = modified_obj.degree_of_asymmetry();
        //double cmp = modified_obj.compactess();
        //Debug.Log("Assymmetry: " + asm.ToString() + ", Compactness: " + cmp.ToString());
        //Debug.Log("Scale: " + m.scale.ToString());
        //is_view_acceptable();
        m.usr_mat[2, 0] = (float)Helper.get_random_double(-2.75, 2.75);
        m.usr_mat[2, 1] = (float)Helper.get_random_double(-2.75, 2.75);
        m.usr_mat[2, 2] = (float)Helper.get_random_double(0.1, 2.75);
        return true;
    }
    void set_display_params()
    {
        float ang_span_Thresh, tmp_scale;
        if (isMonocular)
        {
            m.delta_pos = new Vector3(513.0f, 0, 0.0f);
            m.pos = new Vector3(0.0f, 0, 1000.0f);
            modified_obj.translate = m.pos;
            m.fct = 0.75f;
            line_width = 6.30f;
            wire_frm_dst = 5.50f;
            ang_span_Thresh = 28;
            tmp_scale = 1e-5f;
        }
        else
        {
            m.delta_pos = new Vector3(0.60f, 0.0f, 0.0f);
            m.pos = new Vector3(0.00f, 0, 1.0f);
            modified_obj.translate = m.pos;
            m.fct = 0.75f;
            line_width = 0.006240f;
            wire_frm_dst = 3.80f;
            ang_span_Thresh = 28;
            tmp_scale = 1e-10f;
        }

        //Set scale according to angular span on screen
        obj = new OffObject();
        obj.copy_properties_from(modified_obj);
        Matrix4x4 tmp1 = Matrix4x4.TRS(Vector3.zero, modified_obj.rotation, Vector3.one);
        obj.rotate_object(tmp1);
        //

        obj.scale_object(tmp_scale);
        obj.translate_object(modified_obj.translate);

        while (obj.max_angular_span() < ang_span_Thresh)
        {
            
            //restore to original scale
            obj.translate_object(-obj.centre);
            obj.scale_object((1.0f/tmp_scale));
            //Rescale to new scale and translate
            tmp_scale *= 1.01f;
            obj.scale_object(tmp_scale);
            obj.translate_object(modified_obj.translate);
        }
        //if (modified_obj.compactess() < 0.14)
        //    tmp_scale *= 1.3f;
        m.scale = tmp_scale;
        modified_obj.scale = tmp_scale;
    }

    bool create_object(int isSym, int isCmp, float tSL, float tSH, float tCL, float tCH)
    {
        bool ret = true;
        m = new Model();

        #region 1) Symmetric and Compact
        /* 1) ***************** Symmetric and Compact ******************/
        if (isSym == 1 && isCmp == 1)
        {
            bool attempt_failed = true;
            int outCtr = 0, outCtrLmt = 100;
            do
            {
                m.regenerate();
                modified_obj = m.generate_off_obj();
                //The object is symmetric but if it is not compact
                if (modified_obj.compactess() < tCH)
                {
                    attempt_failed = !modify_compactness(tCH, true);
                }
                outCtr++;

            } while (attempt_failed && outCtr < outCtrLmt);
            if (outCtr >= outCtrLmt)
                ret = false;
        }
        #endregion

        #region 2) Symmetric and in between Compact
        /* 2) ***************** Symmetric and in between Compact ******************/
        if (isSym == 1 && isCmp == 0)
        {
            bool attempt_failed = true;
            int outCtr = 0, outCtrLmt = 100;
            do
            {
                m.regenerate();
                modified_obj = m.generate_off_obj();
                float df = (tCH - tCL);
                //The object is symmetric but if it is not compact
                if (modified_obj.compactess() < tCL)
                {

                    float t = (float)Helper.get_random_double(tCL, tCH - (df / 4.0f));
                    attempt_failed = !modify_compactness(t, true);
                }
                //The object is symmetric but if it is compact
                else if (modified_obj.compactess() > tCH)
                {
                    float t = (float)Helper.get_random_double(tCL + (df / 4.0f), tCH);
                    attempt_failed = !modify_compactness(t, false);
                }
                outCtr++;

            } while (attempt_failed && outCtr < outCtrLmt);
            if (outCtr >= outCtrLmt)
                ret = false;
        }
        #endregion

        #region 3) Symmetric and Non-Compact
        /* 3) ***************** Symmetric and Non-Compact ******************/
        if (isSym == 1 && isCmp == -1)
        {
            bool attempt_failed = true;
            int outCtr = 0, outCtrLmt = 100;
            do
            {
                m.regenerate();
                modified_obj = m.generate_off_obj();
                //The object is symmetric but if it is compact
                if (modified_obj.compactess() > tCL )
                {
                    attempt_failed = !modify_compactness(tCL, false);
                }
                outCtr++;
                if (modified_obj.compactess() <= 0.11)
                    attempt_failed = true;
            } while (attempt_failed && outCtr < outCtrLmt);
            if (outCtr >= outCtrLmt)
                ret = false;
        }
        #endregion

        #region 4) in between Symmetry and Compact
        /* 4) ***************** in between Symmetry and Compact ******************/
        if (isSym == 0 && isCmp == 1)
        {
            bool isSat = false;
            int outOutCntr = 0, outOutCntrLmt = 100;
            do
            {

                int outCntr = 0, outCntrLmt = 100;
                bool tot_attempt_success = true;
                do
                {
                    // First obtain desired compactness
                    bool attempt_failed = true;
                    int ctr = 0, ctrLmt = 100;
                    float df = (tSH - tSL);
                    //first we create a highly compact object, a little more compact than was asked for.
                    float nwTCH = tCH + 0.3f * tCH;
                    do
                    {
                        m.regenerate();
                        modified_obj = m.generate_off_obj();

                        if (modified_obj.compactess() < nwTCH)
                        {
                            attempt_failed = !modify_compactness(nwTCH, true);
                        }
                        ctr++;

                    } while (attempt_failed && ctr < ctrLmt);
                    if (ctr >= ctrLmt)
                        tot_attempt_success = false;
                    // Now modify Assymettry
                    if (tot_attempt_success)
                    {
                        attempt_failed = true;
                        ctr = 0; ctrLmt = 100;
                        Matrix4x4 tmp = Matrix4x4.identity;
                        tmp[2, 0] = 0.1f; tmp[2, 1] = 0.1f;
                        do
                        {
                            if (modified_obj.degree_of_asymmetry() < tSL || modified_obj.degree_of_asymmetry() > tSH)
                            {
                                float ll = (float)Helper.get_random_double(tSL, tSL + 3.0 * df / 4.0f);
                                attempt_failed = !modify_symmetry(ll, tSH, tmp);
                            }
                            ctr++;


                        } while (attempt_failed && ctr < ctrLmt);
                        if (ctr >= ctrLmt)
                            tot_attempt_success = false;
                    }
                    outCntr++;
                } while (!tot_attempt_success && outCntr < outCntrLmt);

                if ((modified_obj.degree_of_asymmetry() >= tSL) && (modified_obj.degree_of_asymmetry() <= tSH) && (modified_obj.compactess() >= tCH))
                    isSat = true;
                outOutCntr++;
            } while (outOutCntr < outOutCntrLmt && !isSat);

            ret = isSat;
        }
        #endregion

        #region 5) in between Symmetry and in between Compactness
        /* 5) ***************** in between Symmetry and in between Compactness ******************/
        if (isSym == 0 && isCmp == 0)
        {
            bool isSat = false;
            int outOutCntr = 0, outOutCntrLmt = 100;
            do
            {

                int outCntr = 0, outCntrLmt = 100;
                bool tot_attempt_success = true;
                do
                {
                    // First obtain desired compactness
                    bool attempt_failed = true;
                    tot_attempt_success = true;
                    int ctr = 0, ctrLmt = 100;
                    float dfS = (tSH - tSL);
                    //first we create a highly compact object, a little more compact than was asked for.
                    do
                    {
                        m.regenerate();
                        modified_obj = m.generate_off_obj();

                        float dfC = (tCH - tCL);
                        //The object is symmetric but if it is not compact
                        if (modified_obj.compactess() < tCH)
                        {

                            //float t = (float)Helper.get_random_double(tCL + (dfC / 2.0f), tCH );
                            attempt_failed = !modify_compactness(tCH - dfC / 5.0f, true);
                        }
                        //The object is symmetric but if it is compact
                        else if (modified_obj.compactess() > tCH)
                        {
                            //float t = (float)Helper.get_random_double(tCL + (dfC / 4.0f), tCH);
                            attempt_failed = !modify_compactness(tCH - dfC / 5.0f, false);
                        }

                        ctr++;

                    } while (attempt_failed && ctr < ctrLmt);
                    if (ctr >= ctrLmt)
                        tot_attempt_success = false;
                    // Now modify Assymettry
                    if (tot_attempt_success)
                    {
                        attempt_failed = true;
                        ctr = 0; ctrLmt = 100;
                        Matrix4x4 tmp = Matrix4x4.identity;
                        tmp[2, 0] = 0.1f; tmp[2, 1] = 0.1f;
                        do
                        {
                            if (modified_obj.degree_of_asymmetry() < tSL || modified_obj.degree_of_asymmetry() > tSH)
                            {
                                float ll = (float)Helper.get_random_double(tSL, tSL + 3.0 * dfS / 4.0f);
                                attempt_failed = !modify_symmetry(ll, tSH, tmp);
                            }
                            ctr++;


                        } while (attempt_failed && ctr < ctrLmt);
                        if (ctr >= ctrLmt)
                            tot_attempt_success = false;
                    }
                    outCntr++;
                } while (!tot_attempt_success && outCntr < outCntrLmt);

                if ((modified_obj.degree_of_asymmetry() >= tSL) && (modified_obj.degree_of_asymmetry() <= tSH) && (modified_obj.compactess() >= tCL) && (modified_obj.compactess() <= tCH))
                    isSat = true;
                outOutCntr++;
            } while (outOutCntr < outOutCntrLmt && !isSat);

            ret = isSat;
        }
        #endregion

        #region 6) in between Symmetry and not Compact
        /* 5) ***************** in between Symmetry and not Compact ******************/
        if (isSym == 0 && isCmp == -1)
        {
            bool isSat = false;
            int outOutCntr = 0, outOutCntrLmt = 100;
            do
            {

                int outCntr = 0, outCntrLmt = 100;
                bool tot_attempt_success = true;
                do
                {
                    // First obtain desired compactness
                    bool attempt_failed = true;
                    tot_attempt_success = true;
                    int ctr = 0, ctrLmt = 100;
                    float dfS = (tSH - tSL);
                    float dfC = (tCH - tCL);
                    //first we create a non compact object.
                    do
                    {
                        m.regenerate();
                        modified_obj = m.generate_off_obj();
                        float t = tCL;
                        if (modified_obj.compactess() > t)
                        {
                            attempt_failed = !modify_compactness(t, false);
                        }

                        ctr++;

                    } while (attempt_failed && ctr < ctrLmt);
                    if (ctr >= ctrLmt)
                        tot_attempt_success = false;
                    // Now modify Assymettry
                    if (tot_attempt_success)
                    {
                        attempt_failed = true;
                        ctr = 0; ctrLmt = 100;
                        Matrix4x4 tmp = Matrix4x4.identity;
                        tmp[2, 0] = 0.1f; tmp[2, 1] = 0.1f;
                        do
                        {
                            if (modified_obj.degree_of_asymmetry() < tSL || modified_obj.degree_of_asymmetry() > tSH)
                            {
                                float ll = (float)Helper.get_random_double(tSL, tSL + 3.0 * dfS / 4.0f);
                                attempt_failed = !modify_symmetry(ll, tSH, tmp);
                            }
                            ctr++;


                        } while (attempt_failed && ctr < ctrLmt);
                        if (ctr >= ctrLmt)
                            tot_attempt_success = false;
                    }
                    outCntr++;
                } while (!tot_attempt_success && outCntr < outCntrLmt);

                if ((modified_obj.degree_of_asymmetry() >= tSL) && (modified_obj.degree_of_asymmetry() <= tSH) && (modified_obj.compactess() <= tCL) && (modified_obj.compactess() >= 0.11))
                    isSat = true;
                outOutCntr++;
            } while (outOutCntr < outOutCntrLmt && !isSat);

            ret = isSat;
        }
        #endregion

        #region 7) Asymmetric and Compact
        /* 7) ***************** Asymmetric and Compact ******************/
        if (isSym == -1 && isCmp == 1)
        {
            bool isSat = false;
            int outOutCntr = 0, outOutCntrLmt = 100;
            do
            {

                int outCntr = 0, outCntrLmt = 100;
                bool tot_attempt_success = true;
                do
                {
                    // First obtain desired compactness
                    bool attempt_failed = true;
                    tot_attempt_success = true;
                    int ctr = 0, ctrLmt = 100;
                    float df = (tSH - tSL);
                    //first we create a highly compact object, a little more compact than was asked for.
                    float nwTCH = tCH + 0.4f * tCH;
                    do
                    {
                        m.regenerate();
                        modified_obj = m.generate_off_obj();

                        if (modified_obj.compactess() < nwTCH)
                        {
                            attempt_failed = !modify_compactness(nwTCH, true);
                        }
                        ctr++;

                    } while (attempt_failed && ctr < ctrLmt);
                    if (ctr >= ctrLmt)
                        tot_attempt_success = false;
                    // Now modify Assymettry
                    if (tot_attempt_success)
                    {
                        attempt_failed = true;
                        ctr = 0; ctrLmt = 100;
                        Matrix4x4 tmp = Matrix4x4.identity;
                        tmp[2, 0] = 0.1f; tmp[2, 1] = 0.1f;
                        do
                        {
                            if (modified_obj.degree_of_asymmetry() < tSH)
                            {
                                float ll = (float)Helper.get_random_double(tSH, tSH + df / 4.0f);
                                attempt_failed = !modify_symmetry(ll, 1.0f, tmp);
                            }
                            ctr++;


                        } while (attempt_failed && ctr < ctrLmt);
                        if (ctr >= ctrLmt)
                            tot_attempt_success = false;
                    }
                    outCntr++;
                } while (!tot_attempt_success && outCntr < outCntrLmt);

                if ((modified_obj.degree_of_asymmetry() >= tSH) && (modified_obj.compactess() >= tCH))
                    isSat = true;
                outOutCntr++;
            } while (outOutCntr < outOutCntrLmt && !isSat);

            ret = isSat;
        }
        #endregion

        #region 8) Asymmetric and in between Compact
        /* 7) ***************** Asymmetric and in between Compact ******************/
        if (isSym == -1 && isCmp == 0)
        {
            bool isSat = false;
            int outOutCntr = 0, outOutCntrLmt = 100;
            do
            {

                int outCntr = 0, outCntrLmt = 100;
                bool tot_attempt_success = true;
                do
                {
                    // First obtain desired compactness
                    bool attempt_failed = true;
                    tot_attempt_success = true;
                    int ctr = 0, ctrLmt = 100;
                    float df = (tSH - tSL);
                    //first we create a highly compact object, a little more compact than was asked for.
                    float nwTCH = tCH + 0.4f * tCH;
                    do
                    {
                        m.regenerate();
                        modified_obj = m.generate_off_obj();

                        float dfC = (tCH - tCL);
                        //The object is symmetric but if it is not compact
                        if (modified_obj.compactess() < tCH)
                        {

                            //float t = (float)Helper.get_random_double(tCL + (dfC / 2.0f), tCH );
                            attempt_failed = !modify_compactness(tCH, true);
                        }
                        //The object is symmetric but if it is compact
                        else if (modified_obj.compactess() > tCH)
                        {
                            //float t = (float)Helper.get_random_double(tCL + (dfC / 4.0f), tCH);
                            attempt_failed = !modify_compactness(tCH + dfC / 8.0f, false);
                        }
                        ctr++;

                    } while (attempt_failed && ctr < ctrLmt);
                    if (ctr >= ctrLmt)
                        tot_attempt_success = false;
                    // Now modify Assymettry
                    if (tot_attempt_success)
                    {
                        attempt_failed = true;
                        ctr = 0; ctrLmt = 100;
                        Matrix4x4 tmp = Matrix4x4.identity;
                        tmp[2, 0] = 0.1f; tmp[2, 1] = 0.1f;
                        do
                        {
                            if (modified_obj.degree_of_asymmetry() < tSH)
                            {
                                float ll = (float)Helper.get_random_double(tSH, tSH + df / 4.0f);
                                attempt_failed = !modify_symmetry(ll, 1.0f, tmp);
                            }
                            ctr++;


                        } while (attempt_failed && ctr < ctrLmt);
                        if (ctr >= ctrLmt)
                            tot_attempt_success = false;
                    }
                    outCntr++;
                } while (!tot_attempt_success && outCntr < outCntrLmt);

                if ((modified_obj.degree_of_asymmetry() >= tSH) && (modified_obj.compactess() >= tCL) && (modified_obj.compactess() <= tCH))
                    isSat = true;
                outOutCntr++;
            } while (outOutCntr < outOutCntrLmt && !isSat);

            ret = isSat;
        }
        #endregion

        #region 9) Asymmetric and non-Compact
        /* 7) ***************** Asymmetric and non-Compact ******************/
        if (isSym == -1 && isCmp == -1)
        {
            bool isSat = false;
            int outOutCntr = 0, outOutCntrLmt = 100;
            do
            {

                int outCntr = 0, outCntrLmt = 100;
                bool tot_attempt_success = true;
                do
                {
                    // First obtain desired compactness
                    bool attempt_failed = true;
                    tot_attempt_success = true;
                    int ctr = 0, ctrLmt = 100;
                    float df = (tSH - tSL);
                    //first we create a highly compact object, a little more compact than was asked for.
                    float nwTCH = tCH + 0.4f * tCH;
                    do
                    {
                        m.regenerate();
                        modified_obj = m.generate_off_obj();

                        float t = tCL + (tCH - tCL) / 2;
                        if (modified_obj.compactess() > t)
                        {
                            attempt_failed = !modify_compactness(t, false);
                        }
                        ctr++;

                    } while (attempt_failed && ctr < ctrLmt);
                    if (ctr >= ctrLmt)
                        tot_attempt_success = false;
                    // Now modify Assymettry
                    if (tot_attempt_success)
                    {
                        attempt_failed = true;
                        ctr = 0; ctrLmt = 100;
                        Matrix4x4 tmp = Matrix4x4.identity;
                        tmp[2, 0] = 0.1f; tmp[2, 1] = 0.1f;
                        do
                        {
                            if (modified_obj.degree_of_asymmetry() < tSH)
                            {
                                float ll = (float)Helper.get_random_double(tSH, tSH + df / 4.0f);
                                attempt_failed = !modify_symmetry(ll, 1.0f, tmp);
                            }
                            ctr++;


                        } while (attempt_failed && ctr < ctrLmt);
                        if (ctr >= ctrLmt)
                            tot_attempt_success = false;
                    }
                    outCntr++;
                } while (!tot_attempt_success && outCntr < outCntrLmt);

                if ((modified_obj.degree_of_asymmetry() >= tSH) && (modified_obj.compactess() <= tCL) && (modified_obj.compactess() >= 0.11))
                    isSat = true;
                outOutCntr++;
            } while (outOutCntr < outOutCntrLmt && !isSat);

            ret = isSat;
        }
        #endregion

        return ret;
    }

    bool modify_compactness(float tC, bool up)
    {
        bool ret = true;
        //choose a random direction
        int ti = UnityEngine.Random.Range(0, 2);
        // stretch along x-axis if ti = 0 and along z-axis if ti = 1
        Matrix4x4 tmp = Matrix4x4.identity;
        if (ti == 1)
            tmp = Helper.get_rot_Y(90);
        // see if it is stretching or compressing that would increase/decrease compactness
        float fct = 1.1f;
        double old_cmp = modified_obj.compactess();
        modified_obj.modify_object(tmp, 0.9f);
        if ((modified_obj.compactess() > old_cmp && up) || (modified_obj.compactess() < old_cmp && !up))
            fct = 0.9f;
        modified_obj.modify_object(tmp, 1.0f / 0.9f);

        //Now keep repeating stretch or compress until the derired compacntess is reached.
        int cntrLmt = 100, cntr = 0;
        while (((modified_obj.compactess() < tC && up) || ((modified_obj.compactess() > tC && !up))) && cntr < cntrLmt)
        {
            old_cmp = modified_obj.compactess();
            modified_obj.modify_object(tmp, fct);
            //If the compactness starts decreasing then we need a new random polyhedron
            if ((modified_obj.compactess() <= old_cmp && up) || (modified_obj.compactess() >= old_cmp && !up))
            {
                ret = false;
                break;
            }
            cntr++;
        }
        if (cntr >= cntrLmt)
            ret = false;

        return ret;
    }
    bool modify_symmetry(float tSL, float tSH, float ang)
    {
        bool ret = true;
        // rotation about y
        Matrix4x4 r2 = Helper.get_rot_Y((double)(90 - ang));
        // random direction, the remaining 1 dof, rotation about z
        Matrix4x4 r1 = Helper.get_rot_X(Helper.get_random_double(0, 360));
        Matrix4x4 tmp = (r2 * r1);

        float fct = 1.1f;

        //Now keep stretching until the derired assymetry is reached.
        int cntrLmt = 100, cntr = 0;
        while ((modified_obj.degree_of_asymmetry() < tSL && modified_obj.degree_of_asymmetry() < tSH) && cntr < cntrLmt)
        {
            modified_obj.modify_object(tmp, fct);
            cntr++;
        }
        if (cntr >= cntrLmt)
            ret = false;

        return ret;
    }
    bool modify_symmetry(float tSL, float tSH, Matrix4x4 mat)
    {
        bool ret = true;

        //Now keep stretching until the derired assymetry is reached.
        int cntrLmt = 100, cntr = 0;
        while ((modified_obj.degree_of_asymmetry() < tSL || modified_obj.degree_of_asymmetry() > tSH) && cntr < cntrLmt)
        {
            modified_obj.rotate_object(mat);
            cntr++;
        }
        if (cntr >= cntrLmt)
            ret = false;

        return ret;
    }
    #endregion
    public bool create_object_set_params(int isSym, int isCmp, float tSL, float tSH, float tCL, float tCH, float view_ang)
    {
        bool obj_generated = false;
        int cntr = 0, cntrLmt = 100;
        while (!obj_generated && cntr < cntrLmt) {
            obj_generated = create_object(isSym, isCmp, tSL, tSH, tCL, tCH);
            cntr++;
        }
        if (cntr >= cntrLmt)
            return false;

        set_display_params();
        //Its importat to set isMono = false and set m.scale to a sensible value to evaluate is_view_acceptable()
        m.scale = 8e-4f;

        //set view direction matrix
        //Quaternion r1, r2;
        bool is_ok = false;
        cntr = 0;
        while (!is_ok && cntr < attLmt)
        {
            //r2 = Quaternion.Euler(0,(90 - view_ang),0);

            double tmp = Helper.get_random_double(70, 110);
            //r1 = Quaternion.Euler((float)tmp, (90 - view_ang), 0);
            if (UnityEngine.Random.Range(0, 2) == 0)
                m.rv_rot_mat = Quaternion.Euler(-(float)tmp, (90 - view_ang), 0);
            else
                m.rv_rot_mat = Quaternion.Euler((float)tmp, 180 + (90 - view_ang), 0);
            //Check if view is acceptable
            is_ok = is_view_acceptable();
            cntr++;
        }
        //
        modified_obj.rotation = m.rv_rot_mat;
        modified_obj.translate = m.pos;
        modified_obj.scale = m.scale;
        //Matrix4x4 tmp1 = Matrix4x4.TRS(Vector3.zero, modified_obj.rotation, Vector3.one);

        if (cntr >= attLmt)
            return false;
        else
            return true;
    }
}
