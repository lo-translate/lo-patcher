namespace LoPatcher.Patcher
{
    public class PatchQueueItem
    {
        public string File { get; private set; }

        public IPatchTarget Container { get; private set; }

        public PatchQueueItem(string file, IPatchTarget container)
        {
            this.File = file;
            this.Container = container;
        }
    }
}