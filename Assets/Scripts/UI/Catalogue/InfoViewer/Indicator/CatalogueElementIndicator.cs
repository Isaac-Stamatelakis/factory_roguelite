using System.Collections.Generic;
using UnityEngine;

namespace UI.Catalogue.InfoViewer.Indicator
{
    public class CatalogueElementIndicator : MonoBehaviour
    {
        private int currentIndex;
        private CatalogueInfoViewer infoViewerParent;
        private CatalogueElementIndicatorNode[] nodes;
        public void Initialize(CatalogueInfoViewer catalogueInfoViewer)
        {
            infoViewerParent = catalogueInfoViewer;
            InitializeNodes();
        }

        private void InitializeNodes()
        {
            nodes = GetComponentsInChildren<CatalogueElementIndicatorNode>();
            if (nodes.Length % 2 != 1) Debug.LogWarning("Number of children of indicator should be even");
        }
        
        private void OnNodeClick(int displayIndex)
        {
            infoViewerParent.MoveDisplayElement(displayIndex);
        }

        public void DisplayNodes()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                int displayOffset = i - nodes.Length / 2;
                nodes[i].Display(infoViewerParent.GetAdjacentDisplayElement(displayOffset),displayOffset, OnNodeClick);
            }
        }
    }
}
