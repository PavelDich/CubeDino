using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.Events;
using Zenject;
using System.Linq;


namespace Minicop.Game.CubeDino
{
    public class NetworkManager : Mirror.NetworkManager
    {
        [Inject]
        public DiContainer diContainer;
        //[SerializeField]
        //private PlayerInfo _playerInfo;
        public bool playerSpawned;
        public static UnityEvent<NetworkIdentity> OnPlayerConnect = new UnityEvent<NetworkIdentity>();
        public static UnityEvent OnPlayerDisconnect = new UnityEvent();
        public static UnityEvent OnClientStarted = new UnityEvent();
        public static UnityEvent OnClientStopped = new UnityEvent();

        /// <summary>
        /// Получить стандартные спавн Mirror
        /// </summary>
        /// <returns>Трансформ спавна</returns>
        public Transform GetRespawn()
        {
            return GetStartPosition();
        }

        #region  MirrorInvokes
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            StartCoroutine(OnServerAddPlayerDelayed(conn));
        }
        public override void OnStartServer()
        {
            base.OnStartServer();
        }
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
        }
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            OnPlayerDisconnect.Invoke();
        }

        public override void OnClientConnect()
        {
            OnClientStarted.Invoke();
            base.OnClientConnect();
        }
        public override void OnClientDisconnect()
        {
            OnClientStopped.Invoke();
            base.OnClientDisconnect();
        }
        public override void OnStopServer()
        {
            for (int index = 0; index < ActiveSubScenes.Count; index++)
            {
                NetworkServer.SendToAll(new SceneMessage { sceneName = ActiveSubScenes[index].name, sceneOperation = SceneOperation.UnloadAdditive });
            }
            //NetworkServer.SendToAll(new SceneMessage { sceneName = LobbyScenes.Scene, sceneOperation = SceneOperation.UnloadAdditive });
            StartCoroutine(ServerUnloadSubScenes());
        }
        public override void OnStopClient()
        {
            if (mode == NetworkManagerMode.Offline)
                StartCoroutine(ClientUnloadSubScenes());
        }
        #endregion MirrorInvokes






        #region  NetworkControll
        /// <summary>
        /// Создание сервера
        /// </summary>
        public void Create()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            try
            {
                if (!NetworkServer.active)//&&!NetworkClient.isConnected)
                {
                    Debug.Log($"Host started");
                    StartServer();
                }
                else
                {
                    Debug.Log($"You'r are client or server active");
                }
            }
            catch
            {
                Debug.Log($"Critical error on create");
            }
#endif
        }

        /// <summary>
        /// Подключение к серверу
        /// </summary>
        /// <param name="ipAdress">адресс</param>
        public void Connect(string ipAdress)
        {
            try
            {
                if (!NetworkClient.isConnected && !NetworkServer.active && !string.IsNullOrWhiteSpace(ipAdress))
                {
                    //Debug.Log($"Client started");
                    networkAddress = ipAdress;
                    StartClient();
                }
                else
                {
                    //Debug.Log($"Ip adress incorrect");
                }
            }
            catch
            {
                //Debug.Log($"Critical error on connect");
            }
        }

        /// <summary>
        /// Отключение от сервера
        /// </summary>
        public void Disconnect()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                //Debug.Log($"Host stopped");
                NetworkManager.singleton.StopClient();
                NetworkManager.singleton.StopHost();
            }
            else
            {
                Debug.Log($"Client stopped");
                NetworkManager.singleton.StopClient();
            }
        }
        #endregion NetworkControll


        #region  NetworkScenes
        public static UnityEvent<NetworkIdentity> OnSubSceneLoad = new UnityEvent<NetworkIdentity>();

        [SerializeField]
        public List<NetworkLevel> ActiveNetworkLevels = new List<NetworkLevel>();
        [SerializeField]
        public List<Scene> ActiveSubScenes = new List<Scene>();


        /// <summary>
        /// Появление подключения клиента на сервере
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerator OnServerAddPlayerDelayed(NetworkConnectionToClient conn)
        {
            yield return new WaitForEndOfFrame();

            base.OnServerAddPlayer(conn);
            OnPlayerConnect.Invoke(conn.identity);
        }





        /// <summary>
        /// Загрузка подсцены
        /// </summary>
        /// <param name="scene">сцена</param>
        /// <param name="subScene">обратный вызов по завершению загрузки</param>
        public void LoadSubScene(string scene, System.Action<Scene, NetworkLevel> subScene)
        {
            StartCoroutine(ServerLoadSubScene(scene, subScene));
        }
        [Server]
        private IEnumerator ServerLoadSubScene(string scene, System.Action<Scene, NetworkLevel> subScene)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            yield return new WaitForEndOfFrame();
            yield return SceneManager.LoadSceneAsync(scene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = LocalPhysicsMode.Physics3D });
            Scene loadScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            foreach (GameObject obj in loadScene.GetRootGameObjects())
                if (obj.TryGetComponent<NetworkLevel>(out NetworkLevel networkLevel))
                {
                    AddSubScenes(loadScene, networkLevel);
                    subScene.Invoke(loadScene, networkLevel);
                }
