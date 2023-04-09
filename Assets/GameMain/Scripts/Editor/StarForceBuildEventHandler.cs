//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace StarForce.Editor
{
    public sealed class StarForceBuildEventHandler : IBuildEventHandler
    {
        private const string UpdateURL = "http://192.168.3.29/Full";
        private static string OutPutDirectory { get; set; }
        private static VersionInfo m_VersionInfo = new VersionInfo();
        
        public bool ContinueOnFailure
        {
            get
            {
                return false;
            }
        }

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions,
            string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            string streamingAssetsPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(streamingAssetsPath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                if (fileName.Contains(".gitkeep"))
                {
                    continue;
                }

                File.Delete(fileName);
            }

            Utility.Path.RemoveEmptyDirectory(streamingAssetsPath);
            
            //赋值Version需要参数
            m_VersionInfo.LatestGameVersion = applicableGameVersion;
            m_VersionInfo.InternalResourceVersion= internalResourceVersion;
            m_VersionInfo.InternalGameVersion = 1;
            OutPutDirectory = outputDirectory;
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions,
            string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
        }

        public void OnPreprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {
        }

        public void OnBuildAssetBundlesComplete(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {
        }

        public void OnOutputUpdatableVersionListData(Platform platform, string versionListPath, int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode)
        {
            //赋值Version版本相关参数
            m_VersionInfo.ForceUpdateGame = false;
            m_VersionInfo.VersionListLength = versionListLength;
            m_VersionInfo.VersionListHashCode = versionListHashCode;
            m_VersionInfo.VersionListCompressedLength = versionListCompressedLength;
            m_VersionInfo.VersionListCompressedHashCode = versionListCompressedHashCode;
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, bool isSuccess)
        {
            if (!outputPackageSelected)
            {
                return;
            }
            //保存Version.txt到Full目录
            var versionName = m_VersionInfo.LatestGameVersion.Replace(".","_") + "_" + m_VersionInfo.InternalResourceVersion;
            m_VersionInfo.UpdatePrefixUri = Utility.Path.GetRegularPath(Path.Combine(UpdateURL, versionName, platform.ToString()));
            var versionJson = JsonConvert.SerializeObject(m_VersionInfo, Formatting.Indented);
            var savePath = Utility.Path.GetRegularPath(Path.Combine(OutPutDirectory, "Full", platform + "Version.txt"));
            File.WriteAllText(savePath,versionJson);
            Debug.Log("Version.txt save success!!!");
            
            if (platform != Platform.Windows)
            {
                return;
            }

            string streamingAssetsPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(outputPackagePath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                string destFileName = Utility.Path.GetRegularPath(Path.Combine(streamingAssetsPath, fileName.Substring(outputPackagePath.Length)));
                FileInfo destFileInfo = new FileInfo(destFileName);
                if (!destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }

                File.Copy(fileName, destFileName);
            }
        }
    }
}
