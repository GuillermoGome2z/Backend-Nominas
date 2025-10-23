using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace ProyectoNomina.Backend.Services
{
    public static class ExportService
    {
        // CSV genérico con columnas + selectores
        public static byte[] ToCsv<T>(IEnumerable<T> data, (string Header, Func<T, object?> Select)[] cols)
        {
            var sb = new StringBuilder();

            // Encabezados
            sb.AppendLine(string.Join(",", cols.Select(c => Escape(c.Header))));

            // Filas
            foreach (var row in data)
            {
                var fields = cols.Select(c => Escape(c.Select(row)?.ToString() ?? string.Empty));
                sb.AppendLine(string.Join(",", fields));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // XLSX genérico con columnas + selectores (ClosedXML)
        public static byte[] ToXlsx<T>(IEnumerable<T> data, (string Header, Func<T, object?> Select)[] cols, string sheetName = "Datos")
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            // Encabezados
            for (int c = 0; c < cols.Length; c++)
                ws.Cell(1, c + 1).SetValue(cols[c].Header);

            // Filas
            int r = 2;
            foreach (var item in data)
            {
                for (int c = 0; c < cols.Length; c++)
                {
                    var val = cols[c].Select(item);
                    // Forzamos string para evitar conversión implícita a XLCellValue
                    ws.Cell(r, c + 1).SetValue(val?.ToString() ?? string.Empty);
                }
                r++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // Escapa campos CSV (comillas dobles, comas y saltos de línea)
        private static string Escape(string s)
        {
            var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
            var safe = s.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{safe}\"" : safe;
        }
    }
}
