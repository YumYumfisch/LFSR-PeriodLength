namespace LFSR_PeriodLength;

internal class Program
{
    /// <summary>
    /// Size of the Matrix in the CSV file
    /// </summary>
    static int MatrixSize { get; } = 128;

    /// <summary>
    /// Application entry point
    /// </summary>
    /// <param name="args">Unused arguments</param>
    public static void Main(string[] args)
    {
        _ = args; // unused

        string filePath = "lfsr_periods.csv";

        WritePeriodMatrixToCsv(filePath);
    }

    /// <summary>
    /// Calculates the next state of the LSFR
    /// </summary>
    /// <param name="state">Current State</param>
    /// <param name="modulo">Modulo value</param>
    /// <returns>Next state of the LSFR</returns>
    static uint NextState(uint state, uint modulo)
    {
        state <<= 1;
        return state % modulo;
    }

    /// <summary>
    /// Calculates the period length of the LSFR
    /// </summary>
    /// <param name="modulo">Modulo value of the linear function</param>
    /// <param name="startValue">Start value of the shift register</param>
    /// <returns>The period length of the LSFR with the specified values</returns>
    static int LfsrPeriod(uint modulo, uint startValue)
    {
        uint state = startValue;
        List<uint> seenStates = [];

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
            state = NextState(state, modulo);
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

        return seenStates.Count;
    }

    /// <summary>
    /// Creates the LSFR Period Matrix CSV File
    /// </summary>
    /// <param name="filePath">Path of the CSV file</param>
    static void WritePeriodMatrixToCsv(string filePath)
    {
        List<string> lines = [];

        List<uint> oneToEnd = [];
        for (uint i = 1; i <= MatrixSize; i++)
        {
            oneToEnd.Add(i);
        }
        lines.Add(" ," + string.Join(",", oneToEnd));

        for (uint startValue = 1; startValue <= MatrixSize; startValue++)
        {
            List<string> line = [];

            for (uint modulo = 1; modulo <= MatrixSize; modulo++)
            {
                int periodLength = LfsrPeriod(modulo, startValue);
                if (periodLength < 0)
                {
                    line.Add(" ");
                }
                else
                {
                    line.Add(periodLength.ToString());
                }
            }

            lines.Add(startValue + "," + string.Join(",", line));
        }

        File.WriteAllLines(filePath, lines);
        Console.WriteLine($"Created '{filePath}'.");
    }
}
