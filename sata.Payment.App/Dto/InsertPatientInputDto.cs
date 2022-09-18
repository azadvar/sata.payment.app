
namespace sata.Payment.App.Dto
{
    public class InsertPatientInputDto
    {
        public long PatientId { get; set; }
        public long PaymentId { get; set; }
        public int Amount { get; set; }
        public string ChqNo { get; set; }
        public string SwitchDateOut { get; set; }
        public string TraceNoOut { get; set; }
        public long RefNoOut { get; set; }
        public string TerminalId { get; set; }
    }
}