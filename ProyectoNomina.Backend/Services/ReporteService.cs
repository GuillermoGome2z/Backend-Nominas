using System;
using System.IO;
using System.Linq;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;
using ClosedXML.Excel;


namespace ProyectoNomina.Backend.Services
{
    public class ReporteService
    {
        /// <summary>
        /// Configuración de fuente para caracteres especiales en español
        /// </summary>
        private static TextStyle ConfigurarFuenteEspanol(TextStyle style)
        {
            // Usar fuentes que soporten caracteres UTF-8 correctamente
            return style.FontFamily(Fonts.Arial);
        }

        /// <summary>
        /// Genera un reporte consolidado de múltiples nóminas con formato profesional
        /// </summary>
        public byte[] GenerarReporteConsolidadoNominas(List<Nomina> nominas)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    // Encabezado profesional
                    page.Header().Column(header =>
                    {
                        header.Item().AlignCenter().Text("REPORTE CONSOLIDADO DE NÓMINAS")
                            .FontSize(18).Bold().FontFamily(Fonts.Arial);
                        
                        header.Item().AlignCenter().Text("REPÚBLICA DE GUATEMALA")
                            .FontSize(14).SemiBold().FontFamily(Fonts.Arial);
                        
                        header.Item().AlignCenter().Text("Sistema de Gestión de Nóminas - 2025")
                            .FontSize(12).FontFamily(Fonts.Arial);
                        
                        header.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Text($"Fecha de Generación: {DateTime.Now:dd/MM/yyyy}")
                                .FontSize(10).FontFamily(Fonts.Arial);
                            row.RelativeItem().AlignRight().Text($"Hora: {DateTime.Now:HH:mm:ss} p. m.")
                                .FontSize(10).FontFamily(Fonts.Arial);
                        });
                    });

                    page.Content().PaddingTop(20).Column(content =>
                    {
                        // Totales consolidados
                        var totalNominas = nominas.Count;
                        var totalEmpleados = nominas.SelectMany(n => n.Detalles).Select(d => d.EmpleadoId).Distinct().Count();
                        var totalDevengado = nominas.SelectMany(n => n.Detalles).Sum(d => d.TotalDevengado);
                        var totalDeducciones = nominas.SelectMany(n => n.Detalles).Sum(d => d.TotalDeducciones);
                        var totalNeto = nominas.SelectMany(n => n.Detalles).Sum(d => d.SalarioNeto);

                        content.Item().PaddingBottom(15).Text("TOTALES CONSOLIDADOS")
                            .FontSize(14).Bold().FontFamily(Fonts.Arial);

                        content.Item().PaddingBottom(10).Column(totales =>
                        {
                            totales.Item().Text($"Total de Nóminas Procesadas: {totalNominas}")
                                .FontSize(12).FontFamily(Fonts.Arial);
                            totales.Item().Text($"Total de Empleados: {totalEmpleados}")
                                .FontSize(12).FontFamily(Fonts.Arial);
                            totales.Item().Text($"Total Devengado: Q{totalDevengado:N2}")
                                .FontSize(12).FontFamily(Fonts.Arial);
                            totales.Item().Text($"Total Deducciones: Q{totalDeducciones:N2}")
                                .FontSize(12).FontFamily(Fonts.Arial);
                            totales.Item().Text($"Total Neto a Pagar: Q{totalNeto:N2}")
                                .FontSize(12).Bold().FontFamily(Fonts.Arial);
                        });

                        // Análisis por tipo de nómina
                        content.Item().PaddingTop(20).PaddingBottom(15).Text("ANÁLISIS POR TIPO DE NÓMINA")
                            .FontSize(14).Bold().FontFamily(Fonts.Arial);

                        var analisisNominas = nominas
                            .GroupBy(n => n.TipoNomina ?? "Ordinaria")
                            .Select(g => new
                            {
                                TipoNomina = g.Key,
                                Cantidad = g.Count(),
                                Empleados = g.SelectMany(n => n.Detalles).Select(d => d.EmpleadoId).Distinct().Count(),
                                TotalDevengado = g.SelectMany(n => n.Detalles).Sum(d => d.TotalDevengado),
                                TotalDeducciones = g.SelectMany(n => n.Detalles).Sum(d => d.TotalDeducciones),
                                TotalNeto = g.SelectMany(n => n.Detalles).Sum(d => d.SalarioNeto),
                                PromedioEmpleado = g.SelectMany(n => n.Detalles).Average(d => d.SalarioNeto)
                            })
                            .ToList();

                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Tipo de Nómina
                                columns.RelativeColumn(1); // Cant.
                                columns.RelativeColumn(1); // Empleados
                                columns.RelativeColumn(2); // Total Devengado
                                columns.RelativeColumn(2); // Total Deducciones
                                columns.RelativeColumn(2); // Total Neto
                                columns.RelativeColumn(2); // Promedio/Emp
                            });

                            // Encabezado de tabla con fondo verde
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyleHeader).Text("Tipo de Nómina").FontFamily(Fonts.Arial);
                                header.Cell().Element(CellStyleHeader).Text("Cant.").FontFamily(Fonts.Arial);
                                header.Cell().Element(CellStyleHeader).Text("Empleados").FontFamily(Fonts.Arial);
                                header.Cell().Element(CellStyleHeader).Text("Total Devengado").FontFamily(Fonts.Arial);
                                header.Cell().Element(CellStyleHeader).Text("Total Deducciones").FontFamily(Fonts.Arial);
                                header.Cell().Element(CellStyleHeader).Text("Total Neto").FontFamily(Fonts.Arial);
                                header.Cell().Element(CellStyleHeader).Text("Promedio/Emp.").FontFamily(Fonts.Arial);
                            });

                            foreach (var analisis in analisisNominas)
                            {
                                table.Cell().Element(CellStyleData).Text(analisis.TipoNomina).FontFamily(Fonts.Arial);
                                table.Cell().Element(CellStyleData).Text(analisis.Cantidad.ToString()).FontFamily(Fonts.Arial);
                                table.Cell().Element(CellStyleData).Text(analisis.Empleados.ToString()).FontFamily(Fonts.Arial);
                                table.Cell().Element(CellStyleData).Text($"Q{analisis.TotalDevengado:N2}").FontFamily(Fonts.Arial);
                                table.Cell().Element(CellStyleData).Text($"Q{analisis.TotalDeducciones:N2}").FontFamily(Fonts.Arial);
                                table.Cell().Element(CellStyleData).Text($"Q{analisis.TotalNeto:N2}").FontFamily(Fonts.Arial);
                                table.Cell().Element(CellStyleData).Text($"Q{analisis.PromedioEmpleado:N2}").FontFamily(Fonts.Arial);
                            }
                        });
                    });

                    // Pie de página profesional
                    page.Footer().AlignCenter().Column(footer =>
                    {
                        footer.Item().Text("Reporte consolidado generado conforme a las leyes laborales de Guatemala 2025 | Decreto 1441, Código de Trabajo")
                            .FontSize(8).FontFamily(Fonts.Arial);
                        footer.Item().Text($"Página 1 de 1 | Generado: {DateTime.Now:dd/MM/yyyy}, {DateTime.Now:HH:mm:ss} p. m.     Sistema de Nóminas Guatemala")
                            .FontSize(8).FontFamily(Fonts.Arial);
                    });
                });
            });

            return document.GeneratePdf();
        }

        // Reporte 1 Nómina Procesada
        public byte[] GenerarReporteNominaPdf(Nomina nomina)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Reporte de Nómina").FontSize(20).Bold().FontFamily(Fonts.Arial).AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Fecha de generación: {nomina.FechaGeneracion:dd/MM/yyyy}").FontSize(12);
                        col.Item().Text($"Descripción: {nomina.Descripcion}").FontSize(12);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Empleado");
                                header.Cell().Element(CellStyle).Text("Bruto");
                                header.Cell().Element(CellStyle).Text("Deducciones");
                                header.Cell().Element(CellStyle).Text("Bonificaciones");
                                header.Cell().Element(CellStyle).Text("Neto");
                            });

                            foreach (var detalle in nomina.Detalles)
                            {
                                table.Cell().Element(CellStyle).Text(detalle.Empleado.NombreCompleto);
                                table.Cell().Element(CellStyle).Text($"Q{detalle.SalarioBruto:F2}");
                                table.Cell().Element(CellStyle).Text($"Q{detalle.Deducciones:F2}");
                                table.Cell().Element(CellStyle).Text($"Q{detalle.Bonificaciones:F2}");
                                table.Cell().Element(CellStyle).Text($"Q{detalle.SalarioNeto:F2}");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text($"Generado por Proyecto Nómina - {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                });
            });

            return document.GeneratePdf();
        }

        // Reporte 2: Estado de Expedientes
        public byte[] GenerarReporteExpediente(System.Collections.Generic.List<ReporteExpedienteDto> expedientes)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Reporte de Estado de Expedientes").FontSize(20).Bold().AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Empleado");
                                header.Cell().Element(CellStyle).Text("Estado");
                                header.Cell().Element(CellStyle).Text("Requeridos");
                                header.Cell().Element(CellStyle).Text("Presentados");
                                header.Cell().Element(CellStyle).Text("Faltantes");
                            });

                            foreach (var item in expedientes)
                            {
                                table.Cell().Element(CellStyle).Text(item.Empleado);
                                table.Cell().Element(CellStyle).Text(item.EstadoExpediente);
                                table.Cell().Element(CellStyle).Text(item.DocumentosRequeridos.ToString());
                                table.Cell().Element(CellStyle).Text(item.DocumentosPresentados.ToString());
                                table.Cell().Element(CellStyle).Text(item.DocumentosFaltantes.ToString());
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text($"Generado por Proyecto Nómina - {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                });
            });

            return document.GeneratePdf();
        }

        //  Reporte 3: Información Académica
        public byte[] GenerarReporteInformacionAcademica(System.Collections.Generic.List<InformacionAcademica> datos)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Reporte de Información Académica").FontSize(20).Bold().AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Empleado");
                                header.Cell().Element(CellStyle).Text("Título");
                                header.Cell().Element(CellStyle).Text("Institución");
                                header.Cell().Element(CellStyle).Text("Fecha");
                                header.Cell().Element(CellStyle).Text("Certificación");
                            });

                            foreach (var item in datos)
                            {
                                table.Cell().Element(CellStyle).Text(item.Empleado.NombreCompleto);
                                table.Cell().Element(CellStyle).Text(item.Titulo);
                                table.Cell().Element(CellStyle).Text(item.Institucion);
                                table.Cell().Element(CellStyle).Text(item.FechaGraduacion.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(item.TipoCertificacion);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text($"Generado por Proyecto Nómina - {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                });
            });

            return document.GeneratePdf();
        }

        // Reporte 4: Ajustes Manuales
        public byte[] GenerarReporteAjustesManuales(System.Collections.Generic.List<AjusteManual> ajustes)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Reporte de Ajustes Manuales").FontSize(20).Bold().AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Empleado");
                                header.Cell().Element(CellStyle).Text("Monto");
                                header.Cell().Element(CellStyle).Text("Motivo");
                                header.Cell().Element(CellStyle).Text("Fecha");
                            });

                            foreach (var ajuste in ajustes)
                            {
                                table.Cell().Element(CellStyle).Text(ajuste.Empleado.NombreCompleto);
                                table.Cell().Element(CellStyle).Text($"Q{ajuste.Monto:F2}");
                                table.Cell().Element(CellStyle).Text(ajuste.Motivo);
                                table.Cell().Element(CellStyle).Text(ajuste.Fecha.ToString("dd/MM/yyyy"));
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text($"Generado por Proyecto Nómina - {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                });
            });

            return document.GeneratePdf();
        }

        //  Reporte 5: Auditoría del sistema
        public byte[] GenerarReporteAuditoria(System.Collections.Generic.List<Auditoria> auditoria)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Reporte de Auditoría").FontSize(20).Bold().AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Usuario");
                                header.Cell().Element(CellStyle).Text("Acción");
                                header.Cell().Element(CellStyle).Text("Detalles");
                                header.Cell().Element(CellStyle).Text("Fecha");
                            });

                            foreach (var log in auditoria)
                            {
                                table.Cell().Element(CellStyle).Text(log.Usuario);
                                table.Cell().Element(CellStyle).Text(log.Accion);
                                table.Cell().Element(CellStyle).Text(log.Detalles);
                                table.Cell().Element(CellStyle).Text(log.Fecha.ToString("dd/MM/yyyy HH:mm"));
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text($"Generado por Proyecto Nómina - {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerarReporteNominaExcel(Nomina nomina)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Nómina");

            worksheet.Cell(1, 1).Value = "Empleado";
            worksheet.Cell(1, 2).Value = "Salario Bruto";
            worksheet.Cell(1, 3).Value = "Deducciones";
            worksheet.Cell(1, 4).Value = "Bonificaciones";
            worksheet.Cell(1, 5).Value = "Salario Neto";

            int row = 2;

            foreach (var detalle in nomina.Detalles)
            {
                worksheet.Cell(row, 1).Value = detalle.Empleado.NombreCompleto;
                worksheet.Cell(row, 2).Value = detalle.SalarioBruto;
                worksheet.Cell(row, 3).Value = detalle.Deducciones;
                worksheet.Cell(row, 4).Value = detalle.Bonificaciones;
                worksheet.Cell(row, 5).Value = detalle.SalarioNeto;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ===============================
        // Paso 17: PDF Nómina (NUEVO)
        // ===============================

        // PDF global de nómina (resumen por empleado)
        public byte[] GenerarNominaGeneral(Nomina nomina, System.Collections.Generic.IEnumerable<DetalleNomina> detalles)
        {
            var items = detalles
                .OrderBy(d => d.Empleado?.NombreCompleto)
                .Select(d => new
                {
                    Nombre = d.Empleado?.NombreCompleto ?? $"Empleado {d.EmpleadoId}",
                    d.EmpleadoId,
                    d.SalarioBruto,
                    d.Bonificaciones,
                    d.Deducciones,
                    d.SalarioNeto
                })
                .ToList();

            var totalBruto = items.Sum(x => x.SalarioBruto);
            var totalBonif = items.Sum(x => x.Bonificaciones);
            var totalDedu = items.Sum(x => x.Deducciones);
            var totalNeto  = items.Sum(x => x.SalarioNeto);

            var doc = Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(30);
                    p.Header().Text($"Nómina #{nomina.Id} — {nomina.Descripcion}")
                        .FontSize(18).Bold().AlignCenter();

                    p.Content().Column(col =>
                    {
                        col.Item().Text($"Generada: {nomina.FechaGeneracion:dd/MM/yyyy HH:mm}").FontSize(11);
                        col.Item().Height(5); // <-- espacio

                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(10);  // Empleado
                                cols.RelativeColumn(3);   // EmpleadoId
                                cols.RelativeColumn(3);   // Bruto
                                cols.RelativeColumn(3);   // Bonif
                                cols.RelativeColumn(3);   // Deducc
                                cols.RelativeColumn(3);   // Neto
                            });

                            t.Header(h =>
                            {
                                h.Cell().Element(CellStyle).Text("Empleado").SemiBold();
                                h.Cell().Element(CellStyle).Text("ID").SemiBold();
                                h.Cell().Element(CellStyle).Text("Bruto").SemiBold();
                                h.Cell().Element(CellStyle).Text("Bonif.").SemiBold();
                                h.Cell().Element(CellStyle).Text("Deducc.").SemiBold();
                                h.Cell().Element(CellStyle).Text("Neto").SemiBold();
                            });

                            foreach (var it in items)
                            {
                                t.Cell().Element(CellStyle).AlignLeft().Text(it.Nombre);
                                t.Cell().Element(CellStyle).Text(it.EmpleadoId.ToString());
                                t.Cell().Element(CellStyle).Text($"Q{it.SalarioBruto:N2}");
                                t.Cell().Element(CellStyle).Text($"Q{it.Bonificaciones:N2}");
                                t.Cell().Element(CellStyle).Text($"Q{it.Deducciones:N2}");
                                t.Cell().Element(CellStyle).Text($"Q{it.SalarioNeto:N2}");
                            }

                            // Totales
                            t.Cell().Element(CellStyle).AlignRight().Text("TOTALES").Bold();
                            t.Cell().Element(CellStyle).Text(""); // columna ID vacía
                            t.Cell().Element(CellStyle).Text($"Q{totalBruto:N2}").Bold();
                            t.Cell().Element(CellStyle).Text($"Q{totalBonif:N2}").Bold();
                            t.Cell().Element(CellStyle).Text($"Q{totalDedu:N2}").Bold();
                            t.Cell().Element(CellStyle).Text($"Q{totalNeto:N2}").Bold();
                        });
                    });

                    p.Footer().AlignCenter().Text($"Generado por Proyecto Nómina - {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                });
            });

            return doc.GeneratePdf();
        }

        // Recibo individual por empleado
        public byte[] GenerarReciboNominaEmpleado(Nomina nomina, DetalleNomina detalle)
        {
            var nombre = detalle.Empleado?.NombreCompleto ?? $"Empleado {detalle.EmpleadoId}";

            var doc = Document.Create(c =>
            {
                c.Page(p =>
                {
                    p.Margin(36);

                    p.Header().Column(h =>
                    {
                        h.Item().Text("Recibo de Pago").FontSize(18).Bold().AlignCenter();
                        h.Item().Text($"Nómina #{nomina.Id} — {nomina.Descripcion}").FontSize(11).AlignCenter();
                        h.Item().Text($"Fecha: {nomina.FechaGeneracion:dd/MM/yyyy HH:mm}").FontSize(10).AlignCenter();
                    });

                    p.Content().Column(col =>
                    {
                        col.Item().Height(8);  // <-- espacio

                        // Datos empleado
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(7);
                            });

                            t.Cell().Element(CellStyle).AlignLeft().Text("Empleado");
                            t.Cell().Element(CellStyle).AlignLeft().Text($"{nombre} (ID: {detalle.EmpleadoId})");
                        });

                        col.Item().Height(10); // <-- espacio

                        // Montos
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(6);
                                cols.RelativeColumn(4);
                            });

                            t.Cell().Element(CellStyle).AlignLeft().Text("Salario bruto:");
                            t.Cell().Element(CellStyle).AlignRight().Text($"Q{detalle.SalarioBruto:N2}");

                            t.Cell().Element(CellStyle).AlignLeft().Text("Bonificaciones:");
                            t.Cell().Element(CellStyle).AlignRight().Text($"Q{detalle.Bonificaciones:N2}");

                            t.Cell().Element(CellStyle).AlignLeft().Text("Deducciones:");
                            t.Cell().Element(CellStyle).AlignRight().Text($"Q{detalle.Deducciones:N2}");

                            t.Cell().Element(CellStyle).AlignLeft().Text(" ");
                            t.Cell().Element(CellStyle).AlignRight().Text(" ");

                            t.Cell().Element(CellStyle).AlignLeft().Text("Total neto:").Bold();
                            t.Cell().Element(CellStyle).AlignRight().Text($"Q{detalle.SalarioNeto:N2}").Bold();
                        });

                        col.Item().Height(20); // <-- espacio
                        col.Item().Text("Firma del empleado: ____________________________").FontSize(10);
                    });

                    p.Footer().AlignCenter().Text($"Generado por Proyecto Nómina - {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                });
            });

            return doc.GeneratePdf();
        }

        //  Estilos de celdas
        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Padding(5)
                .Border(1)
                .BorderColor(Colors.Grey.Medium)
                .AlignCenter()
                .AlignMiddle();
        }

        private static IContainer CellStyleHeader(IContainer container)
        {
            return container
                .Padding(8)
                .Border(1)
                .BorderColor(Colors.Grey.Medium)
                .Background(Colors.Green.Lighten2)
                .AlignCenter()
                .AlignMiddle();
        }

        private static IContainer CellStyleData(IContainer container)
        {
            return container
                .Padding(6)
                .Border(1)
                .BorderColor(Colors.Grey.Medium)
                .AlignCenter()
                .AlignMiddle();
        }
    }
}
