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
            Procedure.ResetProcedure(m_EntranceProcedureTypeName);
        }
    }
}
