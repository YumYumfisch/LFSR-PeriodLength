namespace LFSR_PeriodLength;

public class Program
{
    /// <summary>
    /// Size of the Matrix in the CSV file
    /// </summary>
    static int MatrixSize { get; } = 128;

    /// <summary>
    /// Number of LfsrRegisters for fibonacci and galois LFSR
    /// </summary>
    private static int LfsrRegisterCount { get; } = 4;

    /// <summary>
    /// Application entry point
    /// </summary>
    /// <param name="args">Unused arguments</param>
    public static void Main(string[] args)
    {
        _ = args; // unused

        uint state = 1;
        uint tap = 0b1001;
        Console.WriteLine(state);
        for (int i = 0; i < LfsrRegisterCount * LfsrRegisterCount; i++)
        {
            state = FibonacciNextState(state, tap);
            Console.WriteLine(state);
        }
        Console.WriteLine("Period: " + LfsrPeriod(1, tap, FibonacciNextState));

        WritePeriodMatrixToCsv("modulo_periods.csv", ModuloNextState);
        WritePeriodMatrixToCsv("fibonacci_periods.csv", FibonacciNextState);
        //WritePeriodMatrixToCsv("galois_periods.csv", GaloisNextState);
    }

    /// <summary>
    /// Calculates the next state of the LSFR
    /// </summary>
    /// <param name="state">Current state of the LFSR registers</param>
    /// <param name="functionModifier">Modifier that modifies the function of this LSFR</param>
    /// <returns>Next state of the LSFR</returns>
    private delegate uint NextState(uint state, uint functionModifier);

    /// <summary>
    /// Calculates the next state of the modulo LSFR
    /// </summary>
    /// <param name="state">Current state of the LFSR registers</param>
    /// <param name="modulo">Modulo value</param>
    /// <returns>Next state of the modulo LSFR</returns>
    private static uint ModuloNextState(uint state, uint modulo)
    {
        state <<= 1;
        return state % modulo;
    }

    /// <summary>
    /// Calculates the next state of the fibonacci LSFR
    /// </summary>
    /// <param name="state">Current state of the LFSR registers</param>
    /// <param name="tapMask">Masks which registers are used for the linear function of the LSFR</param>
    /// <returns>Next state of the fibonacci LSFR</returns>
    private static uint FibonacciNextState(uint state, uint tapMask)
    {
        uint leftmostBit = state ^ tapMask;
        leftmostBit = (leftmostBit % 2) << (LfsrRegisterCount - 1);

        state >>= 1;

        return state | leftmostBit;
    }

    /// <summary>
    /// Calculates the period length of the LSFR
    /// </summary>
    /// <param name="startValue">Start configuration of the LFSR</param>
    /// <param name="functionModifier">Modifier that modifies the function of this LSFR</param>
    /// <param name="function">Linear function of this LSFR</param>
    /// <returns>The period length of the LSFR with the specified values</returns>
    private static int LfsrPeriod(uint startValue, uint functionModifier, NextState function)
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
            state = function(state, functionModifier);
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
    /// <param name="function">Linear function of this LSFR</param>
    private static void WritePeriodMatrixToCsv(string filePath, NextState function)
    {
        List<string> lines = [];

        List<uint> modifiers = [];
        for (uint i = 1; i <= MatrixSize; i++)
        {
            modifiers.Add(i);
        }
        lines.Add(" ," + string.Join(",", modifiers));

        for (uint startValue = 1; startValue <= MatrixSize; startValue++)
        {
            List<string> line = [];

            for (uint functionModifier = 1; functionModifier <= MatrixSize; functionModifier++)
            {
                int periodLength = LfsrPeriod(startValue, functionModifier, function);
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
