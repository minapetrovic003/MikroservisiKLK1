using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Events;
using OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Snapshots;

namespace OrganizacijaDogadjajaApp.PredavanjaAPI.EventSourcing.Aggregates
{
    public class PredavanjeAggregate : AggregateRoot
    {
        //State -> ne moze direektno biti promenjen mora da prodje kroz klasu
        //Ovde su reply, biznis pravila
        public string Naziv { get; private set; } = string.Empty;
        //Sprecavamo ->predavanje.Naziv = "Novo"

        public string Predavac { get; private set; } = string.Empty;

        public string Sala { get; private set; } = string.Empty;

        public DateTime VremeOdrzavanja { get; private set; }

        public bool Otkazano { get; private set; }

        public PredavanjeAggregate()
        {
        }

        public PredavanjeAggregate(
            Guid id,
            string naziv,
            string predavac,
            string sala,
            DateTime vreme)
        {
            if (string.IsNullOrWhiteSpace(naziv))
                throw new Exception("Naziv je obavezan.");

            RaiseEvent(new PredavanjeCreatedEvent
            {
                AggregateId = id,
                Naziv = naziv,
                Predavac = predavac,
                Sala = sala,
                VremeOdrzavanja = vreme
            });
        }
        public void RestoreFromSnapshot(
    PredavanjeSnapshot snapshot)
        {
            Id = snapshot.Id;

            Naziv = snapshot.Naziv;

            Predavac = snapshot.Predavac;

            Sala = snapshot.Sala;

            VremeOdrzavanja = snapshot.VremeOdrzavanja;

            Otkazano = snapshot.Otkazano;

            Version = snapshot.Version;
        }
        public void ChangeNaziv(string noviNaziv)
        {
            if (Otkazano)
                throw new Exception("Otkazano predavanje se ne može menjati.");

            if (string.IsNullOrWhiteSpace(noviNaziv))
                throw new Exception("Naziv je obavezan.");

            RaiseEvent(new NazivPromenjenEvent
            {
                AggregateId = Id,
                NoviNaziv = noviNaziv
            });
        }

        public void ChangePredavac(string noviPredavac)
        {
            if (Otkazano)
                throw new Exception("Otkazano predavanje se ne može menjati.");

            RaiseEvent(new PredavacPromenjenEvent
            {
                AggregateId = Id,
                NoviPredavac = noviPredavac
            });
        }

        public void ChangeSala(string novaSala)
        {
            if (Otkazano)
                throw new Exception("Otkazano predavanje se ne može menjati.");

            RaiseEvent(new SalaPromenjenaEvent
            {
                AggregateId = Id,
                NovaSala = novaSala
            });
        }

        public void ChangeVreme(DateTime novoVreme)
        {
            if (Otkazano)
                throw new Exception("Otkazano predavanje se ne može menjati.");

            RaiseEvent(new VremePromenjenoEvent
            {
                AggregateId = Id,
                NovoVreme = novoVreme
            });
        }

        public void Cancel(string razlog)
        {
            if (Otkazano)
                throw new Exception("Predavanje je već otkazano.");

            RaiseEvent(new PredavanjeOtkazanoEvent
            //Svaka promena ide kroz dogadjaj
            {
                AggregateId = Id,
                Razlog = razlog
            });
        }

        protected override void Apply(EventBase @event)
        {
            //ne brise biznis model -> samo ce dodati dogadjaj
            switch (@event)
            {
                case PredavanjeCreatedEvent e:
                    Id = e.AggregateId;
                    Naziv = e.Naziv;
                    Predavac = e.Predavac;
                    Sala = e.Sala;
                    VremeOdrzavanja = e.VremeOdrzavanja;
                    break;

                case NazivPromenjenEvent e:
                    Naziv = e.NoviNaziv;
                    break;

                case PredavacPromenjenEvent e:
                    Predavac = e.NoviPredavac;
                    break;

                case SalaPromenjenaEvent e:
                    Sala = e.NovaSala;
                    break;

                case VremePromenjenoEvent e:
                    VremeOdrzavanja = e.NovoVreme;
                    break;

                case PredavanjeOtkazanoEvent:
                    Otkazano = true;
                    break;
            }
        }
    }
}