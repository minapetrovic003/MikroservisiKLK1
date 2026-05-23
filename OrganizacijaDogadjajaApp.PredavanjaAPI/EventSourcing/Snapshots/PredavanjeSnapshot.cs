namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Snapshots
{
    public class PredavanjeSnapshot
    {
        public Guid Id { get; set; }

        public string Naziv { get; set; } = string.Empty;

        public string Predavac { get; set; } = string.Empty;

        public string Sala { get; set; } = string.Empty;

        public DateTime VremeOdrzavanja { get; set; }

        public bool Otkazano { get; set; }

        public int Version { get; set; }
    }
}