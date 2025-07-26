using MedVoll.Web.Models;

namespace MedVoll.Web.Dtos
{
    public class ConsultaDto
    {
        public ConsultaDto()
        {
        }

        public ConsultaDto(
            long Id,
            long IdMedico,
            string Paciente,
            
            DateTime Data,
            Especialidade Especialidade
        )
        {
            this.Id = Id;
            this.IdMedico = IdMedico;
            this.Paciente = Paciente;
            this.Data = Data;
            this.Especialidade = Especialidade;
        }

        public ConsultaDto(Consulta consulta)
        {
            Id = consulta.Id;
            IdMedico = consulta.MedicoId;
            MedicoNome = consulta.Medico.Nome;
            Paciente = consulta.Paciente;
            Data = consulta.Data;
            Especialidade = consulta.Medico.Especialidade;
        }

        public long? Id { get; set; }
        public string _method { get; set; }

        public long IdMedico { get; set; }
        public string MedicoNome { get; set; }
        public string Paciente { get; set; }
        public DateTime Data { get; set; }
        public Especialidade Especialidade { get; set; }
    }
}


