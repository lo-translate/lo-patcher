namespace LoPatcher.BundlePatch
{
    public class PatchResult
    {
        public bool Success { get; }

        public string Status { get; }

        public PatchResult(bool success, string status = null)
        {
            Success = success;
            Status = status;
        }
    }
}