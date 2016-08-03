namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Coletas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Coletas()
        {
            ClientesColetas = new HashSet<ClientesColetas>();
            Notificacoes = new HashSet<Notificacoes>();
            Rotas = new HashSet<Rotas>();
            Caminhoes = new HashSet<Caminhoes>();
            Funcionarios = new HashSet<Funcionarios>();
        }

        [Key]
        public int coletaId { get; set; }

        public DateTime? horaAgendada { get; set; }

        public double? quantidade { get; set; }

        [Required]
        [StringLength(1)]
        public string coletado { get; set; }

        [Required]
        [StringLength(128)]
        public string cooperativaId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientesColetas> ClientesColetas { get; set; }

        public virtual Cooperativas Cooperativas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Notificacoes> Notificacoes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Rotas> Rotas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Caminhoes> Caminhoes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Funcionarios> Funcionarios { get; set; }
    }
}
