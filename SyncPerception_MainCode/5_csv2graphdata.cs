using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Python.Runtime;
using System.Drawing;
using System.Reflection;

public class GraphDataComponent : GH_Component
{
    private static bool pythonInitialized = false;

    public GraphDataComponent()
        : base("GraphData", "Graph", "Converts CSV data into GNN graph data", "SyncPerception", "1_GraphData")
    {
    }

    protected override Bitmap Icon
    {
       get
       {
           var assembly = Assembly.GetExecutingAssembly();
           var resourceName = "PythonEnvChecker.Resources.05.png";  
           using (Stream stream = assembly.GetManifestResourceStream(resourceName))
           {
               return stream != null ? new Bitmap(stream) : null;
           }
       }
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddTextParameter("Nodelist", "N", "Paths to the node CSV files", GH_ParamAccess.list);
        pManager.AddTextParameter("Edgelist", "E", "Paths to the edge CSV files", GH_ParamAccess.list);
        pManager.AddTextParameter("Save Folder", "S", "Folder to save the graph data", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("GraphData", "G", "Paths to the saved graph data files", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        List<string> nodeCsvPaths = new List<string>();
        List<string> edgeCsvPaths = new List<string>();
        string saveFolder = string.Empty;

        if (!DA.GetDataList(0, nodeCsvPaths) || !DA.GetDataList(1, edgeCsvPaths) || !DA.GetData(2, ref saveFolder))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid input parameters.");
            return;
        }

        if (nodeCsvPaths.Count != edgeCsvPaths.Count)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The number of node CSV paths must match the number of edge CSV paths.");
            return;
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Python initialization failed: {ex.Message}");
                return;
            }
        }

        List<string> graphSavePaths = new List<string>();
        string outputPath = string.Empty;
        using (Py.GIL())
        {
            try
            {
                 
                dynamic sys = Py.Import("sys");
                string scriptDirectory = Path.GetDirectoryName(@"D:\CV+GNN\application\gh_plugin_VS\PythonEnvChecker\pythoncode\5_csv2graph.py");
                sys.path.append(scriptDirectory);

                 
                dynamic csv2graph = Py.Import("5_csv2graph");

                 
                for (int i = 0; i < nodeCsvPaths.Count; i++)
                {
                    string nodeCsvPath = nodeCsvPaths[i];
                    string edgeCsvPath = edgeCsvPaths[i];

                    if (!File.Exists(nodeCsvPath))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Node CSV file not found: {nodeCsvPath}");
                        continue;
                    }

                    if (!File.Exists(edgeCsvPath))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Edge CSV file not found: {edgeCsvPath}");
                        continue;
                    }

                     
                    dynamic savePath = csv2graph.process_graph(
                        new PyString(nodeCsvPath),
                        new PyString(edgeCsvPath),
                        new PyString(saveFolder)
                    );

                    graphSavePaths.Add(savePath.ToString());
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Python execution error: {ex.Message}");
                return;
            }
        }

        DA.SetDataList(0, graphSavePaths);
    }

    public override Guid ComponentGuid => new Guid("12345678-1234-1234-1234-123456789abc");
}