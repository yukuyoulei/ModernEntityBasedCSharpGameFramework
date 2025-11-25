using System;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Text;

public class OnGenerateCSProjectProcessor : AssetPostprocessor
{
    public static string OnGeneratedCSProject(string path, string content)
    {
        if (path.EndsWith("Logic.csproj"))
        {
            content = content.Replace("<RootNamespace />", "<RootNamespace>MEBCGF</RootNamespace>");
            content = content.Replace("<Compile Include=\"Assets\\ExternalCodes\\Logic\\Empty.cs\" />", string.Empty);
            content = content.Replace("<None Include=\"Assets\\ExternalCodes\\Logic\\Logic.asmdef\" />", string.Empty);
            return GenerateCustomProject(path, content, @"Logic\**\*.cs", @"..\Codes\Base\**\*.cs", @"LogicBase\**\*.cs");
        }

        return content;
    }

    private static string GenerateCustomProject(string path, string content, params string[] codesPaths)
    {
        Debug.Log($"GenerateCustomProject {path}");
        int startIdx = content.IndexOf("<OutputPath>", StringComparison.Ordinal);
        int endIdx = content.IndexOf("</OutputPath>", StringComparison.Ordinal);
        string betweenTags = content.Substring(startIdx, endIdx - startIdx);
        content = content.Replace(betweenTags, $"<OutputPath>{CodeLoader.DllOutputPath}");
        content = content.Replace("<DebugType>full<", "<DebugType>portable<");

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(content);

        var newDoc = doc.Clone() as XmlDocument;

        var rootNode = newDoc.GetElementsByTagName("Project")[0];

        var elementNewPropertyGroup = newDoc.CreateElement("PropertyGroup", newDoc.DocumentElement.NamespaceURI);
        var elementPostBuildEvent = newDoc.CreateElement("PostBuildEvent", elementNewPropertyGroup.NamespaceURI);
        elementNewPropertyGroup.AppendChild(elementPostBuildEvent);
        elementPostBuildEvent.InnerText = @"echo f| xcopy /r /y $(TargetDir)$(ProjectName).dll $(TargetDir)..\..\..\Library\ScriptAssemblies\$(ProjectName).dll
echo f| xcopy /r /y $(TargetDir)$(ProjectName).pdb $(TargetDir)..\..\..\Library\ScriptAssemblies\$(ProjectName).pdb";
        rootNode.AppendChild(elementNewPropertyGroup);

        foreach (var codesPath in codesPaths)
        {
            var itemGroup = newDoc.CreateElement("ItemGroup", newDoc.DocumentElement.NamespaceURI);
            var compile = newDoc.CreateElement("Compile", newDoc.DocumentElement.NamespaceURI);
            compile.SetAttribute("Include", codesPath);
            itemGroup.AppendChild(compile);
            rootNode.AppendChild(itemGroup);
        }

        using (StringWriter sw = new StringWriter())
        {
            using (XmlTextWriter tx = new XmlTextWriter(sw))
            {
                tx.Formatting = Formatting.Indented;
                newDoc.WriteTo(tx);
                tx.Flush();
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}