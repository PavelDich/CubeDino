using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Minicop.Game.GravityRave;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Zenject;

namespace Minicop.Game.CubeDino
{
    public class NetworkLevel : NetworkBehaviour
    {

        [Inject]
        public DiContainer _diContainer;
        [Inject]
        public NetworkManager _networkManager;
        [SerializeField]
        private static NetworkIdentity _localConnection;
        public static NetworkIdentity LocalConnection
        {
            get
            {
                return _localConnection;
            }
            set
            {
                if (_localConnection != null) return;
                _localConnection = value;
                Debug.Log("Local Player Find");
            }
        }
        public List<NetworkIdentity> Connections = new List<NetworkIdentity>();
        public UnityEvent OnLocalConnectionLeave = new UnityEvent();
        public UnityEvent OnLocalConnectionEnter = new UnityEvent();
        public UnityEvent<NetworkIdentity> OnClientConnectionEnter = new UnityEvent<NetworkIdentity>();
        public UnityEvent OnClientConnectionLeave = new UnityEvent();
        public UnityEvent<NetworkIdentity> OnServerConnectionEnter = new UnityEvent<NetworkIdentity>();
        public UnityEvent OnServerConnectionLeave = new UnityEvent();
        public int SceneId
        {
            get { return _data.SceneId; }
            set { _data.SceneId = value; }
        }

        private void Start()
        {
            if (!NetworkServer.active) return;
            NetworkManager.OnPlayerDisconnect.AddListener(SrvRemovePlayer);
        }

        private void OnEnable()
        {
            if (!NetworkClient.active) return;
            StartCoroutine(WaitSpawnConnection());
        }

        private void OnDisable()
        {
            if (!NetworkClient.active) return;
            CmdRemoveConnection(LocalConnection);
            OnLocalConnectionLeave.Invoke();
        }

        /// <summary>
        /// Получение локального IP адреса
        /// </summary>
        /// <returns>ip adress</returns>
        public string GetIpAdress()
        {
            string ipAdress = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAdress = ip.ToString();
                }
            }
            return ipAdress;
        }
        /// <summary>
        /// Ожидание появления подключений и их регистрация
        /// </summary>
        /// <returns>null</returns>
        public IEnumerator WaitSpawnConnection()
        {
            yield return new WaitForEndOfFrame();
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                NetworkIdentity networkIdentity = player.GetComponent<NetworkIdentity>();
                if (networkIdentity.isOwned)
                {
                    LocalConnection = networkIdentity;
                    CmdAddConnection(networkIdentity);
                    break;
                }
            }
            OnLocalConnectionEnter.Invoke();
        }
        [Command(requiresAuthority = false)]
        public void CmdAddConnection(NetworkIdentity networkIdentity)
        {
            Connections.Add(networkIdentity);
            OnServerConnectionEnter.Invoke(networkIdentity);
            foreach (NetworkIdentity connection in Connections)
                RpcConnectionEnter(connection.connectionToClient, networkIdentity);
        }
        /// <summary>
        /// Удаление перемещенных подключений
        /// </summary>
        /// <param name="networkIdentity"></param>
        [Command(requiresAuthority = false)]
        void CmdRemoveConnection(NetworkIdentity networkIdentity)
        {
            StartCoroutine(Remove());
            IEnumerator Remove()
            {
                yield return new WaitForEndOfFrame();
                Connections.Remove(networkIdentity);
                Connections.RemoveAll(p => p == null);
            }
        }

        /// <summary>
        /// Обратный вызов локальный клиент зарегестрирован
        /// </summary>
        /// <param name="networkConnection"></param>
        /// <param name="networkIdentity">подключение</param>
        [TargetRpc]
        public void RpcConnectionEnter(NetworkConnection networkConnection, NetworkIdentity networkIdentity)
        {
            OnClientConnectionEnter.Invoke(networkIdentity);
        }

        /// <summary>
        /// Удаление отключившихся подключения
        /// </summary>
        [Server]
        void SrvRemovePlayer()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            StartCoroutine(Remove());
            OnServerConnectionLeave.Invoke();
            IEnumerator Remove()
            {
                yield return new WaitForEndOfFrame();
                Connections.RemoveAll(p => p == null);
            }
#endif
        }

        /// <summary>
        /// Завершение программы
        /// </summary>
        public void Quit()
        {
            Application.Quit();
        }


        /// <summary>
        /// Отключение от сервера
        /// </summary>
        public void Disconnect()
        {
            _networkManager.Disconnect();
        }

        #region Network
        [SyncVar]
        [SerializeField]
        private Data _data = new Data();
        [System.Serializable]
        public struct Data
        {
            public int SceneId;
        }
        #endregion

        /// <summary>
        /// Получение данных в виде обьекта
        /// </summary>
        /// <returns>обьект с данными</returns>
        public object GetData()
        {
            return _data;
        }
        /// <summary>
        /// Загрузка данных из обьекта
        /// </summary>
        /// <param name="data">обьект</param>
        public void SetData(object data)
        {
            _data = (NetworkLevel.Data)data;
        }
    }

    public static class NetworkLevelSerializer
    {
        /// <summary>
        /// Сериализация обьекта в сеть
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="item"></param>
        public static void Write(this NetworkWriter writer, NetworkLevel.Data item)
        {
            writer.WriteInt(item.SceneId);
        }
        /// <summary>
        /// Десериализация обьекта из сети
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static NetworkLevel.Data Read(this NetworkReader reader)
        {
            return new NetworkLevel.Data
            {
                SceneId = reader.ReadInt(),
            };
        }
    }
}