using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;

[CustomEditor(typeof(MonoFacade))]
public class UIFacadeInspector : Editor
{
    string filepath = "..\\Logic\\UI\\";
    int? removeIdx;
    static string targetSubDir;
    
    // 记录使用过的子目录
    private static List<string> usedSubDirs = new List<string>();
    private static int selectedDirIndex = 0;
    private static bool isHistoryLoaded = false; // 标记是否已加载历史记录
    
    // 用于编辑的字段
    private string newDirEntry = "";
    private bool showDirManager = false;
    
    // PlayerPrefs键名
    private const string PREFS_KEY = "UIFacadeInspector_UsedSubDirs";
    
    // 自动生成代码的标记
    private const string AUTO_GENERATED_START = "    //[AUTO_GENERATED_START]";
    private const string MEMBER_VARIABLES_START = "    //[MEMBER_VARIABLES_START]";
    private const string MEMBER_VARIABLES_END = "    //[MEMBER_VARIABLES_END]";
    private const string INITIALIZATION_START = "        //[INITIALIZATION_START]";
    private const string INITIALIZATION_END = "        //[INITIALIZATION_END]";

    public override void OnInspectorGUI()
    {
        var facade = target as MonoFacade;

        // 在第一次使用时加载历史记录
        if (!isHistoryLoaded)
        {
            LoadHistoryFromPrefs();
            isHistoryLoaded = true;
        }

        if (string.IsNullOrEmpty(facade.uiname))
            facade.uiname = facade.name;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("uiname:");
        facade.uiname = EditorGUILayout.TextField(facade.uiname);
        EditorGUILayout.EndHorizontal();

        if (facade.uielements == null)
            facade.uielements = new();
        for (var i = 0; i < facade.uielements.Count; i++)
        {
            var ele = facade.uielements[i];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i}.", GUILayout.Width(20));
            string name = string.IsNullOrEmpty(ele.name) ? (ele.component == null ? "" : ele.component.name) : ele.name;
            name = string.IsNullOrEmpty(name) ? "" : name;
            ele.name = EditorGUILayout.TextField(name);
            if (string.IsNullOrEmpty(ele.comtype) || ele.comtype == "Transform")
            {
                ele.comtype = GetPreferType(ele.component);
            }
            if (ele.component != null)
                ele.originalName = ele.component.name;
            EditorGUILayout.TextField(ele.originalName);
            ele.comtype = EditorGUILayout.TextField(ele.comtype);
            ele.component = EditorGUILayout.ObjectField(ele.component, GetElementComponentType(ele.component), true, GUILayout.Width(120)) as Component;
            if (GUILayout.Button("-", GUILayout.Width(20)))
                removeIdx = i;
            EditorGUILayout.EndHorizontal();
        }
        if (removeIdx.HasValue)
        {
            facade.uielements.RemoveAt(removeIdx.Value);
            removeIdx = null;
        }
        EditorGUILayout.Space(1);
        if (GUILayout.Button("Add a element"))
            facade.uielements.Add(new UIElement());

        EditorGUILayout.Space(5);
        if (GUILayout.Button("Auto Collect"))
        {
            AutoCollect(facade);
        }

        EditorGUILayout.Space(5);
        var dir = Path.Combine(Application.dataPath, filepath);

