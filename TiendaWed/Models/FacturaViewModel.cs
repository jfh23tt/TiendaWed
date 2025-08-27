namespace TiendaWed.Models
{
    public class FacturaViewModel
    {
        public int FacturaId { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }

        // Información del usuario
        public string NombreCliente { get; set; }
        public string CorreoCliente { get; set; }

        // Lista de productos facturados
        public List<FacturaDetalleViewModel> Detalles { get; set; } = new List<FacturaDetalleViewModel>();
    }

    public class FacturaDetalleViewModel
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

}
