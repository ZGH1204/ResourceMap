using System.Collections.Generic;

namespace Assets.Editor.ResourceMap
{
    public interface IDependencyInfo
    {
        void Load(bool onlyAB);
        List<string> GetChildren(string path);
        List<string> GetParent(string path);
    }
}
