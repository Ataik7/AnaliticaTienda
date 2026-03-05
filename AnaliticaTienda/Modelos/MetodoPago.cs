using System;

namespace AnaliticaTienda.Modelos
{
    // Métodos de pago posibles en una venta
    public enum MetodoPago
    {
        Efectivo = 0,
        Tarjeta = 1,
        Bizum = 2,
        Transferencia = 3
    }
}