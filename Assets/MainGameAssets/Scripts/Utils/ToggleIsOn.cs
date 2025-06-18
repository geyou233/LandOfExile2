
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MainGameAssets.Scripts.Utils
{
    public class ToggleIsOn : MonoBehaviour
    {
        private Toggle toggle;
        //未选中图片，Background
        private Image unselectedImage;
        //选中图片，Checkmark
        private Image selectedImage;
        private void Start()
        {
            toggle = GetComponent<Toggle>();
            unselectedImage = toggle.targetGraphic as Image;
            selectedImage = toggle.graphic as Image;
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
        private void OnToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                unselectedImage.color = new Color(255,255,255,0);
            }
            else
            {
                unselectedImage.color = new Color(255,255,255,255);
            }
        }
    }
}