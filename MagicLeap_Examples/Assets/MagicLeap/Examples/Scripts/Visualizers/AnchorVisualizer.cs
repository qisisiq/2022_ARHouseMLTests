using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Examples
{
    public class AnchorVisualizer : MonoBehaviour
    {
        public GameObject anchorPrefab;
        public MLAnchors.Request query;
        private Transform mainCamera;
        private Dictionary<string, AnchorVisual> map = new Dictionary<string, AnchorVisual>();

        void Start()
        {
            mainCamera = Camera.main.transform;
            query = new MLAnchors.Request();
        }

        void Update()
        {
            if (query == null)
                return;

            query.Start(new MLAnchors.Request.Params(mainCamera.position, 0, 0, true));
            query.TryGetResult(out MLAnchors.Request.Result result);

            foreach (var anchor in result.anchors)
            {
                string id = anchor.Id;
                if (map.ContainsKey(id))
                {
                    map[id].Set(anchor);
                }
                else
                {
                    GameObject anchorGO = Instantiate(anchorPrefab);
                    map.Add(id, anchorGO.AddComponent<AnchorVisual>());
                }
            }
        }
    }
}


