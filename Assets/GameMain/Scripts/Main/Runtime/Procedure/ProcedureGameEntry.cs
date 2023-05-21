//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.Resource;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace StarForce
{
    public class ProcedureGameEntry : ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get { return true; }
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            StartGameEntry();
        }

        void StartGameEntry()
        {
            GameEntryMain.Resource.LoadAsset(AssetUtility.GetHotfixPrefab("GameEntry"), typeof(GameObject), Constant.AssetPriority.DLLAsset,
                new LoadAssetCallbacks((assetName, asset, duration, userData) =>
                    {
                        Debug.Log("加载成功：GameEntry");
                        GameObject hotfixEntry = GameObject.Instantiate(asset as GameObject);
                        hotfixEntry.name = "Custom";
                        hotfixEntry.transform.SetParent(GameEntryMain.Base.transform.parent);
                    }, (assetName, status, errorMessage, userData) =>
                    {
                        Debug.LogErrorFormat("Can not load file '{0}' from '{1}' with error message '{2}'.", assetName, assetName, errorMessage);
                    }));
        }
    }
}