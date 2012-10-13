using Goodreads8.Common;
using Goodreads8.ViewModel;
using Goodreads8.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Authentication.Web;
using System.Diagnostics;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Chq.OAuth;
using Chq.OAuth.Credentials;
using System.Threading.Tasks;
using Microsoft.Advertising.WinRT.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Goodreads8
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Goodreads8.Common.LayoutAwarePage
    {
        private MainPageViewModel model;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private const String m_keyToken = "Key_Token";
        private const String m_keySecret = "Key_Secret";
        private const String m_keyID = "Key_UserID";

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ConfigureApp();
        }

        private async Task ConfigureApp()
        {
            GoodreadsAPI api = GoodreadsAPI.Instance;
            if (!api.IsConfigured())
            {
                if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(m_keyToken) &&
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(m_keySecret) &&
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(m_keyID))
                {
                    //Load config
                    String token = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[m_keyToken];
                    String secret = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[m_keySecret];
                    int UserId = (int)Windows.Storage.ApplicationData.Current.LocalSettings.Values[m_keyID];
                    api.ConfigureApi(token, secret, UserId);
                }
                else
                {
                    MessageDialog dialog = new MessageDialog("To use Goodreads RT you must first login to you must first login to your Goodreads account");
                    UICommandInvokedHandler cmdHandler = new UICommandInvokedHandler(cmd =>
                    {
                        Debug.WriteLine("id:{0} label:{1}", cmd.Id, cmd.Label);
                    });

                    UICommand cmd1 = new UICommand("Login", cmdHandler, 1);
                    UICommand cmd2 = new UICommand("Quit", cmdHandler, 2);

                    dialog.Commands.Add(cmd1);
                    dialog.Commands.Add(cmd2);
                    dialog.DefaultCommandIndex = 0;

                    IUICommand result = await dialog.ShowAsync();
                    if (result.Label.Equals("Quit"))
                    {
                        Application.Current.Exit();
                        return;
                    }

                    bool authResult = await DoOAuthLogin();
                    if (!authResult)
                    {
                        MessageDialog failDialog = new MessageDialog("Unable to login to Goodreads");
                        await failDialog.ShowAsync();
                        Application.Current.Exit();
                    }
                }
            }

            //if they were able to login
            if (api.IsConfigured())
            {
                model = new MainPageViewModel();
                this.DataContext = model;
            }
        }

        private const string _consumerKey = "RXBTaDaKuEfeRrFR7XMjRw";
        private const string _consumerSecretKey = "7pf1F6j5PmN9cy5bzqFJcfhPyQTCRiplMPYNp2mBXQ";
        private const string _linkedInRequestTokenUrl = "http://www.goodreads.com/oauth/request_token";
        private const string _linkedInAccessTokenUrl = "http://www.goodreads.com/oauth/access_token";
        private const string _linkedInAuthorizeTokenUrl = "https://www.goodreads.com/oauth/authorize?mobile=1";
        private const string _callbackUrl = "https://www.goodreads.com/oauth/authorize?mobile=1";
        private async Task<bool> DoOAuthLogin()
        {
            try
            {
            var context = new OAuthContext(_consumerKey, _consumerSecretKey, _linkedInRequestTokenUrl, _linkedInAuthorizeTokenUrl, _linkedInAccessTokenUrl, "http://www.hactar.com");
            var client = new Client(context);

            String requestTokenResponse = await client.MakeRequest("GET")
                                .ForRequestToken()
                                .WithParameters(new { scope = "email" }) //Optional, changes depending on provider
                                .Sign()
                                .ExecuteRequest();

            if (string.IsNullOrEmpty(requestTokenResponse))
                return false;

            client.RequestToken = TokenContainer.Parse(requestTokenResponse);

            String authURL = "https://www.goodreads.com/oauth/authorize?oauth_token=" + client.RequestToken.Token + "&oauth_callback=http://www.hactar.com";
            Uri authorizationUri = new Uri(authURL);
            WebAuthenticationResult WebAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, authorizationUri, client.Context.CallbackUri);
            if (WebAuthenticationResult.ResponseStatus != WebAuthenticationStatus.Success)
                return false;

            String result = WebAuthenticationResult.ResponseData;
            if (!result.Contains("authorize=1"))
                return false;

            int start = result.IndexOf('=');
            int end = result.LastIndexOf('&');
            String verifier = result.Substring(start + 1, end - start - 1);

            String accessTokenResponse = await client.MakeRequest("GET")
                    .ForAccessToken(client.RequestToken.Token, verifier)
                    .Sign(client.RequestToken.Secret)
                    .ExecuteRequest();

            client.AccessToken = TokenContainer.Parse(accessTokenResponse);

            GoodreadsAPI api = GoodreadsAPI.Instance;
            String token = client.AccessToken.Token;
            String secret = client.AccessToken.Secret;
            int id = await api.GetAuthenticatedId(token, secret);

            if(string.IsNullOrEmpty(token) ||
                string.IsNullOrEmpty(secret) ||
                id <= 0)
                return false;

            Windows.Storage.ApplicationData.Current.LocalSettings.Values[m_keyToken] = token;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[m_keySecret] = secret;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values[m_keyID] = id;

            api.ConfigureApi(token, secret, id);

            return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private void Shelf_ItemClick(object sender, ItemClickEventArgs e)
        {
            Shelf clicked = e.ClickedItem as Shelf;

            GoodreadsAPI api = GoodreadsAPI.Instance;
            BrowseShelfPage.Args arg = new BrowseShelfPage.Args();
            arg.UserId = api.AuthenticatedUserId;
            arg.ShelfName = clicked.Name;

            this.Frame.Navigate(typeof(BrowseShelfPage), arg);
        }

        private void Group_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GroupListPage));
        }

        private void Friend_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BrowseFriendsPage));
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SearchPage));
        }

        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ViewStatusPage), 18330707);
            //GoodreadsAPI api = GoodreadsAPI.Instance;
            //api.PostComment(422858193, GoodreadsAPI.CommentType.review, "comment1");
            //api.PostStatusUpdate(3642452, "body", 161, 0);
        }

        private void Update_Click(object sender, ItemClickEventArgs e)
        {
            Update u = e.ClickedItem as Update;
            if (u.Type == Update.Actions.review)
            {
                String link = u.Link;
                if(string.IsNullOrEmpty(link) || !link.StartsWith("http://www.goodreads.com/review/show/"))
                    return;

                String parse = link.Replace("http://www.goodreads.com/review/show/", "");
                int reviewId = int.Parse(parse);

                this.Frame.Navigate(typeof(ViewReviewPage), reviewId);
            }
            else if (u.Type == Update.Actions.comment)
            {
                if(string.IsNullOrEmpty(u.Link) || !u.Link.StartsWith("http://www.goodreads.com/topic/show/"))
                    return;

                String parse = u.Link.Replace("http://www.goodreads.com/topic/show/", "");
                int pos = parse.IndexOf('-');
                if(pos > 0)
                    parse = parse.Substring(0, pos);

                int topicId = int.Parse(parse);

                this.Frame.Navigate(typeof(TopicPage), topicId );
            }
            else if (u.Type == Update.Actions.userstatus)
            {
                if (string.IsNullOrEmpty(u.Link) || !u.Link.StartsWith("http://www.goodreads.com/user_status/show/"))
                    return;

                String parse = u.Link.Replace("http://www.goodreads.com/user_status/show/", "");
                int statusId = int.Parse(parse);

                this.Frame.Navigate(typeof(ViewStatusPage), statusId);
            }
        }

        private const String m_updatesKey = "TileUpdateTime";
        public static async void SetupTiles()
        {
            DateTime expiration = DateTime.MinValue;
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(m_updatesKey))
            {
                expiration = DateTime.FromBinary((long)Windows.Storage.ApplicationData.Current.LocalSettings.Values[m_updatesKey]);
            }

            if (expiration.CompareTo(DateTime.Now) > 0)
                return;

            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();

            GoodreadsAPI api = GoodreadsAPI.Instance;
            List<Update> updates = await api.GetUpdates();
            if (updates == null || updates.Count == 0)
                return;

            for(int x=0; x<updates.Count && x<5; x++)
            {
                Update u = updates[x];

                //Wide
                var wideTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideSmallImageAndText03);
                var imageXml = wideTileXml.GetElementsByTagName("image");
                imageXml.Item(0).Attributes.GetNamedItem("src").InnerText = u.ImageUrl;
                var textXml = wideTileXml.GetElementsByTagName("text");
                if(u.UpdateText != null)
                    textXml.Item(0).InnerText = u.UpdateText;

                //Square
                var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquarePeekImageAndText04);
                imageXml = tileXml.GetElementsByTagName("image");
                imageXml.Item(0).Attributes.GetNamedItem("src").InnerText = u.ImageUrl;
                textXml = tileXml.GetElementsByTagName("text");
                if (u.UpdateText != null)
                    textXml.Item(0).InnerText = u.UpdateText;

                // Merge the two XML :
                var tempNode = wideTileXml.ImportNode(tileXml.GetElementsByTagName("binding").Item(0), true);
                wideTileXml.GetElementsByTagName("visual").Item(0).AppendChild(tempNode);

                // create notification
                var tileNotification = new TileNotification(wideTileXml);
                tileNotification.ExpirationTime = DateTime.Now.AddHours(2);

                // send notification
                updater.Update(tileNotification);
            }

            Windows.Storage.ApplicationData.Current.LocalSettings.Values[m_updatesKey] = DateTime.Now.AddHours(2).ToBinary();
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            GoodreadsAPI api = GoodreadsAPI.Instance;
            api.LogoutApi();

            model = null;
            this.DataContext = null;

            //Clear updates
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();
            Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove(m_updatesKey);

            //Flush stored credentials
            Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove(m_keyToken);
            Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove(m_keySecret);
            Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove(m_keyID);            

            //Try to reconfigure
            await ConfigureApp();
        }
    }
}
