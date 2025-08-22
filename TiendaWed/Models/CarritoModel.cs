

namespace TiendaWed.Models
{
    public class CarritoModel
    {
        public int Id { get; set; }
        public int Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string Urlimagen { get; set; }
        public string Categoria { get; set; }
    }
    public class CarritoItem
    {
        public CarritoModel Producto { get; set; }
        public int Cantidad { get; set; }
        public int Precio { get; internal set; }
        public int Id { get; internal set; }
    }
    public class productoSelecionados
    {
        public List<CarritoItem> Items { get; set; } = new List<CarritoItem>();
        public decimal TotalPrecio => Items.Sum(item => item.Producto.Precio * item.Cantidad);
    }
}
