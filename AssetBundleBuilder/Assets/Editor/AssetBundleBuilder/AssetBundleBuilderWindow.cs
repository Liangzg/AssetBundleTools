﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetBundleBuilder
{
    public class AssetBundleBuilderWindow : EditorWindow
    {

        private enum EToolbar
        {
            Home , Setting
        }

        private EToolbar toolbarIndex = EToolbar.Home;

        private Styles styles;

        //left 
        private float letfGUIWidth = 250;

        private int sdkConfigIndex;
        
        private int autoBuildIndex;

        //right
        private SearchField m_SearchField;

        private AssetTreeView treeView;
        //[SerializeField]
        private TreeViewState treeViewState; // Serialized in the window layout file so it survives assembly reloading
        //[SerializeField]
        private MultiColumnHeaderState multiColumnHeaderState;

        private AssetTreeModel treeModel;

        //other
        private AssetBuildRuleManager rulManger;

        private AssetBundleBuilder builder;

        [MenuItem("[Build]/Asset Bundle Builder")]
        public static void ShowWindow()
        {
            AssetBundleBuilderWindow window = EditorWindow.GetWindow<AssetBundleBuilderWindow>("Bundle Builder");
            window.Show();
        }


        Rect multiColumnTreeViewRect
        {
            get { return new Rect(letfGUIWidth + 10, 70, position.width - letfGUIWidth - 20, position.height - 80); }
        }

        Rect searchbarRect
        {
            get { return new Rect(letfGUIWidth + 10, 25f, position.width - letfGUIWidth - 20, 20f); }
        }

        #region Unity Standard API

        private void OnEnable()
        {
            styles = new Styles();
            rulManger = AssetBuildRuleManager.Instance;
            builder = new AssetBundleBuilder();

            this.initTreeView(); 
        }


        private void initTreeView()
        {
            treeViewState = new TreeViewState();

            List<AssetElement> treeEles = new List<AssetElement>();
            treeEles.Add(new AssetElement("root" , -1,0));

            treeModel = new AssetTreeModel(treeEles);
            treeModel.modelChanged += onTreeModelChanged;

            var headerState = AssetTreeView.CreateDefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(multiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(multiColumnHeaderState, headerState);
            multiColumnHeaderState = headerState;

            MultiColumnHeader multiColumnHeader = new MultiColumnHeader(headerState);
            multiColumnHeader.ResizeToFit();

            treeView = new AssetTreeView(treeViewState, multiColumnHeader, treeModel);
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
                

            
        }

        private void onTreeModelChanged()
        {
            
        }


        private void OnGUI()
        {
            drawToolbar();

            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical(GUI.skin.box ,GUILayout.Width(letfGUIWidth));
            drawLeftCenterGUI();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(position.width - letfGUIWidth));
            drawRightCenterGUI();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            drawBottomGUI();
        }


        private void OnDestroy()
        {
            
        }

        #endregion


        #region ----draw gui----


        private void drawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            string[] enumNames = Enum.GetNames(typeof(EToolbar));
            EToolbar[] enumValues = (EToolbar[])Enum.GetValues(typeof(EToolbar));

            for (int i = 0; i < enumNames.Length - 1; i++)
            {
                if (GUILayout.Toggle(toolbarIndex == enumValues[i], enumNames[i], EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    toolbarIndex = enumValues[i];
                    
                }
            }

            GUILayout.Toolbar(0, new[] {""}, EditorStyles.toolbar, GUILayout.ExpandWidth(true));

            if (GUILayout.Toggle(enumNames[(int)toolbarIndex].Equals(enumNames[enumNames.Length - 1]), enumNames[enumNames.Length - 1], EditorStyles.toolbarButton , GUILayout.Width(100)))
            {
                toolbarIndex = enumValues[enumNames.Length - 1];
            }

            EditorGUILayout.EndHorizontal();
        }


        private void drawLeftCenterGUI()
        {
            GUILayout.Label("Configs:");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Asset Version" , GUILayout.MaxWidth(100));
            GUI.color = Color.gray;
            GUILayout.TextField("0.0.1");
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("App   Version", GUILayout.MaxWidth(100));
            GUI.color = Color.gray;
            GUILayout.TextField("0.0.1");
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SDK Config", GUILayout.Width(100)))
            {
                Debug.Log("编辑打开！！！！");
            }
            GUI.backgroundColor = Color.red;
            sdkConfigIndex = EditorGUILayout.Popup(sdkConfigIndex, styles.SDKConfigs, GUILayout.MaxWidth(160));
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Development Build", builder.IsBuildDev ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.MaxWidth(160));
            builder.IsBuildDev = EditorGUILayout.Toggle(builder.IsBuildDev, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();
            if (!this.builder.IsBuildDev)
            {
                this.builder.IsAutoConnectProfile = false;
                this.builder.IsScriptDebug = false;
            }

            EditorGUI.BeginDisabledGroup(!this.builder.IsBuildDev);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Script Debugging", this.builder.IsScriptDebug ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.MaxWidth(160));
            this.builder.IsScriptDebug = EditorGUILayout.Toggle(this.builder.IsScriptDebug, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Autoconnect Profile", this.builder.IsAutoConnectProfile ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.MaxWidth(160));
            this.builder.IsAutoConnectProfile = EditorGUILayout.Toggle(this.builder.IsAutoConnectProfile, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);

            GUILayout.Label("", "IN Title");

            GUI.color = Color.yellow;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Local Debug", GUILayout.MaxWidth(160));
            this.builder.IsDebug = EditorGUILayout.Toggle(this.builder.IsDebug, GUILayout.MaxWidth(30));
            GUILayout.EndHorizontal();
            GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resource", GUILayout.MaxWidth(100));
            this.builder.BuildAssetType = (BuildType)EditorGUILayout.EnumMaskField(this.builder.BuildAssetType, GUILayout.MaxWidth(160));
            GUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Config Table", GUILayout.MaxWidth(160));
//            this.builder.IsDebug = EditorGUILayout.Toggle(this.builder.IsDebug, GUILayout.MaxWidth(30));
//            GUILayout.EndHorizontal();
//
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField("Lua Script", GUILayout.MaxWidth(160));
//            this.builder.IsDebug = EditorGUILayout.Toggle(this.builder.IsDebug, GUILayout.MaxWidth(30));
//            GUILayout.EndHorizontal();

            GUILayout.Space(10);

//            if (GUILayout.Button("Build Sub Package"))
//            {
//
//            }
//
//            if (GUILayout.Button("Build Sub Assets"))
//            {
//
//            }
            if (GUILayout.Button("Build"))
            {

            }

            GUILayout.Space(10);
            GUILayout.Label("", "IN Title");
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Auto Build", GUILayout.Width(100));
            autoBuildIndex = EditorGUILayout.Popup(autoBuildIndex, styles.OnekeyBuilds);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Go"))
            {

            }
            GUI.backgroundColor = Color.white;
        }


        private void drawRightCenterGUI()
        {
            GUILayoutOption largeButtonWidth = GUILayout.MaxWidth(120);
            GUILayoutOption nomaleButtonWidth = GUILayout.MaxWidth(80);
            GUILayoutOption miniButtonWidth = GUILayout.MaxWidth(30);
            m_SearchField.OnGUI(searchbarRect , "");

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Options:", nomaleButtonWidth))
            {
                treeView.Toggle = !treeView.Toggle;
            }

            if (GUILayout.Button("+", GUI.skin.button, miniButtonWidth))
            {
                this.addNewRootFolder();
            }
            
            if (GUILayout.Button("-", GUI.skin.button, miniButtonWidth))
            {
                this.treeModel.RemoveSelectElement();
            }
            
            if (GUILayout.Button("Refresh", GUI.skin.button , nomaleButtonWidth))
            {
                treeModel.AddChildrens(treeView.GetSelection());
                treeView.Reload();
            }                
            

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Clear All AB", GUI.skin.button , largeButtonWidth))
            {
                BuildUtil.ClearAssetBundleName();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Save", GUI.skin.button, nomaleButtonWidth))
            {
                treeModel.Save();
            }

            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            Rect treeViewRect = multiColumnTreeViewRect;
            this.treeView.OnGUI(treeViewRect);
        }


        private void drawBottomGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            GUILayout.Toggle(false, "Version：0.0.1", EditorStyles.toolbarButton, GUILayout.MaxWidth(100));
            EditorGUILayout.EndHorizontal();
        }

        #endregion


        /// <summary>
        /// 添加新的根目录
        /// </summary>
        private void addNewRootFolder()
        {
            string path = EditorUtility.OpenFolderPanel(ABLanguage.SET_RESOURCE_FOLDER, "Assets", "");
            if (string.IsNullOrEmpty(path)) return;

            string relativePath = path.Replace(Application.dataPath, "Assets").Replace('\\', '/');
//            ABData data = new ABData(null, relativePath, "", 0, 0, 0, 0, false, false, 0, false, 0, 0, false);
//            ABData.datas.Add(relativePath, data);

            treeModel.AddRoot(relativePath);

            treeView.Reload();

            //        ABData.datas.Clear();
            //        string[] pathes = Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
            //        for (int i = 0; i < pathes.Length; ++i)
            //        {
            //            var abPath = "Assets" + Path.GetFullPath(pathes[i]).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
            //            var data = new ABData(null, abPath, "", 0, 0, 0, 0, false, false, 0, false, 0, 0, false);
            //            ABData.datas.Add(abPath, data);
            //        }
        }



        
    }

}


