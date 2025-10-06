using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

namespace Minicop.Game.GravityRave
{
    /// <summary>
    /// Общий ввод
    /// </summary>
    [DisallowMultipleComponent]
    public class InputPlayer : NetworkBehaviour
    {
        [Header("Settings")]
        public InputSettings inputCode;
        [SerializeField]
        private GameObject[] LocalObjects;
        [SerializeField]
        private GameObject[] NonLocalObjects;
        [field: SerializeField]
        public bool isMenuOpen { get; set; }
        [field: SerializeField]
        public MenuEvents menuEvents { get; private set; }
        [System.Serializable]
        public struct MenuEvents
        {
            [field: SerializeField]
            public UnityEvent OnEscape { get; private set; }
            [field: SerializeField]
            public UnityEvent OnMenuOpen { get; private set; }
            [field: SerializeField]
            public UnityEvent OnMenuClose { get; private set; }
        }
        [field: SerializeField]
        public MoveEvents moveEvents { get; private set; }
        [System.Serializable]
        public struct MoveEvents
        {
            [field: SerializeField]
            public UnityEvent OnStay { get; private set; }
            [field: SerializeField]
            public UnityEvent<Vector3> OnWalk { get; private set; }
            [field: SerializeField]
            public UnityEvent<Vector3> OnSprint { get; private set; }
            [field: SerializeField]
            public UnityEvent<Vector3> OnCrawl { get; private set; }
            [field: SerializeField]
            public UnityEvent<Vector2> OnHead { get; private set; }
            [field: SerializeField]
            public UnityEvent OnJump { get; private set; }
        }

        [field: SerializeField]
        public InventoryEvents inventoryEvents { get; private set; }
        [System.Serializable]
        public struct InventoryEvents
        {
            public UnityEvent<int> OnSwith { get; private set; }
            [field: SerializeField]
            public UnityEvent OnGrab { get; private set; }
            [field: SerializeField]
            public UnityEvent OnPut { get; private set; }
            [field: SerializeField]
            public UnityEvent OnDrop { get; private set; }
        }
        [field: SerializeField]
        public ItemEvents itemEvents { get; private set; }
        [System.Serializable]
        public struct ItemEvents
        {
            [field: SerializeField]
            public UnityEvent OnBaseUse { get; private set; }
            [field: SerializeField]
            public UnityEvent OnBaseStop { get; private set; }
            [field: SerializeField]
            public UnityEvent OnActiveUse { get; private set; }
            [field: SerializeField]
            public UnityEvent OnActiveStop { get; private set; }
            [field: SerializeField]
            public UnityEvent OnPassiveUse { get; private set; }
            [field: SerializeField]
            public UnityEvent OnPassiveStop { get; private set; }
            [field: SerializeField]
            public UnityEvent OnReload { get; private set; }
        }
        [Header("Other")]
        public UnityEvent OnInteract;


        private void Start()
        {
            foreach (GameObject localObject in LocalObjects)
                localObject.SetActive(false);
            foreach (GameObject nonLocalObject in NonLocalObjects)
                nonLocalObject.SetActive(true);

            if (!isOwned) return;

            foreach (GameObject localObject in LocalObjects)
                localObject.SetActive(true);
            foreach (GameObject nonLocalObject in NonLocalObjects)
                nonLocalObject.SetActive(false);
            isMenuOpen = false;
            menuEvents.OnMenuClose.Invoke();
            Cursor.lockState = CursorLockMode.Locked;
        }
        private void FixedUpdate()
        {
            if (!isOwned) return;

            if ((Input.GetKey(inputCode.move.Forward) || Input.GetKey(inputCode.move.Back) ||
             Input.GetKey(inputCode.move.Left) || Input.GetKey(inputCode.move.Right)) && !isMenuOpen)
            {
                Vector3 direction = Vector3.zero;
                if (Input.GetKey(inputCode.move.Forward)) direction += Vector3.up;
                if (Input.GetKey(inputCode.move.Back)) direction += Vector3.down;
                if (Input.GetKey(inputCode.move.Left)) direction += Vector3.left;
                if (Input.GetKey(inputCode.move.Right)) direction += Vector3.right;

                if (Input.GetKey(inputCode.move.Sprint)) moveEvents.OnSprint.Invoke(new Vector3(direction.x, 0f, direction.y));
                else if (Input.GetKeyUp(inputCode.move.Crawl)) moveEvents.OnCrawl.Invoke(new Vector3(direction.x, 0f, direction.y));
                else moveEvents.OnWalk.Invoke(new Vector3(direction.x, 0f, direction.y));

            }
            else moveEvents.OnStay.Invoke();
        }

        /// <summary>
        /// Переключения состояния меню
        /// </summary>
        public void SwithcMenu()
        {
            isMenuOpen = !isMenuOpen;
            if (isMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                menuEvents.OnMenuOpen.Invoke();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                menuEvents.OnMenuClose.Invoke();
            }
        }
        private void Update()
        {
            //Input.GetAxis("Mouse ScrollWheel");

            if (!isOwned) return;

            if (Input.GetKeyDown(inputCode.menu.Open))
            {
                SwithcMenu();
            }

            if (isMenuOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    menuEvents.OnEscape.Invoke();
                }
                return;
            }

            moveEvents.OnHead.Invoke(new Vector2(Input.GetAxis("Mouse X") * inputCode.move.SensativityX, Input.GetAxis("Mouse Y") * inputCode.move.SensativityY));
            if (Input.GetKeyDown(inputCode.move.Jump))
                moveEvents.OnJump.Invoke();

            if (Input.GetKeyDown(inputCode.inventory.Grab))
                inventoryEvents.OnGrab.Invoke();
            if (Input.GetKeyDown(inputCode.inventory.Put))
                inventoryEvents.OnPut.Invoke();
            if (Input.GetKeyDown(inputCode.inventory.Drop))
                inventoryEvents.OnDrop.Invoke();

            if (Input.GetKey(inputCode.item.BaseUse))
                itemEvents.OnBaseUse.Invoke();
            if (Input.GetKeyUp(inputCode.item.BaseUse))
                itemEvents.OnBaseStop.Invoke();
            if (Input.GetKey(inputCode.item.ActiveUse))
                itemEvents.OnActiveUse.Invoke();
            if (Input.GetKeyUp(inputCode.item.ActiveUse))
                itemEvents.OnActiveStop.Invoke();
            if (Input.GetKey(inputCode.item.PassiveUse))
                itemEvents.OnPassiveUse.Invoke();
            if (Input.GetKeyUp(inputCode.item.PassiveUse))
                itemEvents.OnPassiveStop.Invoke();
            if (Input.GetKeyDown(inputCode.item.Reload))
                itemEvents.OnReload.Invoke();

            if (Input.GetKeyDown(inputCode.Interact))
                OnInteract.Invoke();
        }
    }
}