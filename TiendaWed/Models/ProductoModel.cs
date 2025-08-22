using iText.Kernel.Pdf.Canvas.Wmf;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TiendaWed.Models;


public class ProductoModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El Campo {0} Es Requerido")]
    public string Codigo { get; set; }

    [Required(ErrorMessage = "El Campo {0} Es Requerido")]
    public string Nombre { get; set; }

    public string Categoria { get; set; }
    //public string Cantidad { get; set; }

    [Required(ErrorMessage = "El Campo {0} Es Requerido")]
    public string Descripcion { get; set; }

    [Required(ErrorMessage = "El Campo {0} Es Requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El Campo {0} Es Requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "Las unidades deben ser al menos 1")]
    public int Unidades { get; set; }

    [Required(ErrorMessage = "El Campo {0} Es Requerido")]
    public Estado estado { get; set; }

    public string Urlimagen { get; set; }

    [Required(ErrorMessage = "Por Favor, Seleccione Una Imagen")]
    [DataType(DataType.Upload)]
    public IFormFile ImageFile { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ScaffoldColumn(false)]
    public DateTime FechaCreacion { get; set; }
}
