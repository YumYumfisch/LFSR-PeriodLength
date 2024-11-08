using Serilog;

namespace CSV_PeriodLength;

public static class Program
{
    private static int MaxRegisterCount { get; } = 14;

    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Information()
            .CreateLogger();

        Log.Information("Searching period length from CSV files");

        int[][] registerPeriods = new int[MaxRegisterCount][];

        for (int registerCount = 1; registerCount <= MaxRegisterCount; registerCount++)
        {
            string filename = $"LFSR_{registerCount}.csv";

            List<int> periods = [];

            string[] lines = File.ReadAllLines(filename).Skip(1).ToArray();

            foreach (string line in lines)
            {
                string[] linePeriods = line.Split(',').Skip(1).ToArray();

                foreach (string linePeriod in linePeriods)
                {
                    if (!int.TryParse(linePeriod, out int period))
                    {
                        continue;
                    }

                    if (!periods.Contains(period))
                    {
                        periods.Add(period);
                    }
                }
            }

            registerPeriods[registerCount - 1] = [.. periods.Order()];
            Log.Information("{registers} registers: {periods}", registerCount, registerPeriods[registerCount - 1]);
        }
    }
}
