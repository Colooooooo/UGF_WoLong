//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityGameFramework.Runtime;

namespace StarForce
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        private static List<Assembly> m_HotfixAssemblys;
        private static ProcedureBase m_EntranceProcedureBase;
        private static string m_EntranceProcedureTypeName = "StarForce.ProcedurePreload";
        public static void Start(object[] objects)
        {
            m_HotfixAssemblys = (List<Assembly>)objects[0];
            
            InitBuiltinComponents();
            InitCustomComponents();

            ResetProcedure();
        }
        
        private static void ResetProcedure() 
        {
            //卸载流程
            Fsm.DestroyFsm<GameFramework.Procedure.IProcedureManager>();
            GameFramework.Procedure.IProcedureManager procedureManager = GameFramework.GameFrameworkEntry.GetModule<GameFramework.Procedure.IProcedureManager>();
            //创建新的流程 HotfixFramework.Runtime
            string[] m_ProcedureTypeNames = TypeUtils.GetRuntimeTypeNames(typeof(ProcedureBase), m_HotfixAssemblys);
            ProcedureBase[] procedures = new ProcedureBase[m_ProcedureTypeNames.Length];
            for (int i = 0; i < m_ProcedureTypeNames.Length; i++)
            {
                Type procedureType = GameFramework.Utility.Assembly.GetType(m_ProcedureTypeNames[i]);
                if (procedureType == null)
                {
                    Log.Error("Can not find procedure type '{0}'.", m_ProcedureTypeNames[i]);
                    return;
                }

                procedures[i] = (ProcedureBase)Activator.CreateInstance(procedureType);
                if (procedures[i] == null)
                {
                    Log.Error("Can not create procedure instance '{0}'.", m_ProcedureTypeNames[i]);
                    return;
                }

                if (m_EntranceProcedureTypeName == m_ProcedureTypeNames[i])
                {
                    m_EntranceProcedureBase = procedures[i];
                }
            }

            if (m_EntranceProcedureBase == null)
            {
                Log.Error("Entrance procedure is invalid.");
                return;
            }
            procedureManager.Initialize(GameFramework.GameFrameworkEntry.GetModule<GameFramework.Fsm.IFsmManager>(), procedures);
            procedureManager.StartProcedure(m_EntranceProcedureBase.GetType());
        }
    }
}
