namespace AppMonitoreo.Pages;

public partial class PegarRuta : ContentPage
{
	public PegarRuta()
	{
		InitializeComponent();
	}
    private void OnMostrarMapaClicked(object sender, EventArgs e)
    {
        string entrada = UrlEntry.Text?.Trim();

        // Buscar el valor del atributo src en el iframe
        string url = ExtraerSrcDeIframe(entrada);

        if (string.IsNullOrEmpty(url))
        {
            DisplayAlert("Error", "No se encontró una URL válida en el iframe.", "OK");
            return;
        }

        string html = $@"
    <html>
        <body style='margin:0;padding:0;'>
            <iframe src='{url}' width='100%' height='100%' style='border:0;' allowfullscreen='' loading='lazy'
                    referrerpolicy='no-referrer-when-downgrade'></iframe>
        </body>
    </html>";

        MapaWebView.Source = new HtmlWebViewSource { Html = html };
    }

    private string ExtraerSrcDeIframe(string iframeHtml)
    {
        // Busca src="...algo..."
        var match = System.Text.RegularExpressions.Regex.Match(iframeHtml, "src\\s*=\\s*\"([^\"]+)\"");
        return match.Success ? match.Groups[1].Value : null;
    }

}