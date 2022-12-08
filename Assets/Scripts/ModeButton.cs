using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TMPro;
using UnityEngine.Rendering.PostProcessing;

public class ModeButton : MonoBehaviour {

	public List<Color> Modes = new List<Color>{new Color(255, 215, 0), new Color(0, 210, 55), new Color(209, 0, 201), new Color(154, 45, 43)};

    public stringModes currentMode = stringModes.SEND;

    public Image[] colourImage;

    public PostProcessProfile[] profiles;

    public PostProcessVolume volume;

    public enum stringModes
    {
        SEND = 0,
        BUY = 1,
        SELL = 2
    }

    public void FixedUpdate()
    {
        GetComponentInChildren<TMP_Text>().text = currentMode.ToString();
    }

    [Range(0, 2)]
    public int i = 0;

    public void ButtonClicked()
    {
        List<string> modes = Enum.GetValues(typeof(stringModes)).Cast<stringModes>().Select(v => v.ToString()).ToList();

        if (currentMode.ToString().Contains(modes[2]))
        {
            currentMode = stringModes.SEND;
            i = 0;
            foreach(Image j in colourImage)
            {
                j.color = Modes[0];
                volume.profile = profiles[0];
            }
        }
        else
        {
            i = Mathf.Clamp(i + 1, 0, 2);
        }

        currentMode = (stringModes)Enum.Parse(typeof(stringModes), modes[i]);
        foreach (Image j in colourImage)
        {
            j.color = Modes[i];
            volume.profile = profiles[i];
        }
    }
}
