namespace ABMB.Controllers.CSV;

public class CsProcessingOptions
{
    public int MaxDegreeOfParallelism { get; set; } = 4;
    public int BatchSize { get; set; } = 1000;
}
