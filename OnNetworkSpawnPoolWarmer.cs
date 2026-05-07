using GrygTools.Pooler;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace GrygTools.NetworkPooler
{
	public class OnNetworkSpawnPoolWarmer : NetworkBehaviour
	{
		[SerializeReference]
		private PoolWarmingConfig[] m_ObjectsToWarm;
		
		private Dictionary<GameObject, NetworkPool> m_Pools = new ();

		public override async void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			foreach (PoolWarmingConfig config in m_ObjectsToWarm)
			{
				PoolManager.Instance.TryGetPool(config.Template, out IPool pool);
				if (pool == null)
				{
					var networkPool = new NetworkPool();
					NetworkManager.Singleton.PrefabHandler.AddHandler(config.Template, networkPool);
					m_Pools.Add(config.Template, networkPool);
					await networkPool.Init(config.Template);
					PoolManager.Instance.AddPool(config.Template, networkPool);
					pool = networkPool;
				}
				pool.WarmPool(config.Amount);
			}
		}
		
		public void OnValidate()
		{
			for (var i = 0; i < m_ObjectsToWarm.Length; i++)
			{
				var prefab = m_ObjectsToWarm[i].Template;
				if (prefab != null)
				{
					Assert.IsNotNull(prefab.GetComponent<NetworkObject>(), $"{nameof(OnNetworkSpawnPoolWarmer)}: Template prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
				}
			}
		}
	}
}
