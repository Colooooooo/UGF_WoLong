//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace StarForce
{
    /// <summary>
    /// Hotfix游戏入口。
    /// </summary>
    public partial class GameEntry: MonoBehaviour
    {
        public void Start()
        {
            InitBuiltinComponents();
            InitCustomComponents();

            Procedure.ResetProcedure();
            HPBar.InitObjectPool();
        }
    }
}
