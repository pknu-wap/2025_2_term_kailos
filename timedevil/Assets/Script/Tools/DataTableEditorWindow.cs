using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SODataTableEditor : EditorWindow
{
    private ScriptableObject targetSO;
    private Vector2 scrollPos;
    private FieldInfo rowsField;
    private IList rowsList;
    private Type rowType;

    [MenuItem("Tools/SO DataTable Editor")]
    public static void Init() => GetWindow<SODataTableEditor>("SO Data Table");

    void OnGUI()
    {
        DrawTargetSelector();

        if (!targetSO) return;
        if (rowsList == null || rowType == null)
        {
            EditorGUILayout.HelpBox("rows(List/Array) 필드를 찾지 못했습니다. Public 또는 [SerializeField]로 선언하세요.", MessageType.Info);
            return;
        }

        DrawToolbar();
        GUILayout.Space(5);
        DrawTable();
    }

    // ---------- Target ----------

    void DrawTargetSelector()
    {
        var newSO = (ScriptableObject)EditorGUILayout.ObjectField("Target SO", targetSO, typeof(ScriptableObject), false);
        if (newSO == targetSO) return;

        targetSO = newSO;
        if (targetSO == null)
        {
            rowsField = null; rowsList = null; rowType = null; return;
        }

        FindRowsField();
        InitializeRowsList();
        DetermineRowType();
    }

    void FindRowsField()
    {
        rowsField = targetSO.GetType().GetField("rows",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (rowsField != null) return;

        foreach (var f in targetSO.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (typeof(IList).IsAssignableFrom(f.FieldType) && f.GetCustomAttribute<SerializeField>() != null)
            {
                rowsField = f; break;
            }
        }
    }

    void InitializeRowsList()
    {
        if (rowsField == null) return;

        rowsList = rowsField.GetValue(targetSO) as IList;
        if (rowsList == null)
        {
            var listType = rowsField.FieldType;
            rowsList = (IList)Activator.CreateInstance(listType);
            rowsField.SetValue(targetSO, rowsList);
            EditorUtility.SetDirty(targetSO);
        }
    }

    void DetermineRowType()
    {
        if (rowsList == null) { rowType = null; return; }

        if (rowsList.Count > 0) { rowType = rowsList[0].GetType(); return; }

        if (rowsField.FieldType.IsArray)
            rowType = rowsField.FieldType.GetElementType();
        else if (rowsField.FieldType.IsGenericType)
            rowType = rowsField.FieldType.GetGenericArguments()[0];
    }

    // ---------- Toolbar ----------

    void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Load CSV", EditorStyles.toolbarButton)) LoadCsv();
        if (GUILayout.Button("Save CSV", EditorStyles.toolbarButton)) SaveCsv();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("+ Add Row", EditorStyles.toolbarButton)) AddRow();
        if (GUILayout.Button("Save SO", EditorStyles.toolbarButton)) SaveSO();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Load JSON", EditorStyles.toolbarButton)) LoadJson();
        if (GUILayout.Button("Save JSON", EditorStyles.toolbarButton)) SaveJson();

        EditorGUILayout.EndHorizontal();
    }

    void LoadCsv()
    {
        string path = EditorUtility.OpenFilePanel("Load CSV", Application.dataPath, "csv");
        if (string.IsNullOrEmpty(path) || rowType == null) return;

        var method = typeof(CsvLoader).GetMethod("LoadCsv").MakeGenericMethod(rowType);
        var loaded = method.Invoke(null, new object[] { path }) as IList;
        if (loaded == null) return;

        rowsList.Clear();
        foreach (var it in loaded) rowsList.Add(it);
        Repaint();
    }

    void SaveCsv()
    {
        string path = EditorUtility.SaveFilePanel("Save CSV", Application.dataPath, targetSO ? targetSO.name : "data", "csv");
        if (string.IsNullOrEmpty(path) || rowType == null) return;

        var method = typeof(CsvLoader).GetMethod("SaveCsv").MakeGenericMethod(rowType);
        method.Invoke(null, new object[] { path, rowsList });
    }

    void LoadJson()
    {
        string path = EditorUtility.OpenFilePanel("Load JSON", Application.dataPath, "json");
        if (string.IsNullOrEmpty(path) || rowType == null) return;

        string json = File.ReadAllText(path);

        // JsonUtility는 루트가 객체여야 해서 래퍼 사용
        var wrapperType = typeof(ListWrapper<>).MakeGenericType(rowType);
        var wrapper = JsonUtility.FromJson(json, wrapperType);
        if (wrapper == null) { Debug.LogError("[SODataTableEditor] JSON parse failed"); return; }

        var dataField = wrapperType.GetField("items");
        var loadedList = dataField.GetValue(wrapper) as IList;
        if (loadedList == null) return;

        rowsList.Clear();
        foreach (var it in loadedList) rowsList.Add(it);
        Repaint();
    }

    void SaveJson()
    {
        string path = EditorUtility.SaveFilePanel("Save JSON", Application.dataPath, targetSO ? targetSO.name : "data", "json");
        if (string.IsNullOrEmpty(path) || rowType == null) return;

        var wrapperType = typeof(ListWrapper<>).MakeGenericType(rowType);
        var wrapper = Activator.CreateInstance(wrapperType);
        var dataField = wrapperType.GetField("items");
        dataField.SetValue(wrapper, rowsList);

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(path, json);
    }

    void AddRow() { if (rowType != null) rowsList.Add(Activator.CreateInstance(rowType)); }
    void SaveSO() { if (targetSO) { EditorUtility.SetDirty(targetSO); AssetDatabase.SaveAssets(); } }

    // ---------- Table ----------

    void DrawTable()
    {
        var fields = rowType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        // 헤더
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("ID", EditorStyles.boldLabel, GUILayout.Width(30));
        foreach (var f in fields)
        {
            if (!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null) continue;
            GUILayout.Label(f.Name, EditorStyles.boldLabel, GUILayout.Width(150));
        }
        GUILayout.Label("Delete", EditorStyles.boldLabel, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();

        // 로우들
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        int removeIndex = -1;

        for (int i = 0; i < rowsList.Count; i++)
        {
            var row = rowsList[i];
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(i.ToString(), GUILayout.Width(30));

            foreach (var f in fields)
            {
                if (!f.IsPublic && f.GetCustomAttribute<SerializeField>() == null) continue;

                object value = f.GetValue(row);
                object newValue = DrawFieldCell(value, f.FieldType, 150);
                if (!Equals(value, newValue)) f.SetValue(row, newValue);
            }

            if (GUILayout.Button("X", GUILayout.Width(50))) removeIndex = i;

            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0) rowsList.RemoveAt(removeIndex);
        EditorGUILayout.EndScrollView();
    }

    // ---------- Field Drawing ----------

    private object DrawFieldCell(object value, Type type, float width)
    {
        if (type == typeof(int)) return EditorGUILayout.IntField((int)(value ?? 0), GUILayout.Width(width));
        if (type == typeof(float)) return EditorGUILayout.FloatField((float)(value ?? 0f), GUILayout.Width(width));
        if (type == typeof(double)) return EditorGUILayout.DoubleField((double)(value ?? 0d), GUILayout.Width(width));
        if (type == typeof(string)) return EditorGUILayout.TextField((string)(value ?? ""), GUILayout.Width(width));
        if (type == typeof(bool)) return EditorGUILayout.Toggle((bool)(value ?? false), GUILayout.Width(width));
        if (type.IsEnum) return EditorGUILayout.EnumPopup((Enum)(value ?? Activator.CreateInstance(type)), GUILayout.Width(width));
        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            return EditorGUILayout.ObjectField((UnityEngine.Object)value, type, false, GUILayout.Width(width));

        // 중첩 데이터(POCO)
        if (!type.IsPrimitive && !type.IsEnum && !typeof(UnityEngine.Object).IsAssignableFrom(type))
            return DrawNestedObject(value, type, width);

        GUILayout.Label("(Unsupported)", GUILayout.Width(width));
        return value;
    }

    private object DrawNestedObject(object value, Type type, float width)
    {
        if (value == null) value = Activator.CreateInstance(type);

        EditorGUILayout.BeginVertical(GUILayout.Width(width));
        var nestedFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var nf in nestedFields)
        {
            if (!nf.IsPublic && nf.GetCustomAttribute<SerializeField>() == null) continue;

            object nestedValue = nf.GetValue(value);
            object newNestedValue = DrawFieldCell(nestedValue, nf.FieldType, width - 10);
            if (!Equals(nestedValue, newNestedValue)) nf.SetValue(value, newNestedValue);
        }
        EditorGUILayout.EndVertical();
        return value;
    }

    [Serializable] private class ListWrapper<T> { public List<T> items; }
}
