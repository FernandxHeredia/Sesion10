using System;
using Microsoft.Maui.Controls;
using UndirLaFlota.Juego;

namespace UndirLaFlota
{
    /// <summary>
    /// Página principal del juego "Hundir la Flota"
    /// Gestiona el flujo de juego, colocación de barcos, disparos y turnos IA.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private Tablero tableroIA; // Tablero donde dispara el jugador (enemigo oculto)

        private Tablero tableroJugador;// Tablero del jugador, donde dispara la IA

        // Matrices de botones (visuales) para cada tablero
        private Button[,] botonesEnemigo;  // botones en el tablero enemigo (para disparar)
        private Button[,] botonesJugador;  // botones en tu tablero (para mostrar IA)

        // Controla si el jugador está en modo de colocación manual
        private bool modoColocacion = false;

        // Índice del barco actual que se está colocando manualmente
        private int barcoActual = 0;

        // Lista con tamaños estándar de los barcos
        private List<int> tamañosBarcos = new() { 6, 5, 3, 3, 2 };

        // Propiedad que devuelve la dirección seleccionada en el Picker
        private string direccionSeleccionada => DireccionPicker.SelectedItem?.ToString() ?? "Horizontal";

        // Lista de disparos ya realizados por la IA (para evitar repetir)
        private List<(int, int)> disparosIARealizados = new();

        // Indica si es turno del jugador
        private bool turnoJugador = true;

        // Última coordenada donde la IA tocó un barco
        private (int, int)? ultimoImpactoIA = null;

        // Cola con coordenadas vecinas pendientes de probar por la IA
        private Queue<(int, int)> posiblesObjetivos = new();

        // Lista estática donde se guarda el historial de partidas jugadas
        public static List<PartidaHistorial> HistorialPartidas = new();

        /// <summary>
        /// Constructor de la página principal. Inicializa el juego y el layout.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            tableroIA = new Tablero(); // tablero con barcos aleatorios
            tableroJugador = new Tablero(); // se limpiará si es colocación manual

            // Detecta orientación vertical u horizontal y ajusta el diseño
            this.SizeChanged += (s, e) =>
            {
                TablerosFlex.Direction = Width > Height
                    ? Microsoft.Maui.Layouts.FlexDirection.Row
                    : Microsoft.Maui.Layouts.FlexDirection.Column;
            };

