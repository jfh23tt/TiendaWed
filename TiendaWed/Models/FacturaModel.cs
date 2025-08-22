namespace TiendaWed.Models
{
    public class FacturaModel
    {
        public int Id { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }

        // Relación con detalles
        public List<DetalleFacturaModel> Detalles { get; set; } = new();
    }

    public class DetalleFacturaModel
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
