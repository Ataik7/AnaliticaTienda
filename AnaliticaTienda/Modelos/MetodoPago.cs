using System;

namespace AnaliticaTienda.Modelos
{
    // Métodos de pago posibles en una venta
    [Serializable]
    public enum MetodoPago
    {
        Efectivo = 0,
        Tarjeta = 1,
        Bizum = 2,
        Transferencia = 3
    }
}