            ColocarAleatorio(); // Arranque inicial con tablero aleatorio
        }

        /// <summary>
        /// Dibuja ambos tableros (jugador e IA) con coordenadas.
        /// </summary>
        private void DibujarTableros()
        {
            int dim = tableroIA.Dim;
            botonesJugador = new Button[dim, dim];
            botonesEnemigo = new Button[dim, dim];

            DibujarTableroConCoordenadas(TableroJugadorGrid, botonesJugador, esJugador: true);
            DibujarTableroConCoordenadas(TableroEnemigoGrid, botonesEnemigo, esJugador: false);
        }

        /// <summary>
        /// Crea el grid visual para un tablero con letras y números como encabezados
        /// </summary>
        /// <param name="grid"> Para detectar el grid correspondiente de la parte xaml</param>
        /// <param name="botones"> Botones correspondientes a cada tablero </param>
        /// <param name="esJugador"> Booleano para saber si es el tablero del jugador o no </param>
        private void DibujarTableroConCoordenadas(Grid grid, Button[,] botones, bool esJugador)
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            int dim = 10;
            string[] letras = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

            // Definir tamaño del grid
            for (int i = 0; i <= dim; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            // Letras (columnas) y números (filas)
            for (int i = 1; i <= dim; i++)
            {
                grid.Add(new Label
                {
                    Text = i.ToString(),
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center
                }, i, 0); // fila de números

                grid.Add(new Label
                {
                    Text = letras[i - 1],
                    FontAttributes = FontAttributes.Bold,
                    VerticalTextAlignment = TextAlignment.Center
                }, 0, i); // columna de letras
            }

            // Botones
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    var btn = new Button
                    {
                        FontSize = 12,
                        BackgroundColor = Colors.LightGray,
                        CommandParameter = new Tuple<int, int>(i, j),
                        Text = ""
                    };

                    if (esJugador && tableroJugador.TableroList[i][j] >= 10 && !modoColocacion)
                    {
                        btn.Text = "🛥️";
                        btn.BackgroundColor = Colors.GreenYellow;
                    }

                    if (esJugador)
                    {
                        // Para colocación manual
                        btn.Clicked += (s, e) =>
                        {
                            if (!modoColocacion) return;
                            var pos = (Tuple<int, int>)btn.CommandParameter;
                            ColocarBarcoManual(pos.Item1, pos.Item2);
                        };
                        botones[i, j] = btn;
                    }
                    else
                    {
                        // Para disparo al enemigo
                        btn.Clicked += DisparoJugador;
                        btn.IsEnabled = !modoColocacion;
                        botones[i, j] = btn;
                    }

                    grid.Add(btn, j + 1, i + 1); // +1 por encabezado
                }
            }
        }

        /// <summary>
        /// Acción del jugador al hacer click en un botón del tablero enemigo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DisparoJugador(object sender, EventArgs e)
        {
            if (!turnoJugador || modoColocacion) return;

            var btn = sender as Button;
            var pos = (Tuple<int, int>)btn.CommandParameter;
            int x = pos.Item1;
            int y = pos.Item2;

            string resultado = tableroIA.Jugada(x, y);
            if (resultado == null) return;

            switch (resultado)
            {
                case "Agua":
                    btn.Text = "🌊";
                    btn.BackgroundColor = Colors.LightBlue;
                    break;
                case "Tocado":
                    btn.Text = "💥";
                    btn.BackgroundColor = Colors.IndianRed;
                    break;
                case "Hundido":
                    btn.Text = "💣";
                    btn.BackgroundColor = Colors.Crimson;
                    await DisplayAlert("¡Hundido!", "Has hundido un barco enemigo.", "OK");
                    break;
                case "Partida finalizada":
                    btn.Text = "💣";
                    btn.BackgroundColor = Colors.Crimson;
                    await DisplayAlert("Victoria", "¡Has ganado!", "OK");

                    //Guarda en historial en caso de ganar el jugador
                    HistorialPartidas.Add(new PartidaHistorial
                    {
                        Resultado = "Ganaste",
                        Jugadas = tableroIA.Jugadas,
                        Fecha = DateTime.Now
                    });

                    ColocarAleatorio(); // reinicio
                    return;
            }

            ActualizarContadores();

            if (resultado == "Agua")
            {
                turnoJugador = false; // Solo se cede turno si fallas
                await Task.Delay(500);
                IADispara(); // Pasa turno
            }
            else
            {
                turnoJugador = true; // Puedes seguir disparando porque has acertado
            }
        }


        private void ColocarAleatorio_Clicked(object sender, EventArgs e) => ColocarAleatorio();

        /// <summary>
        /// Reinicia ambos tableros con colocación automática
        /// </summary>
        private void ColocarAleatorio()
        {
            tableroIA.Reiniciar();
            tableroJugador.Reiniciar();
            DibujarTableros();
            ActualizarContadores();
        }

        /// <summary>
        /// Inicia el modo colocación manual
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColocarManual_Clicked(object sender, EventArgs e)
        {
            tableroJugador = new Tablero(vacio: true); // necesitas una sobrecarga si no quieres generar barcos
            tableroIA.Reiniciar();
            modoColocacion = true;
            barcoActual = 0;
            DibujarTableros();
            DisplayAlert("Colocación", $"Coloca el barco de tamaño {tamañosBarcos[barcoActual]}", "OK");
        }

        /// <summary>
        /// Actualiza los contadores de jugadas, aciertos y partidas
        /// </summary>
        private void ActualizarContadores()
        {
            JugadasLabel.Text = $"Jugadas: {tableroIA.Jugadas}";
            AciertosLabel.Text = $"Aciertos: {tableroIA.aciertos}";
            PartidasLabel.Text = $"Partidas: {tableroIA.PartidasJugadas}";
        }

        /// <summary>
        /// Turno automático de la IA con una estrategia básica
        /// </summary>
        private async void IADispara()
        {
            turnoJugador = false;
            await Task.Delay(800);

            int x = 0, y = 0;
            string resultado = null;

            // Elegir disparo
            if (posiblesObjetivos.Count > 0)
            {
                (x, y) = posiblesObjetivos.Dequeue();
            }
            else if (ultimoImpactoIA is not null)
            {
                var (ix, iy) = ultimoImpactoIA.Value;
                var candidatos = new List<(int, int)>
        {
            (ix - 1, iy),
            (ix + 1, iy),
            (ix, iy - 1),
            (ix, iy + 1)
        };

                foreach (var pos in candidatos)
                {
                    if (EsCoordenadaValida(pos) && !disparosIARealizados.Contains(pos))
                    {
                        posiblesObjetivos.Enqueue(pos);
                    }
                }

                // Intentamos de nuevo si hay objetivos nuevos
                if (posiblesObjetivos.Count > 0)
                {
                    (x, y) = posiblesObjetivos.Dequeue();
                }
                else
                {
                    ultimoImpactoIA = null;
                    ObtenerDisparoAleatorio(out x, out y);
                }
            }
            else
            {
                ObtenerDisparoAleatorio(out x, out y);
            }

            disparosIARealizados.Add((x, y));
            resultado = tableroJugador.Jugada(x, y);
            var btn = botonesJugador[x, y];

            switch (resultado)
            {
                case "Agua":
                    btn.Text = "🌊";
                    btn.BackgroundColor = Colors.LightBlue;
                    await DisplayAlert("IA", $"La IA disparó en {Coord(x, y)}: Agua", "OK");
                    break;

                case "Tocado":
                    btn.Text = "💥";
                    btn.BackgroundColor = Colors.IndianRed;
                    await DisplayAlert("IA", $"La IA disparó en {Coord(x, y)}: ¡Tocado!", "OK");
                    ultimoImpactoIA = (x, y); // Guarda punto de impacto
                    break;

                case "Hundido":
                    btn.Text = "💣";
                    btn.BackgroundColor = Colors.Crimson;
                    await DisplayAlert("IA", $"¡La IA hundió un barco en {Coord(x, y)}!", "OK");
                    ultimoImpactoIA = null;
                    posiblesObjetivos.Clear();
                    break;

                case "Partida finalizada":
                    btn.Text = "💣";
                    btn.BackgroundColor = Colors.Crimson;
                    await DisplayAlert("Derrota", "La IA ha ganado. 😭", "OK");
                    HistorialPartidas.Add(new PartidaHistorial
                    {
                        Resultado = "Perdiste",
                        Jugadas = tableroIA.Jugadas,
                        Fecha = DateTime.Now
                    });
                    ColocarAleatorio(); // Reinicia la partida
                    return;
            }

            turnoJugador = true;
        }

        private void ObtenerDisparoAleatorio(out int x, out int y)
        {
            Random rnd = new Random();
            do
            {
                x = rnd.Next(0, tableroJugador.Dim);
                y = rnd.Next(0, tableroJugador.Dim);
            }
            while (disparosIARealizados.Contains((x, y)));
        }

        private bool EsCoordenadaValida((int, int) pos)
        {
            int x = pos.Item1;
            int y = pos.Item2;
            return x >= 0 && x < tableroJugador.Dim && y >= 0 && y < tableroJugador.Dim;
        }

        private string Coord(int x, int y)
        {
            char letra = (char)('A' + x);
            return $"{letra}{y + 1}";
        }

        /// <summary>
        /// Coloca un barco manualmente durante la fase de colocación
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        private void ColocarBarcoManual(int row, int column)
        {
            int tamaño = tamañosBarcos[barcoActual];
            string direccion = direccionSeleccionada;

            if (!tableroJugador.ColocarBarcoManual(row, column, tamaño, direccion))
            {
                DisplayAlert("Error", "No se puede colocar ahí el barco.", "OK");
                return;
            }

            // Marcar visualmente el barco
            int dx = 0, dy = 0;
            if (direccion == "Horizontal") dy = 1;
            else dx = 1;

            for (int i = 0; i < tamaño; i++)
            {
                int x = row + dx * i;
                int y = column + dy * i;
                var b = botonesJugador[x, y];
                b.Text = "🛥️";
                b.BackgroundColor = Colors.GreenYellow;
            }

            barcoActual++;

            if (barcoActual >= tamañosBarcos.Count)
            {
                modoColocacion = false;
                DisplayAlert("¡Listo!", "Todos los barcos han sido colocados. ¡Empieza el juego!", "OK");

                // Ocultar barcos (limpiar visual sin borrar lógica)
                for (int i = 0; i < tableroJugador.Dim; i++)
                {
                    for (int j = 0; j < tableroJugador.Dim; j++)
                    {
                        botonesJugador[i, j].Text = "";
                        botonesJugador[i, j].BackgroundColor = Colors.LightGray;
                    }
                }
            }
            else
            {
                DisplayAlert("Siguiente", $"Coloca el barco de tamaño {tamañosBarcos[barcoActual]}", "OK");
            }
        }

        private async void VerHistorial_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HistorialPage());
        }

    }
}
