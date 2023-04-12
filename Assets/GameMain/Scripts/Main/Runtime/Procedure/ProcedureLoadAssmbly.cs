//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using GameFramework;
using GameFramework.Resource;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
using HybridCLR;
using UnityEngine;

namespace StarForce
{
    public class ProcedureLoadAssembly : ProcedureBase
    {
        static List<string> AotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };

        static List<string> HotfixAssemblyFiles = new List<string>()
        {
            "Hotfix.Runtime.dll",
        };

        private bool LoadDllSuccess;
        private Assembly m_HotfixAssembly;
        private int m_Loaded = 0;
        private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

        public static byte[] GetAssetData(string dllName)
        {
            return s_assetDatas[dllName];
        }

        public override bool UseNativeDialog
        {
            get { return true; }
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            LoadDllSuccess = false;
            if (!GameEntryMain.Base.EditorResourceMode)
            {
                var Loadfiles = new List<string>();
                Loadfiles.AddRange(AotMetaAssemblyFiles);
                Loadfiles.AddRange(HotfixAssemblyFiles);
                m_Loaded = 0;
                foreach (var fileName in Loadfiles)
                {
                    GameEntryMain.Resource.LoadAsset(AssetUtility.GetHotfixAssembly(fileName),
                        Constant.AssetPriority.DLLAsset,
                        new LoadAssetCallbacks(
                            (assetName, asset, duration, userData) =>
                            {
                                Debug.Log("加载成功：" + fileName);

                                var textAsset = asset as TextAsset;
                                s_assetDatas.Add(fileName, textAsset.bytes);

                                m_Loaded++;
                                if (m_Loaded == Loadfiles.Count)
                                {
                                    LoadDllSuccess = true;
                                }
                            },
                            (assetName, status, errorMessage, userData) =>
                            {
                                Debug.LogErrorFormat("Can not load file '{0}' from '{1}' with error message '{2}'.",
                                    fileName,
                                    assetName, errorMessage);
                            }));
                }
            }
            else
            {
                m_HotfixAssembly = Assembly.Load("Hotfix.Runtime");
                LoadDllSuccess = true;
            }
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (LoadDllSuccess)
            {
                LoadDllSuccess = false;

                LoadMetadataForAOTAssemblies();
                m_HotfixAssembly = Assembly.Load(GetAssetData("Hotfix.Runtime.dll"));

                if (null == m_HotfixAssembly)
                {
                    Debug.LogError("Hotfix logic assembly missing.");
                    return;
                }

                StartGameEntry();
            }
        }

        void StartGameEntry()
        {
            GameEntryMain.Resource.LoadAsset(AssetUtility.GetHotfixPrefab("GameEntry"), typeof(GameObject),
                Constant.AssetPriority.DLLAsset,
                new LoadAssetCallbacks(
                    (assetName, asset, duration, userData) =>
                    {
                        Debug.Log("加载成功：GameEntry");
                        GameObject hotfixEntry = GameObject.Instantiate(asset as GameObject);
                        hotfixEntry.transform.SetParent(GameEntryMain.Base.transform.parent);
                    },
                    (assetName, status, errorMessage, userData) =>
                    {
                        Debug.LogErrorFormat("Can not load file '{0}' from '{1}' with error message '{2}'.",
                            assetName,
                            assetName, errorMessage);
                    }));
        }

        void StartGameEntry(Assembly assembly)
        {
            var appType = assembly.GetType("StarForce.GameEntry");
            if (null == appType)
            {
                Debug.LogError("Hotfix.dll type 'GameEntry' missing.");
                return;
            }

            var entryMethod = appType.GetMethod("Start");
            if (null == entryMethod)
            {
                Debug.LogError("StarForce.GameEntry method 'Start' missing.");
                return;
            }

            object[] objects = { new object[] { new List<Assembly>() { m_HotfixAssembly } } };
            entryMethod.Invoke(appType, objects);
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private static void LoadMetadataForAOTAssemblies()
        {
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in AotMetaAssemblyFiles)
            {
                byte[] dllBytes = GetAssetData(aotDllName);
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }
    }
}