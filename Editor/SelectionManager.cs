using System;

namespace Sabresaurus.RemoteActions
{
    [Serializable]
    public class SelectionManager
    {
        string selectedPath;

        public string SelectedPath => selectedPath;

        public void SetSelectedPath(string newPath)
        {
            selectedPath = newPath;
        }
    }
}