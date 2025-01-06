using PlasticGui.WebApi.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class GenerateCsvToAsset : EditorWindow
{
    static string saveCsvPath = "Assets/GameAssets/ExeclData/Csv/";
    static string saveCSPath = "/Scripts/Execl/";
    static string saveAssetPath = "Assets/GameAssets/ExeclData/DataAsset/";

    static UnityEngine.Object selectObj;
    static string[][] array = null;
    static int id;
    static List<int> childListFlag = new List<int>();
    static List<string> childListMark = new List<string>();
    static FileInfo[] fInfo = null;
    static Assembly assembly;

    private int toolbarInt = 0;
    private string[] toolbarStrings = { "��������", "��������" };

    GenerateCsvToAsset()
    {
        this.titleContent = new GUIContent("Csv���Cs����Asset�ļ�");
    }

    [MenuItem("Tools/CsvתAsset")]
    static void SetWindow()
    {
        GenerateCsvToAsset window = EditorWindow.GetWindow<GenerateCsvToAsset>();
    }

    private void OnGUI()
    {

        toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);
        if (toolbarInt == 0)
        {
            string csvPath = string.Empty;
            GUILayout.Label("��������asset��.cs�ļ���");
            if (Selection.activeObject != null)
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (Path.GetExtension(path.ToLower()).Equals(".cs") && path.Substring(path.IndexOf('/'), path.LastIndexOf('/') - path.IndexOf('/') + 1) == saveCSPath)
                {
                    selectObj = Selection.activeObject;
                    GUILayout.Label(path);
                }
                else
                {
                    selectObj = null;
                }
            }
            GUILayout.Space(10);

            if (GUILayout.Button("����asset�ļ�"))
            {
                GenerateAsset();
            }
        }
        else if (toolbarInt == 1)
        {
            selectObj = null;
            string fiName = string.Empty;
            GetFiles(Application.dataPath + saveCSPath);
            if (fInfo.Length <= 0)
                return;
            GUILayout.Label("�ļ����ڴ��ڵ�cs�ļ���");
            foreach (FileInfo fi in fInfo)
            {
                GUILayout.Label(fi.Name);
            }

            GUILayout.Space(15);
            if (GUILayout.Button("��������as������"))
            {
                for (int i = 0; i < fInfo.Length; i++)
                {
                    fiName = Path.GetFileNameWithoutExtension(fInfo[i].Name);
                    GenerateAsset(fiName);
                }
            }
        }

    }

    private static void GenerateAsset(string selectObjName = "-1")
    {
        assembly = System.Reflection.Assembly.Load("Assembly-CSharp");
        if (selectObj != null || selectObjName != "-1")
        {
            string dataName;   // cs����
            if (selectObj != null)
            {
                dataName = selectObj.name;
            }
            else
            {
                dataName = selectObjName;
            }
            string className = dataName.Substring(0, dataName.Length - 1);  // csv����
            Type dataType = assembly.GetType(dataName);
            ScriptableObject dataObj = ScriptableObject.CreateInstance(dataType);

            string csvFullPath = saveCsvPath + className + ".csv";
            ReadCsvData(csvFullPath);

            if (array == null)
            {
                Debug.LogWarning("û�ҵ���Ӧ��Csv�ļ�" + className);
                return;
            }
            Debug.Log(array.Length);

            /*
            array[0]��һ�У�����˵�����Թ���
            array[1]�ڶ��У��Ƿ��ж�������
            array[2]�����У��ֶ���
            array[3]�����У�����
            array[4]�����У����ݣ��ɶ����֣�
            */
            for (int m = 1; m < array.Length - 1; m++)  // ת�������һ�У���Ҫ����
            {
                if (array[m] != null && m == 1)     // ���ڶ�������ΪǶ��list���ֶ���Ϣ
                {
                    for (int n = 0; n < array[m].Length; n++)
                    {
                        if (array[m][n] != "")
                        {
                            childListFlag.Add(n);
                            childListMark.Add(array[m][n]);
                        }
                        //Debug.Log(array[m][n]);
                    }
                }

                if (m <= 3)
                    continue;

                // ���������Ƿ���ֵ���жϴ洢�����ݶ�ȡ
                var firstColumn = Regex.IsMatch(array[m][0], @"^\d+$");
                if (firstColumn)
                {
                    id = StrToInt(array[m][0]);
                    // ������������
                    Type classType = assembly.GetType(className);
                    object classObj = Activator.CreateInstance(classType);
                    FieldInfo[] fiInfo = classType.GetFields();
                    for (int i = 0; i < fiInfo.Length; i++)
                    {
                        string[] strs = fiInfo[i].ToString().Split(' ');
                        IdentifyDataParameters(m, i, classObj, fiInfo, i, strs);
                    }

                    MethodInfo methodInfo = dataType.GetMethod("AddList");      // ��ȡdataType��AddList�ķ���
                    object[] parameters = new object[] { id, classObj };
                    methodInfo.Invoke(dataObj, parameters);

                    // �����ڲ������
                    for (int f = 0; f < fiInfo.Length; f++)
                    {
                        string[] strs = fiInfo[f].ToString().Split(' ');
                        if (!strs[0].Contains("List"))  // ������List����һ��
                        {
                            continue;
                        }
                        else
                        {
                            // ���ں���Ҫ�ҵ�List�ı��λ
                            int startStr = strs[0].LastIndexOf("[");
                            int endStr = strs[0].LastIndexOf("]");
                            string childClassName = strs[0].Substring(startStr + 1, endStr - startStr - 1);
                            Type childClassType = assembly.GetType(childClassName);
                            object childClassObj = Activator.CreateInstance(childClassType);
                            FieldInfo[] childFiInfo = childClassType.GetFields();
                            for (int j = 0; j < childFiInfo.Length; j++)
                            {
                                string[] childStrs = childFiInfo[j].ToString().Split(' ');
                                string childName = childStrs[1];
                                int index = FindArrayIndex(childClassName, childName);
                                if (index == -1)
                                {
                                    Debug.LogWarning("���ݶ�ȡ�쳣���ǹ��� " + childName);
                                    continue;
                                }
                                IdentifyDataParameters(m, index, childClassObj, childFiInfo, j, childStrs);
                            }

                            MethodInfo childMethodInfo = dataType.GetMethod("SecondAddList");
                            object[] childParameters = new object[] { id, childClassObj };
                            childMethodInfo.Invoke(dataObj, childParameters);
                        }
                    }
                }
                else
                {
                    // ������������ݣ�ֻ�����ڲ������
                    Type classType = assembly.GetType(className);
                    object classObj = Activator.CreateInstance(classType);
                    FieldInfo[] fiInfo = classType.GetFields();
                    for (int f = 0; f < fiInfo.Length; f++)
                    {
                        string[] strs = fiInfo[f].ToString().Split(' ');
                        if (!strs[0].Contains("List"))  // ������List����һ��
                        {
                            continue;
                        }
                        else
                        {
                            // ���ں���Ҫ�ҵ�List�ı��λ
                            int startStr = strs[0].LastIndexOf("[");
                            int endStr = strs[0].LastIndexOf("]");
                            string childClassName = strs[0].Substring(startStr + 1, endStr - startStr - 1);
                            Type childClassType = assembly.GetType(childClassName);
                            object childClassObj = Activator.CreateInstance(childClassType);
                            FieldInfo[] childFiInfo = childClassType.GetFields();
                            for (int j = 0; j < childFiInfo.Length; j++)
                            {
                                string[] childStrs = childFiInfo[j].ToString().Split(' ');
                                string childName = childStrs[1];
                                int index = FindArrayIndex(childClassName, childName);
                                if (index == -1)
                                {
                                    Debug.LogWarning("���ݶ�ȡ�쳣���ǹ��� " + childName);
                                    continue;
                                }
                                IdentifyDataParameters(m, index, childClassObj, childFiInfo, j, childStrs);
                            }

                            MethodInfo childMethodInfo = dataType.GetMethod("SecondAddList");
                            object[] childParameters = new object[] { id, childClassObj };
                            childMethodInfo.Invoke(dataObj, childParameters);
                        }
                    }
                }
            }

            AssetDatabase.CreateAsset((UnityEngine.Object)dataObj, saveAssetPath + dataName + ".asset");

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("ConfigCreater", string.Format($"���ɳɹ���{dataName}.asset", dataObj), "OK");

        }
    }

    /// <summary>
    /// ��ʶ���ݵĲ�����ת��Ϊ��Ӧ���ֶ���Ϣ
    /// </summary>
    /// <param name="m">����array��ѭ��</param>
    /// <param name="n">����array[m]��ѭ��</param>
    /// <param name="classObj">��һ���Class</param>
    /// <param name="fiInfo">��ӦClass������FieldInfo</param>
    /// <param name="fiI">������FieldInfo��ѭ��</param>
    /// <param name="strs">������FieldInfo�ָ�������ַ�����</param>
    private static void IdentifyDataParameters(int m, int n, object classObj, FieldInfo[] fiInfo, int fiI, string[] strs)
    {
        switch (strs[0])
        {
            case "System.Int32":
                fiInfo[fiI].SetValue(classObj, int.Parse(array[m][n] is "" or null ? "0" : array[m][n]));
                break;
            case "System.Int32[]":
                string[] aStr = array[m][n].Split("|");
                int[] aInt = Array.ConvertAll(aStr, new Converter<string, int>(StrToInt));
                fiInfo[fiI].SetValue(classObj, aInt);
                break;
            case "System.Int64":
                fiInfo[fiI].SetValue(classObj, long.Parse(array[m][n] is "" or null ? "0" : array[m][n]));
                break;
            case "System.Int64[]":
                string[] aStr1 = array[m][n].Split("|");
                long[] aLong = Array.ConvertAll(aStr1, new Converter<string, long>(StrToLong));
                fiInfo[fiI].SetValue(classObj, aLong);
                break;
            case "System.Single":
                fiInfo[fiI].SetValue(classObj, float.Parse(array[m][n] is "" or null ? "0" : array[m][n]));
                break;
            case "System.Single[]":
                string[] aStr2 = array[m][n].Split("|");
                float[] aSingle = Array.ConvertAll(aStr2, new Converter<string, float>(StrToSingle));
                fiInfo[fiI].SetValue(classObj, aSingle);
                break;
            case "System.String":
                fiInfo[fiI].SetValue(classObj, array[m][n]);
                break;
            case "System.String[]":
                string[] aStr3 = array[m][n].Split("|");
                fiInfo[fiI].SetValue(classObj, aStr3);
                break;
            case "System.Boolean":
                fiInfo[fiI].SetValue(classObj, bool.Parse(array[m][n]));
                break;
            default:
                if (array[3][n] == "enum")
                {
                    Type t = assembly.GetType(array[2][n]);
                    string str = array[m][n].Replace("|", ",");
                    fiInfo[fiI].SetValue(classObj, Enum.Parse(t, str));
                }
                else if (array[3][n] == "Vector2Int")
                {
                    if (array[m][n] != "")
                    {
                        string[] str = array[m][n].Split("|");
                        fiInfo[fiI].SetValue(classObj, new Vector2Int(int.Parse(str[0]), int.Parse(str[1])));
                    }
                    else
                        fiInfo[fiI].SetValue(classObj, new Vector2Int(0, 0));
                }
                else if (array[3][n] == "Vector2")
                {
                    if (array[m][n] != "")
                    {
                        string[] str = array[m][n].Split("|");
                        fiInfo[fiI].SetValue(classObj, new Vector2(float.Parse(str[0]), float.Parse(str[1])));
                    }
                    else
                        fiInfo[fiI].SetValue(classObj, new Vector2(0f, 0f));
                }
                else if (array[3][n] == "Vector3Int")
                {
                    if (array[m][n] != "")
                    {
                        string[] str = array[m][n].Split("|");
                        fiInfo[fiI].SetValue(classObj, new Vector3Int(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2])));
                    }
                    else
                        fiInfo[fiI].SetValue(classObj, new Vector3Int(0, 0, 0));
                }
                else if (array[3][n] == "Vector3")
                {
                    if (array[m][n] != "")
                    {
                        string[] str = array[m][n].Split("|");
                        fiInfo[fiI].SetValue(classObj, new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2])));
                    }
                    else
                        fiInfo[fiI].SetValue(classObj, new Vector3(0f, 0f, 0f));
                }
                break;
        }
    }

    private void OnSelectionChange()        // ���༭��ѡ�ж���ı�ʱ����
    {
        Repaint();     // ���»��ƽ���
    }

    private static void ReadCsvData(string path)
    {
        string str = File.ReadAllText(path);    // ���ļ������ı����ַ�����Ȼ��ر��ļ�
                                                // ��ȡÿһ�е����ݣ����зֻ���Ϊֹ
        string[] lineArray = str.Split("\r\n");
        // ������ά����
        array = new string[lineArray.Length][];

        // ��csv�е����ݴ洢�ڶ�ά������
        for (int i = 0; i < lineArray.Length; i++)
        {
            array[i] = lineArray[i].Split(',');
        }
    }

    private static int StrToInt(string str)
    {
        int result = int.Parse(str is "" or null ? "0" : str);
        return result;
    }

    private static long StrToLong(string str)
    {
        long result = long.Parse(str is "" or null ? "0" : str);
        return result;
    }

    private static float StrToSingle(string str)
    {
        float result = float.Parse(str is "" or null ? "0" : str);
        return result;
    }

    private static int FindArrayIndex(string className, string fieldName)
    {
        for (int i = 0; i < childListMark.Count; i++)
        {
            if (childListMark[i] == className)
            {
                int index = childListFlag[i];
                if (array[2][index] == fieldName)
                {
                    return index;
                }
            }
        }
        return -1;
    }

    public void GetFiles(string path)
    {
        if (Directory.Exists(path))
        {
            DirectoryInfo dInfo = new DirectoryInfo(path);
            fInfo = dInfo.GetFiles("*.cs");
        }
    }
}
