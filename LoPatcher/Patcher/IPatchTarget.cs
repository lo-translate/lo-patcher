using System.IO;

namespace LoPatcher.Patcher
{
    public interface IPatchTarget
    {
        public bool CanPatch(Stream stream);

        public bool Patch(Stream stream);
    }
}