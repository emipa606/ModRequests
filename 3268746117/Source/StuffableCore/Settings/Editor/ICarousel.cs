using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StuffableCore.Settings.Editor
{
    public interface ICarousel : ISettings
    {
        void ChangeIndex(int index);

        bool Search(string search, out int index);

        int MaxFilterSize();
    }
}
