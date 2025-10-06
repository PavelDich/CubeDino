using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Minicop.Game.CubeDino
{
    public class Settings : MonoBehaviour
    {
        private void Start()
        {
            JSONController.Load<Settings.Data>(ref _data, "PlayerSettings");
            ConnectionInfo.Data.LocalName = _data.NickName;
            OnNickNameLoad.Invoke(_data.NickName);
            OnIpAdressLoad.Invoke(_data.IpAdress);
        }

        public UnityEvent<string> OnNickNameLoad = new UnityEvent<string>();
        public UnityEvent<string> OnIpAdressLoad = new UnityEvent<string>();
        public string NickName
        {
            get { return _data.NickName; }
            set
            {
                ConnectionInfo.Data.LocalName = value;
                _data.NickName = value;
                JSONController.Save(_data, "PlayerSettings");
            }
        }
        public string IpAdress
        {
            get { return _data.IpAdress; }
            set
            {
                _data.IpAdress = value;
                JSONController.Save(_data, "PlayerSettings");
            }
        }

        [SerializeField]
        private Data _data = new Data();
        [System.Serializable]
        public struct Data
        {
            public string NickName;
            public string IpAdress;
        }
    }
}