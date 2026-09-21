using CuraLink.Application.Common.Interfaces.Prescriptions;
using CuraLink.Application.Common.Interfaces.Presistence;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CuraLink.Infrastructure.Services.Prescriptions;

public class PrescriptionPdfService : IPrescriptionPdfService
{
    // CuraLink brand palette (mirrors auth.css / doctor dashboard tokens)
    private const string Primary = "#2f6fed";
    private const string PrimaryDark = "#1a3fae";
    private const string PrimaryLight = "#eaf1ff";
    private const string Bg = "#f5f7fb";
    private const string Text = "#14213d";
    private const string TextMuted = "#5b6b85";
    private const string Border = "#e2e7f2";
    private const string Success = "#1e8e5a";

    private readonly IUnitOfWork _unitOfWork;

    public PrescriptionPdfService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<byte[]> GenerateAsync(
        Guid prescriptionId,
        CancellationToken cancellationToken = default)
    {
        var prescription =
            await _unitOfWork.Prescriptions
                .GetByIdWithDetailsAsync(
                    prescriptionId,
                    cancellationToken);

        if (prescription == null)
        {
            throw new KeyNotFoundException(
                "Prescription not found.");
        }

        var doctor = await _unitOfWork.Users
            .GetByIdAsync(
                prescription.Doctor.ApplicationUserId,
                cancellationToken);

        var patient = await _unitOfWork.Users
            .GetByIdAsync(
                prescription.Patient.ApplicationUserId,
                cancellationToken);

        if (doctor == null)
        {
            throw new KeyNotFoundException(
                "Doctor user not found.");
        }

        if (patient == null)
        {
            throw new KeyNotFoundException(
                "Patient user not found.");
        }

        var doctorName =
            $"{doctor.FirstName} {doctor.LastName}";

        var patientName =
            $"{patient.FirstName} {patient.LastName}";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);

                page.DefaultTextStyle(x => x
                    .FontSize(11)
                    .FontColor(Text));

                // ---------------- Header band ----------------
                page.Header().Background(Primary).Padding(30).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("CuraLink")
                            .FontSize(26)
                            .Bold()
                            .FontColor(Colors.White);

                        col.Item().PaddingTop(2).Text("Medical Prescription")
                            .FontSize(13)
                            .FontColor(PrimaryLight);
                    });

                    row.ConstantItem(140).AlignRight().AlignBottom()
                        .Text($"{prescription.CreatedAt:dd/MM/yyyy}")
                        .FontSize(10)
                        .FontColor(PrimaryLight);
                });

                // ---------------- Content ----------------
                page.Content().PaddingHorizontal(30).PaddingVertical(20)
                    .Column(column =>
                    {
                        column.Spacing(16);

                        // Info card: doctor + patient + dates
                        column.Item()
                            .Background(Bg)
                            .Border(1)
                            .BorderColor(Border)
                            .Padding(15)
                            .Column(infoColumn =>
                            {
                                infoColumn.Spacing(8);

                                infoColumn.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(t =>
                                    {
                                        t.Span("Doctor: ").FontColor(TextMuted).SemiBold();
                                        t.Span(doctorName).FontColor(Text).Bold();
                                    });

                                    r.RelativeItem().Text(t =>
                                    {
                                        t.Span("Specialty: ").FontColor(TextMuted).SemiBold();
                                        t.Span($"{prescription.Doctor.Specialty}").FontColor(Text);
                                    });
                                });

                                infoColumn.Item().LineHorizontal(1).LineColor(Border);

                                infoColumn.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(t =>
                                    {
                                        t.Span("Patient: ").FontColor(TextMuted).SemiBold();
                                        t.Span(patientName).FontColor(Text).Bold();
                                    });

                                    r.RelativeItem().Text(t =>
                                    {
                                        t.Span("Prescription Date: ").FontColor(TextMuted).SemiBold();
                                        t.Span($"{prescription.CreatedAt:dd/MM/yyyy}").FontColor(Text);
                                    });
                                });

                                infoColumn.Item().Text(t =>
                                {
                                    t.Span("Treatment Period: ").FontColor(TextMuted).SemiBold();
                                    t.Span(
                                        $"{prescription.StartDate:dd/MM/yyyy} - " +
                                        $"{prescription.EndDate:dd/MM/yyyy}")
                                        .FontColor(Text);
                                });
                            });

                        // Section title with accent bar
                        column.Item().Row(r =>
                        {
                            r.ConstantItem(4).Height(18).Background(Primary);

                            r.RelativeItem().PaddingLeft(8).AlignMiddle()
                                .Text("Medications")
                                .FontSize(15)
                                .Bold()
                                .FontColor(Text);
                        });

                        // Medication cards
                        foreach (var item in prescription.Items)
                        {
                            column.Item()
                                .Border(1)
                                .BorderColor(Border)
                                .Column(itemColumn =>
                                {
                                    itemColumn.Item()
                                        .Background(PrimaryLight)
                                        .Padding(10)
                                        .Text(item.MedicationName)
                                        .FontSize(12.5f)
                                        .Bold()
                                        .FontColor(PrimaryDark);

                                    itemColumn.Item().Padding(10).Column(details =>
                                    {
                                        details.Spacing(5);

                                        details.Item().Text(t =>
                                        {
                                            t.Span("Dosage: ").FontColor(TextMuted).SemiBold();
                                            t.Span(item.Dosage).FontColor(Text);
                                        });

                                        if (item.Schedules.Any())
                                        {
                                            var times = string.Join(
                                                ", ",
                                                item.Schedules
                                                    .OrderBy(x => x.DosageTime)
                                                    .Select(x =>
                                                        x.DosageTime.ToString(@"hh\:mm")));

                                            details.Item().Text(t =>
                                            {
                                                t.Span("Dosage Times: ").FontColor(TextMuted).SemiBold();
                                                t.Span(times).FontColor(Text);
                                            });
                                        }

                                        if (!string.IsNullOrWhiteSpace(item.Instructions))
                                        {
                                            details.Item().Text(t =>
                                            {
                                                t.Span("Instructions: ").FontColor(TextMuted).SemiBold();
                                                t.Span(item.Instructions).FontColor(Success);
                                            });
                                        }
                                    });
                                });
                        }
                    });

                // ---------------- Footer ----------------
                page.Footer().PaddingHorizontal(30).PaddingBottom(15).Column(col =>
                {
                    col.Item().PaddingBottom(6).LineHorizontal(1).LineColor(Border);

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("CuraLink · Digital Prescription")
                            .FontSize(9)
                            .FontColor(TextMuted);

                        r.RelativeItem().AlignRight()
                            .Text($"Generated {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(9)
                            .FontColor(TextMuted);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}