        // 绘制历史子目录下拉框
        EditorGUILayout.LabelField("历史子目录:");
        if (usedSubDirs.Count > 0)
        {
            string[] dirOptions = new string[usedSubDirs.Count + 1];
            dirOptions[0] = "请选择或输入新目录...";
            for (int i = 0; i < usedSubDirs.Count; i++)
            {
                dirOptions[i + 1] = usedSubDirs[i];
            }

            int newIndex = EditorGUILayout.Popup(selectedDirIndex, dirOptions);
            if (newIndex != selectedDirIndex)
            {
                selectedDirIndex = newIndex;
                if (selectedDirIndex > 0 && selectedDirIndex <= usedSubDirs.Count)
                {
                    targetSubDir = usedSubDirs[selectedDirIndex - 1];
                }
                else
                {
                    targetSubDir = "";
                }
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("sub dir:");
        targetSubDir = EditorGUILayout.TextField(targetSubDir);
        EditorGUILayout.EndHorizontal();
        
        // 显示目录管理面板的开关
        showDirManager = EditorGUILayout.Foldout(showDirManager, "目录管理", true);
        if (showDirManager)
        {
            DrawDirectoryManager();
        }

        if (!string.IsNullOrEmpty(targetSubDir))
        {
            dir = Path.Combine(dir, targetSubDir);
        }

        var facadefile = Path.Combine(dir, facade.uiname + ".cs");
        var rawfile = $"{dir}{facade.uiname}.cs";//暂时没用了
        EditorGUILayout.LabelField($"Save to {facadefile}");

        if (GUILayout.Button("Gen C# File"))
        {
            // 只在点击生成按钮时记录目录
            if (!string.IsNullOrEmpty(targetSubDir) && !usedSubDirs.Contains(targetSubDir))
            {
                usedSubDirs.Add(targetSubDir);
                SaveHistoryToPrefs(); // 保存到PlayerPrefs
            }
            GenFile(facade, facadefile, rawfile);
        }
        if (GUILayout.Button("Gen Widget Code"))
        {
            GenWidgetCode(facade);
        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Save"))
        {
            SaveByGameObject(facade.gameObject);
        }
    }

    /// <summary>
    /// 绘制目录管理面板
    /// </summary>
    private void DrawDirectoryManager()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 添加新目录
        EditorGUILayout.LabelField("添加新目录:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        newDirEntry = EditorGUILayout.TextField("新目录", newDirEntry);
        if (GUILayout.Button("添加", GUILayout.Width(60)))
        {
            if (!string.IsNullOrEmpty(newDirEntry) && !usedSubDirs.Contains(newDirEntry))
            {
                usedSubDirs.Add(newDirEntry);
                SaveHistoryToPrefs(); // 保存到PlayerPrefs
                newDirEntry = ""; // 清空输入框
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 显示和管理现有目录
        EditorGUILayout.LabelField("现有目录:", EditorStyles.boldLabel);
        if (usedSubDirs.Count > 0)
        {
            for (int i = 0; i < usedSubDirs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(20));
                
                // 编辑目录名称
                string oldValue = usedSubDirs[i];
                usedSubDirs[i] = EditorGUILayout.TextField(usedSubDirs[i]);
                
                // 如果目录名称发生变化，保存到PlayerPrefs
                if (oldValue != usedSubDirs[i])
                {
                    SaveHistoryToPrefs();
                }
                
                // 删除按钮
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    usedSubDirs.RemoveAt(i);
                    SaveHistoryToPrefs(); // 保存到PlayerPrefs
                    
                    // 更新选中索引
                    if (selectedDirIndex > i + 1)
                    {
                        selectedDirIndex--;
                    }
                    else if (selectedDirIndex == i + 1)
                    {
                        selectedDirIndex = 0; // 重置为默认选项
                    }
                    break; // 删除后需要重新绘制列表
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.LabelField("暂无目录记录");
        }
        
        EditorGUILayout.Space();
        
        // 清空所有目录按钮
        if (usedSubDirs.Count > 0 && GUILayout.Button("清空所有目录"))
        {
            usedSubDirs.Clear();
            selectedDirIndex = 0;
            SaveHistoryToPrefs(); // 保存到PlayerPrefs
        }
        
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 保存历史记录到PlayerPrefs
    /// </summary>
    private static void SaveHistoryToPrefs()
    {
        try
        {
            // 将目录列表转换为JSON字符串
            string json = JsonUtility.ToJson(new StringListWrapper(usedSubDirs));
            PlayerPrefs.SetString(PREFS_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError("保存历史目录到PlayerPrefs失败: " + e.Message);
        }
    }

    /// <summary>
    /// 从PlayerPrefs加载历史记录
    /// </summary>
    private static void LoadHistoryFromPrefs()
    {
        try
        {
            if (PlayerPrefs.HasKey(PREFS_KEY))
            {
                string json = PlayerPrefs.GetString(PREFS_KEY);
                StringListWrapper wrapper = JsonUtility.FromJson<StringListWrapper>(json);
                if (wrapper != null && wrapper.items != null)
                {
                    usedSubDirs = wrapper.items;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("从PlayerPrefs加载历史目录失败: " + e.Message);
        }
    }

    private string GenWidgetCode(MonoFacade facade)
    {
        var codes = @$"
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

class {facade.uiname} : Entity
{{
[--vs]
    public override void OnStart()
    {{
        base.OnStart();
[--vsgetref]
    }}
}}
";
        var vs = "";
        var vsgetref = "";
        foreach (var ele in facade.uielements)
        {
            vs += @$"    {ele.comtype} {ele.name};
";
            vsgetref += $@"        {ele.name} = this.GetMonoComponent<{ele.comtype}>(""{ele.name}"");
";
        }
        codes = codes.Replace("[--vs]", vs).Replace("[--vsgetref]", vsgetref);

        UnityEngine.GUIUtility.systemCopyBuffer = codes;

        return codes;
    }

    private void AutoCollect(MonoFacade facade)
    {
        var trs = facade.GetComponentsInChildren<Transform>();
        foreach (var tr in trs)
        {
            if (facade.Contains(tr))
                continue;
            facade.uielements.Add(new UIElement()
            {
                component = tr,
                comtype = GetPreferType(tr),
                name = tr.name,
            });
        }
    }

    private string GetPreferType(Component component)
    {
        if (component == null)
            return "Transform";
        if (component.GetComponent<Button>() != null)
            return "Button";
        if (component.GetComponent<Text>() != null)
            return "Text";
        if (component.GetComponent<InputField>() != null)
            return "InputField";
        if (component.GetComponent<Image>() != null)
            return "Image";
        if (component.GetComponent<RawImage>() != null)
            return "RawImage";
        if (component.GetComponent<Animation>() != null)
            return "Animation";
        if (component.GetComponent<TextMeshProUGUI>() != null)
            return "TextMeshProUGUI";
        if (component.GetComponent<RectTransform>() != null)
            return "RectTransform";
        return "Transform";
    }

    private void GenFile(MonoFacade facade, string facadefile, string rawfile)
    {
        // 生成成员变量声明和初始化代码
        string memberVariablesCode = GenerateMemberVariablesCode(facade);
        string initializationCode = GenerateInitializationCode(facade);
        
        // 检查文件是否存在
        if (File.Exists(facadefile))
        {
            // 如果文件存在，只替换成员变量和初始化部分
            string existingContent = File.ReadAllText(facadefile);
            string updatedContent = ReplaceCodeSections(existingContent, memberVariablesCode, initializationCode);
            File.WriteAllText(facadefile, updatedContent, Encoding.UTF8);
        }
        else
        {
            // 如果文件不存在，创建新文件
            string fullContent = GenerateFullFileContent(facade, memberVariablesCode, initializationCode);
            var finfo = new FileInfo(facadefile);
            if (!finfo.Directory.Exists)
            {
                finfo.Directory.Create();
            }
            File.WriteAllText(facadefile, fullContent, Encoding.UTF8);
        }
        
        Debug.Log($"Save file {facadefile}");
    }

    /// <summary>
    /// 生成成员变量声明代码
    /// </summary>
    private string GenerateMemberVariablesCode(MonoFacade facade)
    {
        var vs = "";
        foreach (var ele in facade.uielements)
        {
            vs += @$"    public {ele.comtype} {ele.name};
";
        }

        return vs;
    }

    /// <summary>
    /// 生成初始化代码
    /// </summary>
    private string GenerateInitializationCode(MonoFacade facade)
    {
        var vsgetref = "";
        foreach (var ele in facade.uielements)
        {
            vsgetref += $@"        {ele.name} = this.GetMonoComponent<{ele.comtype}>(""{ele.name}"");
";
        }

        return vsgetref;
    }

    /// <summary>
    /// 生成完整文件内容（用于创建新文件）
    /// </summary>
    private string GenerateFullFileContent(MonoFacade facade, string memberVariablesCode, string initializationCode)
    {
        string fullContent = $@"// 文件中部分内容为自动生成，请勿修改标记内的代码防止被再次生成时覆盖
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class {facade.uiname} : Entity
{{
{AUTO_GENERATED_START}
    // 自动生成的成员变量部分，不要手动修改
{MEMBER_VARIABLES_START}
{memberVariablesCode}{MEMBER_VARIABLES_END}

    public override void OnStart()
    {{
        base.OnStart();
        // 初始化代码部分，不要手动修改
{INITIALIZATION_START}
{initializationCode}{INITIALIZATION_END}
    }}
}}";
        return fullContent;
    }

    /// <summary>
    /// 替换文件中的成员变量和初始化代码部分
    /// </summary>
    private string ReplaceCodeSections(string existingContent, string newMemberVariablesCode, string newInitializationCode)
    {
        string result = existingContent;
        
        // 替换成员变量部分
        int memberStartIndex = result.IndexOf(MEMBER_VARIABLES_START);
        int memberEndIndex = result.IndexOf(MEMBER_VARIABLES_END);
        
        if (memberStartIndex >= 0 && memberEndIndex >= 0)
        {
            memberStartIndex += MEMBER_VARIABLES_START.Length;
            string beforeMember = result.Substring(0, memberStartIndex);
            string afterMember = result.Substring(memberEndIndex);
            result = beforeMember + "\n" + newMemberVariablesCode + afterMember;
        }
        else
        {
            // 如果没有找到成员变量标记，尝试在自动生成部分添加
            int autoStartIndex = result.IndexOf(AUTO_GENERATED_START);
            if (autoStartIndex >= 0)
            {
                int insertPosition = autoStartIndex + AUTO_GENERATED_START.Length;
                string beforeInsert = result.Substring(0, insertPosition);
                string afterInsert = result.Substring(insertPosition);
                result = beforeInsert + $@"
    // 自动生成的成员变量部分
{MEMBER_VARIABLES_START}
{newMemberVariablesCode}{MEMBER_VARIABLES_END}" + afterInsert;
            }
        }
        
        // 替换初始化代码部分
        int initStartIndex = result.IndexOf(INITIALIZATION_START);
        int initEndIndex = result.IndexOf(INITIALIZATION_END);
        
        if (initStartIndex >= 0 && initEndIndex >= 0)
        {
            initStartIndex += INITIALIZATION_START.Length;
            string beforeInit = result.Substring(0, initStartIndex);
            string afterInit = result.Substring(initEndIndex);
            result = beforeInit + "\n" + newInitializationCode + afterInit;
        }
        else
        {
            // 如果没有找到初始化标记，尝试在OnStart方法中添加
            int onStartIndex = result.IndexOf("public override void OnStart()");
            if (onStartIndex >= 0)
            {
                int methodStart = result.IndexOf("{", onStartIndex);
                if (methodStart >= 0)
                {
                    methodStart++; // 跳过 {
                    string beforeMethod = result.Substring(0, methodStart);
                    string afterMethod = result.Substring(methodStart);
                    result = beforeMethod + $@"
        base.OnStart();
        // 初始化代码部分
{INITIALIZATION_START}
{newInitializationCode}{INITIALIZATION_END}" + afterMethod;
                }
            }
        }
        
        return result;
    }

    public static void SaveByGameObject(GameObject go)
    {
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            PrefabUtility.SaveAsPrefabAsset(prefabStage.prefabContentsRoot, prefabStage.assetPath);
            Debug.Log($"Prefab Mode Saved {prefabStage.assetPath}");
        }
        else
        {
            GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (instanceRoot == null)
            {
                AssetDatabase.SaveAssets();
                return;
            }

            GameObject instancePrefab = PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot);
            string assetPath = AssetDatabase.GetAssetPath(instancePrefab);
            PrefabUtility.SaveAsPrefabAsset(instanceRoot, assetPath);
            Debug.Log($"Scene Mode Saved {assetPath}");
        }
    }

    private System.Type GetElementComponentType(Component component)
    {
        if (component == null)
            return typeof(Transform);
        else
            return component.GetType();
    }
    
    // 用于序列化的包装类
    [Serializable]
    private class StringListWrapper
    {
        public List<string> items;
        
        public StringListWrapper(List<string> items)
        {
            this.items = items;
        }
    }
}
