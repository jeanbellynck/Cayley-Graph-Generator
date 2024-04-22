using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InactiveSetter : MonoBehaviour
{

    public void SetInactive(bool inactive) {
        gameObject.SetActive(!inactive);
    }
}
