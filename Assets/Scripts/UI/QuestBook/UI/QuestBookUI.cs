using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class QuestBookUI : MonoBehaviour
    {
        [SerializeField] private bool editMode = true;
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private Transform lineContainer;
        [SerializeField] private Transform contentContainer;
        [SerializeField] private Transform contentMaskContainer;
        private string editModePath = "UI/Quest/EditModeElements";
        private QuestEditModeController editModeController;

        public Transform NodeContainer { get => nodeContainer;}
        public Transform LineContainer { get => lineContainer;}
        public Transform ContentContainer {get => contentContainer;}
        public Transform ContentMaskContainer {get => contentMaskContainer;}

        private float minScale = 0.35f;
        private float maxScale = 3f;
        private float zoomSpeed = 0.3f;

        // Start is called before the first frame update
        void Start()
        {
            if (editMode) {
                initEditMode();
            }   
        }

        public void FixedUpdate() {
            
            handleRightClick();
        }

        public void Update() {
            handleZoom();
        }

        private void initEditMode() {
            GameObject prefab = Resources.Load<GameObject>(editModePath);
            if (prefab == null) {
                Debug.LogError("QuestBookUI edit mode prefab is null");
                return;
            }
            GameObject instianated = GameObject.Instantiate(prefab);
            editModeController = instianated.GetComponent<QuestEditModeController>();
            editModeController.transform.SetParent(transform,false);
            editModeController.init(this);
            
        }
        private void handleZoom() {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            
            if (scrollInput != 0)
            {
                Vector3 mousePosition = Input.mousePosition;
                Transform containerTransform = ContentContainer;
                Vector3 newScale = containerTransform.localScale + Vector3.one * scrollInput * zoomSpeed;
                newScale = new Vector3(
                    Mathf.Clamp(newScale.x, minScale, maxScale),
                    Mathf.Clamp(newScale.y, minScale, maxScale),
                    Mathf.Clamp(newScale.z, minScale, maxScale)
                );
                Vector3 scaleChange = (newScale - containerTransform.localScale);
                Vector3 newOffset = scaleChange.x/newScale.x *  (containerTransform.position - mousePosition);
                containerTransform.localScale = newScale;
                containerTransform.position = containerTransform.position + newOffset;
            }
        }

        private void handleRightClick() {
            if (!Input.GetMouseButton(1)) {
                return;
            }
            int maxPosition = 2000;
            int maxSpeed = 250;
            Vector3 mousePosition = Input.mousePosition;
            Vector3 dif = (mousePosition - ContentMaskContainer.transform.position);
            dif.x = Mathf.Clamp(dif.x,-maxSpeed,maxSpeed);
            dif.y = Mathf.Clamp(dif.y,-maxSpeed,maxSpeed);
            Vector2 newPosition = contentContainer.position - dif * 0.05f;
            float clampedX = Mathf.Clamp(newPosition.x,-maxPosition,maxPosition);
            float clampedY = Mathf.Clamp(newPosition.y,-maxPosition,maxPosition);
            contentContainer.position = new Vector3(clampedX,clampedY);
        }
    }
    
}

