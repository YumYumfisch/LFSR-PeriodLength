namespace LFSR_PeriodLength;

public class Program
{
    /// <summary>
    /// Size of the Matrix in the CSV file
    /// </summary>
    static int MaxNumberOfRegisters { get; } = 8;

    /// <summary>
    /// Application entry point
    /// </summary>
    /// <param name="args">Unused arguments</param>
    public static void Main(string[] args)
    {
        _ = args; // unused

        WritePeriodMatrixToCsv("fibonacci_periods.csv", FibonacciNextState);
    }

    /// <summary>
    /// Calculates the next state of the LSFR
    /// </summary>
    /// <param name="state">Current state of the LFSR registers</param>
    /// <param name="functionModifier">Modifier that modifies the function of this LSFR</param>
    /// <param name="registerCount">Number of registers in this LSFR</param>
    /// <returns>Next state of the LSFR</returns>
    private delegate uint NextState(uint state, uint functionModifier, uint registerCount);

    /// <summary>
    /// Calculates the next state of the fibonacci LSFR
    /// </summary>
    /// <param name="state">Current state of the LFSR registers</param>
    /// <param name="tapMask">Masks which registers are used for the linear function of the LSFR</param>
    /// <param name="registerCount">Number of registers in the LSFR</param>
    /// <returns>Next state of the fibonacci LSFR</returns>
    private static uint FibonacciNextState(uint state, uint tapMask, uint registerCount)
    {
        object[] initialValues = [state, Convert.ToString(state, 2), tapMask, Convert.ToString(tapMask, 2), registerCount, Convert.ToString(registerCount, 2)];
        _ = initialValues;

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

        if ((tapMask & 1) != 1)
        {
            return 0;
        }

        byte feedback = 0;

        for (int i = 0; i < registerCount; i++)
        {
            if ((tapMask & 1) == 1 && registers[i] == 1)
            {
                feedback++;
                feedback %= 2;
            }
            tapMask >>= 1;
        }

        if (tapMask != 0)
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

    /// <summary>
    /// Calculates the period length of the LSFR
    /// </summary>
    /// <param name="registerCount">Start configuration of the LFSR</param>
    /// <param name="functionModifier">Modifier that modifies the function of this LSFR</param>
    /// <param name="function">Linear function of this LSFR</param>
    /// <returns>The period length of the LSFR with the specified values</returns>
    private static int LfsrPeriod(uint registerCount, uint functionModifier, NextState function)
    {
        List<uint> seenStates = [];

        for (uint state = 1; state <= registerCount * registerCount; state++)
        {
            while (true)
            {
                if (state == 0)
                {
                    return 1;
                }

                if (seenStates.Contains(state))
                {
                    break;
                }

                seenStates.Add(state);
                state = function(state, functionModifier, registerCount);
            }

            if (state != seenStates[0])
            {
                int i = 1;
                while (seenStates.Skip(i).Contains(state))
                {
                    i++;
                }
                i--;

                return seenStates.Count - i;
            }
        }

        return seenStates.Count;
    }

    /// <summary>
    /// Creates the LSFR Period Matrix CSV File
    /// </summary>
    /// <param name="filePath">Path of the CSV file</param>
    /// <param name="function">Linear function of this LSFR</param>
    private static void WritePeriodMatrixToCsv(string filePath, NextState function)
    {
        List<string> lines = [];

        List<uint> taps = [];
        for (uint i = 1; i <= MaxNumberOfRegisters * MaxNumberOfRegisters; i++)
        {
            taps.Add(i);
        }
        lines.Add(" ," + string.Join(",", taps));

        for (uint registerCount = 1; registerCount <= MaxNumberOfRegisters; registerCount++)
        {
            List<string> line = [];

            for (uint functionModifier = 1; functionModifier <= Math.Pow(2, registerCount); functionModifier++)
            {
                int periodLength = LfsrPeriod(registerCount, functionModifier, function);
                if (periodLength < 0)
                {
                    line.Add(" ");
                }
                else
                {
                    line.Add(periodLength.ToString());
                }
            }

            lines.Add(registerCount + "," + string.Join(",", line));
        }

        File.WriteAllLines(filePath, lines);
        Console.WriteLine($"Created '{filePath}'.");
    }
}
