using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ProyectoNomina.Backend.Models;
using ProyectoNomina.Shared.Models.DTOs;

namespace ProyectoNomina.Backend.Services
{
    public class ReporteService
    {
        // ✅ Reporte 1: Nómina Procesada
        public byte[] GenerarReporteNominaPdf(Nomina nomina)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Reporte de Nómina").FontSize(20).Bold().AlignCenter();

                    page.Content().Element(e =>
                    {
                        e.Text($"Fecha de generación: {nomina.FechaGeneracion:dd/MM/yyyy}").FontSize(12);
                        e.Text($"Descripción: {nomina.Descripcion}").FontSize(12);

                        e.Table(table =>
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

        // ✅ Reporte 2: Estado de Expedientes
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

        // ✅ Reporte 3: Información Académica
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

        // ✅ Reporte 4: Ajustes Manuales
        public byte[] GenerarReporteAjustesManuales(List<AjusteManual> ajustes)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Reporte de Ajustes Manuales").FontSize(20).Bold().AlignCenter();

                    page.Content().Element(e =>
                    {
                        e.Table(table =>
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

        // ✅ Reporte 5: Auditoría del sistema
        public byte[] GenerarReporteAuditoria(List<Auditoria> auditoria)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Reporte de Auditoría").FontSize(20).Bold().AlignCenter();

                    page.Content().Element(e =>
                    {
                        e.Table(table =>
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

        // 🔁 Estilo común de celdas
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

