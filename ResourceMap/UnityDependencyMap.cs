using UnityEngine;
using UnityEditor;
using System.IO;

namespace Assets.Editor.ResourceMap
{
    public class UnityDependencyMap : IDependencyMap
    {
        UnityDependencyInfo UnityDependencyInfo;

        public string Name
        {
            get
            {
                return "Unity";
            }
        }

        public int Priority
        {
            get
            {
                return 100;
            }
        }

        public IDependencyInfo DependencyInfo
        {
            get
            {
                if(UnityDependencyInfo == null)
                {
                    var info = ScriptableObject.CreateInstance<UnityDependencyInfo>();
                    var script = MonoScript.FromScriptableObject(info);
                    string path = AssetDatabase.GetAssetPath(script);
                    var dir = Path.GetDirectoryName(path);
                    path = Path.Combine(dir, "Config/UnityDependencyInfo.asset");
                    if(File.Exists(path))
                    {
                        UnityDependencyInfo = AssetDatabase.LoadAssetAtPath<UnityDependencyInfo>(path);
                        Object.DestroyImmediate(info);
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(info, path);
                        UnityDependencyInfo = info;
                    }
                }
                return UnityDependencyInfo;
            }
        }

        public void InitNode(MapNode node, string path)
        {
            node.Path = path;
            node.Id = node.Path.GetHashCode();
            node.Name = Path.GetFileName(node.Path);

            var count = UnityDependencyInfo == null ? 0 : UnityDependencyInfo.GetParent(path).Count;
            var txt = string.Format(" [{0}] {1}", count, node.Name);
            node.GUIContent = new GUIContent(txt, AssetDatabase.GetCachedIcon(node.Path), path);
        }

        public void CalcNodeSize(MapNode node)
        {
            node.Rect.size = new Vector2(GUI.skin.label.CalcSize(new GUIContent(node.GUIContent.text)).x + 20, 40);
        }

        public bool CheckIsLoopDependency(string orgPath, string toPath)
        {
            var depInfo = UnityDependencyInfo != null ? UnityDependencyInfo.GetParent(orgPath) : null;
            if (depInfo != null)
            {
                if (depInfo.Contains(toPath))
                {
                    return true;
                }
                else
                {
                    foreach (var p in depInfo)
                    {
                        if (CheckIsLoopDependency(p, toPath))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
