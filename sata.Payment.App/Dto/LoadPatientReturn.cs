namespace sata.Payment.App.Dto
{
   public class LoadPatientReturn
    {
        public long Id { get; set; }
        public int Typ { get; set; }
        public string TypDes { get; set; }
        public string AdmD { get; set; }
        public int Amount { get; set; }
        public int PaymentAmount { get; set; }
        public int CshAmount { get; set; }
        public int ReturnAmount { get; set; }
        public string TerminalId { get; set; }
        public string PartAccount { get; set; }
        public int PatientId { get; set; }
        public string PatientFirstName { get; set; }
        public string PatientLastName { get; set; }
        public string PatientNumber { get; set; }

    }
}