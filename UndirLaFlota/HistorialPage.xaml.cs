namespace UndirLaFlota;

public partial class HistorialPage : ContentPage
{
    /// <summary>
    /// Página que muestra el historial de partidas jugadas.
    /// Se enlaza a una lista estática (HistorialPartidas) desde el MainPage
    /// </summary>
	public HistorialPage()
	{
        InitializeComponent();

        //Enlaza el ListView definido en el XAML con la lista de partidas
        //Esta lista se actualiza en tiempo real cada vez que se gana o pierde una partida
        HistorialListView.ItemsSource = MainPage.HistorialPartidas;
    }
}