namespace TiendaWed.Models
{
    public class PedidoModel
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public int UsuarioId { get; set; } // ID del usuario logueado
        public string ClienteNombre { get; set; }
        public decimal Total { get; set; }

        // Relación con los detalles del pedido
        public List<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    }

    public class PedidoDetalle
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }   // FK hacia Pedido
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        // 🔥 Nueva propiedad para la imagen del producto
        public string UrlImagen { get; set; }

        // Subtotal calculado automáticamente
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
