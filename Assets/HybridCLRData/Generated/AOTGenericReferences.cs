public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	// GameFramework.dll
	// UnityEngine.CoreModule.dll
	// UnityGameFramework.Runtime.dll
	// mscorlib.dll
	// protobuf-net.dll
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// GameFramework.DataTable.IDataTable<object>
	// GameFramework.Fsm.FsmState<object>
	// GameFramework.Fsm.IFsm<object>
	// GameFramework.GameFrameworkAction<object>
	// GameFramework.ObjectPool.IObjectPool<object>
	// GameFramework.Variable<byte>
	// System.Collections.Generic.Dictionary<StarForce.GameMode,object>
	// System.Collections.Generic.Dictionary<System.Collections.Generic.KeyValuePair<StarForce.CampType,StarForce.RelationType>,object>
	// System.Collections.Generic.Dictionary<StarForce.AIUtility.CampPair,StarForce.RelationType>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary.Enumerator<object,byte>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.KeyValuePair<object,byte>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<StarForce.CampType,StarForce.RelationType>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.List<StarForce.CampType>
	// System.EventHandler<object>
	// System.Nullable<int>
	// }}

	public void RefMethods()
	{
		// System.Void GameFramework.Fsm.FsmState<object>.ChangeState<object>(GameFramework.Fsm.IFsm<object>)
		// object GameFramework.Fsm.IFsm<object>.GetData<object>(string)
		// System.Void GameFramework.Fsm.IFsm<object>.SetData<object>(string,object)
		// object GameFramework.GameFrameworkEntry.GetModule<object>()
		// System.Void GameFramework.Network.INetworkChannel.Send<object>(object)
		// object GameFramework.ReferencePool.Acquire<object>()
		// string GameFramework.Utility.Text.Format<object>(string,object)
		// string GameFramework.Utility.Text.Format<int>(string,int)
		// System.Void ProtoBuf.Serializer.Serialize<object>(System.IO.Stream,object)
		// object[] System.Array.Empty<object>()
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInParent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object)
		// object UnityExtension.GetOrAddComponent<object>(UnityEngine.GameObject)
		// GameFramework.DataTable.IDataTable<object> UnityGameFramework.Runtime.DataTableComponent.GetDataTable<object>()
		// bool UnityGameFramework.Runtime.FsmComponent.DestroyFsm<object>()
		// object UnityGameFramework.Runtime.GameEntry.GetComponent<object>()
		// System.Void UnityGameFramework.Runtime.Log.Error<object,object>(string,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Error<object>(string,object)
		// System.Void UnityGameFramework.Runtime.Log.Error<object,object,object>(string,object,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object,object,object>(string,object,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object>(string,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object,object>(string,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Info<object,object,object,object>(string,object,object,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Warning<object>(string,object)
		// System.Void UnityGameFramework.Runtime.Log.Warning<object,object,object>(string,object,object,object)
		// System.Void UnityGameFramework.Runtime.Log.Warning<object,object>(string,object,object)
		// GameFramework.ObjectPool.IObjectPool<object> UnityGameFramework.Runtime.ObjectPoolComponent.CreateSingleSpawnObjectPool<object>(string,int)
	}
}