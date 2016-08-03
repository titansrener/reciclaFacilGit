namespace ReciclaFacil.Models.Entities_RF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Clientes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Clientes()
        {
            Carteiras = new HashSet<Carteiras>();
            ClientesColetas = new HashSet<ClientesColetas>();
            Notificacoes = new HashSet<Notificacoes>();
            Dicas = new HashSet<Dicas>();
        }

        [Key]
        public string clienteId { get; set; }

        [StringLength(11)]
        public string cpf { get; set; }

        [Required]
        [StringLength(1)]
        public string tipo { get; set; }

        [Required]
        [StringLength(75)]
        public string nome { get; set; }

        [Required]
        [StringLength(100)]
        public string endereco { get; set; }

        public DbGeometry enderecoCoordenada { get; set; }

        [Required]
        [StringLength(45)]
        public string email { get; set; }

        [Required]
        [StringLength(1)]
        public string sexo { get; set; }

        public DateTime dataNascimento { get; set; }

        [StringLength(11)]
        public string telefone { get; set; }

        [Required]
        [StringLength(11)]
        public string celular { get; set; }

        [Required]
        [StringLength(128)]
        public string cooperativaId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Carteiras> Carteiras { get; set; }

        public virtual Cooperativas Cooperativas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientesColetas> ClientesColetas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Notificacoes> Notificacoes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Dicas> Dicas { get; set; }
    }
}
