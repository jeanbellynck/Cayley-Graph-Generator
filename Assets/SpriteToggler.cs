using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteToggler : MonoBehaviour
{
    [SerializeField] List<Sprite> sprites;

    Image _spriteRenderer;
    Image spriteRenderer => _spriteRenderer == null ? _spriteRenderer = GetComponent<Image>() : _spriteRenderer;

    int state;

    public void SetState(int newState) {
        this.state = newState;
        spriteRenderer.sprite = sprites[newState % sprites.Count];
    }

    public void SetState(bool newState) => SetState(newState ? 1 : 0);

    public void ToggleState() => SetState(state + 1);
}
