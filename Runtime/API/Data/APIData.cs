using System.IO;

namespace Sabresaurus.RemoteActions
{
    public abstract class APIData
    {
        #region Create

        public APIData()
        {
        }

        #endregion

        #region Binary Read/Write

        public APIData(BinaryReader br)
        {
        }

        public abstract void Write(BinaryWriter bw);

        #endregion
    }
}