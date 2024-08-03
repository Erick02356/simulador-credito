using credito.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using iText.Layout.Properties;
using System.IO;
using OfficeOpenXml;
using Microsoft.Extensions.Logging;
using iText.IO.Image;

namespace credito.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(LoanSimulation model)
        {
            // Calculate amortization schedule
            model.AmortizationSchedule = CalculateAmortizationSchedule(model.LoanAmount, model.InterestRate, model.NumberOfPayments);

            return View("Index", model);
        }
        [HttpGet]
        public IActionResult ExportToExcel(LoanSimulation model)
        {
            if (model == null || model.AmortizationSchedule == null || model.AmortizationSchedule.Count == 0)
            {
                return NotFound(); // Devolver un error 404 si no hay datos disponibles
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Tabla de Amortización");

                worksheet.Cells[1, 1].Value = "Mes";
                worksheet.Cells[1, 2].Value = "Saldo Restante";
                worksheet.Cells[1, 3].Value = "Interés";
                worksheet.Cells[1, 4].Value = "Intereses Pagados";
                worksheet.Cells[1, 5].Value = "Pago Mensual";
                worksheet.Cells[1, 6].Value = "Aporte";
                worksheet.Cells[1, 7].Value = "Pagado";


                int row = 2;
                foreach (var item in model.AmortizationSchedule)
                {
                    worksheet.Cells[row, 1].Value = item.PaymentNumber;
                    worksheet.Cells[row, 2].Value = item.RemainingBalance;
                    worksheet.Cells[row, 3].Value = item.Interest;
                    worksheet.Cells[row, 4].Value = item.AcumuladorInteres;
                    worksheet.Cells[row, 5].Value = item.PaymentAmount;
                    worksheet.Cells[row, 6].Value = item.Aporte;
                    worksheet.Cells[row, 7].Value = item.Pagado;

                    row++;
                }  
                byte[] excelBytes = package.GetAsByteArray();

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "tabla_amortizacion.xlsx");
            }
        }
        /// <summary>
        /// Calcula la tabla de amortización para un préstamo dado.
        /// </summary>
        /// <param name="loanAmount">El monto del préstamo.</param>
        /// <param name="interestRate">La tasa de interés anual del préstamo en porcentaje.</param>
        /// <param name="numberOfPayments">El número total de cuotas para el préstamo.</param>

        private List<AmortizationItem> CalculateAmortizationSchedule(decimal loanAmount, decimal interestRate, int numberOfPayments)
        {

            // Inicializar variables y realizar calculos
            List<AmortizationItem> amortizationSchedule = new List<AmortizationItem>();
            decimal AcumuladorInteres = 0;
            decimal pagado = 0;
            decimal remainingBalance = loanAmount;
            decimal monthlyInterestRate = interestRate / 100 ;
            decimal monthlyPayment = loanAmount * monthlyInterestRate / (1 - (decimal)Math.Pow((double)(1 + monthlyInterestRate), -numberOfPayments));
            // Calcular cada cuota de la amortización

            for (int i = 1; i <= numberOfPayments; i++)
            {
                // Calcular el interés para esta cuota
                decimal interest = remainingBalance * monthlyInterestRate;

                // Actualizar el acumulador de intereses
                AcumuladorInteres += interest;

                // Calcular el aporte al capital
                decimal aporte = monthlyPayment - interest;
                // Actualizar el saldo restante
                remainingBalance -= aporte;

                // Actualizar el monto total pagado
                pagado += monthlyPayment;
                
                // Agregar la cuota a la tabla de amortización
                amortizationSchedule.Add(new AmortizationItem
                {
                    PaymentNumber = i,
                    PaymentAmount = monthlyPayment,
                    Interest = interest,
                    Aporte = aporte,
                    RemainingBalance = remainingBalance,
                    AcumuladorInteres = AcumuladorInteres,
                    Pagado=pagado
                }); 
            }

            return amortizationSchedule;
        }

        //GENERAR PDF
        [HttpPost]
        public IActionResult GeneratePDFWithChart([FromBody] GeneratePDFRequest requestData)
        {
            byte[] pdfBytes = GeneratePDFBytes(requestData.AmortizationSchedule, requestData.ChartDataURL);
            return File(pdfBytes, "application/pdf", "Tabla_Amortizacion_Con_Grafico.pdf");
        }

        public class GeneratePDFRequest
        {
            public List<AmortizationItem> AmortizationSchedule { get; set; }
            public string ChartDataURL { get; set; }
        }

        // Método para generar los bytes del PDF
        private byte[] GeneratePDFBytes(List<AmortizationItem> data, string chartDataURL)
        {
            // Crear un MemoryStream para almacenar el PDF
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Crear el documento PDF
                using (PdfWriter writer = new PdfWriter(memoryStream))
                {
                    
                    using (PdfDocument pdf = new PdfDocument(writer))
                    {
                        // Crear un documento iTextSharp
                        Document document = new Document(pdf);


                        // Título del PDF
                        Paragraph title = new Paragraph("Tabla de Amortización - BanCastri")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(20);
                        document.Add(title);

                        // Espacio en blanco
                        document.Add(new Paragraph("\n"));

                        // Crear la tabla en el PDF
                        Table table = new Table(7);
                        table.SetHorizontalAlignment(HorizontalAlignment.CENTER); // Alineación de la tabla
                        table.SetWidth(UnitValue.CreatePercentValue(50)); // Ancho de la tabla

                        // Encabezados de la tabla
                        Cell cell;
                        cell = new Cell().Add(new Paragraph("Mes"));
                        table.AddHeaderCell(cell);
                        cell = new Cell().Add(new Paragraph("Saldo Restante"));
                        table.AddHeaderCell(cell);
                        cell = new Cell().Add(new Paragraph("Interés"));
                        table.AddHeaderCell(cell);
                        cell = new Cell().Add(new Paragraph("Acumulador de Interés"));
                        table.AddHeaderCell(cell);
                        cell = new Cell().Add(new Paragraph("Pago Mensual"));
                        table.AddHeaderCell(cell);
                        cell = new Cell().Add(new Paragraph("Aporte"));
                        table.AddHeaderCell(cell);
                        cell = new Cell().Add(new Paragraph("Pagado"));
                        table.AddHeaderCell(cell);

                        // Agregar los datos a la tabla
                        foreach (var item in data)
                        {
                            table.AddCell(item.PaymentNumber.ToString());
                            table.AddCell(Math.Round(item.RemainingBalance, 2).ToString());
                            table.AddCell(Math.Round(item.Interest, 2).ToString());
                            table.AddCell(Math.Round(item.AcumuladorInteres, 2).ToString());
                            table.AddCell(Math.Round(item.PaymentAmount, 2).ToString());
                            table.AddCell(Math.Round(item.Aporte, 2).ToString());
                            table.AddCell(Math.Round(item.Pagado, 2).ToString());
                        }
                     

                        // Espacio en blanco
                        document.Add(new Paragraph("\n"));
                        document.Add(table);
                        byte[] chartBytes = Convert.FromBase64String(chartDataURL.Split(',')[1]);
                        Image chartImage = new Image(ImageDataFactory.Create(chartBytes));
                        document.Add(chartImage);

                        document.Close();
                    }
                }
                // Devolver el arreglo de bytes del MemoryStream

                return memoryStream.ToArray();
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}