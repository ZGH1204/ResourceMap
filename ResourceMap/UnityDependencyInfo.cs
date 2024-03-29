﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.ResourceMap
{
    [PreferBinarySerialization]
    public class UnityDependencyInfo : ResourceDependencyInfo
    {
        public override List<string> GetChildren(string path)
        {
            // if (Childrens.ContainsKey(path))
                // return Childrens[path];

            var list = new List<string>();
            var arr = AssetDatabase.GetDependencies(path, false);
            foreach (var p in arr)
            {
                var ext = Path.GetExtension(p);
                if (!ext.Equals(".cs"))
                {
                    list.Add(p);
                }
            }
            list.Sort((p1, p2) => {
                var ext1 = Path.GetExtension(p1).ToLower();
                var ext2 = Path.GetExtension(p2).ToLower();

                if (ext1[1] != ext2[1])
                {
                    return ext1[1] - ext2[1];
                }
                if (ext1.LastOrDefault() != ext2.LastOrDefault())
                {
                    return ext1.LastOrDefault() - ext2.LastOrDefault();
                }
                return ext1.Length - ext2.Length;
            });

            return list;
        }

        public override List<string> GetParent(string path)
        {
            if (Parents.ContainsKey(path))
                return Parents[path];
            return new List<string>();
        }

        public override void Load(bool onlyAB)
        {
            Childrens.Clear();
            Parents.Clear();

            EditorUtility.DisplayCancelableProgressBar("Hold on", "", 0);
            AssetDatabase.SaveAssets();
            string[] allAsset = AssetDatabase.GetAllAssetPaths();
            
            allAsset = onlyAB ? allAsset.Where((s) => { return s.StartsWith("Assets/ResourcesAB"); }).ToArray() : allAsset;
            int count = allAsset.Length;
            for (int i = 0; i < count; i++)
            {
                if (i % 100 == 0)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Hold on", "GetDependencies " + i + "/" + count, (float)i / count))
                    {
                        EditorUtility.ClearProgressBar();
                        return;
                    }
                }
                string p = allAsset[i];
                if (Childrens.ContainsKey(p))
                {
                    foreach (var ps in Childrens[p])
                    {
                        if (Parents.ContainsKey(ps))
                            Parents[ps].Remove(p);
                    }
                }
                var dp = AssetDatabase.GetDependencies(p, false);
                Childrens[p] = new UStringList();
                Childrens[p].AddRange(dp);
                Childrens[p].Remove(p);
                foreach (var d in dp)
                {
                    if (d == p)
                        continue;
                    if (!Parents.ContainsKey(d))
                        Parents.Add(d, new UStringList());
                    Parents[d].Remove(p);
                    Parents[d].Add(p);
                }
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets(); 
            EditorUtility.ClearProgressBar();
        }
    }
}
