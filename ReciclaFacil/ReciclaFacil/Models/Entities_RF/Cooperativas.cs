namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Cooperativas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Cooperativas()
        {
            Caminhoes = new HashSet<Caminhoes>();
            Clientes = new HashSet<Clientes>();
            Coletas = new HashSet<Coletas>();
            Funcionarios = new HashSet<Funcionarios>();
            MateriaisComercializados = new HashSet<MateriaisComercializados>();
            Notificacoes = new HashSet<Notificacoes>();
        }

        [Key]
        public string cooperativaId { get; set; }

        [Required]
        [StringLength(14)]
        public string cnpj { get; set; }

        [Required]
        [StringLength(100)]
        public string razaoSocial { get; set; }

        [Required]
        [StringLength(150)]
        public string endereco { get; set; }

        [Required]
        [StringLength(80)]
        public string cidade { get; set; }

        [Required]
        [StringLength(2)]
        public string estado { get; set; }

        public DbGeometry enderecoCoordenada { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Caminhoes> Caminhoes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Clientes> Clientes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Coletas> Coletas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Funcionarios> Funcionarios { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MateriaisComercializados> MateriaisComercializados { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Notificacoes> Notificacoes { get; set; }
    }
}
