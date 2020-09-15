using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class DBAccess : MonoBehaviour
{
    public GameObject barPrefab;
    public string databaseTable = "poptop.db";
    public float topMax = 13640;
    public float popMax = 5344;
    List<string> years = new List<string>() { "2000", "2005", "2010", "2015", "2020" };
    public Dropdown YearDropdown;
    public string yearSelected;
    List<string> plots1 = new List<string>() { "PopDensity", "Elevation", "Both" };
    public Dropdown PlotsDropdown;
    public string plotsSelected;
    List<float> popdensity = new List<float>() { 0f, 1f, 10f, 100f, 1000f };
    public Dropdown MinPopDensityDropdown;
    public float minpopdensitySelected;
    List<float> popbias = new List<float>() { 1f, 2f, 4f, 8f, 16f };
    public Dropdown PopBiasDropdown;
    public float popbiasSelected;

    public void YearDropdown_IndexChanged(int index)
    {
        yearSelected = years[index];
        QueryPlot();
    }

    public void PlotsDropdown_IndexChanged(int index)
    {
        plotsSelected = plots1[index];
        QueryPlot();
    }

    public void MinPopDensityDropdown_IndexChanged(int index)
    {
        minpopdensitySelected = popdensity[index];
        QueryPlot();
    }

    public void PopBiasDropdown_IndexChanged(int index)
    {
        popbiasSelected = popbias[index];
        QueryPlot();
    }

    void QueryPlot()
    {
        var clones = GameObject.FindGameObjectsWithTag("BarPrefabTag");
        foreach (var clone in clones)
            {
                Destroy(clone);
            }
        // Output some information to the Console at runtime
        Debug.Log(Application.dataPath + "/" + databaseTable + " MPD - " + minpopdensitySelected);
        //Base SQLite code - opens database
        string connectionString = "URI=file:" + Application.dataPath + "/" + databaseTable; //Path to database.
        IDbConnection dbcon;
        dbcon = (IDbConnection) new SqliteConnection(connectionString);
        dbcon.Open(); //Open connection to the database.
            IDbCommand dbcmd = dbcon.CreateCommand();

        dbcmd.CommandText = "SELECT lat, long, pop, year, el " + "FROM poptop_db WHERE year = " + yearSelected;
            IDataReader reader = dbcmd.ExecuteReader();

        while (reader.Read())
            {
            float poptop_lat = reader.GetFloat(0);
            float poptop_long = reader.GetFloat(1);
            float poptop_pop = reader.GetFloat(2);
            float poptop_year = reader.GetFloat(3);
            float poptop_el = reader.GetFloat(4);
            //float poptop_popnorm = reader.GetFloat(5);
            //float poptop_elnorm = reader.GetFloat(6);

            if (plotsSelected == "Elevation" || plotsSelected == "Both")
            {
                GameObject instanceBar_top = (GameObject)Instantiate(barPrefab, new Vector3(poptop_lat/20, 0, poptop_long/20), Quaternion.identity);
                instanceBar_top.transform.localScale = new Vector3(0.05f, poptop_el/topMax, 0.05f);
                instanceBar_top.GetComponent<Renderer>().material.color = Color.green;
            }
            if (plotsSelected == "PopDensity")
            {
                if (poptop_pop >= minpopdensitySelected & popbiasSelected > 0f )
                {
                    GameObject instanceBar_pop = (GameObject)Instantiate(barPrefab, new Vector3(poptop_lat/20, 0, poptop_long/20), Quaternion.identity);
                    instanceBar_pop.transform.localScale = new Vector3(0.05f, poptop_pop*popbiasSelected/popMax, 0.05f);
                    instanceBar_pop.GetComponent<Renderer>().material.color = Color.red;
                }
            }
            if (plotsSelected == "Both")
            {
                if (poptop_pop >= minpopdensitySelected & popbiasSelected > 0f)
                {
                    GameObject instanceBar_pop = (GameObject)Instantiate(barPrefab, new Vector3(poptop_lat/20, poptop_el/topMax/2, poptop_long/20), Quaternion.identity);
                    instanceBar_pop.transform.localScale = new Vector3(0.05f, poptop_pop*popbiasSelected/popMax, 0.05f);
                    instanceBar_pop.GetComponent<Renderer>().material.color = Color.red;
                }
            }
        }

        // clean up
        reader.Close();
        reader = null;
        dbcmd.Dispose();
        dbcmd = null;
        dbcon.Close();
        dbcon = null;

    }

    void Start()
    {
    //    yearDropdown.AddOptions(years);
    //    plotsDropdown.AddOptions(plots1);
    }

    // Update is called once per frame
    void Update()
    {

    }
}