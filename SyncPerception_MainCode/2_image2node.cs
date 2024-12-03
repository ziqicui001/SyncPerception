using System;
using System.IO;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Python.Runtime;
using System.Drawing;
using System.Reflection;

public class ImageAnalysisComponent : GH_Component
{
    private static bool pythonInitialized = false;

    public ImageAnalysisComponent()
        : base("Image2Node", "I2N", "Processes an input image and outputs CSV with element data", "SyncPerception", "0_ImageAnalysis")
    {
    }

    protected override Bitmap Icon
    {
       get
       {
           var assembly = Assembly.GetExecutingAssembly();
           var resourceName = "PythonEnvChecker.Resources.image.png";  
           using (Stream stream = assembly.GetManifestResourceStream(resourceName))
           {
               return stream != null ? new Bitmap(stream) : null;
           }
       }
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddBooleanParameter("Run", "R", "Set to true to run the analysis", GH_ParamAccess.item);
        pManager.AddTextParameter("Color Image", "P", "Path to the input image file", GH_ParamAccess.item);
        pManager.AddTextParameter("Save Folder", "S", "Folder to save the output CSV file", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Image analysis result", "I", "Path to the saved CSV file", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        bool run = false;
        string imagePath = string.Empty;
        string saveFolderPath = string.Empty;

    
        if (!DA.GetData(0, ref run) || !run)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Set the Run input to true to execute");
            DA.SetData(0, "Execution skipped");
            return;
        }

        if (!DA.GetData(1, ref imagePath) || !File.Exists(imagePath))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "无效的图像路径。");
            return;
        }

        if (!DA.GetData(2, ref saveFolderPath))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "无效的保存文件夹路径。");
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Python 引擎初始化失败: {ex.Message}");
                return;
            }
        }

         
        string outputPath = string.Empty;
        using (Py.GIL())
        {
            try
            {
                
                dynamic sys = Py.Import("sys");
                string scriptDirectory = Path.GetDirectoryName(@"D:\CV+GNN\application\gh_plugin_VS\PythonEnvChecker\pythoncode\2_image_analysis.py"); // 确保路径正确
                sys.path.append(scriptDirectory);

                 
                dynamic imageAnalysis = Py.Import("2_image_analysis");

                 
                dynamic labels_ade = new PyList();

                 
                var labelDict1 = new PyDict();
                labelDict1.SetItem("label_name", new PyString("wall"));
                labelDict1.SetItem("class_id", new PyInt(0));
                labelDict1.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(120), new PyInt(120), new PyInt(120) }));
                labels_ade.Append(labelDict1);

                var labelDict2 = new PyDict();
                labelDict2.SetItem("label_name", new PyString("building"));
                labelDict2.SetItem("class_id", new PyInt(1));
                labelDict2.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(180), new PyInt(120), new PyInt(120) }));
                labels_ade.Append(labelDict2);

                var labelDict3 = new PyDict();
                labelDict3.SetItem("label_name", new PyString("sky"));
                labelDict3.SetItem("class_id", new PyInt(2));
                labelDict3.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(230), new PyInt(230) }));
                labels_ade.Append(labelDict3);

                var labelDict4 = new PyDict();
                labelDict4.SetItem("label_name", new PyString("floor"));
                labelDict4.SetItem("class_id", new PyInt(3));
                labelDict4.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(80), new PyInt(50), new PyInt(50) }));
                labels_ade.Append(labelDict4);

                var labelDict5 = new PyDict();
                labelDict5.SetItem("label_name", new PyString("tree"));
                labelDict5.SetItem("class_id", new PyInt(4));
                labelDict5.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(200), new PyInt(0) }));
                labels_ade.Append(labelDict5);

                var labelDict6 = new PyDict();
                labelDict6.SetItem("label_name", new PyString("ceiling"));
                labelDict6.SetItem("class_id", new PyInt(5));
                labelDict6.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(120), new PyInt(120), new PyInt(80) }));
                labels_ade.Append(labelDict6);

                var labelDict7 = new PyDict();
                labelDict7.SetItem("label_name", new PyString("road"));
                labelDict7.SetItem("class_id", new PyInt(6));
                labelDict7.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(140), new PyInt(140), new PyInt(140) }));
                labels_ade.Append(labelDict7);

                var labelDict8 = new PyDict();
                labelDict8.SetItem("label_name", new PyString("grass"));
                labelDict8.SetItem("class_id", new PyInt(9));
                labelDict8.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(255), new PyInt(0) }));
                labels_ade.Append(labelDict8);

                var labelDict9 = new PyDict();
                labelDict9.SetItem("label_name", new PyString("sidewalk"));
                labelDict9.SetItem("class_id", new PyInt(11));
                labelDict9.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(235), new PyInt(255), new PyInt(0) }));
                labels_ade.Append(labelDict9);

                var labelDict10 = new PyDict();
                labelDict10.SetItem("label_name", new PyString("person"));
                labelDict10.SetItem("class_id", new PyInt(12));
                labelDict10.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(150), new PyInt(5), new PyInt(61) }));
                labels_ade.Append(labelDict10);

                var labelDict11 = new PyDict();
                labelDict11.SetItem("label_name", new PyString("earth"));
                labelDict11.SetItem("class_id", new PyInt(13));
                labelDict11.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(120), new PyInt(120), new PyInt(70) }));
                labels_ade.Append(labelDict11);

                var labelDict12 = new PyDict();
                labelDict12.SetItem("label_name", new PyString("door"));
                labelDict12.SetItem("class_id", new PyInt(14));
                labelDict12.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(8), new PyInt(255), new PyInt(51) }));
                labels_ade.Append(labelDict12);

                var labelDict13 = new PyDict();
                labelDict13.SetItem("label_name", new PyString("table"));
                labelDict13.SetItem("class_id", new PyInt(15));
                labelDict13.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(6), new PyInt(82) }));
                labels_ade.Append(labelDict13);

                var labelDict14 = new PyDict();
                labelDict14.SetItem("label_name", new PyString("mountain"));
                labelDict14.SetItem("class_id", new PyInt(16));
                labelDict14.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(143), new PyInt(255), new PyInt(140) }));
                labels_ade.Append(labelDict14);

                var labelDict15 = new PyDict();
                labelDict15.SetItem("label_name", new PyString("plant"));
                labelDict15.SetItem("class_id", new PyInt(17));
                labelDict15.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(204), new PyInt(255), new PyInt(4) }));
                labels_ade.Append(labelDict15);

                var labelDict16 = new PyDict();
                labelDict16.SetItem("label_name", new PyString("chair"));
                labelDict16.SetItem("class_id", new PyInt(19));
                labelDict16.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(204), new PyInt(70), new PyInt(3) }));
                labels_ade.Append(labelDict16);

                var labelDict17 = new PyDict();
                labelDict17.SetItem("label_name", new PyString("car"));
                labelDict17.SetItem("class_id", new PyInt(20));
                labelDict17.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(102), new PyInt(200) }));
                labels_ade.Append(labelDict17);

                var labelDict18 = new PyDict();
                labelDict18.SetItem("label_name", new PyString("water"));
                labelDict18.SetItem("class_id", new PyInt(21));
                labelDict18.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(61), new PyInt(230), new PyInt(250) }));
                labels_ade.Append(labelDict18);

                var labelDict19 = new PyDict();
                labelDict19.SetItem("label_name", new PyString("house"));
                labelDict19.SetItem("class_id", new PyInt(25));
                labelDict19.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(9), new PyInt(224) }));
                labels_ade.Append(labelDict19);

                var labelDict20 = new PyDict();
                labelDict20.SetItem("label_name", new PyString("sea"));
                labelDict20.SetItem("class_id", new PyInt(26));
                labelDict20.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(9), new PyInt(7), new PyInt(230) }));
                labels_ade.Append(labelDict20);

                var labelDict21 = new PyDict();
                labelDict21.SetItem("label_name", new PyString("field"));
                labelDict21.SetItem("class_id", new PyInt(29));
                labelDict21.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(112), new PyInt(9), new PyInt(255) }));
                labels_ade.Append(labelDict21);

                var labelDict22 = new PyDict();
                labelDict22.SetItem("label_name", new PyString("armchair"));
                labelDict22.SetItem("class_id", new PyInt(30));
                labelDict22.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(8), new PyInt(255), new PyInt(214) }));
                labels_ade.Append(labelDict22);

                var labelDict23 = new PyDict();
                labelDict23.SetItem("label_name", new PyString("seat"));
                labelDict23.SetItem("class_id", new PyInt(31));
                labelDict23.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(7), new PyInt(255), new PyInt(224) }));
                labels_ade.Append(labelDict23);

                var labelDict24 = new PyDict();
                labelDict24.SetItem("label_name", new PyString("fence"));
                labelDict24.SetItem("class_id", new PyInt(32));
                labelDict24.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(184), new PyInt(6) }));
                labels_ade.Append(labelDict24);

                var labelDict25 = new PyDict();
                labelDict25.SetItem("label_name", new PyString("desk"));
                labelDict25.SetItem("class_id", new PyInt(33));
                labelDict25.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(10), new PyInt(255), new PyInt(71) }));
                labels_ade.Append(labelDict25);

                var labelDict26 = new PyDict();
                labelDict26.SetItem("label_name", new PyString("rock"));
                labelDict26.SetItem("class_id", new PyInt(34));
                labelDict26.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(41), new PyInt(10) }));
                labels_ade.Append(labelDict26);

                var labelDict27 = new PyDict();
                labelDict27.SetItem("label_name", new PyString("railing"));
                labelDict27.SetItem("class_id", new PyInt(38));
                labelDict27.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(61), new PyInt(6) }));
                labels_ade.Append(labelDict27);

                var labelDict28 = new PyDict();
                labelDict28.SetItem("label_name", new PyString("base"));
                labelDict28.SetItem("class_id", new PyInt(40));
                labelDict28.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(122), new PyInt(8) }));
                labels_ade.Append(labelDict28);

                var labelDict29 = new PyDict();
                labelDict29.SetItem("label_name", new PyString("column"));
                labelDict29.SetItem("class_id", new PyInt(42));
                labelDict29.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(8), new PyInt(41) }));
                labels_ade.Append(labelDict29);

                var labelDict30 = new PyDict();
                labelDict30.SetItem("label_name", new PyString("signboard"));
                labelDict30.SetItem("class_id", new PyInt(43));
                labelDict30.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(5), new PyInt(153) }));
                labels_ade.Append(labelDict30);

                var labelDict31 = new PyDict();
                labelDict31.SetItem("label_name", new PyString("sand"));
                labelDict31.SetItem("class_id", new PyInt(46));
                labelDict31.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(160), new PyInt(150), new PyInt(20) }));
                labels_ade.Append(labelDict31);

                var labelDict32 = new PyDict();
                labelDict32.SetItem("label_name", new PyString("grandstand"));
                labelDict32.SetItem("class_id", new PyInt(51));
                labelDict32.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(31), new PyInt(255), new PyInt(0) }));
                labels_ade.Append(labelDict32);

                var labelDict33 = new PyDict();
                labelDict33.SetItem("label_name", new PyString("path"));
                labelDict33.SetItem("class_id", new PyInt(52));
                labelDict33.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(31), new PyInt(0) }));
                labels_ade.Append(labelDict33);

                var labelDict34 = new PyDict();
                labelDict34.SetItem("label_name", new PyString("stairs"));
                labelDict34.SetItem("class_id", new PyInt(53));
                labelDict34.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(224), new PyInt(0) }));
                labels_ade.Append(labelDict34);

                var labelDict35 = new PyDict();
                labelDict35.SetItem("label_name", new PyString("runway"));
                labelDict35.SetItem("class_id", new PyInt(54));
                labelDict35.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(153), new PyInt(255), new PyInt(0) }));
                labels_ade.Append(labelDict35);

                var labelDict36 = new PyDict();
                labelDict36.SetItem("label_name", new PyString("screen door"));
                labelDict36.SetItem("class_id", new PyInt(58));
                labelDict36.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(173), new PyInt(255) }));
                labels_ade.Append(labelDict36);

                var labelDict37 = new PyDict();
                labelDict37.SetItem("label_name", new PyString("stairway"));
                labelDict37.SetItem("class_id", new PyInt(59));
                labelDict37.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(31), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict37);

                var labelDict38 = new PyDict();
                labelDict38.SetItem("label_name", new PyString("river"));
                labelDict38.SetItem("class_id", new PyInt(60));
                labelDict38.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(11), new PyInt(200), new PyInt(200) }));
                labels_ade.Append(labelDict38);

                var labelDict39 = new PyDict();
                labelDict39.SetItem("label_name", new PyString("bridge"));
                labelDict39.SetItem("class_id", new PyInt(61));
                labelDict39.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(82), new PyInt(0) }));
                labels_ade.Append(labelDict39);

                var labelDict40 = new PyDict();
                labelDict40.SetItem("label_name", new PyString("coffee table"));
                labelDict40.SetItem("class_id", new PyInt(64));
                labelDict40.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(255), new PyInt(112) }));
                labels_ade.Append(labelDict40);

                var labelDict41 = new PyDict();
                labelDict41.SetItem("label_name", new PyString("flower"));
                labelDict41.SetItem("class_id", new PyInt(66));
                labelDict41.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(0), new PyInt(0) }));
                labels_ade.Append(labelDict41);

                var labelDict42 = new PyDict();
                labelDict42.SetItem("label_name", new PyString("hill"));
                labelDict42.SetItem("class_id", new PyInt(68));
                labelDict42.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(102), new PyInt(0) }));
                labels_ade.Append(labelDict42);

                var labelDict43 = new PyDict();
                labelDict43.SetItem("label_name", new PyString("bench"));
                labelDict43.SetItem("class_id", new PyInt(69));
                labelDict43.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(194), new PyInt(255), new PyInt(0) }));
                labels_ade.Append(labelDict43);

                var labelDict44 = new PyDict();
                labelDict44.SetItem("label_name", new PyString("palm"));
                labelDict44.SetItem("class_id", new PyInt(72));
                labelDict44.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(82), new PyInt(255) }));
                labels_ade.Append(labelDict44);

                var labelDict45 = new PyDict();
                labelDict45.SetItem("label_name", new PyString("swivel chair"));
                labelDict45.SetItem("class_id", new PyInt(75));
                labelDict45.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(10), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict45);

                var labelDict46 = new PyDict();
                labelDict46.SetItem("label_name", new PyString("boat"));
                labelDict46.SetItem("class_id", new PyInt(76));
                labelDict46.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(173), new PyInt(255), new PyInt(0) }));
                labels_ade.Append(labelDict46);

                var labelDict47 = new PyDict();
                labelDict47.SetItem("label_name", new PyString("bat"));
                labelDict47.SetItem("class_id", new PyInt(77));
                labelDict47.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(255), new PyInt(153) }));
                labels_ade.Append(labelDict47);

                var labelDict48 = new PyDict();
                labelDict48.SetItem("label_name", new PyString("hovel"));
                labelDict48.SetItem("class_id", new PyInt(79));
                labelDict48.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict48);

                var labelDict49 = new PyDict();
                labelDict49.SetItem("label_name", new PyString("bus"));
                labelDict49.SetItem("class_id", new PyInt(80));
                labelDict49.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(0), new PyInt(245) }));
                labels_ade.Append(labelDict49);

                var labelDict50 = new PyDict();
                labelDict50.SetItem("label_name", new PyString("light"));
                labelDict50.SetItem("class_id", new PyInt(82));
                labelDict50.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(173), new PyInt(0) }));
                labels_ade.Append(labelDict50);

                var labelDict51 = new PyDict();
                labelDict51.SetItem("label_name", new PyString("truck"));
                labelDict51.SetItem("class_id", new PyInt(83));
                labelDict51.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(0), new PyInt(20) }));
                labels_ade.Append(labelDict51);

                var labelDict52 = new PyDict();
                labelDict52.SetItem("label_name", new PyString("tower"));
                labelDict52.SetItem("class_id", new PyInt(84));
                labelDict52.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(184), new PyInt(184) }));
                labels_ade.Append(labelDict52);

                var labelDict53 = new PyDict();
                labelDict53.SetItem("label_name", new PyString("awning"));
                labelDict53.SetItem("class_id", new PyInt(86));
                labelDict53.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(255), new PyInt(61) }));
                labels_ade.Append(labelDict53);

                var labelDict54 = new PyDict();
                labelDict54.SetItem("label_name", new PyString("streetlight"));
                labelDict54.SetItem("class_id", new PyInt(87));
                labelDict54.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(71), new PyInt(255) }));
                labels_ade.Append(labelDict54);

                var labelDict55 = new PyDict();
                labelDict55.SetItem("label_name", new PyString("booth"));
                labelDict55.SetItem("class_id", new PyInt(88));
                labelDict55.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(0), new PyInt(204) }));
                labels_ade.Append(labelDict55);

                var labelDict56 = new PyDict();
                labelDict56.SetItem("label_name", new PyString("airplane"));
                labelDict56.SetItem("class_id", new PyInt(90));
                labelDict56.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(255), new PyInt(82) }));
                labels_ade.Append(labelDict56);

                var labelDict57 = new PyDict();
                labelDict57.SetItem("label_name", new PyString("dirt track"));
                labelDict57.SetItem("class_id", new PyInt(91));
                labelDict57.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(10), new PyInt(255) }));
                labels_ade.Append(labelDict57);

                var labelDict58 = new PyDict();
                labelDict58.SetItem("label_name", new PyString("pole"));
                labelDict58.SetItem("class_id", new PyInt(93));
                labelDict58.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(51), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict58);

                var labelDict59 = new PyDict();
                labelDict59.SetItem("label_name", new PyString("land"));
                labelDict59.SetItem("class_id", new PyInt(94));
                labelDict59.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(194), new PyInt(255) }));
                labels_ade.Append(labelDict59);

                var labelDict60 = new PyDict();
                labelDict60.SetItem("label_name", new PyString("bannister"));
                labelDict60.SetItem("class_id", new PyInt(95));
                labelDict60.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(122), new PyInt(255) }));
                labels_ade.Append(labelDict60);

                var labelDict61 = new PyDict();
                labelDict61.SetItem("label_name", new PyString("stage"));
                labelDict61.SetItem("class_id", new PyInt(101));
                labelDict61.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(82), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict61);

                var labelDict62 = new PyDict();
                labelDict62.SetItem("label_name", new PyString("van"));
                labelDict62.SetItem("class_id", new PyInt(102));
                labelDict62.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(163), new PyInt(255), new PyInt(0) }));
                labels_ade.Append(labelDict62);

                var labelDict63 = new PyDict();
                labelDict63.SetItem("label_name", new PyString("ship"));
                labelDict63.SetItem("class_id", new PyInt(103));
                labelDict63.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(235), new PyInt(0) }));
                labels_ade.Append(labelDict63);

                var labelDict64 = new PyDict();
                labelDict64.SetItem("label_name", new PyString("fountain"));
                labelDict64.SetItem("class_id", new PyInt(104));
                labelDict64.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(8), new PyInt(184), new PyInt(170) }));
                labels_ade.Append(labelDict64);

                var labelDict65 = new PyDict();
                labelDict65.SetItem("label_name", new PyString("canopy"));
                labelDict65.SetItem("class_id", new PyInt(106));
                labelDict65.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(255), new PyInt(92) }));
                labels_ade.Append(labelDict65);

                var labelDict66 = new PyDict();
                labelDict66.SetItem("label_name", new PyString("swimming pool"));
                labelDict66.SetItem("class_id", new PyInt(109));
                labelDict66.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(184), new PyInt(255) }));
                labels_ade.Append(labelDict66);

                var labelDict67 = new PyDict();
                labelDict67.SetItem("label_name", new PyString("stool"));
                labelDict67.SetItem("class_id", new PyInt(110));
                labelDict67.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(214), new PyInt(255) }));
                labels_ade.Append(labelDict67);

                var labelDict68 = new PyDict();
                labelDict68.SetItem("label_name", new PyString("barrel"));
                labelDict68.SetItem("class_id", new PyInt(111));
                labelDict68.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(0), new PyInt(112) }));
                labels_ade.Append(labelDict68);

                var labelDict69 = new PyDict();
                labelDict69.SetItem("label_name", new PyString("waterfall"));
                labelDict69.SetItem("class_id", new PyInt(113));
                labelDict69.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(224), new PyInt(255) }));
                labels_ade.Append(labelDict69);

                var labelDict70 = new PyDict();
                labelDict70.SetItem("label_name", new PyString("tent"));
                labelDict70.SetItem("class_id", new PyInt(114));
                labelDict70.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(112), new PyInt(224), new PyInt(255) }));
                labels_ade.Append(labelDict70);

                var labelDict71 = new PyDict();
                labelDict71.SetItem("label_name", new PyString("minibike"));
                labelDict71.SetItem("class_id", new PyInt(116));
                labelDict71.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(163), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict71);

                var labelDict72 = new PyDict();
                labelDict72.SetItem("label_name", new PyString("step"));
                labelDict72.SetItem("class_id", new PyInt(121));
                labelDict72.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(0), new PyInt(143) }));
                labels_ade.Append(labelDict72);

                var labelDict73 = new PyDict();
                labelDict73.SetItem("label_name", new PyString("pot"));
                labelDict73.SetItem("class_id", new PyInt(125));
                labelDict73.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(245), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict73);

                var labelDict74 = new PyDict();
                labelDict74.SetItem("label_name", new PyString("animal"));
                labelDict74.SetItem("class_id", new PyInt(126));
                labelDict74.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(0), new PyInt(122) }));
                labels_ade.Append(labelDict74);

                var labelDict75 = new PyDict();
                labelDict75.SetItem("label_name", new PyString("bicycle"));
                labelDict75.SetItem("class_id", new PyInt(127));
                labelDict75.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(245), new PyInt(0) }));
                labels_ade.Append(labelDict75);

                var labelDict76 = new PyDict();
                labelDict76.SetItem("label_name", new PyString("lake"));
                labelDict76.SetItem("class_id", new PyInt(128));
                labelDict76.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(10), new PyInt(190), new PyInt(212) }));
                labels_ade.Append(labelDict76);

                var labelDict77 = new PyDict();
                labelDict77.SetItem("label_name", new PyString("screen"));
                labelDict77.SetItem("class_id", new PyInt(130));
                labelDict77.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(204), new PyInt(255) }));
                labels_ade.Append(labelDict77);

                var labelDict78 = new PyDict();
                labelDict78.SetItem("label_name", new PyString("sculpture"));
                labelDict78.SetItem("class_id", new PyInt(132));
                labelDict78.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(255), new PyInt(255), new PyInt(0) }));
                labels_ade.Append(labelDict78);

                var labelDict79 = new PyDict();
                labelDict79.SetItem("label_name", new PyString("traffic light"));
                labelDict79.SetItem("class_id", new PyInt(136));
                labelDict79.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(41), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict79);

                var labelDict80 = new PyDict();
                labelDict80.SetItem("label_name", new PyString("ashcan"));
                labelDict80.SetItem("class_id", new PyInt(138));
                labelDict80.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(173), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict80);

                var labelDict81 = new PyDict();
                labelDict81.SetItem("label_name", new PyString("pier"));
                labelDict81.SetItem("class_id", new PyInt(140));
                labelDict81.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(71), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict81);

                var labelDict82 = new PyDict();
                labelDict82.SetItem("label_name", new PyString("crt screen"));
                labelDict82.SetItem("class_id", new PyInt(141));
                labelDict82.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(122), new PyInt(0), new PyInt(255) }));
                labels_ade.Append(labelDict82);

                var labelDict83 = new PyDict();
                labelDict83.SetItem("label_name", new PyString("plate"));
                labelDict83.SetItem("class_id", new PyInt(142));
                labelDict83.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(0), new PyInt(255), new PyInt(184) }));
                labels_ade.Append(labelDict83);

                var labelDict84 = new PyDict();
                labelDict84.SetItem("label_name", new PyString("bulletin board"));
                labelDict84.SetItem("class_id", new PyInt(144));
                labelDict84.SetItem("colorRGB", new PyList(new PyObject[] { new PyInt(184), new PyInt(255), new PyInt(0) }));
                labels_ade.Append(labelDict84);

                 
                dynamic csv_path = imageAnalysis.process_single_image(
                    new PyString(imagePath),
                    new PyString(saveFolderPath),
                    labels_ade
                );

                outputPath = csv_path.ToString();
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Python execute error: {ex.Message}");
                return;
            }
        }

        DA.SetData(0, outputPath);
    }

    public override Guid ComponentGuid
    {
        get { return new Guid("dffac8b3-8c5f-4fb5-9a2e-d82cb6abfe9b"); }
    }
}