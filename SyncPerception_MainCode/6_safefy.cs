using System;
using System.IO;
using Grasshopper.Kernel;
using Python.Runtime;
using System.Drawing;
using System.Reflection;

public class PredictSafeComponent : GH_Component
{
    private static bool pythonInitialized = false;

     
    private const string ModelPath = @"D:\CV+GNN\application\gh_plugin_VS\PythonEnvChecker\pre_model\complete_model_1.13.pth";

    public PredictSafeComponent()
        : base("Predict Safety", "PredSafe",
              "Predict class probabilities for a graph using a trained GNN model",
              "SyncPerception", "2_Prediction")
    {
    }

    protected override Bitmap Icon
    {
       get
       {
           var assembly = Assembly.GetExecutingAssembly();
           var resourceName = "PythonEnvChecker.Resources.safe.png"; 
           using (Stream stream = assembly.GetManifestResourceStream(resourceName))
           {
               return stream != null ? new Bitmap(stream) : null;
           }
       }
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddBooleanParameter("Run", "R", "Set to true to perform the prediction", GH_ParamAccess.item);
        pManager.AddTextParameter("GraphData", "G", "Path to the input graph (.pt) file", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Safety=0", "P0", "Predicted probability for class 0", GH_ParamAccess.item);
        pManager.AddNumberParameter("Safety=1", "P1", "Predicted probability for class 1", GH_ParamAccess.item);
        pManager.AddNumberParameter("Safety=2", "P2", "Predicted probability for class 2", GH_ParamAccess.item);
        pManager.AddNumberParameter("Safety=3", "P3", "Predicted probability for class 3", GH_ParamAccess.item);
        pManager.AddNumberParameter("Safety=4", "P4", "Predicted probability for class 4", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        bool run = false;
        string graphPath = string.Empty;

        if (!DA.GetData(0, ref run) || !run)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Set 'Run' input to true to execute.");
            for (int i = 0; i < 5; i++) DA.SetData(i, 0.0);
            return;
        }

        if (!DA.GetData(1, ref graphPath) || !File.Exists(graphPath))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid graph path.");
            return;
        }

        if (!File.Exists(ModelPath))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Model file not found at the specified path: {ModelPath}");
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
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Python initialization failed: {ex.Message}");
                return;
            }
        }

        using (Py.GIL())
        {
            try
            {
                 
                dynamic sys = Py.Import("sys");
                string scriptDirectory = Path.GetDirectoryName(@"D:\CV+GNN\application\gh_plugin_VS\PythonEnvChecker\pythoncode\6_pre_safe.py");
                sys.path.append(scriptDirectory);

                 
                dynamic PredictSafe = Py.Import("6_pre_safe");

                
                dynamic probabilities = PredictSafe.predict_probabilities(
                    new PyString(graphPath), 
                    new PyString(ModelPath),  
                    new PyString("cpu")
                );

                 
                if (probabilities == null || !probabilities.HasAttr("__len__"))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Python function returned null or invalid probabilities.");
                    for (int i = 0; i < 5; i++) DA.SetData(i, 0.0);
                    return;
                }

                int length = probabilities.__len__();
                if (length != 5)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unexpected number of probabilities returned from Python.");
                    for (int i = 0; i < 5; i++) DA.SetData(i, 0.0);
                    return;
                }

                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        double probability = Convert.ToDouble(probabilities[i]);
                        DA.SetData(i, Math.Round(probability, 6));
                    }
                    catch (Exception ex)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Probability for class {i} could not be converted: {ex.Message}");
                        DA.SetData(i, 0.0);
                    }
                }
            }
            catch (PythonException pyEx)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Python execution error: {pyEx.Message}");
                for (int i = 0; i < 5; i++) DA.SetData(i, 0.0);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unexpected error: {ex.Message}");
                for (int i = 0; i < 5; i++) DA.SetData(i, 0.0);
            }
        }
    }

    public override Guid ComponentGuid => new Guid("dffac8b3-8c5f-4fb5-9a2e-d82cb6abfe9f");
}