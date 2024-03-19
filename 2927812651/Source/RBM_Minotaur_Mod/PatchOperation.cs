using System.Xml;
using Verse;

namespace RBM_Minotaur
{
    public class PatchOperationRockChunk : PatchOperation
    {
        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (this.match != null)
            {
                if (MinotaurSettings.disableChunks == false)
                {
                    return this.match.Apply(xml);
                }
                return true;
            }
            return false;
        }
        private PatchOperation match;
    }

}
