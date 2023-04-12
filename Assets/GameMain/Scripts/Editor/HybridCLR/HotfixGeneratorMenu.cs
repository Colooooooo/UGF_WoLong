using System.Collections;
using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;

public class HotfixGeneratorMenu : MonoBehaviour
{
    private const string HotfixAssetPath = "GameMain/Hotfix";

    [MenuItem("HybridCLR/Build/Build Hotfix.dll")]
    public static void GeneratorHotfix()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        CompileDllCommand.CompileDll(target);

        CopyToAsset();
    }

    private static void CopyToAsset()
    {
        CopyAOTAssembliesToAssets();
        CopyHotUpdateAssembliesToAssets();
    }

    public static void CopyAOTAssembliesToAssets()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        string aotAssembliesDstDir = Path.Combine(Application.dataPath, HotfixAssetPath);
        if (!Directory.Exists(aotAssembliesDstDir))
        {
            Directory.CreateDirectory(aotAssembliesDstDir);
        }
        foreach (var dll in SettingsUtil.AOTAssemblyNames)
        {
            string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
            if (!File.Exists(srcDllPath))
            {
                Debug.LogError($"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                continue;
            }

            string dllBytesPath = $"{aotAssembliesDstDir}/{dll}.dll.bytes";
            File.Copy(srcDllPath, dllBytesPath, true);
            Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
        }
    }

    public static void CopyHotUpdateAssembliesToAssets()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;

        string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        string hotfixAssembliesDstDir = Path.Combine(Application.dataPath, HotfixAssetPath);
        if (!Directory.Exists(hotfixAssembliesDstDir))
        {
            Directory.CreateDirectory(hotfixAssembliesDstDir);
        }
        foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            string dllPath = $"{hotfixDllSrcDir}/{dll}";
            string dllBytesPath = $"{hotfixAssembliesDstDir}/{dll}.bytes";
            File.Copy(dllPath, dllBytesPath, true);
            Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
        }
    }
}