#endif
            yield return null;
        }
        /// <summary>
        /// Добавление сцены к общим загруженным сетевым сценам
        /// </summary>
        /// <param name="scene">префаб сцены</param>
        /// <param name="networkLevel">компонент сетевой сцены</param>
        [Server]
        public void AddSubScenes(Scene scene, NetworkLevel networkLevel)
        {
            for (int i = 0; i < ActiveSubScenes.Count; i++)
            {
                if (ActiveNetworkLevels[i]) continue;
                ActiveNetworkLevels[i] = networkLevel;
                ActiveSubScenes[i] = scene;
                networkLevel.SceneId = i;
                return;
            }
            ActiveNetworkLevels.Add(networkLevel);
            ActiveSubScenes.Add(scene);
            networkLevel.SceneId = ActiveNetworkLevels.Count - 1;
        }


        /// <summary>
        /// Подключить клиента к сцене
        /// </summary>
        /// <param name="networkIdentity">подключение</param>
        /// <param name="id">id сцены</param>
        [Server]
        public void ConnectToScene(NetworkIdentity networkIdentity, int id)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            OnSubSceneLoad.Invoke(networkIdentity);
            StartCoroutine(SceneConnecting(networkIdentity.connectionToClient, id));
#endif
        }
        [Server]
        private IEnumerator SceneConnecting(NetworkConnectionToClient conn, int id)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            yield return new WaitForEndOfFrame();
            conn.Send(new SceneMessage { sceneName = ActiveSubScenes[id].name, sceneOperation = SceneOperation.LoadAdditive });

            SceneManager.MoveGameObjectToScene(conn.identity.gameObject, ActiveSubScenes[id]);
#endif
            yield return null;
        }


        /// <summary>
        /// Выгрузка сетевой сцены
        /// </summary>
        /// <param name="scene">загруженная сцена</param>
        public void UnloadSubScene(Scene scene)
        {
            StartCoroutine(ServerUnloadSubScenes(scene));
        }
        private IEnumerator ServerUnloadSubScenes(Scene scene)
        {
            yield return SceneManager.UnloadSceneAsync(scene);
            yield return Resources.UnloadUnusedAssets();
        }
        private IEnumerator ServerUnloadSubScenes()
        {
            for (int index = 0; index < ActiveSubScenes.Count; index++)
                if (ActiveSubScenes[index].IsValid())
                    yield return SceneManager.UnloadSceneAsync(ActiveSubScenes[index]);
            ActiveSubScenes.Clear();

            yield return Resources.UnloadUnusedAssets();
        }



        public void ClientUnloadOfSubScenes()
        {
            StartCoroutine(ClientUnloadSubScenes());
        }

        IEnumerator ClientUnloadSubScenes()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
                if (SceneManager.GetSceneAt(index) != SceneManager.GetActiveScene())
                    yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index));
        }
        #endregion NetworkScenes
    }

    public interface IData
    {
        public object GetData();
        public void SetData(object data);
    }
}
