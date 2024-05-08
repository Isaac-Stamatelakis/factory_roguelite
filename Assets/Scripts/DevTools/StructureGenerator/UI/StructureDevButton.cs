using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DevTools.Structures {
    public class StructureDevButton : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            DevToolUIController controller = DevToolUIControllerContainer.GetController();
            controller.setHomeVisibility(false);
            StructureDevControllerUI structureDevControllerUI = StructureDevControllerUI.newInstance();
            structureDevControllerUI.init();
            controller.addUI(structureDevControllerUI.transform);
            controller.setTitleText("Structure Creator");
        }
    }
}

