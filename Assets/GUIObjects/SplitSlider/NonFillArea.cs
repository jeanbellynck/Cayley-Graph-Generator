using UnityEngine;

public class NonFillArea : MonoBehaviour {
    [SerializeField] RectTransform leftRect;
    [SerializeField] RectTransform fullRect;
    RectTransform rightRect;

    void Start() => rightRect = GetComponent<RectTransform>();

    void Update()
    {
        rightRect.SetInsetAndSizeFromParentEdge(
            RectTransform.Edge.Left, 
            leftRect.rect.width, 
            fullRect.rect.width - leftRect.rect.width);
    }
}
