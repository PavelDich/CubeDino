using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Minicop.Game.CubeDino
{
    public class Spawner : NetworkBehaviour
    {
        public float SpawnRange = 2f;
        private Transform _transform;
        private void Start()
        {
            _transform = transform;
        }
        [SerializeField]
        private NetworkIdentity _object;


        /// <summary>
        /// Спавн обьекта перед лицом
        /// </summary>
        public void Spawn()
        {
            CmdSpawn();
        }
        [Command(requiresAuthority = false)]
        public void CmdSpawn()
        {
            SrvSpawn();
        }
        [Server]
        public void SrvSpawn()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            GameObject go = Instantiate(_object, _transform).gameObject;
            go.transform.position += _transform.forward * SpawnRange;
            go.transform.parent = null;
            NetworkServer.Spawn(go);
#endif
        }
    }
}