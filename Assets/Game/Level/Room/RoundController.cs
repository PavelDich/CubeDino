using System.Collections;
using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using Zenject;
using Random = UnityEngine.Random;
using System.Linq;
using Mirror.BouncyCastle.Ocsp;
using Mirror.BouncyCastle.Asn1.Cms;

namespace Minicop.Game.CubeDino
{
    public class RoundController : NetworkBehaviour
    {

        [Inject]
        private NetworkManager _networkManager;
        [Inject]
        private NetworkLevel _networkLevel;
        [SerializeField]
        private Transform[] _spawns;
        [SerializeField]
        private NetworkIdentity _player;
        public UnityEvent OnPlayerSpawned = new UnityEvent();

        /// <summary>
        /// Вызов появления игрока
        /// </summary>
        public void SpawnPlayer()
        {
            CmdSpawnPlayer(NetworkLevel.LocalConnection, ConnectionInfo.LocalName);
        }
        [Command(requiresAuthority = false)]
        public void CmdSpawnPlayer(NetworkIdentity networkIdentity, string name)
        {
            SrvSpawnPlayer(networkIdentity, name);
        }
        [Server]
        public void SrvSpawnPlayer(NetworkIdentity networkIdentity, string name)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            GameObject go = Instantiate(_player, _spawns[Random.Range(0, _spawns.Length - 1)]).gameObject;
            go.transform.parent = null;
            if (String.IsNullOrEmpty(name)) name = $"Player {networkIdentity.netId}";
            go.GetComponent<TextVisualize>().Text = name;
            NetworkServer.Spawn(go, networkIdentity.connectionToClient);
#endif
        }
    }
}