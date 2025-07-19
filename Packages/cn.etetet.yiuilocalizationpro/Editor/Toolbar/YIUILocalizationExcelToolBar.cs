using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using YIUIFramework.Editor;

namespace ExtenderToolBar
{
    [InitializeOnLoad]
    public static class YIUILocalizationExcelToolBar
    {
        static YIUILocalizationExcelToolBar()
        {
            //左右位置,间隔,可随意切换
            //ToolbarExtender.RightToolbarGUI.Add(OnLubanExcelToolbarGUI);
            ToolbarExtender.LeftToolbarGUI.Add(OnLubanExcelToolbarGUI);
        }

        private static void OnLubanExcelToolbarGUI()
        {
            GUILayout.Space(5);
            var iconContent = EditorGUIUtility.IconContent("d_Profiler.UI");
            iconContent.image = AssetDatabase.LoadAssetAtPath<Texture>("Packages/cn.etetet.yiuilocalizationpro/Editor/Toolbar/Icon/icon_localization_chn.png");
            iconContent.tooltip = "多语言导出";
            if (GUILayout.Button(iconContent))
            {
                YIUILocalizationCreate.CreateI2LocalizationByXlsx();
            }
            GUILayout.Space(5);
        }
    }
}