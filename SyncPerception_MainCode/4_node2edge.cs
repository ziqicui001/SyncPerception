using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Python.Runtime;
using System.Drawing;
using System.Reflection;

public class NodeToEdgeComponent : GH_Component
{
    private static bool pythonInitialized = false;

    public NodeToEdgeComponent()
        : base("Node2Edge", "N2E",
              "Processes node CSV and image to generate edge CSV",
              "SyncPerception", "1_GraphData")
    {
    }

    protected override Bitmap Icon
    {
       get
       {
           var assembly = Assembly.GetExecutingAssembly();
           var resourceName = "PythonEnvChecker.Resources.edge.png";  
           using (Stream stream = assembly.GetManifestResourceStream(resourceName))
           {
               return stream != null ? new Bitmap(stream) : null;
           }
       }
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddBooleanParameter("Run", "R", "Set to true to run the analysis", GH_ParamAccess.item);
        pManager.AddTextParameter("Color Image", "I", "List of paths to the segmented image files", GH_ParamAccess.list);
        pManager.AddTextParameter("Nodelist", "C", "List of paths to the node CSV files", GH_ParamAccess.list);
        pManager.AddTextParameter("Save Folder", "S", "Folder to save the output edge CSV files", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Edgelist", "E", "List of paths to the saved edge CSV files", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        bool run = false;
        List<string> imagePaths = new List<string>();
        List<string> csvPaths = new List<string>();
        string outputFolder = string.Empty;

         
        if (!DA.GetData(0, ref run) || !run)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Set 'Run' input to true to execute.");
            DA.SetDataList(0, new List<string> { "Execution skipped" });
            return;
        }

        if (!DA.GetDataList(1, imagePaths) || imagePaths.Count == 0)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid image paths.");
            return;
        }

        if (!DA.GetDataList(2, csvPaths) || csvPaths.Count == 0 || imagePaths.Count != csvPaths.Count)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid CSV paths or mismatch with image paths.");
            return;
        }

        if (!DA.GetData(3, ref outputFolder))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid output folder.");
            return;
        }

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

       
        if (!pythonInitialized)
        {
            try
            {
                string pythonDll = @"C:\Users\HP\.conda\envs\RL\python39.dll";
                Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
                PythonEngine.Initialize();
                pythonInitialized = true;
                using (Py.GIL())
                {
                    dynamic locale = Py.Import("locale");
                    locale.setlocale(locale.LC_ALL, "C");
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Python engine initialization failed: {ex.Message}");
                return;
            }
        }

        
        List<string> outputPaths = new List<string>();
        using (Py.GIL())
        {
            try
            {
                
                dynamic sys = Py.Import("sys");
                string scriptDirectory = Path.GetDirectoryName(@"D:\CV+GNN\application\gh_plugin_VS\PythonEnvChecker\pythoncode\4_edge.py");
                sys.path.append(scriptDirectory);

                dynamic nodeToEdge = Py.Import("4_edge");

                 
                for (int i = 0; i < imagePaths.Count; i++)
                {
                    string imagePath = imagePaths[i];
                    string csvPath = csvPaths[i];

                     
                    if (!File.Exists(imagePath))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Image file does not exist: {imagePath}");
                        continue;
                    }
                    if (!File.Exists(csvPath))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"CSV file does not exist: {csvPath}");
                        continue;
                    }

                     
                    dynamic result = nodeToEdge.process_single_file(
                        new PyString(imagePath),
                        new PyString(csvPath),
                        new PyString(outputFolder)
                    );

                    outputPaths.Add(result.ToString());
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unexpected error: {ex.Message}");
                return;
            }
        }

        
        DA.SetDataList(0, outputPaths);
    }

    public override Guid ComponentGuid
    {
        get { return new Guid("dffac8b3-8c5f-4fb5-9a2e-d82cb6abfe9e"); }
    }
}