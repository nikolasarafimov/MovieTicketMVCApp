using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MovieTicketMVC.Models;

namespace MovieTicketMVC.Services
{
    public class TicketPdfGenerator
    {
        public byte[] GeneratePdf(Ticket ticket)
        {
            using (var ms = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
                var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                var title = new Paragraph(ticket.Movie.Title, titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                doc.Add(title);

                var table = new PdfPTable(2)
                {
                    WidthPercentage = 60f,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    SpacingBefore = 10f
                };
                table.SetWidths(new float[] { 1.2f, 2.8f });
                table.DefaultCell.BorderWidth = 1;
                table.DefaultCell.Padding = 8;

                AddRow(table, "Date:", ticket.SelectedDay.ToString("dd.MM.yyyy"), labelFont, valueFont, BaseColor.LIGHT_GRAY);
                AddRow(table, "Time:", ticket.SelectedTime, labelFont, valueFont, BaseColor.WHITE);
                AddRow(table, "Seats:", ticket.SelectedSeats.Replace(",", ", "), labelFont, valueFont, BaseColor.LIGHT_GRAY);
                AddRow(table, "Price:", $"{ticket.TotalPrice} ден", labelFont, valueFont, BaseColor.WHITE);

                doc.Add(table);

                doc.Close();
                return ms.ToArray();
            }
        }

        private void AddRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont, BaseColor background)
        {
            var lbl = new PdfPCell(new Phrase(label, labelFont))
            {
                BackgroundColor = background,
                BorderColor = BaseColor.DARK_GRAY,
                Padding = 6,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            table.AddCell(lbl);

            var val = new PdfPCell(new Phrase(value, valueFont))
            {
                BackgroundColor = background,
                BorderColor = BaseColor.DARK_GRAY,
                Padding = 6,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            table.AddCell(val);
        }
    }
}