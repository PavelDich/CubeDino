using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Minicop.Game.CubeDino
{
    public class ConnectionInfo : NetworkBehaviour, IData
    {
        private void Start()
        {
            if (NetworkClient.active)
                CmdSyncName(LocalName);
        }
        [Command(requiresAuthority = false)]
        private void CmdSyncName(string name)
        {
            Name = name;
        }
        public string Name
        {
            get { return _data.Name; }
            set { _data.Name = value; }
        }
        public static string LocalName
        {
            get { return Data.LocalName; }
            set { Data.LocalName = value; }
        }

        #region Network
        [SyncVar]
        [SerializeField]
        private Data _data = new Data();
        [System.Serializable]
        public struct Data
        {
            public string Name;
            public static string LocalName;
        }
        #endregion


        public object GetData()
        {
            return _data;
        }
        public void SetData(object data)
        {
            _data = (ConnectionInfo.Data)data;
        }
        public void SetData(Data data)
        {
            _data = data;
        }
    }

    public static class ConnectionInfoSerializer
    {
        /// <summary>
        /// Сериализация обьекта в сеть
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="item"></param>
        public static void Write(this NetworkWriter writer, ConnectionInfo.Data item)
        {
            writer.WriteString(item.Name);
        }

        /// <summary>
        /// Десериализация обьекта из сети
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ConnectionInfo.Data Read(this NetworkReader reader)
        {
            return new ConnectionInfo.Data
            {
                Name = reader.ReadString(),
            };
        }
    }
}