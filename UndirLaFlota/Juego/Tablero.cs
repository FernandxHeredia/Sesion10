

namespace UndirLaFlota.Juego;

/// <summary>
/// Representa el tablero de juego, tanto para el jugador como para la IA.
/// Gestiona colocaci�n de barcos, jugadas, reinicio y detecci�n de hundimientos.
/// </summary>
public class Tablero
{
    /// <summary>
    /// Matriz 10x10 que representa el estado del tablero.
    /// 0: agua sin disparar, 1: agua disparada, 3: parte de barco tocada, >=10: barco con ID �nico.
    /// </summary>
    public List<List<int>> TableroList { get; set; } 

    private List<int> Barcos; // Lista con los tama�os de los barcos

    public int aciertos = 0; //N�mero de partes de barco tocadas

    private int TotalPuntos; //N�mero total de casillas que representan partes de barco

    public int Dim; //Dimensi�n del tablero (10 por defecto)

    private Dictionary<int, List<(int, int)>> BarcosCoordenadas; //Diccionario que guarda las coordenadas de cada barco por su respectiva ID

    private int barcoIdCounter = 10; //Contador para asignar IDs unicas a los diferentes barcos
    public int Jugadas { get; private set; } = 0; //N�mero total de partidas jugadas
    public int PartidasJugadas { get; private set; } = 0; //Numero total de jugadas 

    /// <summary>
    /// Constructor con opci�n de crear el tablero vacio
    /// </summary>
    /// <param name="vacio"> Determina si el tablero est� vacio o no</param>
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
    /// Constructor por defecto que g�nera los barcos autom�ticamente
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
    /// Coloca un �nico barco aleatoriamente en el tablero
    /// </summary>
    /// <param name="tamano">Tama�o del barco</param>
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
                // Verifica que est� dentro de los l�mites y que la celda est� libre
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

        // Coloca el barco usando el ID �nico
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
            return null; // ya se dispar� aqu�
        }
        else if (valor >= 10)
        {
            Jugadas++;
            aciertos++;
            TableroList[x][y] = 3;

            if (aciertos >= TotalPuntos)
                return "Partida finalizada";

            // Comprobar si el barco al que pertenece est� hundido
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
    /// Coloca el barco en una posici�n y direcci�n dadas manualmente
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tama�o"></param>
    /// <param name="direccion"></param>
    /// <returns> True si se pudo colocar, False si hubo colisi�n o desborde </returns>
    public bool ColocarBarcoManual(int x, int y, int tama�o, string direccion)
    {
        int dx = 0, dy = 0;
        switch (direccion)
        {
            case "Horizontal": dy = 1; break;
            case "Vertical": dx = 1; break;
        }

        // Validaci�n
        for (int i = 0; i < tama�o; i++)
        {
            int nx = x + dx * i;
            int ny = y + dy * i;

            if (nx < 0 || nx >= Dim || ny < 0 || ny >= Dim || TableroList[nx][ny] != 0)
                return false;
        }

        // Colocaci�n
        int id = barcoIdCounter++;
        BarcosCoordenadas[id] = new List<(int, int)>();

        for (int i = 0; i < tama�o; i++)
        {
            int nx = x + dx * i;
            int ny = y + dy * i;
            TableroList[nx][ny] = id;
            BarcosCoordenadas[id].Add((nx, ny));
        }

        TotalPuntos += tama�o;
        return true;
    }

}
