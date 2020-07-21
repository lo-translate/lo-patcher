namespace LoPatcher.Patcher
{
    public class PatchProgress
    {
        public string SetTargetAndReset { get; internal set; }
        public int IncreaseTotal { get; internal set; }
        public int IncreaseCurrent { get; internal set; }
    }
}