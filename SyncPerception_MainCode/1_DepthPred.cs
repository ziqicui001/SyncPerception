using System;
using System.IO;
using Grasshopper.Kernel;
using Python.Runtime;
using System.Drawing;
using System.Reflection;

public class DepthPredictionComponent : GH_Component
{
    private static bool pythonInitialized = false;

    public DepthPredictionComponent()
        : base("Depth Prediction", "DepthPred", "Predicts depth map for input image using a pretrained model", "SyncPerception", "0_ImageAnalysis")
    {
    }

    protected override Bitmap Icon
    {
       get
       {
           var assembly = Assembly.GetExecutingAssembly();
           var resourceName = "PythonEnvChecker.Resources.depth.png";  
           using (Stream stream = assembly.GetManifestResourceStream(resourceName))
           {
               return stream != null ? new Bitmap(stream) : null;
           }
       }
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddBooleanParameter("Run", "R", "Set to true to run the prediction", GH_ParamAccess.item);
        pManager.AddTextParameter("Original Image", "P", "Path to the input image file", GH_ParamAccess.item);
        pManager.AddTextParameter("Save Folder", "S", "Folder to save the output depth map image", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Depth Image", "D", "Path to the saved depth map image", GH_ParamAccess.item);
    }

    private void InitializePython()
    {
        if (!pythonInitialized)
        {
            try
            {
                string pythonDll = @"C:\Users\HP\.conda\envs\RL\python39.dll";
                Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
                PythonEngine.Initialize();
                pythonInitialized = true;

                // Set Python locale
                using (Py.GIL())
                {
                    dynamic locale = Py.Import("locale");
                    locale.setlocale(locale.LC_ALL, "C");
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Python engine initialization failed: {ex.Message}");
            }
        }
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        InitializePython();

        bool run = false;
        string imagePath = string.Empty;
        string saveFolderPath = string.Empty;

         
        if (!DA.GetData(0, ref run) || !run)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Set 'Run' input to true to execute.");
            DA.SetData(0, "Execution skipped");
            return;
        }

        if (!DA.GetData(1, ref imagePath) || !File.Exists(imagePath))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid image path.");
            return;
        }

        if (!DA.GetData(2, ref saveFolderPath))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid save folder path.");
            return;
        }

         
        string outputPath = string.Empty;
        using (Py.GIL())
        {
            try
            {
                 
                dynamic sys = Py.Import("sys");
                string scriptDirectory = Path.GetDirectoryName(@"D:\CV+GNN\application\gh_plugin_VS\PythonEnvChecker\pythoncode\1_depth_prediction.py");
                sys.path.append(scriptDirectory);

                 
                dynamic depthPrediction = Py.Import("1_depth_prediction");

                 
                dynamic result = depthPrediction.process_image(imagePath, saveFolderPath);
                outputPath = result.ToString();
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Python execution error: {ex.Message}");
                return;
            }
        }

        DA.SetData(0, outputPath);
    }

    public override Guid ComponentGuid
    {
        get { return new Guid("dffac8b3-8c5f-4fb5-9a2e-d82cb6abfe9a"); }
    }
}
