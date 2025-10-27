using System.Text.Json;

namespace ProyectoNomina.Backend.Services
{
    /// <summary>
    /// Helper para cálculo de ISR con tabla progresiva
    /// Implementa el sistema de impuesto sobre la renta de Guatemala
    /// </summary>
    public static class IsrHelper
    {
        /// <summary>
        /// Tramo de la tabla progresiva de ISR
        /// </summary>
        public class TramoIsr
        {
            public decimal Desde { get; set; }
            public decimal Hasta { get; set; }
            public decimal Tasa { get; set; }
            public decimal ExcesoSobre { get; set; }
            public decimal ImpuestoFijo { get; set; }
        }

        /// <summary>
        /// Calcula el ISR usando la tabla progresiva configurada
        /// </summary>
        /// <param name="baseImponible">Monto sobre el que se calcula ISR (usualmente salario bruto - IGSS)</param>
        /// <param name="isrEscalaJson">JSON con la tabla progresiva de ISR</param>
        /// <param name="decimales">Número de decimales para redondeo</param>
        /// <param name="politicaRedondeo">Política de redondeo: "Arriba", "Normal", "Abajo"</param>
        /// <returns>Monto de ISR a retener</returns>
        public static decimal CalcularIsr(
            decimal baseImponible,
            string isrEscalaJson,
            int decimales = 2,
            string politicaRedondeo = "Normal")
        {
            if (baseImponible <= 0) return 0;

            try
            {
                var tramos = JsonSerializer.Deserialize<List<TramoIsr>>(isrEscalaJson);

                if (tramos == null || !tramos.Any())
                {
                    throw new InvalidOperationException("No se pudo deserializar la tabla de ISR");
                }

                // Ordenar tramos por "Desde" ascendente
                tramos = tramos.OrderBy(t => t.Desde).ToList();

                // Encontrar el tramo correspondiente
                foreach (var tramo in tramos)
                {
                    if (baseImponible >= tramo.Desde && baseImponible <= tramo.Hasta)
                    {
                        decimal exceso = baseImponible - tramo.ExcesoSobre;
                        decimal impuesto = tramo.ImpuestoFijo + (exceso * tramo.Tasa);

                        return Redondear(impuesto, decimales, politicaRedondeo);
                    }
                }

                // Si supera el último tramo, usar el último
                var ultimoTramo = tramos.Last();
                decimal excesoUltimo = baseImponible - ultimoTramo.ExcesoSobre;
                decimal impuestoUltimo = ultimoTramo.ImpuestoFijo + (excesoUltimo * ultimoTramo.Tasa);

                return Redondear(impuestoUltimo, decimales, politicaRedondeo);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error al parsear IsrEscalaJson: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Redondea un decimal según la política especificada
        /// </summary>
        public static decimal Redondear(decimal valor, int decimales, string politica)
        {
            return politica switch
            {
                "Arriba" => Math.Ceiling(valor * (decimal)Math.Pow(10, decimales)) / (decimal)Math.Pow(10, decimales),
                "Abajo" => Math.Floor(valor * (decimal)Math.Pow(10, decimales)) / (decimal)Math.Pow(10, decimales),
                _ => Math.Round(valor, decimales, MidpointRounding.AwayFromZero) // "Normal"
            };
        }

        /// <summary>
        /// Obtiene la tasa marginal de ISR para un monto dado
        /// </summary>
        public static decimal ObtenerTasaMarginal(decimal baseImponible, string isrEscalaJson)
        {
            if (baseImponible <= 0) return 0;

            try
            {
                var tramos = JsonSerializer.Deserialize<List<TramoIsr>>(isrEscalaJson);

                if (tramos == null || !tramos.Any())
                    return 0;

                tramos = tramos.OrderBy(t => t.Desde).ToList();

                foreach (var tramo in tramos)
                {
                    if (baseImponible >= tramo.Desde && baseImponible <= tramo.Hasta)
                    {
                        return tramo.Tasa;
                    }
                }

                return tramos.Last().Tasa;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Valida que un JSON de escala ISR tenga el formato correcto
        /// </summary>
        public static bool ValidarEscalaIsr(string isrEscalaJson, out string error)
        {
            error = string.Empty;

            try
            {
                var tramos = JsonSerializer.Deserialize<List<TramoIsr>>(isrEscalaJson);

                if (tramos == null || !tramos.Any())
                {
                    error = "La escala ISR está vacía";
                    return false;
                }

                // Validar que los tramos estén ordenados
                var tramosOrdenados = tramos.OrderBy(t => t.Desde).ToList();
                for (int i = 0; i < tramosOrdenados.Count; i++)
                {
                    var tramo = tramosOrdenados[i];

                    if (tramo.Desde < 0 || tramo.Hasta < 0)
                    {
                        error = $"Tramo {i + 1}: Los montos no pueden ser negativos";
                        return false;
                    }

                    if (tramo.Desde > tramo.Hasta)
                    {
                        error = $"Tramo {i + 1}: 'Desde' no puede ser mayor que 'Hasta'";
                        return false;
                    }

                    if (tramo.Tasa < 0 || tramo.Tasa > 1)
                    {
                        error = $"Tramo {i + 1}: La tasa debe estar entre 0 y 1";
                        return false;
                    }

                    // Validar continuidad (opcional, pero recomendado)
                    if (i > 0 && tramo.Desde != tramosOrdenados[i - 1].Hasta + 1)
                    {
                        // Advertencia, no error crítico
                        Console.WriteLine($"Advertencia: Posible discontinuidad en tramo {i + 1}");
                    }
                }

                return true;
            }
            catch (JsonException ex)
            {
                error = $"Error de formato JSON: {ex.Message}";
                return false;
            }
        }
    }
}
