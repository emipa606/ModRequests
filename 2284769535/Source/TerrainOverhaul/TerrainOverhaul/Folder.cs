using System.Collections.Generic;

namespace TerrainOverhaul
{
    public class Folder<T>
    {
        public static void Insert(Folder<T> root, string path, T item)
        {
            string[] pathParts = path.Split('/');
            Folder<T> current = root;
            for (int i = 0; i < pathParts.Length; i++)
            {
                string name = pathParts[i];
                if (i != pathParts.Length - 1)
                {
                    current = current.Subs[name];
                }
                else
                {
                    current.Files[name] = item;
                }
            }
        }

        public readonly string Name;

        public Dictionary<string, Folder<T>> Subs = new Dictionary<string, Folder<T>>();
        public Dictionary<string, T> Files = new Dictionary<string, T>();

        public Folder(string name)
        {
            this.Name = name;
        }
    }
}
