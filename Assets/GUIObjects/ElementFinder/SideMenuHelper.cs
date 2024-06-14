using DanielLochner.Assets.SimpleSideMenu;
using UnityEngine;
using UnityEngine.Events;

public class SideMenuHelper : MonoBehaviour
{
    SimpleSideMenu sideMenu;
    Color onColor;
    Color offColor = new(0, 0, 0, 0);
    [SerializeField] CameraManager cameraManager;

    void Awake() {
        sideMenu = GetComponent<SimpleSideMenu>();
        sideMenu.OnStateChanged.AddListener(StateChanged);
        sideMenu.OnStateChanging.AddListener(StateChanging);
        onColor = sideMenu.OverlayColour;
    }

    public bool OverlayVisible {
        get => sideMenu.OverlayColour.a > 0;
        set => sideMenu.OverlayColour = value ? onColor : offColor;
    }

    public bool OverlayInvisible {
        get => !OverlayVisible;
        set => OverlayVisible = !value;
    }

    public bool CameraLeftInsetToThis {
        get => cameraManager.LeftInset == sideMenu.GetComponent<RectTransform>().rect.width;
        set => cameraManager.LeftInset = value ? sideMenu.GetComponent<RectTransform>().rect.width : 0;
    }

    public bool CameraRightInsetToThis {
        get => cameraManager.RightInset == sideMenu.GetComponent<RectTransform>().rect.width;
        set => cameraManager.RightInset = value ? sideMenu.GetComponent<RectTransform>().rect.width : 0;
    }

    public UnityEvent Opened = new();
    public UnityEvent Closed = new();
    public UnityEvent Opening = new();
    public UnityEvent Closing = new();

    public void StateChanged(State before, State after) {
        if (after == State.Open) Opened.Invoke();
        if (after == State.Closed) Closed.Invoke();
    }
    public void StateChanging(State before, State after) {
        if (after == State.Open) Opening.Invoke();
        if (after == State.Closed) Closing.Invoke();
    }
}
