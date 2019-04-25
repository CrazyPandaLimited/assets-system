using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "RSDebugToolResources", menuName = "Resource system/Debug tool resources", order = 1)]
public class ResourceSystemDebugToolResources : ScriptableObject
{
    
    [Header("Edit mode settings")]
    
    public Rect EditorModeTextPositionOffset;
    public Rect EditorModeImagePositionOffset;
    public string EditorModeRunOnlyRuntimeString;
    public Sprite[] EditorModeAnimationSprites;
    public GUIStyle EditorModeRunOnlyRuntimeStringStyle;
    
    
    
    [Header("Play mode settings")]
    
    [Header("Select system section")]
    [Space]
    [Space]
    public string PlayModeSelectRSTitle;
    public GUIStyle PlayModeSelectRSTitleStyle;
    public GUIStyle PlayModeSelectRSButtomStyle;
    public int PlayModeSelectContentButtonsHeight;
    
    [Space]
    [Space]
    [Header("Select system buttons")]
    public string PlayModeSelectRSButtonTemplate;
    public GUIStyle PlayModeSelectRSButtonTemplateTextStyle;
    public GUIStyle PlayModeSelectRSButtonTemplateButtonStyle;
    
    [Space]
    [Space]
    [Header("System Loaders section")]
    [Space]
    [Header("Foldout")]
    public string PlayMode_LoadersTogle_Title;
    public GUIStyle PlayMode_LoadersTogle_Style;
    public GUIStyle PlayMode_LoadersTogleBg_Style;
    public int PlayMode_LoadersTogle_Title_Height;

    [Header("Context area")]
    public GUIStyle PlayMode_LoadersTogleButtom_Style;
    public int PlayMode_LoadersTogle_Buttom_Height;

    [Header("Loader area")]
    [Header("Foldout")]
    public string PlayMode_Loader_Togle_TitleTemplate;
    public GUIStyle PlayMode_Loader_Togle_Style;
    public GUIStyle PlayMode_Loader_TogleBg_Style;
    public Rect PlayMode_Loader_Togle_Title_Offsets;
    
    [Header("Loader body")]
    public GUIStyle PlayMode_Loader_Buttom_BG_Style;
    public int PlayMode_Loader_CacheTitle_Height;
    
    public string PlayMode_Loader_CacheTitle_Name;
    public GUIStyle PlayMode_Loader_CacheTitle_Style;
    
    public float[] PlayMode_Loader_CacheElement_ColumsPercent;
    public Vector2Int PlayMode_Loader_CacheElement_ColumsPositionCorrection;
    
    public string[] PlayMode_Loader_CacheColumnsNames;
    public int PlayMode_Loader_CacheCol_Height;
    public GUIStyle PlayMode_Loader_CacheCol_Style;
    
    public int PlayMode_Loader_CacheElement_Height;
    public GUIStyle PlayMode_Loader_CacheElement_Style;
    public GUIStyle PlayMode_Loader_CacheElement_Style2;
    
    
    [Space]
    [Space]
    [Header("System tasks queue section")]
    [Space]
    [Header("Foldout")]
    public string PlayMode_QueueTogle_Title;
    public GUIStyle PlayMode_QueueTogle_Style;
    public GUIStyle PlayMode_QueueTogleBg_Style;
    public int PlayMode_QueueTogle_Title_Height;

    [Header("Context area")] public GUIStyle PlayMode_QueueTogleButtom_Style;
    public int PlayMode_QueueTogle_Buttom_Height;

    [Header("Queue area")] [Header("Foldout")]
    public string PlayMode_Queue_Togle_TitleTemplate_Waiting;
    

    public GUIStyle PlayMode_Queue_Togle_Style;
    public GUIStyle PlayMode_Queue_TogleBg_Style;
    public Rect PlayMode_Queue_Togle_Title_Offsets;

    [Header("Loader body")] public GUIStyle PlayMode_Queue_Buttom_BG_Style;
    public int PlayMode_Queue_CacheTitle_Height;

    public string PlayMode_Queue_CacheTitle_Name;
    public GUIStyle PlayMode_Queue_CacheTitle_Style;

    public float[] PlayMode_Queue_CacheElement_ColumsPercent;
    public Vector2Int PlayMode_Queue_CacheElement_ColumsPositionCorrection;

    public string[] PlayMode_Queue_CacheColumnsNames;
    public int PlayMode_Queue_CacheCol_Height;
    public GUIStyle PlayMode_Queue_CacheCol_Style;

    public int PlayMode_Queue_CacheElement_Height;
    public GUIStyle PlayMode_Queue_CacheElement_Style;
    public GUIStyle PlayMode_Queue_CacheElement_Style2;
    
    [Space]
    public string PlayMode_Queue_Togle_TitleTemplate_Execution;
}
