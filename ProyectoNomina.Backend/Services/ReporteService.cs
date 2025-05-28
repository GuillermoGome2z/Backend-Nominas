using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Backend.DTOs;

namespace ProyectoNomina.Backend.Services
{
    public class ReporteService
    {
        // ✅ 1. Reporte de Nómina Procesada
        public byte[] GenerarReporteNominaPdf(Nomina nomina)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // Encabezado del reporte
                    page.Header().Text("Reporte de Nómina").FontSize(20).Bold().AlignCenter();

                    // Contenido del reporte
                    page.Content().Element(e =>
                    {
                        e.Text($"Fecha de generación: {nomina.FechaGeneracion:dd/MM/yyyy}").FontSize(12).Bold();
                        e.Text($"Descripción: {nomina.Descripcion}").FontSize(12).Bold();

                        // Tabla con los detalles de la nómina
                        e.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Nombre del empleado
                                columns.RelativeColumn();  // Salario bruto
                                columns.RelativeColumn();  // Deducciones
                                columns.RelativeColumn();  // Bonificaciones
                                columns.RelativeColumn();  // Salario neto
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

                    // Pie de página
                    page.Footer().AlignCenter().Text($"Generado por Proyecto Nómina - {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                });
            });

            return document.GeneratePdf();
        }

        // ✅ 2. Reporte de Estado de Expedientes
        public byte[] GenerarReporteExpediente(List<ReporteExpedienteDto> expedientes)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Reporte de Estado de Expedientes").FontSize(20).Bold().AlignCenter();

                    page.Content().Element(e =>
                    {
                        e.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Empleado
                                columns.RelativeColumn();  // Estado
                                columns.RelativeColumn();  // Requeridos
                                columns.RelativeColumn();  // Presentados
                                columns.RelativeColumn();  // Faltantes
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

        // ✅ 3. Reporte de Información Académica (corregido para usar propiedades válidas)
        public byte[] GenerarReporteInformacionAcademica(List<InformacionAcademica> datos)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Reporte de Información Académica").FontSize(20).Bold().AlignCenter();

                    page.Content().Element(e =>
                    {
                        e.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Empleado
                                columns.RelativeColumn(2); // Título
                                columns.RelativeColumn(2); // Institución
                                columns.RelativeColumn();  // Fecha de graduación
                                columns.RelativeColumn(2); // Tipo de certificación
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

        // 🔁 Estilo común de celdas para tablas
        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Padding(5)
                .Border(1)
                .BorderColor(Colors.Grey.Medium)
                .AlignCenter()
                .AlignMiddle();
        }
    }
}
