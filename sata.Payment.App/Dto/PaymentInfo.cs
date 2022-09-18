namespace sata.Payment.App.Dto
{
   public class PaymentInfo
    {
        public int Row { get; set; }
        public long Id { get; set; }
        public string TypDes { get; set; }
        public string AdmD { get; set; }
        public int PaymentAmount { get; set; }
        public int ReturnAmount { get; set; }
        public int PatientId { get; set; }
        public string TerminalId { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
    }
}