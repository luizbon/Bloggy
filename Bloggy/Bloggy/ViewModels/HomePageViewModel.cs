using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Xamarin.Forms;

namespace Bloggy.ViewModels
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _client;
        private bool _isLoading;
        private string _message;
        private string _url = "https://channel9.msdn.com/Feeds/RSS";
        private readonly XNamespace _media = "http://search.yahoo.com/mrss/";

        public HomePageViewModel()
        {
            _client = new HttpClient();
            Items = new ObservableCollection<Article>();
            GoCommand = new Command(async () => await GetFeed());
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                if (value == _url) return;

                _url = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message == value) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public ICommand GoCommand { get; set; }
        public ObservableCollection<Article> Items { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private async Task GetFeed()
        {
            if (IsLoading) return;
            IsLoading = true;
            Message = string.Empty;
            try
            {
                var response = await _client.GetAsync(Url);

                if (response.IsSuccessStatusCode)
                {
                    var rssContent = await response.Content.ReadAsStringAsync();

                    var RssData = XElement.Parse(rssContent).Descendants("item").Select(x => new Article
                    {
                        Title = x.Element("title").Value,
                        Detail = x.Element("description").Value,
                        ImgSrc = x.Elements(_media + "thumbnail").OrderBy(e => e.Attribute("width").Value).First().Attribute("url").Value
                    });

                    Items.Clear();
                    foreach (var article in RssData)
                    {
                        Items.Add(article);
                    }
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Article
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public string ImgSrc { get; set; }
    }
}