//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;
using GameFramework.Resource;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
#if !HybridCLR
namespace StarForce
{
    public class ProcedureLoadAssembly : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Debug.LogWarning("开启热更请导入HybridCLR");
        }
    }
}

#else
using HybridCLR;
namespace GT.Main
{
    public class ProcedureLoadAssembly : ProcedureBase
    {
        static List<string> AotMetaAssemblyFiles = new()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
            "UniRx.dll",
            "UniTask.dll",
        };

        static List<string> HotfixAssemblyFiles = new()
        {
            "Hotfix.Runtime.dll",
        };

        private bool LoadDllSuccess;
        private Assembly m_HotfixAssembly;
        private int m_Loaded;
        
        private Dictionary<string, byte[]> s_assetDatas = new();
        public byte[] GetAssetData(string dllName)
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
            if (!GameEntryMain.Base.EditorResourceMode)
            {
                LoadDllSuccess = false;
                LoadDlls();
            }
            else
            {
                ChangeState<ProcedureGameEntry>(procedureOwner);
            }
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (LoadDllSuccess)
            {
                LoadMetadataForAOTAssemblies();
                LoadHotfixAssembly();

                ChangeState<ProcedureGameEntry>(procedureOwner);
            }
        }

        private void LoadDlls()
        {
            var loadfiles = new List<string>();
            loadfiles.AddRange(AotMetaAssemblyFiles);
            loadfiles.AddRange(HotfixAssemblyFiles);
            m_Loaded = 0;
            foreach (var fileName in loadfiles)
            {
                GameEntryMain.Resource.LoadAsset(AssetUtility.GetHotfixAssembly(fileName), Constant.AssetPriority.DLLAsset,
                    new LoadAssetCallbacks((assetName, asset, duration, userData) =>
                    {
                        Debug.Log("加载成功：" + fileName);

                        var textAsset = asset as TextAsset;
                        s_assetDatas.Add(fileName, textAsset.bytes);
                        if (++m_Loaded == loadfiles.Count)
                        {
                            LoadDllSuccess = true;
                        }
                    }, (assetName, status, errorMessage, userData) =>
                    {
                        Debug.LogErrorFormat("Can not load file '{0}' from '{1}' with error message '{2}'.",
                            fileName, assetName, errorMessage);
                    }));
            }
        }
        /// <summary>
        /// 加载Hotfix程序集
        /// </summary>
        private void LoadHotfixAssembly()
        {
            m_HotfixAssembly = Assembly.Load(GetAssetData("Hotfix.Runtime.dll"));
            if (null == m_HotfixAssembly)
            {
                Debug.LogError("Hotfix logic assembly missing.");
            }
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private void LoadMetadataForAOTAssemblies()
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
#endif