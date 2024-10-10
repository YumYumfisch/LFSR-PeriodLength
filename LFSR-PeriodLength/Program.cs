namespace LFSR_PeriodLength;

internal class Program
{
    static int MatrixSize { get; } = 128;

    public static void Main(string[] args)
    {
        _ = args; // unused

        string filePath = "lfsr_periods.csv";

        WritePeriodMatrixToCsv(filePath);
    }

    // Methode zur Berechnung des nächsten Zustands
    static uint NextState(uint state, uint modulo)
    {
        state <<= 1;
        return state % modulo;
    }

    // Methode zur Berechnung der Periodenlänge
    static int LfsrPeriod(uint modulo, uint startValue)
    {
        uint state = startValue;
        List<uint> seenStates = [];

        while (true)
        {
            if (state == 0)
            {
                return 1; // Nullzustand erreicht, Periodenlänge ist 1
            }

            if (seenStates.Contains(state))
            {
                break; // Zustand schon mal gesehen, Periodenlänge gefunden
            }

            seenStates.Add(state); // Zustand speichern
            state = NextState(state, modulo);
        }

        if (state != seenStates[0])
        {
            int i = 0;
            while (seenStates.Skip(i).Contains(state))
            {
                i++;
            }

            return seenStates.Count + 1 - i;
        }

        return seenStates.Count;
    }

    // Methode zum Schreiben der CSV-Datei
    static void WritePeriodMatrixToCsv(string filePath)
    {
        List<string> lines = []; // Liste zur Speicherung der Zeilen

        List<uint> oneToEnd = new List<uint>();
        for (uint i = 1; i <= MatrixSize; i++)
        {
            oneToEnd.Add(i);
        }
        lines.Add(" ," + string.Join(",", oneToEnd));

        // Schleife für StartValue von 1 bis 100
        for (uint startValue = 1; startValue <= MatrixSize; startValue++)
        {
            List<string> line = []; // Liste zur Speicherung der Periodenlängen für die aktuelle Zeile

            // Schleife für Modulo von 1 bis MatrixSize
            for (uint modulo = 1; modulo <= MatrixSize; modulo++)
            {
                int periodLength = LfsrPeriod(modulo, startValue);
                if (periodLength < 0)
                {
                    line.Add(" ");
                }
                else
                {
                    line.Add(periodLength.ToString()); // Periodenlänge zur aktuellen Zeile hinzufügen
                }
            }

            // Zeile zur Liste der Linien hinzufügen (kommagetrennt)
            lines.Add(startValue + "," + string.Join(",", line));
        }

        // Alle Zeilen auf einmal in die CSV-Datei schreiben
        File.WriteAllLines(filePath, lines);

        Console.WriteLine($"CSV-Datei '{filePath}' erfolgreich erstellt.");
    }
}
