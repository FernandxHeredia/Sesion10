using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UndirLaFlota
{
    /// <summary>
    /// Representa una entrada en el historial de partidas.
    /// Guarda el resultado (victoria o derrota), número de jugadas totales y la fecha de la partida.
    /// ¡Es necesario completar la partida para que esta se muestre!
    /// </summary>
    public class PartidaHistorial
    {
        public string Resultado { get; set; }
        public int Jugadas { get; set; }
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Propiedad de solo lectura que genera un resumen en una sola línea
        /// Se usa para mostrar la partida en una lista visual
        /// </summary>
        public string Resumen => $"{Resultado} | {Jugadas} jugadas | {Fecha:g}";

        // Ejemplo de salida: "Ganaste | 23 jugadas | 10/05/2025 17:22"
    }
}
