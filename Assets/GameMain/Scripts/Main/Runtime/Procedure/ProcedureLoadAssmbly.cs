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

        public override bool UseNativeDialog
        {
            get { return true; }
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            LoadDllSuccess = false;
            foreach (var fileName in HotfixAssemblyFiles)
            {
                GameEntryMain.Resource.LoadAsset(AssetUtility.GetHotfixAsset(fileName), Constant.AssetPriority.DLLAsset,
                    new LoadAssetCallbacks(
                        (assetName, asset, duration, userData) =>
                        {
                            var textAsset = asset as TextAsset;
                            m_HotfixAssembly = Assembly.Load(textAsset.bytes);
                            LoadDllSuccess = true;
                            Debug.Log("加载成功：" + fileName);
                        },
                        (assetName, status, errorMessage, userData) =>
                        {
                            Debug.LogErrorFormat("Can not load file '{0}' from '{1}' with error message '{2}'.",
                                fileName,
                                assetName, errorMessage);
                        }));
            }
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (LoadDllSuccess)
            {
                LoadDllSuccess = false;
                AllAsmLoadComplete();
            }
        }

        private void AllAsmLoadComplete()
        {
            if (null == m_HotfixAssembly)
            {
                Debug.LogError("Main logic assembly missing.");
                return;
            }

            var appType = m_HotfixAssembly.GetType("StarForce.GameEntry");
            if (null == appType)
            {
                Debug.LogError("Main logic type 'AppMain' missing.");
                return;
            }

            var entryMethod = appType.GetMethod("Start");
            if (null == entryMethod)
            {
                Debug.LogError("Main logic entry method 'Entrance' missing.");
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
                // byte[] dllBytes = BetterStreamingAssets.ReadAllBytes(aotDllName + ".bytes");
                // // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                // LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                // Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }
    }
}