﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using Goodreads8.ViewModel.Model;
using Chq.OAuth;
using Chq.OAuth.Credentials;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Goodreads8.ViewModel.Cache;
using System.Net;

namespace Goodreads8.ViewModel
{
    class GoodreadsAPI
    {
        private string m_consumerKey = "RXBTaDaKuEfeRrFR7XMjRw";
        private string m_consumerSecretKey = "7pf1F6j5PmN9cy5bzqFJcfhPyQTCRiplMPYNp2mBXQ";
        private string m_requestTokenUrl = "http://www.goodreads.com/oauth/request_token";
        private string m_accessTokenUrl = "http://www.goodreads.com/oauth/access_token";
        private string m_authorizeTokenUrl = "https://www.goodreads.com/oauth/authorize?mobile=1";
        private string m_callbackUrl = "http://www.hactar.com";

        private OAuthContext m_context = null;
        private Client m_client = null;

        private APICache m_cache;

        private GoodreadsAPI()
        {
            m_context = new OAuthContext(m_consumerKey, m_consumerSecretKey, m_requestTokenUrl, m_authorizeTokenUrl, m_accessTokenUrl, m_callbackUrl);
            m_client = new Client(m_context);

            //TODO, remove
            m_client.AccessToken = new TokenContainer();

            //GR@hactar.com
            //m_client.AccessToken.Secret = "9q2uH85GoKg1Hi8UZxfUdaHt43HwNd5ZBgX1vWR381g";
            //m_client.AccessToken.Token = "acThks95mAhQI0FXaKQCxg";
            //AuthenticatedUserId = 12725081;

            //Dan@hactar.com
            m_client.AccessToken.Secret = "ZBAiSbJQWsOHgASt1EKoigcXFqfozi8pJ5Nqaw";
            m_client.AccessToken.Token = "JgNTryl1vDVxh6k3sVGSw";
            AuthenticatedUserId = 889496;

            m_cache = new APICache();
        }

        public bool IsConfigured()
        {
            if(string.IsNullOrEmpty(m_client.AccessToken.Secret) ||
                string.IsNullOrEmpty(m_client.AccessToken.Token) ||
                AuthenticatedUserId <= 0)
                return false;

            return true;
        }

        public void ConfigureApi(String token, String secret, int id)
        {
            m_client.AccessToken.Secret = secret;
            m_client.AccessToken.Token = token;
            AuthenticatedUserId = id;
        }

        public int AuthenticatedUserId { get; set; }

        public async Task<int> GetAuthenticatedId()
        {
            String response = await m_client.MakeRequest("GET")
                    .ForResource(m_client.AccessToken.Token, new Uri("http://www.goodreads.com/api/auth_user"))
                    .Sign(m_client.AccessToken.Secret)
                    .ExecuteRequest();
            if (string.IsNullOrEmpty(response))
                return 0;

            return GoodreadsData.ParseAuthResponse(response);
        }

        public async Task<List<Update>> GetUpdates()
        {
            List<Update> cached = m_cache.GetUpdates();
            if (cached != null)
                return cached;


            String response = await m_client.MakeRequest("GET")
                    .ForResource(m_client.AccessToken.Token, new Uri("http://www.goodreads.com/updates/friends.xml"))
                    .Sign(m_client.AccessToken.Secret)
                    .ExecuteRequest();

            List<Update> data = GoodreadsData.ParseUpdates(response);
            if (data != null)
                m_cache.StoreUpdates(data);

            return data;
        }

        public async Task<Profile> GetProfile(int userId)
        {
            Profile cached = m_cache.GetProfile(userId);
            if (cached != null)
                return cached;

            String requestUrl = "http://www.goodreads.com/user/show/" + userId + ".xml";

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey }) //it's important to add the prameters after the resource, internal paramerters reset on ForResource calls
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            Profile data = GoodreadsData.ParseProfile(response);
            if (data != null)
                m_cache.StoreProfile(userId, data);

