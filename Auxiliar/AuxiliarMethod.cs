using System.Globalization;


namespace DicomDir.Auxiliar
{
   
        // --- MÉTODOS AUXILIARES ---
        public static class AuxiliarMethod
        {
        public static void LogColorido(string arvore, string label, params object[] valores)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(arvore);

            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(label + " ");

            
            if (valores != null)
            {
                for (int i = 0; i < valores.Length; i++)
                {
            
                    if (i % 2 == 0)
                    {
            
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }
                    else
                    {
            
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }

                    Console.Write(valores[i] + " ");
                }
            }

            
            Console.WriteLine();
            Console.ResetColor();
        }

        public static DateTime ParseDicomDate(string dicomDate)
            {
                if (string.IsNullOrWhiteSpace(dicomDate)) return DateTime.MinValue;
                if (DateTime.TryParseExact(dicomDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    return date;
                return DateTime.MinValue;
            }
        }
    }