using System.Collections;
using System.Collections.Generic;
using System.Net;
using kcp2k;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Minicop.Game.CubeDino
{
    public class PlatformDependentCompilation : MonoBehaviour
    {
        public UnityEvent OnAutoConnectEnebled, OnAutoConnectDisabled = new UnityEvent();
        [Inject]
        public NetworkManager _networkManager;
        [Inject]
        public Transport _kcpTransport;

        public string IpAdress
        {
            get { return Data.IpAdress; }
            set { Data.IpAdress = value; }
        }


        private void Start()
        {
#if UNITY_EDITOR
            if (!NetworkServer.active)
                CreateServer();
            else
                ConnectServer();
#endif
#if DEVELOPMENT_BUILD
            CreateServer();
#elif UNITY_WEBGL
            ConnectServer();
#elif !UNITY_EDITOR
            ConnectServer();
#endif
        }

        /// <summary>
        /// Вызов создания сервера
        /// </summary>
        public void CreateServer()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            try
            {
                _networkManager.Create();
            }
            catch
            {
                Debug.Log($"Port denied");
            }
#endif
        }


        /// <summary>
        /// Подключение к серверу
        /// </summary>
        public void ConnectServer()
        {
            _networkManager.Connect(IpAdress);
        }
        /// <summary>
        /// Завершение программы
        /// </summary>
        public void Quit()
        {
            Application.Quit();
        }

        public static DataStruct Data = new DataStruct
        {
#if UNITY_EDITOR
            IpAdress = "localhost",
#else
            IpAdress = "localhost",
#endif
        };
        [System.Serializable]
        public struct DataStruct
        {
            public string IpAdress;
        }
    }
}