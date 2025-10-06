using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Minicop.Game.GravityRave
{
    public class MoveOutMenu : MonoBehaviour
    {
        public GameObject[] Panels;
        public bool IsReady = true;
        public bool IsOpen = false;
        public int OpenedPanel = 0;
        public float OpenTime = 1f;
        public float CloseTime = 1f;
        private void Start()
        {
            for (int i = 0; i < Panels.Length; i++)
            {
                Close(i, 0);
            }
        }

        public void Select(int id) => Select(id, OpenTime, CloseTime);
        public void Select(int id, float openTime, float closeTime)
        {
            if (!IsReady) return;
            for (int i = 0; i < Panels.Length; i++)
            {
                if (id != i)
                    Close(i, closeTime);
                else if (OpenedPanel == id && IsOpen)
                {
                    Close(id, closeTime);
                    IsOpen = false;
                    IsReady = false;
                    DOTween.Sequence()
                        .AppendInterval(CloseTime)
                        .AppendCallback(() => IsReady = true);
                    return;
                }
            }

            if (IsOpen)
            {
                DOTween.Sequence()
                    .AppendInterval(CloseTime)
                    .AppendCallback(() => Open(id, openTime));
            }
            else
                Open(id, openTime);

            OpenedPanel = id;
        }

        public void Open(int id, float time)
        {
            if (!Panels[id]) return;
            IsReady = false;
            Sequence b = DOTween.Sequence()
                .AppendCallback(() => Panels[id].SetActive(true))
                .Append(Panels[id].transform.DOScaleX(1, time))
                .AppendCallback(() => { IsReady = true; IsOpen = true; });
        }
        public void Close(int id, float time)
        {
            if (!Panels[id]) return;
            DOTween.Sequence()
                .Append(Panels[id].transform.DOScaleX(0, time))
                .AppendCallback(() => Panels[id].SetActive(false));
        }
    }
}