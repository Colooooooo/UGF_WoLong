//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace StarForce
{
    public class ProcedureCheckVersion : ProcedureBase
    {
        private bool m_CheckVersionComplete = false;
        private bool m_NeedUpdateVersion = false;
        private VersionInfo m_VersionInfo = null;

        public override bool UseNativeDialog
        {
            get
            {
                return true;
            }
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_CheckVersionComplete = false;
            m_NeedUpdateVersion = false;
            m_VersionInfo = null;

            GameEntryMain.Event.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            GameEntryMain.Event.Subscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            // 向服务器请求版本信息
            GameEntryMain.WebRequest.AddWebRequest(Utility.Text.Format(GameEntryMain.BuiltinData.BuildInfo.CheckVersionUrl, GetPlatformPath()), this);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            GameEntryMain.Event.Unsubscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            GameEntryMain.Event.Unsubscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_CheckVersionComplete)
            {
                return;
            }

            if (m_NeedUpdateVersion)
            {
                procedureOwner.SetData<VarInt32>("VersionListLength", m_VersionInfo.VersionListLength);
                procedureOwner.SetData<VarInt32>("VersionListHashCode", m_VersionInfo.VersionListHashCode);
                procedureOwner.SetData<VarInt32>("VersionListCompressedLength", m_VersionInfo.VersionListCompressedLength);
                procedureOwner.SetData<VarInt32>("VersionListCompressedHashCode", m_VersionInfo.VersionListCompressedHashCode);
                ChangeState<ProcedureUpdateVersion>(procedureOwner);
            }
            else
            {
                ChangeState<ProcedureVerifyResources>(procedureOwner);
            }
        }

        private void GotoUpdateApp(object userData)
        {
            string url = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            url = GameEntryMain.BuiltinData.BuildInfo.WindowsAppUrl;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            url = GameEntryMain.BuiltinData.BuildInfo.MacOSAppUrl;
#elif UNITY_IOS
            url = GameEntryMain.BuiltinData.BuildInfo.IOSAppUrl;
#elif UNITY_ANDROID
            url = GameEntryMain.BuiltinData.BuildInfo.AndroidAppUrl;
#endif
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
        }

        private void OnWebRequestSuccess(object sender, GameEventArgs e)
        {
            WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            // 解析版本信息
            byte[] versionInfoBytes = ne.GetWebResponseBytes();
            string versionInfoString = Utility.Converter.GetString(versionInfoBytes);
            m_VersionInfo = Utility.Json.ToObject<VersionInfo>(versionInfoString);
            if (m_VersionInfo == null)
            {
                Log.Error("Parse VersionInfo failure.");
                return;
            }

            Log.Info("Latest game version is '{0} ({1})', local game version is '{2} ({3})'.", m_VersionInfo.LatestGameVersion, m_VersionInfo.InternalGameVersion.ToString(), Version.GameVersion, Version.InternalGameVersion.ToString());

            if (m_VersionInfo.ForceUpdateGame)
            {
                // 需要强制更新游戏应用
                GameEntryMain.UI.OpenDialog(new DialogParams
                {
                    Mode = 2,
                    Title = GameEntryMain.Localization.GetString("ForceUpdate.Title"),
                    Message = GameEntryMain.Localization.GetString("ForceUpdate.Message"),
                    ConfirmText = GameEntryMain.Localization.GetString("ForceUpdate.UpdateButton"),
                    OnClickConfirm = GotoUpdateApp,
                    CancelText = GameEntryMain.Localization.GetString("ForceUpdate.QuitButton"),
                    OnClickCancel = delegate (object userData) { UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
                });

                return;
            }

            // 设置资源更新下载地址
            GameEntryMain.Resource.UpdatePrefixUri = Utility.Path.GetRegularPath(m_VersionInfo.UpdatePrefixUri);

            m_CheckVersionComplete = true;
            m_NeedUpdateVersion = GameEntryMain.Resource.CheckVersionList(m_VersionInfo.InternalResourceVersion) == CheckVersionListResult.NeedUpdate;
        }

        private void OnWebRequestFailure(object sender, GameEventArgs e)
        {
            WebRequestFailureEventArgs ne = (WebRequestFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Warning("Check version failure, error message is '{0}'.", ne.ErrorMessage);
        }

        private string GetPlatformPath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";

                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "MacOS";

                case RuntimePlatform.IPhonePlayer:
                    return "IOS";

                case RuntimePlatform.Android:
                    return "Android";

                default:
                    throw new System.NotSupportedException(Utility.Text.Format("Platform '{0}' is not supported.", Application.platform));
            }
        }
    }
}
