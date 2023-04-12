using System;
using System.Collections.Generic;
using System.Reflection;
using UnityGameFramework.Runtime;

namespace StarForce
{
    public static class ProcedureComponentHotfix
    {
        private const string EntranceProcedureTypeName = "StarForce.ProcedurePreload";

        private static List<Assembly> s_HotfixAssemblys;
        private static ProcedureBase s_EntranceProcedureBase;

        public static void ResetProcedure(this ProcedureComponent component)
        {
            //卸载流程
            GameEntry.Fsm.DestroyFsm<GameFramework.Procedure.IProcedureManager>();
            var procedureManager =
                GameFramework.GameFrameworkEntry.GetModule<GameFramework.Procedure.IProcedureManager>();
            s_HotfixAssemblys = new List<Assembly>() { Assembly.GetAssembly(typeof(GameEntry)) };

            //创建新的流程 HotfixFramework.Runtime
            string[] procedureTypeNames = TypeUtils.GetRuntimeTypeNames(typeof(ProcedureBase), s_HotfixAssemblys);

            ProcedureBase[] procedures = new ProcedureBase[procedureTypeNames.Length];
            for (int i = 0; i < procedureTypeNames.Length; i++)
            {
                Type procedureType = GameFramework.Utility.Assembly.GetType(procedureTypeNames[i]);
                if (procedureType == null)
                {
                    Log.Error("Can not find procedure type '{0}'.", procedureTypeNames[i]);
                    return;
                }

                procedures[i] = (ProcedureBase)Activator.CreateInstance(procedureType);
                if (procedures[i] == null)
                {
                    Log.Error("Can not create procedure instance '{0}'.", procedureTypeNames[i]);
                    return;
                }

                if (EntranceProcedureTypeName == procedureTypeNames[i])
                {
                    s_EntranceProcedureBase = procedures[i];
                }
            }

            if (s_EntranceProcedureBase == null)
            {
                Log.Error("Entrance procedure is invalid.");
                return;
            }

            procedureManager.Initialize(GameFramework.GameFrameworkEntry.GetModule<GameFramework.Fsm.IFsmManager>(),
                procedures);
            procedureManager.StartProcedure(s_EntranceProcedureBase.GetType());
        }
    }
}