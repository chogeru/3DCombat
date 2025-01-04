using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class JP_Anime_FixedFPS : MonoBehaviour
{
    [Header("Target Fixed FPS")]
    [Tooltip("This need for right perception \"as Anime\". For Anime can be acceptable from 6 to 24 FPS. Will be best - 12 fps")]
    [SerializeField]
    [Range(4,60)]
    private int TargetFPS = 12;
    [SerializeField]
    [Header("Enable Fixed FPS")]
    
    private bool Enable = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Enable == true)
        {
            Application.targetFrameRate = TargetFPS;
        }
       
    }
}
