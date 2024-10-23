using Serilog;

namespace LFSR_PeriodLength;

public class Program
{
    static int MaxRegisterCount { get; } = 8;

    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            //.MinimumLevel.Debug()
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
                Log.Debug("Done with tap {tap}", Convert.ToString(tap, 2));
            }
            Log.Information("Done with {count} registers", registerCount);
            foreach ((uint tap, uint[][] periods) in tapPeriods.OrderBy(tapPeriod => tapPeriod.periods.Length))
            {
                if (periods.Length == 1)
                {
                    Log.Information("Tap {tap} has periods {periods} which is perfect", Convert.ToString(tap, 2), periods);
                }
                else
                {
                    Log.Information("Tap {tap} has periods {periods}", Convert.ToString(tap, 2), periods);
                }
            }
        }
        Log.CloseAndFlush();
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
#if DEBUG && true
        object[] initialValues = // DEBUG
        [
            state, Convert.ToString(state, 2),
            tap, Convert.ToString(tap, 2),
            registerCount, Convert.ToString(registerCount, 2)
        ];
#endif

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

        registers.RemoveAt(0); // = dequeue
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
