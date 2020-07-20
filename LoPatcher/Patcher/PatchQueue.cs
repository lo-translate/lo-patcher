using System.Collections.Generic;

namespace LoPatcher.Patcher
{
    public class PatchQueue
    {
        public IList<PatchQueueItem> Items { get; } = new List<PatchQueueItem>();
    }
}