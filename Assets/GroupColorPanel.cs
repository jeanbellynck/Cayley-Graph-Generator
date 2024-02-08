using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

public class GroupColorPanel : MonoBehaviour {
    public TMP_Text textField;

    public void updateView(IEnumerable<char> generators, IEnumerable<Color> colors) {
        textField.text = string.Concat(
            generators.Zip(colors, 
                (generator, color) => 
                    $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{generator}</color>"
                )
            );
    }
}
