using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPGroupsNET;
using TMPro;

public class GetRandomGroup : MonoBehaviour
{
    private RandomGroups randomizer = new();
    public void GetNewGroup()
    {
        this.GetComponent<TextMeshPro>().text = "Random Group: " + RandomGroups.ToString();
    }
}
