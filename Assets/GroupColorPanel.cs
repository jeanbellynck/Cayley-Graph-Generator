using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

public class GroupColorPanel : MonoBehaviour {
    public TMP_Text textField;

    public void updateView(IDictionary<char, Color> colors) {
        textField.text = string.Concat(
            from kvp in colors
            let generator = kvp.Key
            let color = kvp.Value
            select $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{generator}</color>"    
        );
    }
}
