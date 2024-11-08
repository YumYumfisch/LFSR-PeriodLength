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
            Log.Information("{registerPadding}{registers} registers: {periods}", new string(' ', 2 - registerCount.ToString().Length), registerCount, registerPeriods[registerCount - 1]);
        }

        Log.Information("Possible register count to get each period, ordered by minimum number of registers:");

        Dictionary<int, List<int>> periodRegisters = [];
        for (int i = 0; i < registerPeriods.Length; i++)
        {
            for (int j = 0; j < registerPeriods[i].Length; j++)
            {
                if (!periodRegisters.ContainsKey(registerPeriods[i][j]))
                {
                    periodRegisters.Add(registerPeriods[i][j], [i + 1]);
                }
                else
                {
                    List<int> registers = periodRegisters.GetValueOrDefault(registerPeriods[i][j])!;
                    if (!registers.Contains(i + 1))
                    {
                        registers.Add(i + 1);
                    }
                }
            }
        }

        foreach (KeyValuePair<int, List<int>> period in periodRegisters)
        {
            Log.Information("Period {periodPadding}{period}: {registers}", new string(' ', 5 - period.Key.ToString().Length), period.Key, period.Value);
        }

        Log.Information("Possible register count to get each period, ordered by period:");

        IOrderedEnumerable<KeyValuePair<int, List<int>>> orderedPeriodRegisters = periodRegisters.OrderBy(periodRegister => periodRegister.Key);
        foreach (KeyValuePair<int, List<int>> period in orderedPeriodRegisters)
        {
            Log.Information("Period {periodPadding}{period}: {registers}", new string(' ', 5 - period.Key.ToString().Length), period.Key, period.Value);
        }
    }
}
