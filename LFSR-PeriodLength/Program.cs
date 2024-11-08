using Serilog;

namespace LFSR_PeriodLength;

public static class Program
{
    private static int MaxRegisterCount { get; } = 10;

    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Information()
            .CreateLogger();

        Log.Information("Calculating Fibonacci LFSR periods");
        NextState nextState = FibonacciNextState;

        for (uint registerCount = 1; registerCount <= MaxRegisterCount; registerCount++)
        {
            uint maxTap = (uint)Math.Pow(2, registerCount) - 1;
            uint minTap = (uint)Math.Pow(2, registerCount - 1);

            List<(uint tap, uint[][] periods)> tapPeriods = [];

            for (uint tap = minTap; tap <= maxTap; tap++)
            {
                uint maxState = maxTap;
                List<uint> seenStates = [];
                List<uint[]> periods = [];

                for (uint state = 1; state <= maxState; state++)
                {
                    if (seenStates.Contains(state))
                    {
                        continue;
                    }

                    #region LFSR
                    List<uint> lfsrStates = [];
                    while (true)
                    {
                        if (lfsrStates.Contains(state))
                        {
                            break;
                        }

                        seenStates.Add(state);
                        lfsrStates.Add(state);

                        state = nextState(state, tap, registerCount);
                    }

                    if (state == lfsrStates[0])
                    {
                        periods.Add([.. lfsrStates]);
                        continue;
                    }

                    int i = 1;
                    while (lfsrStates.Skip(i).Contains(state))
                    {
                        i++;
                    }
                    i--;

                    uint[] period = [.. lfsrStates.Skip(i)];
                    if (period.Length != lfsrStates.Count - i)
                    {
                        throw new Exception("Fatal error period length"); // Should never happen
                    }
                    periods.Add(period);
                    #endregion LFSR
                }

                tapPeriods.Add((tap, [.. periods]));
                Console.Title = $"LFSR with {registerCount} registers, {(double)tap / maxTap * 100:0,00}%";
            }
            LogRegisterData(registerCount, tapPeriods);
            CreateCsv(registerCount, tapPeriods);
        }
        Log.CloseAndFlush();
    }

    private static void CreateCsv(uint registerCount, List<(uint tap, uint[][] periods)> tapPeriods)
    {
        Log.Information("Creating CSV");

        string filePath = $"./LFSR_{registerCount}.csv";
        List<string> lines = [];

        List<string> startValues = [];
        uint maxStartValue = (uint)Math.Pow(2, registerCount) - 1;
        for (uint i = 1; i <= maxStartValue; i++)
        {
            startValues.Add($"{i}");
        }
        lines.Add(" ," + string.Join(",", startValues));

        uint maxTap = (uint)Math.Pow(2, registerCount) - 1;
        uint minTap = (uint)Math.Pow(2, registerCount - 1);
        for (uint tap = minTap; tap <= maxTap; tap++)
        {
            List<string> line = [];

            for (uint startValue = 1; startValue <= maxStartValue; startValue++)
            {
                int periodLength = tapPeriods
                    .First(tapPeriod => tapPeriod.tap == tap)
                    .periods
                    .First(period => period.Contains(startValue))
                    .Length;

                line.Add($"{periodLength}");
            }

            lines.Add(Convert.ToString(tap, 2) + "," + string.Join(",", line));
        }

        File.WriteAllLines(filePath, lines);
        Log.Information("Created {filePath}", filePath);
    }

    private static void LogRegisterData(uint registerCount, List<(uint tap, uint[][] periods)> tapPeriods)
    {
        Log.Information("Done with {count} registers", registerCount);
        foreach ((uint tap, uint[][] periods) in tapPeriods
            .OrderBy(tapPeriod => tapPeriod.periods.Length)
            .OrderByDescending(tapPeriod => tapPeriod.periods.Max(period => period.Length)))
        {
            int maxPeriodLength = periods.Max(period => period.Length);

            uint[][] periodsToDisplay = periods;
            if (registerCount > 8)
            {
                periodsToDisplay = [[999]];
            }

            if (periods.Length == 1)
            {
                Log.Information("Tap {0btap} ({0xtap}) has {count} period. This periods length is {maxLength} which is perfect: {periods}", $"0b{Convert.ToString(tap, 2)}", $"0x{Convert.ToString(tap, 16)}", periods.Length, maxPeriodLength, periodsToDisplay);
            }
            else
            {
                Log.Information("Tap {0btap} ({0xtap}) has {count} periods. The maximum period length is {maxLength}: {periods}", $"0b{Convert.ToString(tap, 2)}", $"0x{Convert.ToString(tap, 16)}", periods.Length, maxPeriodLength, periodsToDisplay);
            }
        }
    }

    /// <summary>
    /// Calculates the next state of the LFSR
    /// </summary>
    /// <param name="state">Current state of the LFSR registers</param>
    /// <param name="tap">Taps of this LFSR</param>
    /// <param name="registerCount">Number of registers in this LFSR</param>
    /// <returns>Next state of the LFSR</returns>
    private delegate uint NextState(uint state, uint tap, uint registerCount);

    /// <summary>
    /// Calculates the next state of the fibonacci LFSR
    /// </summary>
    /// <param name="state">Current state of the LFSR registers</param>
    /// <param name="tap">Taps of the LFSR</param>
    /// <param name="registerCount">Number of registers in the LFSR</param>
    /// <returns>Next state of the fibonacci LFSR</returns>
    private static uint FibonacciNextState(uint state, uint tap, uint registerCount)
    {
        List<byte> registers = [];

        for (int i = 0; i < registerCount; i++)
        {
            byte register = (byte)(state & 1);
            registers.Add(register);
            state >>= 1;
        }

        if (state != 0)
        {
            throw new Exception("State is not zero");
        }

        uint mustHaveTap = (uint)Math.Pow(2, registerCount - 1);
        if ((tap & mustHaveTap) == 0)
        {
            throw new Exception("Taps do not tap first register");
        }

        byte feedback = 0;

        for (int i = 0; i < registerCount; i++)
        {
            if ((tap & 1) == 1 && registers[(int)(registerCount - 1 - i)] == 1)
            {
                feedback++;
                feedback %= 2;
            }
            tap >>= 1;
        }

        if (tap != 0)
        {
            throw new Exception("Tap mask is not zero");
        }

        if (feedback > 1)
        {
            throw new Exception("Feedback is bigger than 1");
        }

        registers.RemoveAt(0);
        registers.Add(feedback);

        for (int i = 0; i < registerCount; i++)
        {
            state <<= 1;
            state |= registers.Last();
            registers.RemoveAt(registers.Count - 1);
        }

        return state;
    }
}
