using Verse;

namespace StevesDoors
{
    public class GraphicDataEnhancedDoors : GraphicData
    {
        public bool isAccentGraphic = false;
        public bool isLeftSideGraphic = false;
        public bool shouldFade = false;
        public bool shouldArch = false;

        public float xMoveAmount = 1.0f;
        public float doorIrisMaxAngle = 0f;
        public float spinFactor = 0f;
        public float fadeFactor = 0f;
        public float archFactor = 0f;

        public GraphicDataEnhancedDoors() : base()
        {

        }
    }
}
