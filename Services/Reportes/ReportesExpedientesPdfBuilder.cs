using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProyectoNomina.Backend.Services.Reportes
{
    public class ExpedienteRow
    {
        public string Empleado { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int Requeridos { get; set; }
        public int Presentados { get; set; }
        public int Faltantes { get; set; }
    }

    public static class ReportesExpedientesPdfBuilder
    {
        public static byte[] Generar(List<ExpedienteRow> data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configurar página en A4 horizontal
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);

                    // Encabezado
                    page.Header().Column(header =>
                    {
                        header.Item().PaddingBottom(10).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("REPORTE DE ESTADO DE EXPEDIENTES")
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken3);
                                
                                col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken2);
                            });

                            row.ConstantItem(100).AlignRight().Column(col =>
                            {
                                col.Item().Text("Proyecto Nómina")
                                    .FontSize(12)
                                    .SemiBold();
                            });
                        });

                        // Línea separadora
                        header.Item().LineHorizontal(2).LineColor(Colors.Blue.Darken3);
                    });

                    // Contenido principal
                    page.Content().PaddingTop(10).Column(content =>
                    {
                        // Resumen estadístico
                        var totalEmpleados = data.Count;
                        var completos = data.Count(x => x.Estado == "Completo");
                        var incompletos = data.Count(x => x.Estado == "Incompleto");
                        var enProceso = data.Count(x => x.Estado == "En proceso");

                        content.Item().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text("RESUMEN ESTADÍSTICO")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                col.Item().Row(statsRow =>
                                {
                                    statsRow.RelativeItem().Text($"Total empleados: {totalEmpleados}");
                                    statsRow.RelativeItem().Text($"Completos: {completos} ({(totalEmpleados > 0 ? (completos * 100 / totalEmpleados) : 0)}%)")
                                        .FontColor(Colors.Green.Darken2);
                                    statsRow.RelativeItem().Text($"En proceso: {enProceso} ({(totalEmpleados > 0 ? (enProceso * 100 / totalEmpleados) : 0)}%)")
                                        .FontColor(Colors.Orange.Darken2);
                                    statsRow.RelativeItem().Text($"Incompletos: {incompletos} ({(totalEmpleados > 0 ? (incompletos * 100 / totalEmpleados) : 0)}%)")
                                        .FontColor(Colors.Red.Darken2);
                                });
                            });
                        });

                        // Tabla principal
                        content.Item().Table(table =>
                        {
                            // Definir columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);  // Empleado
                                columns.RelativeColumn(2);  // Estado
                                columns.RelativeColumn(1.5f);  // Requeridos
                                columns.RelativeColumn(1.5f);  // Presentados
                                columns.RelativeColumn(1.5f);  // Faltantes
                                columns.RelativeColumn(2);  // % Completado
                            });

                            // Encabezado de la tabla
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("EMPLEADO").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("ESTADO").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("REQUERIDOS").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("PRESENTADOS").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("FALTANTES").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("% COMPLETADO").Bold();
                            });

                            // Filas de datos con zebra alternada
                            for (int i = 0; i < data.Count; i++)
                            {
                                var item = data[i];
                                var isEven = i % 2 == 0;
                                var porcentaje = item.Requeridos > 0 ? (item.Presentados * 100 / item.Requeridos) : 100;

                                table.Cell().Element(container => DataCellStyle(container, isEven))
                                    .Text(item.Empleado)
                                    .Medium();

                                table.Cell().Element(container => DataCellStyle(container, isEven))
                                    .Text(item.Estado)
                                    .FontColor(GetEstadoColor(item.Estado))
                                    .SemiBold();

                                table.Cell().Element(container => DataCellStyle(container, isEven))
                                    .AlignCenter()
                                    .Text(item.Requeridos.ToString());

                                table.Cell().Element(container => DataCellStyle(container, isEven))
                                    .AlignCenter()
                                    .Text(item.Presentados.ToString())
                                    .FontColor(item.Presentados > 0 ? Colors.Green.Darken1 : Colors.Grey.Darken1);

                                table.Cell().Element(container => DataCellStyle(container, isEven))
                                    .AlignCenter()
                                    .Text(item.Faltantes.ToString())
                                    .FontColor(item.Faltantes > 0 ? Colors.Red.Darken1 : Colors.Green.Darken1);

                                table.Cell().Element(container => DataCellStyle(container, isEven))
                                    .AlignCenter()
                                    .Text($"{porcentaje}%")
                                    .FontColor(GetPorcentajeColor(porcentaje))
                                    .SemiBold();
                            }
                        });

                        // Totales al final
                        if (data.Any())
                        {
                            var totalRequeridos = data.Sum(x => x.Requeridos);
                            var totalPresentados = data.Sum(x => x.Presentados);
                            var totalFaltantes = data.Sum(x => x.Faltantes);
                            var promedioCompletado = totalRequeridos > 0 ? (totalPresentados * 100 / totalRequeridos) : 100;

                            content.Item().PaddingTop(15).Background(Colors.Blue.Lighten4).Padding(10).Row(row =>
                            {
                                row.RelativeItem().Text("TOTALES GENERALES")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken3);

                                row.RelativeItem().AlignRight().Row(totalsRow =>
                                {
                                    totalsRow.RelativeItem().Text($"Total requeridos: {totalRequeridos}")
                                        .SemiBold();
                                    totalsRow.RelativeItem().Text($"Total presentados: {totalPresentados}")
                                        .SemiBold()
                                        .FontColor(Colors.Green.Darken2);
                                    totalsRow.RelativeItem().Text($"Total faltantes: {totalFaltantes}")
                                        .SemiBold()
                                        .FontColor(Colors.Red.Darken2);
                                    totalsRow.RelativeItem().Text($"Promedio: {promedioCompletado}%")
                                        .Bold()
                                        .FontColor(GetPorcentajeColor(promedioCompletado));
                                });
                            });
                        }
                    });

                    // Pie de página
                    page.Footer().AlignCenter().Text($"Proyecto Nómina - Sistema de Gestión de RRHH")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken1);
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer HeaderCellStyle(IContainer container)
        {
            return container
                .Background(Colors.Blue.Darken3)
                .Padding(8)
                .AlignCenter()
                .AlignMiddle();
        }

        private static IContainer DataCellStyle(IContainer container, bool isEven)
        {
            return container
                .Background(isEven ? Colors.White : Colors.Grey.Lighten5)
                .Padding(8)
                .AlignMiddle()
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2);
        }

        private static string GetEstadoColor(string estado)
        {
            return estado switch
            {
                "Completo" => Colors.Green.Darken2,
                "En proceso" => Colors.Orange.Darken2,
                "Incompleto" => Colors.Red.Darken2,
                _ => Colors.Grey.Darken2
            };
        }

        private static string GetPorcentajeColor(int porcentaje)
        {
            return porcentaje switch
            {
                >= 100 => Colors.Green.Darken2,
                >= 75 => Colors.Green.Medium,
                >= 50 => Colors.Orange.Darken1,
                >= 25 => Colors.Orange.Darken2,
                _ => Colors.Red.Darken2
            };
        }
    }
}