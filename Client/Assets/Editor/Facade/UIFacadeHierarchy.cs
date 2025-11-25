using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class UIFacadeHierarchy : Editor
{
    // 存储当前正在编辑的控件信息
    private static int editingInstanceID = -1;
    private static string editingText = "";

    static UIFacadeHierarchy()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItem;
    }

    static void HierarchyWindowItem(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (obj == null)
            return;
        if (obj.GetComponent<MonoFacade>() != null)
        {
            var text = "★";
            Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
            float xx = selectionRect.x + selectionRect.width - textSize.x;
            float yy = selectionRect.y;
            GUI.Label(new Rect(xx, yy, textSize.x, textSize.y), text, styleUIFacade.Value);
        }
        if (obj.transform.parent != null)
        {
            var uif = obj.transform.parent.GetComponentInParent<MonoFacade>();
            if (uif == null)
                return;
            if (uif.uielements == null)
                return;
            UIElement found = null;
            foreach (var uiele in uif.uielements)
            {
                if (uiele == null)
                    continue;
                if (uiele.component == null)
                    continue;
                if (uiele.component?.gameObject == obj)
                {
                    found = uiele;
                    break;
                }
            }
            if (found == null)
            {
                float x = selectionRect.x + selectionRect.width - 54;
                float y = selectionRect.y;
                if (GUI.Button(new Rect(x, y, 35, 18), "+"))
                {
                    uif.uielements.Add(new UIElement()
                    {
                        component = obj.transform,
                        comtype = "Transform",
                        name = obj.name,
                    });
                    var o = uif.gameObject;
                    while (o.transform.parent != null)
                    {
                        var p = o.transform.parent;
                        var pf = p.GetComponentInParent<MonoFacade>();
                        if (pf == null)
                            break;
                        o = pf.gameObject;
                    }
                    UIFacadeInspector.SaveByGameObject(o);
                }
            }
            else
            {
                // 使用可编辑的文本字段替代只读文本显示
                string text = found.name;
                Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));

                // 计算文本框和按钮的位置
                float textFieldWidth = textSize.x + 5;
                float textFieldX = selectionRect.x + selectionRect.width - textFieldWidth - 35; // 调整位置为文本框留出空间
                float textFieldY = selectionRect.y;
                
                // 检查是否正在编辑这个控件
                Rect textFieldRect = new Rect(textFieldX, textFieldY, textFieldWidth, textSize.y);
                if (editingInstanceID == instanceID)
                {
                    // 处理文本编辑
                    string newText = EditorGUI.TextField(textFieldRect, editingText, styleUIElement.Value);
                    
                    // 如果文本发生变化，更新编辑中的文本
                    if (newText != editingText)
                    {
                        editingText = newText;
                    }
                    
                    // 检查是否失去焦点（通过检查鼠标点击是否在文本框外）
                    if (Event.current.type == EventType.MouseDown && !textFieldRect.Contains(Event.current.mousePosition))
                    {
                        // 失去焦点，保存更改
                        if (editingText != text)
                        {
                            found.name = editingText;
                            var o = uif.gameObject;
                            while (o.transform.parent != null)
                            {
                                var p = o.transform.parent;
                                var pf = p.GetComponentInParent<MonoFacade>();
                                if (pf == null)
                                    break;
                                o = pf.gameObject;
                            }
                            UIFacadeInspector.SaveByGameObject(o);
                        }
                        
                        // 结束编辑状态
                        editingInstanceID = -1;
                        editingText = "";
                        Event.current.Use(); // 防止事件继续传播
                    }
                }
                else
                {
                    // 显示只读文本，点击时进入编辑状态
                    if (GUI.Button(textFieldRect, text, styleUIElement.Value))
                    {
                        // 进入编辑状态
                        editingInstanceID = instanceID;
                        editingText = text;
                        GUI.FocusControl(""); // 清除焦点以避免冲突
                    }
                }
                
                // 在文本框右侧添加断开引用按钮
                float buttonX = textFieldX + textFieldWidth + 2;
                if (GUI.Button(new Rect(buttonX, textFieldY, 15, 18), "-"))
                {
                    // 从uielements列表中完全移除该元素
                    uif.uielements.Remove(found);
                    var o = uif.gameObject;
                    while (o.transform.parent != null)
                    {
                        var p = o.transform.parent;
                        var pf = p.GetComponentInParent<MonoFacade>();
                        if (pf == null)
                            break;
                        o = pf.gameObject;
                    }
                    UIFacadeInspector.SaveByGameObject(o);
                    
                    // 如果正在编辑这个控件，退出编辑状态
                    if (editingInstanceID == instanceID)
                    {
                        editingInstanceID = -1;
                        editingText = "";
                    }
                }
            }
        }
    }
    static Lazy<GUIStyle> styleUIElement = new Lazy<GUIStyle>(() =>
     {
         return new GUIStyle()
         {
             alignment = TextAnchor.MiddleRight,
             fontSize = 12,
             normal = new GUIStyleState()
             {
                 textColor = Color.green,
             },
         };
     });
    static Lazy<GUIStyle> styleUIFacade = new Lazy<GUIStyle>(() =>
     {
         return new GUIStyle()
         {
             alignment = TextAnchor.MiddleCenter,
             fontSize = 15,
             normal = new GUIStyleState()
             {
                 textColor = Color.yellow,
             },
         };
     });
}
