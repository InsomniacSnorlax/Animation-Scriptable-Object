using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

namespace Snorlax.Combo
{
    public class ComboEditor : EditorWindow
    {
        Vector2 scrollBar = Vector2.zero;
        string SearchString = "";
        bool checkToggle;
        #region Default Methods
        [MenuItem("Snorlax's Tools/Combo Create")]
        public static void ShowWindow()
        {
            GetWindow<ComboEditor>("Combo Editor");
        }

        private void OnGUI()
        {
            DrawWindow();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnDestroy()
        {

        }
        #endregion

        private Animator TargetAnim;
        private List<ComboItem> FoundComboItems = null;
        private List<AnimationClip> FoundClips = null;
        private List<AnimationClip> CurrentAnimatorClips = null;
        private void DrawWindow()
        {
            // Animator
            // Need Save Location
            // Button to create list
            // View List of Combos
            // Preivew Scene to see combo?
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    TargetAnim = (Animator)EditorGUILayout.ObjectField("Animator ", TargetAnim, typeof(Animator), true);
                    SavePathEditor.HandleSaveFolderEditorPref(SavePathEditor.ComboKeyName, SavePathEditor.ComboSavePath, "Combo Item");

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Create Combo Items"))
                        {
                            var clips = TargetAnim.runtimeAnimatorController.animationClips;
                            for (int i = 0; i < clips.Length; i++)
                            {
                                if (FoundClips.Contains(clips[i]))
                                {
                                    continue;
                                }
                                ComboItem newItem = ScriptableObject.CreateInstance<ComboItem>();
                                newItem.animClip = clips[i];
                                newItem.ItemName = clips[i].name;
                                SavePathEditor.SavePath(newItem, clips[i].name);
                                // Creates new Combo SO with that animation clip
                                // Afterwards Init again to reset
                            }
                        }

                        if (GUILayout.Button("Refresh", GUILayout.MaxWidth(70)))
                        {
                            Init();
                        }

                        if(GUILayout.Button("Check", GUILayout.MaxWidth(50)))
                        {
                            if(TargetAnim != null)
                            {
                                checkToggle = !checkToggle;
                                CurrentAnimatorClips = TargetAnim.runtimeAnimatorController.animationClips.ToList<AnimationClip>();
                            }
                            else
                            {
                                Debug.LogError("Animator is null, to check animation clips please input an animator");
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                    
                    SearchBarMethod(ref SearchString);

                    if (FoundComboItems.Count > 0)
                    {
                        scrollBar = GUILayout.BeginScrollView(scrollBar);
                        {
                            for (int i = 0; i < FoundComboItems.Count; i++)// (ComboItem item in FoundComboItems)
                            {
                                if (!StringContains(FoundComboItems[i].name, SearchString) && !String.IsNullOrEmpty(SearchString))
                                {
                                    continue;
                                }

                                if (checkToggle)
                                {
                                    if (CurrentAnimatorClips.Contains(FoundComboItems[i].animClip))
                                    {
                                        GUI.backgroundColor = Color.green;
                                    }
                                    else
                                    {
                                        GUI.backgroundColor = Color.red;
                                    }
                                }
                               

                                GUILayout.BeginVertical("box");
                                {
                                    GUILayout.BeginHorizontal();
                                    {
                                        FoundComboItems[i].isOpen = EditorGUILayout.Foldout(FoundComboItems[i].isOpen, FoundComboItems[i].name);

                                        if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
                                        { 
                                            string assetPath = AssetDatabase.GetAssetPath(new SerializedObject(FoundComboItems[i]).targetObject);

                                            AssetDatabase.DeleteAsset(assetPath);
                                            FoundComboItems.Remove(FoundComboItems[i]);
                                            AssetDatabase.SaveAssets();
                                            AssetDatabase.Refresh();
                                            Init();
                                        }
                                    }
                                    GUILayout.EndHorizontal();

                                    if (FoundComboItems[i].isOpen)
                                    {
                                        EditorGUI.indentLevel++;
                                        EditorGUILayout.ObjectField("Animation Clip", FoundComboItems[i].animClip, typeof(AnimationClip), allowSceneObjects: false);
                                        FoundComboItems[i].ItemName = EditorGUILayout.TextField("Combo Name", FoundComboItems[i].ItemName);
                                        FoundComboItems[i].ItemDescription = EditorGUILayout.TextField("Combo Description", FoundComboItems[i].ItemDescription);
                                        EditorGUI.indentLevel--;
                                    }
                                }
                                GUILayout.EndVertical();
                            }
                        }
                        GUILayout.EndScrollView();
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                   
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void Init()
        {
            FoundComboItems?.Clear();
            FoundClips?.Clear();
            FoundClips = new List<AnimationClip>();

            FoundComboItems = FindAllComboItems<ComboItem>();

            if(FoundComboItems.Count > 0)
            {
                for(int i = 0; i<FoundComboItems.Count; i++)
                {
                    FoundClips.Add(FoundComboItems[i].animClip);
                }
            }
        }

        private static List<T>FindAllComboItems<T>() where T : ComboItem
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return a.ToList();
        }

        public void SearchBarMethod(ref string SearchString)
        {
            EditorGUILayout.BeginHorizontal();
            {
                SearchString = GUILayout.TextField(SearchString, GUI.skin.FindStyle("ToolbarSeachTextField"));

                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    SearchString = "";
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool StringContains(string source, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }

    public static class SavePathEditor
    {
        public static readonly string ComboSavePath = "Assets/Scripts/Modular Combo/ScriptableObjects";
        public static readonly string ComboKeyName = "ComboItems";

        private static DefaultAsset targetFolder = null;

        private static void Example()
        {
            HandleSaveFolderEditorPref(ComboKeyName, ComboSavePath, "Spells");
        }

        public static void SavePath( ScriptableObject saveObject, string name)
        {
            if (name == null)
            {
                Debug.Log("Nothing here");
                return;
            }

            string path = ComboSavePath;
            if (PlayerPrefs.HasKey(ComboKeyName)) path = PlayerPrefs.GetString(ComboKeyName);
            else PlayerPrefs.SetString(ComboKeyName, ComboSavePath);
            path += "/";
            if (!System.IO.Directory.Exists(path))
            {
                EditorUtility.DisplayDialog("The desired save folder doesn't exist",
                    "Set a valid save folder", "Ok");
                return;
            }

            path += name;
            string fullPath = path + ".asset";
            if (System.IO.File.Exists(fullPath))
            {
                SavePathIndent(path, saveObject);
            }
            else DoSaving(fullPath, saveObject);
        }

        private static void SavePathIndent(string path, ScriptableObject saveObject, int i = 1)
        {
            int number = i;
            string newPath = path + "_" + number.ToString();
            string fullPath = newPath + ".asset";
            if (System.IO.File.Exists(fullPath))
            {
                number++;
                SavePathIndent(path, saveObject, number);
            }
            else
            {
                DoSaving(fullPath, saveObject);
            }
        }

        private static void DoSaving(string fileName, ScriptableObject saveObject)
        {
            AssetDatabase.CreateAsset(saveObject, fileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void HandleSaveFolderEditorPref(string keyName, string defaultPath, string logsFeatureName)
        {
            if (!PlayerPrefs.HasKey(keyName))
                PlayerPrefs.SetString(keyName, defaultPath);

            targetFolder = (DefaultAsset)AssetDatabase.LoadAssetAtPath(PlayerPrefs.GetString(keyName), typeof(DefaultAsset));

            if (targetFolder == null)
            {
                PlayerPrefs.SetString(keyName, defaultPath);
                targetFolder = (DefaultAsset)AssetDatabase.LoadAssetAtPath(PlayerPrefs.GetString(keyName), typeof(DefaultAsset));

                if (targetFolder == null)
                {
                    targetFolder = (DefaultAsset)AssetDatabase.LoadAssetAtPath("Assets/", typeof(DefaultAsset));
                    if (targetFolder == null)
                        Debug.LogWarning("The desired save folder doesn't exist. " + PlayerPrefs.GetString(keyName));
                    else
                        PlayerPrefs.SetString("Assets/", defaultPath);
                }
            }

            targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("New " + logsFeatureName + " Folder", targetFolder, typeof(DefaultAsset), false);

            if (targetFolder != null && IsAssetAFolder(targetFolder))
            {
                string path = AssetDatabase.GetAssetPath(targetFolder); //EditorUtility.OpenFilePanel("Open Folder", "", "");
                PlayerPrefs.SetString(keyName, path);
                EditorGUILayout.HelpBox("Valid folder! " + logsFeatureName + " save path: " + path, MessageType.Info, true);
            }
            else EditorGUILayout.HelpBox("Select the new " + logsFeatureName + " Folder", MessageType.Warning, true);
        }

        private static bool IsAssetAFolder(UnityEngine.Object obj)
        {
            string path = "";

            if (obj == null) return false;

            path = AssetDatabase.GetAssetPath(obj.GetInstanceID());

            if (path.Length > 0)
            {
                if (Directory.Exists(path)) return true;
                else return false;
            }

            return false;
        }
    }
}