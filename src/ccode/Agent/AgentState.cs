namespace ccode.Agent;

public class AgentState
{
    public int Steps { get; private set; }
    public bool Done { get; private set; }
    public string? LastResult { get; private set; }

    public void IncrementStep() => Steps++;
    public void MarkDone() => Done = true;
    public void SetLastResult(string result) => LastResult = result;
    public void Reset()
    {
        Steps = 0;
        Done = false;
        LastResult = null;
    }

    public override string ToString() =>
        $"Step {Steps}. Last result: {LastResult ?? "none"}";
}
