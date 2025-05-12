

namespace UndirLaFlota.Juego;

/// <summary>
/// Representa el tablero de juego, tanto para el jugador como para la IA.
/// Gestiona colocación de barcos, jugadas, reinicio y detección de hundimientos.
/// </summary>
public class Tablero
{
    /// <summary>
    /// Matriz 10x10 que representa el estado del tablero.
    /// 0: agua sin disparar, 1: agua disparada, 3: parte de barco tocada, >=10: barco con ID único.
    /// </summary>
    public List<List<int>> TableroList { get; set; } 

    private List<int> Barcos; // Lista con los tamaños de los barcos

    public int aciertos = 0; //Número de partes de barco tocadas

    private int TotalPuntos; //Número total de casillas que representan partes de barco

    public int Dim; //Dimensión del tablero (10 por defecto)

    private Dictionary<int, List<(int, int)>> BarcosCoordenadas; //Diccionario que guarda las coordenadas de cada barco por su respectiva ID

    private int barcoIdCounter = 10; //Contador para asignar IDs unicas a los diferentes barcos
    public int Jugadas { get; private set; } = 0; //Número total de partidas jugadas
    public int PartidasJugadas { get; private set; } = 0; //Numero total de jugadas 

    /// <summary>
    /// Constructor con opción de crear el tablero vacio
    /// </summary>
    /// <param name="vacio"> Determina si el tablero está vacio o no</param>
    public Tablero(bool vacio)
    {
        Dim = 10;
        TableroList = new List<List<int>>();
        Barcos = new List<int> { 6, 5, 3, 3, 2 };
        BarcosCoordenadas = new Dictionary<int, List<(int, int)>>();

        foreach (int i in Barcos)
            TotalPuntos += i;

        TableroLimpio();

        if (!vacio)
            GenerarTablero();
    }

    /// <summary>
    /// Constructor por defecto que génera los barcos automáticamente
    /// </summary>
    public Tablero()
    {
        Dim = 10;
        TableroList = new List<List<int>>();
        Barcos = new List<int> { 6, 5, 3, 3, 2 };
        BarcosCoordenadas = new Dictionary<int, List<(int, int)>>();

        foreach (int i in Barcos)
            TotalPuntos += i;

        TableroLimpio();
        GenerarTablero();
    }

    /// <summary>
    /// Limpia el tablero, reinicia la matriz con todo 0.
    /// </summary>
    public void TableroLimpio()
    {
        aciertos = 0;
        TableroList.Clear(); 
        for (int i = 0; i < Dim; i++)
        {
            TableroList.Add(new List<int>());
            for (int j = 0; j < Dim; j++)
            {
                TableroList[i].Add(0);
            }
        }
    }

    /// <summary>
    /// Genera todos los barcos definidos en la lista Barcos
    /// </summary>
    public void GenerarTablero()
    {
        foreach (int tamano in Barcos)
        {
            GenerarBarco(tamano);
        }
    }

    /// <summary>
    /// Coloca un único barco aleatoriamente en el tablero
    /// </summary>
    /// <param name="tamano">Tamaño del barco</param>
    private void GenerarBarco(int tamano)
    {
        Random random = new Random();
        int x = 0, y = 0, dir = 0, P_x = 0, P_y = 0;
        bool entra = false;
        int pos;
        while (!entra)
        {
            entra = true;
            x = random.Next(0, Dim);
            y = random.Next(0, Dim);
            dir = random.Next(0, 4);  // 0: abajo, 1: arriba, 2: derecha, 3: izquierda
            P_x = x;
            P_y = y;
            pos = 0;
            while (pos < tamano)
            {
                // Verifica que esté dentro de los límites y que la celda esté libre
                if (P_x < 0 || P_x >= 10 || P_y < 0 || P_y >= 10 || TableroList[P_x][P_y] != 0)
                {
                    entra = false;
                    break;
                }
                switch (dir)
                {
                    case 0: P_x++; break;
                    case 1: P_x--; break;
                    case 2: P_y++; break;
                    case 3: P_y--; break;
                }
                pos++;
            }
        }

        // Coloca el barco usando el ID único
        P_x = x;
        P_y = y;
        int currentId = barcoIdCounter++;
        BarcosCoordenadas[currentId] = new List<(int, int)>();

        for (int i = 0; i < tamano; i++)
        {
            TableroList[P_x][P_y] = currentId;
            BarcosCoordenadas[currentId].Add((P_x, P_y));

            switch (dir)
            {
                case 0: P_x++; break;
                case 1: P_x--; break;
                case 2: P_y++; break;
                case 3: P_y--; break;
            }
        }
    }

    /// <summary>
    /// Procesa una jugada en la celda con coordenadas (x,y)
    /// </summary>
    /// <param name="x"> Coordenada en el eje X </param>
    /// <param name="y">Coordenada en el eje Y </param>
    /// <returns> "Agua", "Tocado", "Hundido" o "Partida finalizada" </returns>
    public string Jugada(int x, int y)
    {
        int valor = TableroList[x][y];

        if (valor == 0)
        {
            TableroList[x][y] = 1;
            Jugadas++;
            return "Agua";
        }
        else if (valor == 1 || valor == 3)
        {
            return null; // ya se disparó aquí
        }
        else if (valor >= 10)
        {
            Jugadas++;
            aciertos++;
            TableroList[x][y] = 3;

            if (aciertos >= TotalPuntos)
                return "Partida finalizada";

            // Comprobar si el barco al que pertenece está hundido
            var coords = BarcosCoordenadas[valor];
            bool hundido = coords.All(coord => TableroList[coord.Item1][coord.Item2] == 3);

            return hundido ? "Hundido" : "Tocado";
        }

        return null;
    }

    /// <summary>
    /// Reinicia el tablero y vuelve a colocar los barcos aleatoriamente.
    /// </summary>
    public void Reiniciar()
    {
        TotalPuntos = 0;
        PartidasJugadas++;
        BarcosCoordenadas.Clear();
        barcoIdCounter = 10;
        Jugadas = 0;
        aciertos = 0;

        foreach (int i in Barcos)
            TotalPuntos += i;

        TableroLimpio();
        GenerarTablero();
    }

    /// <summary>
    /// Coloca el barco en una posición y dirección dadas manualmente
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tamaño"></param>
    /// <param name="direccion"></param>
    /// <returns> True si se pudo colocar, False si hubo colisión o desborde </returns>
    public bool ColocarBarcoManual(int x, int y, int tamaño, string direccion)
    {
        int dx = 0, dy = 0;
        switch (direccion)
        {
            case "Horizontal": dy = 1; break;
            case "Vertical": dx = 1; break;
        }

        // Validación
        for (int i = 0; i < tamaño; i++)
        {
            int nx = x + dx * i;
            int ny = y + dy * i;

            if (nx < 0 || nx >= Dim || ny < 0 || ny >= Dim || TableroList[nx][ny] != 0)
                return false;
        }

        // Colocación
        int id = barcoIdCounter++;
        BarcosCoordenadas[id] = new List<(int, int)>();

        for (int i = 0; i < tamaño; i++)
        {
            int nx = x + dx * i;
            int ny = y + dy * i;
            TableroList[nx][ny] = id;
            BarcosCoordenadas[id].Add((nx, ny));
        }

        TotalPuntos += tamaño;
        return true;
    }

}