            return data;
        }

        public async Task<ReviewSet> GetShelfContents(int userId, String shelf, int page, String sort, String order, int perBooksPage)
        {
            if (string.IsNullOrEmpty(shelf) || string.IsNullOrEmpty(sort) || string.IsNullOrEmpty(order) || userId < 1)
                return null;

            if (page < 1)
                page = 1;
            if (perBooksPage < 1)
                perBooksPage = 20;

            //Check for cached item
            ReviewSet cached = m_cache.GetShelfContents(userId, shelf, page, sort, order);
            if (cached != null)
                return cached;

            shelf = Uri.EscapeUriString(shelf);

            String requestUrl = "http://www.goodreads.com/review/list/" + userId + ".xml";

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey, shelf = shelf, page = page, order = order, per_page = perBooksPage, v = "2", sort = sort }) //it's important to add the prameters after the resource, internal paramerters reset on ForResource calls
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            ReviewSet data = GoodreadsData.ParseShelf(response);

            if (data != null)
                m_cache.StoreShelfContents(userId, shelf, page, sort, order, data);

            return data;
        }

        public async Task<Book> GetBook(int bookId)
        {
            Book cached = m_cache.GetBook(bookId);
            if (cached != null)
                return cached;

            String requestUrl = "http://www.goodreads.com/book/show/" + bookId;

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey, format = "xml" })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            Book data = GoodreadsData.ParseBook(response);

            if (data != null)
                m_cache.StoreBook(bookId, data);

            return data;
        }

        public async Task<Author> GetAuthor(int authorId)
        {
            Author cached = m_cache.GetAuthor(authorId);
            if (cached != null)
                return cached;

            String requestUrl = "http://www.goodreads.com/author/show/" + authorId + ".xml";

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            Author data = GoodreadsData.ParseAuthor(response);
            if (data != null)
                m_cache.StoreAuthor(authorId, data);

            return data;
        }

        public async Task<FriendSet> GetFriends(int userId, int page)
        {
            FriendSet cached = m_cache.GetFriends(userId, page);
            if (cached != null)
                return cached;

            String requestUrl = "http://www.goodreads.com/friend/user/" + userId + ".xml";

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey, format = "xml", page = page }) //it's important to add the prameters after the resource, internal paramerters reset on ForResource calls
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            FriendSet data = GoodreadsData.ParseFriends(response);
            if (data != null)
                m_cache.StoreFriends(userId, page, data);

            return data;
        }

        public async Task<Review> GetReview(int reviewId)
        {
            Review cached = m_cache.GetReview(reviewId);
            if (cached != null)
                return cached;

            String requestUrl = "http://www.goodreads.com/review/show/" + reviewId + ".xml";

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            Review data = GoodreadsData.ParseReview(response);

            if (data != null)
                m_cache.StoreReview(reviewId, data);

            return data;
        }

        public async Task<List<Book>> GetSearchResults(String query)
        {
            //query = Uri.EscapeUriString(query);

            String requestUrl = "http://www.goodreads.com/search/search";

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey, format = "xml", q = query })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            return GoodreadsData.ParseSearchResults(response);
        }

        public async Task<Review> GetUserReview(int bookId, int userId)
        {
            Review cached = m_cache.GetUserReview(bookId, userId);
            if (cached != null)
                return cached;

            try
            {
                String requestUrl = "http://www.goodreads.com/review/show_by_user_and_book.xml";

                String response = await m_client.MakeRequest("GET")
                      .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                      .WithParameters(new { key = m_consumerKey, user_id = userId, book_id = bookId })
                      .Sign(m_client.AccessToken.Secret)
                      .ExecuteRequest();

                Review data = GoodreadsData.ParseReview(response);

                if (data != null)
                    m_cache.StoreUserReview(bookId, userId, data);

                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<Group>> GetGroups()
        {
            List<Group> cached = m_cache.GetGroups();
            if (cached != null)
                return cached;

            String requestUrl = "http://www.goodreads.com/group/list/" + AuthenticatedUserId + ".xml";

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            List<Group> data = GoodreadsData.ParseGroups(response);
            if (data != null)
                m_cache.StoreGroups(data);

            return data;
        }

        public async Task<Group> GetGroup(int groupId)
        {
            Group cached = m_cache.GetGroup(groupId);
            if (cached != null)
                return cached;

            String requestUrl = "http://www.goodreads.com/group/show/" + groupId + ".xml";

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            Group data = GoodreadsData.ParseGroup(response);

            if (data != null)
                m_cache.StoreGroup(groupId, data);

            return data;
        }

        public async Task<TopicSet> GetTopics(int groupId, int folderId, int page)
        {
            TopicSet cached = m_cache.GetTopicSet(groupId, folderId, page);
            if (cached != null)
                return cached;

            String folderValue = folderId.ToString();
            if (folderId == -1)
                folderValue = "unread";

            String requestUrl = "http://www.goodreads.com/topic/group_folder/" + folderValue + ".xml";
            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey, group_id = groupId, page = page })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            TopicSet data = GoodreadsData.ParseTopics(response);

            if (data != null)
               m_cache.StoreTopicSet(groupId, folderId, page, data);

            return data;
        }

        public async Task<Topic> GetTopic(int topicId)
        {
            Topic cached = m_cache.GetTopic(topicId);
            if (cached != null)
                return cached;

            String requestUrl = "http://www.goodreads.com/topic/show/" + topicId + ".xml";

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            Topic data = GoodreadsData.ParseTopic(response);

            if (data != null)
                m_cache.StoreTopic(topicId, data);

            return data;
        }

        public async Task<BookSet> GetAuthorBooks(int authorId, int page)
        {
            BookSet cached = m_cache.GetAuthorWorks(authorId, page);
            if (cached != null)
                return cached;

            String requestUrl = "http://www.goodreads.com/author/list.xml";
            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey, id = authorId, page = page })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            BookSet data = GoodreadsData.ParseAuthorShelf(response);

            if (data != null)
                m_cache.StoreAuthorWorks(authorId, page, data);

            return data;
        }

        public async Task<Status> GetStatus(int statusId)
        {
            Status cached = m_cache.GetStatus(statusId);
            if (cached != null)
                return cached;

            String requestUrl = "http://www.goodreads.com/user_status/show/" + statusId;

            String response = await m_client.MakeRequest("GET")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { key = m_consumerKey, format = "xml" })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            Status data = GoodreadsData.ParseStatus(response);

            if (data != null)
                m_cache.StoreStatus(statusId, data);

            return data;
        }


        /*-----------------------------------
         * Modification
         * --------------------------------*/
        public async Task<bool> AddToShelf(int bookId, String shelfName)
        {
            String requestUrl = "http://www.goodreads.com/shelf/add_to_shelf.xml";

            String response = await m_client.MakeRequest("POST")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { name = shelfName, book_id = bookId })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            if(string.IsNullOrEmpty(response) || !response.Contains("<shelf>"))
                return false;

            m_cache.InvalidateMyReview(AuthenticatedUserId, bookId);
            return true;
        }

        public async Task<bool> RemoveFromShelf(int bookId, String shelfName)
        {
            String requestUrl = "http://www.goodreads.com/shelf/add_to_shelf.xml";

            String response = await m_client.MakeRequest("POST")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { name = shelfName, book_id = bookId, a = "remove" })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            if(string.IsNullOrEmpty(response))
                return false;

            if(!response.Contains("removed") && !response.Contains("<shelf>"))
                return false;

            m_cache.InvalidateMyReview(AuthenticatedUserId, bookId);
            return true;
        }

        /// <summary>
        /// Post a status update
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <param name="body">Update text (optional)</param>
        /// <param name="page">Current page</param>
        /// <param name="percent">Percent done</param>
        /// <returns>true for success</returns>
        public async Task<bool> PostStatusUpdate(int bookId, String body, int page, int percent)
        {
            if (bookId <= 0)
                return false;

            if ((page > 0 && percent > 0) || (page <= 0 && percent <=0))
                return false;

            Dictionary<string, string> reqParams = new Dictionary<string,string>();
            reqParams.Add("user_status[book_id]", bookId.ToString());

            if(!string.IsNullOrEmpty(body))
                reqParams.Add("user_status[body]", body);

            if(page > 0)
                reqParams.Add("user_status[page]", page.ToString());
            else
                reqParams.Add("user_status[percent]", percent.ToString());


            HttpWebRequest req = await BuildRequest("http://www.goodreads.com/user_status.xml", reqParams);

            HttpWebResponse Response = (HttpWebResponse)await req.GetResponseAsync();
            StreamReader ResponseDataStream = new StreamReader(Response.GetResponseStream());
            String response = ResponseDataStream.ReadToEnd();
            if (response != null && response.Contains("user-status"))
                return true;

            return false;
        }

        public enum CommentType { review, topic, user_status }
        public async Task<bool> PostComment(int objectId, CommentType objectType, String body)
        {
            if (objectId <= 0 || string.IsNullOrEmpty(body))
                return false;

            Dictionary<string, string> reqParams = new Dictionary<string, string>();
            reqParams.Add("type", objectType.ToString());
            reqParams.Add("id", objectId.ToString());
            reqParams.Add("comment[body]", body);

            HttpWebRequest req = await BuildRequest("http://www.goodreads.com/comment.xml", reqParams);

            HttpWebResponse Response = (HttpWebResponse)await req.GetResponseAsync();
            StreamReader ResponseDataStream = new StreamReader(Response.GetResponseStream());
            String response = ResponseDataStream.ReadToEnd();
            if (response == null || !response.Contains("comment"))
                return false;

            if (objectType == CommentType.review)
                m_cache.InvalidateReview(objectId);
            else if (objectType == CommentType.user_status)
                m_cache.InvalidateStatus(objectId);
            else if (objectType == CommentType.topic)
                m_cache.InvalidateTopic(objectId);

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public async Task<bool> DestroyReview(int bookId)
        {
            if (bookId <= 0)
                return false;

            String requestUrl = "http://www.goodreads.com/review/destroy.xml";
            String response = await m_client.MakeRequest("POST")
                  .ForResource(m_client.AccessToken.Token, new Uri(requestUrl))
                  .WithParameters(new { book_id = bookId })
                  .Sign(m_client.AccessToken.Secret)
                  .ExecuteRequest();

            m_cache.InvalidateMyReview(AuthenticatedUserId, bookId);
            return true;
        }

        public async Task<bool> PostBookReview(int bookId, String review, int rating)
        {
            if (bookId <= 0 || rating <= 0)
                return false;

            Review myReview = await GetUserReview(bookId, AuthenticatedUserId);
            if (myReview != null)
            {
                await DestroyReview(bookId);
            }

            //new review
            Dictionary<string, string> reqParams = new Dictionary<string, string>();
            reqParams.Add("book_id", bookId.ToString());

            if (!string.IsNullOrEmpty(review))
                reqParams.Add("review[review]", review);

            reqParams.Add("review[rating]", rating.ToString());

            String dateRead = DateTime.Now.ToString("yyyy-MM-dd");
            reqParams.Add("review[read_at]", dateRead);


            HttpWebRequest req = await BuildRequest("http://www.goodreads.com/review.xml", reqParams);

            HttpWebResponse Response = (HttpWebResponse)await req.GetResponseAsync();
            StreamReader ResponseDataStream = new StreamReader(Response.GetResponseStream());
            String response = ResponseDataStream.ReadToEnd();
            if (response != null && response.Contains("review"))
                return true;

            return false;
        }

        /********************************
         * Utility
         * *****************************/
        private async Task<HttpWebRequest> BuildRequest(String oAuthURL, IDictionary<string, string> parameters)
        {
            var rawRequestUrl = new Uri(oAuthURL +
                ToQueryString(parameters)
            );
            string normalizedUrl;
            string requestParameters;
            var oAuth = new OAuthBase();
            var signature = oAuth.GenerateSignature(
                rawRequestUrl,
                m_consumerKey, m_consumerSecretKey,
                m_client.AccessToken.Token, m_client.AccessToken.Secret,
                "POST", oAuth.GenerateTimeStamp(), oAuth.GenerateNonce(),
                OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl,
                out requestParameters
            );
            var requestUrl = new Uri(normalizedUrl);
            var httpRequest = (HttpWebRequest)System.Net.WebRequest.Create(requestUrl);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            using (var requestStream = new StreamWriter(await httpRequest.GetRequestStreamAsync()))
            {
                requestStream.Write(
                    requestParameters
                    + "&oauth_signature="
                    + oAuth.UrlEncode(signature)
                );
            }

            return httpRequest;
        }

        private string ToQueryString(IDictionary<string, string> parameters)
        {
            var oAuth = new OAuthBase();
            List<String> converted = new List<string>();
            foreach (KeyValuePair<string, string> pair in parameters)
            {
                converted.Add(string.Format(
                        "{0}={1}",
                        oAuth.UrlEncode(pair.Key),
                        oAuth.UrlEncode(pair.Value)));
            }
            return "?" + string.Join("&", converted);

        }


        /*public static string UnZipStr(byte[] input)
        {
            using (MemoryStream inputStream = new MemoryStream(input))
            {
                using (DeflateStream gzip =
                  new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader =
                      new StreamReader(gzip, System.Text.Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }*/


    private static GoodreadsAPI instance;
    public static GoodreadsAPI Instance
        {
        get
            {
            if (instance == null)
                {
                instance = new GoodreadsAPI();
                }
            return instance;
            }
        }

    }
}
